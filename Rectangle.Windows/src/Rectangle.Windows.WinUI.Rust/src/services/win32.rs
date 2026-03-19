use crate::core::rect::WorkArea;
use crate::services::logger::Logger;
use windows::Win32::Foundation::{HWND, LPARAM, LRESULT, POINT, RECT};
use windows::Win32::Graphics::Gdi::{HDC, HMONITOR, MONITORINFO};
use windows::Win32::UI::WindowsAndMessaging::{
    EnumDisplayMonitors, GetCursorPos, GetForegroundWindow, GetMonitorInfo,
    GetWindowLongPtrW, GetWindowRect, GetWindowThreadProcessId, IsWindow, IsWindowVisible,
    MonitorFromPoint, MonitorFromWindow, SetWindowPos, ShowWindow, GWL_STYLE,
    MONITOR_DEFAULTTONEAREST, SW_RESTORE, SWP_ASYNCWINDOWPOS, SWP_FRAMECHANGED, SWP_NOZORDER,
};

/// Win32 窗口服务
pub struct Win32WindowService;

impl Win32WindowService {
    /// 创建新的窗口服务
    pub fn new() -> Self {
        Self
    }

    /// 获取前台窗口句柄
    pub fn get_foreground_window_handle(&self) -> isize {
        let hwnd = GetForegroundWindow();
        hwnd.0 as isize
    }

    /// 获取窗口矩形
    pub fn get_window_rect(&self, hwnd: isize) -> Option<(i32, i32, i32, i32)> {
        let hwnd = HWND(hwnd as *mut std::ffi::c_void);
        let mut rect = RECT::default();
        unsafe {
            if GetWindowRect(hwnd, &mut rect).as_bool() {
                Some((rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top))
            } else {
                None
            }
        }
    }

    /// 设置窗口矩形
    pub fn set_window_rect(&self, hwnd: isize, x: i32, y: i32, width: i32, height: i32) -> bool {
        let hwnd = HWND(hwnd as *mut std::ffi::c_void);
        unsafe {
            if !IsWindow(hwnd).as_bool() {
                return false;
            }

            // 检查窗口是否最大化或最小化
            let style = GetWindowLongPtrW(hwnd, GWL_STYLE) as u32;
            if (style & 0x01000000) != 0 || (style & 0x20000000) != 0 {
                ShowWindow(hwnd, SW_RESTORE);
                std::thread::sleep(std::time::Duration::from_millis(50));
            }

            let result = SetWindowPos(
                hwnd,
                None,
                x,
                y,
                width,
                height,
                SWP_NOZORDER | SWP_FRAMECHANGED | SWP_ASYNCWINDOWPOS,
            );

            if !result.as_bool() {
                Logger::warning("Win32WindowService", &format!("SetWindowPos 失败，重试..."));
                SetWindowPos(hwnd, None, x, y, width, height, SWP_NOZORDER | SWP_FRAMECHANGED)
                    .as_bool()
            } else {
                true
            }
        }
    }

    /// 最大化窗口
    pub fn show_window_maximize(&self, hwnd: isize) {
        let hwnd = HWND(hwnd as *mut std::ffi::c_void);
        unsafe {
            ShowWindow(hwnd, windows::Win32::UI::WindowsAndMessaging::SW_MAXIMIZE);
        }
    }

    /// 恢复窗口
    pub fn show_window_restore(&self, hwnd: isize) {
        let hwnd = HWND(hwnd as *mut std::ffi::c_void);
        unsafe {
            ShowWindow(hwnd, SW_RESTORE);
        }
    }

    /// 获取窗口所在工作区
    pub fn get_work_area_from_window(&self, hwnd: isize) -> Option<WorkArea> {
        let hwnd = HWND(hwnd as *mut std::ffi::c_void);
        unsafe {
            let h_monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);
            let mut monitor_info = MONITORINFO {
                cbSize: std::mem::size_of::<MONITORINFO>() as u32,
                rcMonitor: RECT::default(),
                rcWork: RECT::default(),
                dwFlags: 0,
            };
            if GetMonitorInfo(h_monitor, &mut monitor_info).as_bool() {
                Some(WorkArea::new(
                    monitor_info.rcWork.left,
                    monitor_info.rcWork.top,
                    monitor_info.rcWork.right,
                    monitor_info.rcWork.bottom,
                ))
            } else {
                None
            }
        }
    }

    /// 获取所有显示器工作区
    pub fn get_monitor_work_areas(&self) -> Vec<WorkArea> {
        let mut work_areas: Vec<WorkArea> = Vec::new();

        unsafe {
            let callback: unsafe extern "system" fn(HMONITOR, HDC, *mut RECT, LPARAM) -> i32 =
                monitor_enum_callback;
            EnumDisplayMonitors(
                None,
                None,
                Some(callback),
                LPARAM(&mut work_areas as *mut _ as isize),
            );
        }

        work_areas
    }

    /// 从光标位置获取工作区
    pub fn get_work_area_from_cursor(&self) -> Option<WorkArea> {
        unsafe {
            let mut point = POINT::default();
            if GetCursorPos(&mut point).as_bool() {
                let h_monitor = MonitorFromPoint(point, MONITOR_DEFAULTTONEAREST);
                let mut monitor_info = MONITORINFO {
                    cbSize: std::mem::size_of::<MONITORINFO>() as u32,
                    rcMonitor: RECT::default(),
                    rcWork: RECT::default(),
                    dwFlags: 0,
                };
                if GetMonitorInfo(h_monitor, &mut monitor_info).as_bool() {
                    Some(WorkArea::new(
                        monitor_info.rcWork.left,
                        monitor_info.rcWork.top,
                        monitor_info.rcWork.right,
                        monitor_info.rcWork.bottom,
                    ))
                } else {
                    None
                }
            } else {
                None
            }
        }
    }

    /// 获取窗口进程名
    pub fn get_process_name_from_window(&self, hwnd: isize) -> String {
        let hwnd = HWND(hwnd as *mut std::ffi::c_void);
        unsafe {
            let mut process_id: u32 = 0;
            GetWindowThreadProcessId(hwnd, &mut process_id as *mut u32);
            if process_id == 0 {
                return "未知".to_string();
            }

            // 使用 windows crate 获取进程名
            match std::process::Command::new("tasklist")
                .args(&["/FI", &format!("PID eq {}", process_id), "/FO", "CSV", "/NH"])
                .output()
            {
                Ok(output) => {
                    let output_str = String::from_utf8_lossy(&output.stdout);
                    let parts: Vec<&str> = output_str.split(',').collect();
                    if parts.len() > 0 {
                        parts[0].trim().trim_matches('"').to_string()
                    } else {
                        "未知".to_string()
                    }
                }
                Err(_) => "未知".to_string(),
            }
        }
    }

    /// 判断窗口是否可见
    pub fn is_window_visible(&self, hwnd: isize) -> bool {
        let hwnd = HWND(hwnd as *mut std::ffi::c_void);
        unsafe { IsWindowVisible(hwnd).as_bool() }
    }

    /// 判断是否为窗口
    pub fn is_window(&self, hwnd: isize) -> bool {
        let hwnd = HWND(hwnd as *mut std::ffi::c_void);
        unsafe { IsWindow(hwnd).as_bool() }
    }
}

impl Default for Win32WindowService {
    fn default() -> Self {
        Self::new()
    }
}

/// 显示器枚举回调
unsafe extern "system" fn monitor_enum_callback(
    h_monitor: HMONITOR,
    _hdc: HDC,
    _rect: *mut RECT,
    lparam: LPARAM,
) -> i32 {
    let work_areas = &mut *(lparam.0 as *mut Vec<WorkArea>);

    let mut monitor_info = MONITORINFO {
        cbSize: std::mem::size_of::<MONITORINFO>() as u32,
        rcMonitor: RECT::default(),
        rcWork: RECT::default(),
        dwFlags: 0,
    };

    if GetMonitorInfo(h_monitor, &mut monitor_info).as_bool() {
        work_areas.push(WorkArea::new(
            monitor_info.rcWork.left,
            monitor_info.rcWork.top,
            monitor_info.rcWork.right,
            monitor_info.rcWork.bottom,
        ));
    }

    1 // 继续枚举
}
