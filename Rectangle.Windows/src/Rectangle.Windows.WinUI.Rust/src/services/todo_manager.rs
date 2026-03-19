use crate::core::rect::{WorkArea, WindowRect};
use crate::services::config::ConfigService;
use crate::services::win32::Win32WindowService;
use crate::services::window_enumerator::WindowEnumerator;
use crate::services::tile_all_manager::TileAllManager;
use crate::services::logger::Logger;

/// Todo 模式管理器
pub struct TodoManager {
    win32: Win32WindowService,
    config_service: ConfigService,
}

impl TodoManager {
    pub fn new(win32: Win32WindowService, config_service: ConfigService) -> Self {
        Self { win32, config_service }
    }

    /// 检查 Todo 模式是否启用
    pub fn is_todo_mode_enabled(&self) -> bool {
        let config = self.config_service.load();
        config.todo_mode && !config.todo_application.is_empty()
    }

    /// 获取 Todo 应用窗口句柄
    pub fn get_todo_window(&self) -> Option<isize> {
        let config = self.config_service.load();
        if config.todo_application.is_empty() {
            return None;
        }

        let windows = WindowEnumerator::enumerate_windows_by_process(&config.todo_application, true);
        windows.into_iter().next()
    }

    /// 调整 Todo 窗口到侧边栏位置
    pub fn adjust_todo_window(&self, work_area: &WorkArea) {
        let config = self.config_service.load();
        if !config.todo_mode {
            return;
        }

        let todo_hwnd = self.get_todo_window();
        if todo_hwnd.is_none() {
            return;
        }
        let todo_hwnd = todo_hwnd.unwrap();

        let sidebar_width = config.todo_sidebar_width;
        let is_left_side = config.todo_sidebar_side == 0; // 0 = Left

        let x = if is_left_side { work_area.left } else { work_area.right - sidebar_width };
        let y = work_area.top;
        let width = sidebar_width;
        let height = work_area.height();

        self.win32.set_window_rect(todo_hwnd, x, y, width, height);

        Logger::info("TodoManager", &format!("调整 Todo 窗口到 {}: ({}, {}, {}, {})",
            if is_left_side { "左侧" } else { "右侧" }, x, y, width, height));
    }

    /// 获取排除 Todo 区域后的可用工作区
    pub fn get_available_work_area(&self, full_work_area: &WorkArea) -> WorkArea {
        let config = self.config_service.load();
        if !config.todo_mode || config.todo_application.is_empty() {
            return *full_work_area;
        }

        let todo_hwnd = self.get_todo_window();
        if todo_hwnd.is_none() {
            return *full_work_area;
        }

        let sidebar_width = config.todo_sidebar_width;
        let is_left_side = config.todo_sidebar_side == 0;

        if is_left_side {
            WorkArea::new(
                full_work_area.left + sidebar_width,
                full_work_area.top,
                full_work_area.right,
                full_work_area.bottom,
            )
        } else {
            WorkArea::new(
                full_work_area.left,
                full_work_area.top,
                full_work_area.right - sidebar_width,
                full_work_area.bottom,
            )
        }
    }

    /// 检查窗口是否是 Todo 应用窗口
    pub fn is_todo_window(&self, hwnd: isize) -> bool {
        let config = self.config_service.load();
        if config.todo_application.is_empty() {
            return false;
        }

        let process_name = WindowEnumerator::get_process_name_from_window(hwnd);
        process_name.eq_ignore_ascii_case(&config.todo_application)
    }

    /// 获取所有非 Todo 窗口
    pub fn get_non_todo_windows(&self) -> Vec<isize> {
        let all_windows = WindowEnumerator::enumerate_visible_windows(true, true);
        all_windows.into_iter()
            .filter(|hwnd| !self.is_todo_window(*hwnd))
            .collect()
    }

    /// 重新布局所有非 Todo 窗口（在可用区域内）
    pub fn relayout_non_todo_windows(&self, work_area: &WorkArea) {
        let available_work_area = self.get_available_work_area(work_area);
        let windows = self.get_non_todo_windows();

        if windows.is_empty() {
            return;
        }

        // 使用平铺布局
        let tile_manager = TileAllManager::new(self.win32.clone(), Some(self.config_service.clone()));
        // 平铺窗口
        for (index, hwnd) in windows.iter().enumerate() {
            if index >= windows.len() {
                break;
            }
            // 简化的平铺逻辑
            let col = (index % 2) as i32;
            let row = (index / 2) as i32;
            let cols = 2;
            let rows = (windows.len() + 1) / 2;

            let cell_width = available_work_area.width() / cols as i32;
            let cell_height = available_work_area.height() / rows as i32;

            let x = available_work_area.left + col * cell_width;
            let y = available_work_area.top + row * cell_height;

            self.win32.set_window_rect(*hwnd, x, y, cell_width, cell_height);
        }

        Logger::info("TodoManager", &format!("重新布局 {} 个非 Todo 窗口", windows.len()));
    }

    /// 切换 Todo 模式
    pub fn toggle_todo_mode(&self, work_area: &WorkArea) {
        if self.is_todo_mode_enabled() {
            self.adjust_todo_window(work_area);
            self.relayout_non_todo_windows(work_area);
        }
    }
}
