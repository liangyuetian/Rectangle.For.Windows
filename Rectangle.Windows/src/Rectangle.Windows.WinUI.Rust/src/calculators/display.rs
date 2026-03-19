use crate::core::rect::{WorkArea, WindowRect};
use crate::core::action::WindowAction;
use crate::core::calculator::RectCalculator;
use crate::services::win32::Win32WindowService;

/// 显示器切换计算器 trait
pub trait DisplayCalculator: RectCalculator {
    /// 获取目标显示器索引
    fn get_target_display(&self, current_display: i32, display_count: i32) -> i32;
}

/// 下一个显示器计算器
pub struct NextDisplayCalculator {
    win32_service: Win32WindowService,
}

impl NextDisplayCalculator {
    pub fn new() -> Self {
        Self {
            win32_service: Win32WindowService::new(),
        }
    }
}

impl RectCalculator for NextDisplayCalculator {
    fn calculate(&self, work_area: &WorkArea, current_window: &WindowRect, _action: WindowAction, gap: i32) -> WindowRect {
        // 获取所有显示器
        let work_areas = self.win32_service.get_monitor_work_areas();
        if work_areas.len() <= 1 {
            // 只有一个显示器，保持当前位置
            return *current_window;
        }

        // 找到当前窗口所在的显示器
        let mut current_display = 0;
        for (i, wa) in work_areas.iter().enumerate() {
            if wa.left == work_area.left && wa.top == work_area.top {
                current_display = i;
                break;
            }
        }

        // 计算下一个显示器
        let next_display = (current_display + 1) % work_areas.len();
        let target_work_area = &work_areas[next_display];

        // 计算在新工作区中的相对位置
        let relative_x = (current_window.x - work_area.left) as f64 / work_area.width() as f64;
        let relative_y = (current_window.y - work_area.top) as f64 / work_area.height() as f64;
        let relative_width = current_window.width as f64 / work_area.width() as f64;
        let relative_height = current_window.height as f64 / work_area.height() as f64;

        // 应用到新工作区
        let new_x = target_work_area.left + (relative_x * target_work_area.width() as f64) as i32;
        let new_y = target_work_area.top + (relative_y * target_work_area.height() as f64) as i32;
        let new_width = (relative_width * target_work_area.width() as f64) as i32;
        let new_height = (relative_height * target_work_area.height() as f64) as i32;

        WindowRect::new(new_x, new_y, new_width, new_height)
    }
}

/// 上一个显示器计算器
pub struct PreviousDisplayCalculator {
    win32_service: Win32WindowService,
}

impl PreviousDisplayCalculator {
    pub fn new() -> Self {
        Self {
            win32_service: Win32WindowService::new(),
        }
    }
}

impl RectCalculator for PreviousDisplayCalculator {
    fn calculate(&self, work_area: &WorkArea, current_window: &WindowRect, _action: WindowAction, gap: i32) -> WindowRect {
        // 获取所有显示器
        let work_areas = self.win32_service.get_monitor_work_areas();
        if work_areas.len() <= 1 {
            // 只有一个显示器，保持当前位置
            return *current_window;
        }

        // 找到当前窗口所在的显示器
        let mut current_display = 0;
        for (i, wa) in work_areas.iter().enumerate() {
            if wa.left == work_area.left && wa.top == work_area.top {
                current_display = i;
                break;
            }
        }

        // 计算上一个显示器
        let prev_display = if current_display == 0 {
            work_areas.len() - 1
        } else {
            current_display - 1
        };
        let target_work_area = &work_areas[prev_display];

        // 计算在新工作区中的相对位置
        let relative_x = (current_window.x - work_area.left) as f64 / work_area.width() as f64;
        let relative_y = (current_window.y - work_area.top) as f64 / work_area.height() as f64;
        let relative_width = current_window.width as f64 / work_area.width() as f64;
        let relative_height = current_window.height as f64 / work_area.height() as f64;

        // 应用到新工作区
        let new_x = target_work_area.left + (relative_x * target_work_area.width() as f64) as i32;
        let new_y = target_work_area.top + (relative_y * target_work_area.height() as f64) as i32;
        let new_width = (relative_width * target_work_area.width() as f64) as i32;
        let new_height = (relative_height * target_work_area.height() as f64) as i32;

        WindowRect::new(new_x, new_y, new_width, new_height)
    }
}
