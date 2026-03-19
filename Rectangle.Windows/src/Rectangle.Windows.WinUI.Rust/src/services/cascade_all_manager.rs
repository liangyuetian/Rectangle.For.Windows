use crate::core::rect::{WorkArea, WindowRect};
use crate::services::win32::Win32WindowService;
use crate::services::window_enumerator::WindowEnumerator;
use crate::services::config::ConfigService;
use crate::services::logger::Logger;

/// 层叠所有窗口管理器
pub struct CascadeAllManager {
    win32: Win32WindowService,
    config_service: Option<ConfigService>,
}

impl CascadeAllManager {
    pub fn new(win32: Win32WindowService, config_service: Option<ConfigService>) -> Self {
        Self { win32, config_service }
    }

    /// 层叠所有窗口
    pub fn cascade_all(&self, work_area: &WorkArea) {
        let windows = WindowEnumerator::enumerate_visible_windows(true, true);
        if windows.is_empty() {
            return;
        }

        let offset = self.get_cascade_offset();
        let (window_width, window_height) = self.calculate_window_size(work_area, windows.len());

        for (index, hwnd) in windows.iter().enumerate() {
            let x = work_area.left + (index as i32) * offset;
            let y = work_area.top + (index as i32) * offset;

            // 确保不超出工作区边界
            let x = x.min(work_area.right - window_width);
            let y = y.min(work_area.bottom - window_height);

            self.win32.set_window_rect(*hwnd, x, y, window_width, window_height);
        }

        Logger::info("CascadeAllManager", &format!("层叠 {} 个窗口", windows.len()));
    }

    /// 层叠当前应用的所有窗口
    pub fn cascade_active_app(&self, work_area: &WorkArea, process_name: &str) {
        let windows = WindowEnumerator::enumerate_windows_by_process(process_name, true);
        if windows.is_empty() {
            return;
        }

        let offset = self.get_cascade_offset();
        let (window_width, window_height) = self.calculate_window_size(work_area, windows.len());

        for (index, hwnd) in windows.iter().enumerate() {
            let x = work_area.left + (index as i32) * offset;
            let y = work_area.top + (index as i32) * offset;

            // 确保不超出工作区边界
            let x = x.min(work_area.right - window_width);
            let y = y.min(work_area.bottom - window_height);

            self.win32.set_window_rect(*hwnd, x, y, window_width, window_height);
        }

        Logger::info("CascadeAllManager", &format!("层叠应用 {} 的 {} 个窗口", process_name, windows.len()));
    }

    /// 获取层叠偏移量
    fn get_cascade_offset(&self) -> i32 {
        if let Some(ref service) = self.config_service {
            let config = service.load();
            config.cascade_all_delta_size
        } else {
            30
        }
    }

    /// 计算窗口大小
    fn calculate_window_size(&self, work_area: &WorkArea, window_count: usize) -> (i32, i32) {
        let offset = self.get_cascade_offset();
        let total_offset = (window_count.saturating_sub(1) as i32) * offset;

        let width = (work_area.width() - total_offset).max(400);
        let height = (work_area.height() - total_offset).max(300);

        (width, height)
    }
}

impl Default for CascadeAllManager {
    fn default() -> Self {
        Self::new(Win32WindowService::new(), None)
    }
}
