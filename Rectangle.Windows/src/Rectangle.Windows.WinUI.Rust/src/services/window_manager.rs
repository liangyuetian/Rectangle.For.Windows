use crate::core::action::{SubsequentExecutionMode, WindowAction};
use crate::core::calculator_factory::CalculatorFactory;
use crate::core::history::WindowHistory;
use crate::core::rect::{WindowRect, WorkArea};
use crate::services::config::ConfigService;
use crate::services::logger::Logger;
use crate::services::win32::Win32WindowService;
use std::collections::HashSet;
use std::sync::{Arc, Mutex};

/// 窗口管理器
pub struct WindowManager {
    config_service: ConfigService,
    win32_service: Win32WindowService,
    calculator_factory: CalculatorFactory,
    window_history: Arc<Mutex<WindowHistory>>,
    ignored_apps: Arc<Mutex<HashSet<String>>>,
}

impl WindowManager {
    /// 创建新的窗口管理器
    pub fn new(config_service: ConfigService) -> Self {
        let calculator_factory = CalculatorFactory::new(Some(&config_service));
        let config = config_service.load();
        let ignored_apps: HashSet<String> = config.ignored_apps.iter().cloned().collect();

        Self {
            config_service,
            win32_service: Win32WindowService::new(),
            calculator_factory,
            window_history: Arc::new(Mutex::new(WindowHistory::new())),
            ignored_apps: Arc::new(Mutex::new(ignored_apps)),
        }
    }

    /// 执行窗口动作
    pub fn execute(&self, action: WindowAction, force_direct_action: bool) -> bool {
        let hwnd = self.win32_service.get_foreground_window_handle();
        if hwnd == 0 {
            Logger::warning("WindowManager", "没有活动窗口");
            return false;
        }

        // 检查窗口是否被忽略
        if self.is_app_ignored(hwnd) {
            Logger::info("WindowManager", "窗口在忽略列表中，跳过");
            return false;
        }

        // 获取当前窗口矩形
        let current_rect = match self.win32_service.get_window_rect(hwnd) {
            Some(rect) => WindowRect::new(rect.0, rect.1, rect.2, rect.3),
            None => {
                Logger::warning("WindowManager", "无法获取窗口矩形");
                return false;
            }
        };

        // 获取工作区
        let work_area = match self.win32_service.get_work_area_from_window(hwnd) {
            Some(wa) => wa,
            None => {
                Logger::warning("WindowManager", "无法获取工作区");
                return false;
            }
        };

        // 获取配置
        let config = self.config_service.load();
        let gap = config.gap_size;

        // 处理特殊动作
        match action {
            WindowAction::Undo => return self.undo(hwnd),
            WindowAction::Redo => return self.redo(hwnd),
            WindowAction::Restore => return self.restore(hwnd),
            WindowAction::Maximize => {
                self.win32_service.show_window_maximize(hwnd);
                return true;
            }
            _ => {}
        }

        // 保存恢复点
        {
            let mut history = self.window_history.lock().unwrap();
            history.save_restore_rect_if_not_exists(hwnd, current_rect.x, current_rect.y, current_rect.width, current_rect.height);
        }

        // 获取计算器
        let calculator = match self.calculator_factory.get_calculator(action) {
            Some(calc) => calc,
            None => {
                Logger::warning("WindowManager", &format!("未找到动作 {:?} 的计算器", action));
                return false;
            }
        };

        // 计算新位置
        let new_rect = calculator.calculate(&work_area, &current_rect, action, gap);

        // 应用新位置
        let result = self.win32_service.set_window_rect(
            hwnd,
            new_rect.x,
            new_rect.y,
            new_rect.width,
            new_rect.height,
        );

        if result {
            // 记录操作
            let mut history = self.window_history.lock().unwrap();
            history.record_action(hwnd, action, new_rect.x, new_rect.y, new_rect.width, new_rect.height);
            history.mark_as_program_adjusted(hwnd);

            Logger::info("WindowManager", &format!("执行动作 {:?}: ({}, {}, {}, {})",
                action, new_rect.x, new_rect.y, new_rect.width, new_rect.height));
        }

        result
    }

    /// 撤销
    fn undo(&self, hwnd: isize) -> bool {
        let mut history = self.window_history.lock().unwrap();
        if let Some(action) = history.try_get_last_action(hwnd) {
            let (x, y, w, h) = action.rect;
            // 恢复到之前的状态
            let _ = self.win32_service.set_window_rect(hwnd, x, y, w, h);
            history.remove_last_action(hwnd);
            Logger::info("WindowManager", "执行撤销");
            true
        } else {
            Logger::warning("WindowManager", "没有可撤销的操作");
            false
        }
    }

    /// 重做
    fn redo(&self, _hwnd: isize) -> bool {
        // TODO: 实现重做功能
        Logger::info("WindowManager", "重做功能尚未实现");
        false
    }

    /// 恢复
    fn restore(&self, hwnd: isize) -> bool {
        let history = self.window_history.lock().unwrap();
        if let Some((x, y, w, h)) = history.try_get_restore_rect(hwnd) {
            let result = self.win32_service.set_window_rect(hwnd, x, y, w, h);
            Logger::info("WindowManager", "执行恢复");
            result
        } else {
            Logger::warning("WindowManager", "没有可恢复的窗口位置");
            false
        }
    }

    /// 检查应用是否在忽略列表中
    fn is_app_ignored(&self, hwnd: isize) -> bool {
        let process_name = self.win32_service.get_process_name_from_window(hwnd);
        let ignored = self.ignored_apps.lock().unwrap();
        ignored.iter().any(|app| {
            process_name.eq_ignore_ascii_case(app)
                || process_name.eq_ignore_ascii_case(&format!("{}.exe", app))
        })
    }

    /// 刷新忽略列表
    pub fn refresh_ignored_apps(&self) {
        let config = self.config_service.load();
        let mut ignored = self.ignored_apps.lock().unwrap();
        *ignored = config.ignored_apps.iter().cloned().collect();
    }
}

/// 操作历史管理器
pub struct OperationHistoryManager {
    history: Vec<(isize, WindowRect)>,
    current_index: usize,
    max_size: usize,
}

impl OperationHistoryManager {
    /// 创建新的操作历史管理器
    pub fn new(max_size: usize) -> Self {
        Self {
            history: Vec::new(),
            current_index: 0,
            max_size,
        }
    }

    /// 记录操作
    pub fn record(&mut self, hwnd: isize, rect: WindowRect) {
        // 移除当前位置之后的历史
        if self.current_index < self.history.len() {
            self.history.truncate(self.current_index);
        }

        self.history.push((hwnd, rect));
        self.current_index += 1;

        // 限制历史大小
        if self.history.len() > self.max_size {
            self.history.remove(0);
            self.current_index -= 1;
        }
    }

    /// 撤销
    pub fn undo(&mut self) -> Option<(isize, WindowRect)> {
        if self.current_index > 0 {
            self.current_index -= 1;
            self.history.get(self.current_index).copied()
        } else {
            None
        }
    }

    /// 重做
    pub fn redo(&mut self) -> Option<(isize, WindowRect)> {
        if self.current_index < self.history.len() {
            let result = self.history.get(self.current_index).copied();
            self.current_index += 1;
            result
        } else {
            None
        }
    }

    /// 是否可以撤销
    pub fn can_undo(&self) -> bool {
        self.current_index > 0
    }

    /// 是否可以重做
    pub fn can_redo(&self) -> bool {
        self.current_index < self.history.len()
    }
}
