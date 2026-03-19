use crate::core::rect::{WorkArea, WindowRect};
use crate::services::win32::Win32WindowService;
use crate::services::window_enumerator::WindowEnumerator;
use crate::services::logger::Logger;

/// 反转所有窗口管理器
pub struct ReverseAllManager {
    win32: Win32WindowService,
}

impl ReverseAllManager {
    pub fn new(win32: Win32WindowService) -> Self {
        Self { win32 }
    }

    /// 水平镜像反转所有窗口位置
    pub fn reverse_all(&self, work_area: &WorkArea) {
        let windows = WindowEnumerator::enumerate_visible_windows(true, true);
        if windows.is_empty() {
            return;
        }

        // 记录所有窗口的当前位置
        let window_rects: Vec<(isize, WindowRect)> = windows.iter()
            .filter_map(|hwnd| {
                self.win32.get_window_rect(*hwnd)
                    .map(|(x, y, w, h)| (*hwnd, WindowRect::new(x, y, w, h)))
            })
            .collect();

        // 水平镜像反转每个窗口
        for (hwnd, rect) in window_rects {
            let new_x = work_area.right - (rect.x - work_area.left) - rect.width;
            self.win32.set_window_rect(hwnd, new_x, rect.y, rect.width, rect.height);
        }

        Logger::info("ReverseAllManager", &format!("反转 {} 个窗口位置", windows.len()));
    }

    /// 垂直镜像反转所有窗口位置
    pub fn reverse_all_vertical(&self, work_area: &WorkArea) {
        let windows = WindowEnumerator::enumerate_visible_windows(true, true);
        if windows.is_empty() {
            return;
        }

        // 记录所有窗口的当前位置
        let window_rects: Vec<(isize, WindowRect)> = windows.iter()
            .filter_map(|hwnd| {
                self.win32.get_window_rect(*hwnd)
                    .map(|(x, y, w, h)| (*hwnd, WindowRect::new(x, y, w, h)))
            })
            .collect();

        // 垂直镜像反转每个窗口
        for (hwnd, rect) in window_rects {
            let new_y = work_area.bottom - (rect.y - work_area.top) - rect.height;
            self.win32.set_window_rect(hwnd, rect.x, new_y, rect.width, rect.height);
        }

        Logger::info("ReverseAllManager", &format!("垂直反转 {} 个窗口位置", windows.len()));
    }

    /// 完全反转（水平和垂直）
    pub fn reverse_all_full(&self, work_area: &WorkArea) {
        let windows = WindowEnumerator::enumerate_visible_windows(true, true);
        if windows.is_empty() {
            return;
        }

        // 记录所有窗口的当前位置
        let window_rects: Vec<(isize, WindowRect)> = windows.iter()
            .filter_map(|hwnd| {
                self.win32.get_window_rect(*hwnd)
                    .map(|(x, y, w, h)| (*hwnd, WindowRect::new(x, y, w, h)))
            })
            .collect();

        // 完全反转每个窗口
        for (hwnd, rect) in window_rects {
            let new_x = work_area.right - (rect.x - work_area.left) - rect.width;
            let new_y = work_area.bottom - (rect.y - work_area.top) - rect.height;
            self.win32.set_window_rect(hwnd, new_x, new_y, rect.width, rect.height);
        }

        Logger::info("ReverseAllManager", &format!("完全反转 {} 个窗口位置", windows.len()));
    }
}

impl Default for ReverseAllManager {
    fn default() -> Self {
        Self::new(Win32WindowService::new())
    }
}
