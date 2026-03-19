use crate::core::rect::WorkArea;
use crate::services::win32::Win32WindowService;
use crate::services::window_enumerator::WindowEnumerator;
use crate::services::config::ConfigService;
use crate::services::logger::Logger;

/// 平铺所有窗口管理器
pub struct TileAllManager {
    win32: Win32WindowService,
    config_service: Option<ConfigService>,
}

impl TileAllManager {
    pub fn new(win32: Win32WindowService, config_service: Option<ConfigService>) -> Self {
        Self { win32, config_service }
    }

    /// 平铺所有窗口
    pub fn tile_all(&self, work_area: &WorkArea) {
        let windows = WindowEnumerator::enumerate_visible_windows(true, true);
        if windows.is_empty() {
            return;
        }

        self.tile_windows(work_area, &windows);

        Logger::info("TileAllManager", &format!("平铺 {} 个窗口", windows.len()));
    }

    /// 平铺当前应用的所有窗口
    pub fn tile_active_app(&self, work_area: &WorkArea, process_name: &str) {
        let windows = WindowEnumerator::enumerate_windows_by_process(process_name, true);
        if windows.is_empty() {
            return;
        }

        self.tile_windows(work_area, &windows);

        Logger::info("TileAllManager", &format!("平铺应用 {} 的 {} 个窗口", process_name, windows.len()));
    }

    /// 平铺窗口（内部实现）
    fn tile_windows(&self, work_area: &WorkArea, windows: &[isize]) {
        let window_count = windows.len();
        let (cols, rows) = self.calculate_grid_layout(window_count, work_area);

        let cell_width = work_area.width() / cols;
        let cell_height = work_area.height() / rows;

        for (index, hwnd) in windows.iter().enumerate() {
            if index >= (cols * rows) as usize {
                break;
            }

            let col = (index as i32) % cols;
            let row = (index as i32) / cols;

            let x = work_area.left + col * cell_width;
            let y = work_area.top + row * cell_height;

            self.win32.set_window_rect(*hwnd, x, y, cell_width, cell_height);
        }
    }

    /// 计算最佳网格布局
    fn calculate_grid_layout(&self, window_count: usize, work_area: &WorkArea) -> (i32, i32) {
        let count = window_count as i32;

        match window_count {
            0 => (1, 1),
            1 => (1, 1),
            2 => (2, 1),
            3 => (3, 1),
            4 => (2, 2),
            5 | 6 => (3, 2),
            7 | 8 | 9 => (3, 3),
            10 | 11 | 12 => (4, 3),
            13 | 14 | 15 | 16 => (4, 4),
            _ => {
                // 更多窗口时按比例计算
                let aspect_ratio = work_area.width() as f64 / work_area.height() as f64;
                let total = count as f64;
                let cols = (total.sqrt() * aspect_ratio.sqrt()).ceil() as i32;
                let rows = (total / cols as f64).ceil() as i32;
                (cols.max(1), rows.max(1))
            }
        }
    }
}

impl Default for TileAllManager {
    fn default() -> Self {
        Self::new(Win32WindowService::new(), None)
    }
}
