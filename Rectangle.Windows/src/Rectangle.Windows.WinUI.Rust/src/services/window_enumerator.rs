use crate::services::logger::Logger;
use crate::services::win32::Win32WindowService;
use windows::Win32::Foundation::HWND;
use windows::Win32::UI::WindowsAndMessaging::{
    EnumWindows, GetWindowTextW, GetWindowTextLengthW, IsWindowVisible, IsIconic,
    GetWindowThreadProcessId, GetWindowLongPtrW, GWL_EXSTYLE,
};

/// 窗口枚举器
pub struct WindowEnumerator;

impl WindowEnumerator {
    /// 枚举所有可见窗口
    pub fn enumerate_visible_windows(exclude_minimized: bool, exclude_tool_windows: bool) -> Vec<isize> {
        let mut windows: Vec<isize> = Vec::new();
        let windows_ptr: *mut Vec<isize> = &mut windows;

        unsafe {
            let callback: windows::Win32::UI::WindowsAndMessaging::WNDENUMPROC =
                Some(Self::enum_windows_callback);

            EnumWindows(callback, windows_ptr as isize);
        }

        // 过滤窗口
        windows.into_iter()
            .filter(|hwnd| Self::is_window_valid(*hwnd, exclude_minimized, exclude_tool_windows))
            .collect()
    }

    /// 枚举指定进程的所有窗口
    pub fn enumerate_windows_by_process(process_name: &str, exclude_minimized: bool) -> Vec<isize> {
        let all_windows = Self::enumerate_visible_windows(exclude_minimized, true);

        all_windows.into_iter()
            .filter(|hwnd| {
                let proc = Self::get_process_name_from_window(*hwnd);
                proc.eq_ignore_ascii_case(process_name)
            })
            .collect()
    }

    /// 窗口枚举回调
    unsafe extern "system" fn enum_windows_callback(
        hwnd: HWND,
        lparam: windows::Win32::Foundation::LPARAM,
    ) -> windows::Win32::Foundation::BOOL {
        let windows = &mut *(lparam.0 as *mut Vec<isize>);
        windows.push(hwnd.0 as isize);
        windows::Win32::Foundation::BOOL(1) // 继续枚举
    }

    /// 检查窗口是否有效
    fn is_window_valid(hwnd: isize, exclude_minimized: bool, exclude_tool_windows: bool) -> bool {
        unsafe {
            let hwnd_ptr = HWND(hwnd as *mut std::ffi::c_void);

            // 检查窗口是否可见
            if !IsWindowVisible(hwnd_ptr).as_bool() {
                return false;
            }

            // 检查窗口标题
            let title = Self::get_window_title(hwnd);
            if title.is_empty() {
                return false;
            }

            // 排除最小化窗口
            if exclude_minimized && IsIconic(hwnd_ptr).as_bool() {
                return false;
            }

            // 排除工具窗口
            if exclude_tool_windows {
                let ex_style = GetWindowLongPtrW(hwnd_ptr, GWL_EXSTYLE) as u32;
                const WS_EX_TOOLWINDOW: u32 = 0x00000080;
                if (ex_style & WS_EX_TOOLWINDOW) != 0 {
                    return false;
                }
            }

            // 获取窗口矩形
            let win32 = Win32WindowService::new();
            if let Some((_, _, w, h)) = win32.get_window_rect(hwnd) {
                if w <= 0 || h <= 0 {
                    return false;
                }
            } else {
                return false;
            }

            true
        }
    }

    /// 获取窗口标题
    fn get_window_title(hwnd: isize) -> String {
        unsafe {
            let hwnd_ptr = HWND(hwnd as *mut std::ffi::c_void);
            let len = GetWindowTextLengthW(hwnd_ptr);
            if len == 0 {
                return String::new();
            }

            let mut buffer: Vec<u16> = vec![0; len as usize + 1];
            let result = GetWindowTextW(hwnd_ptr, &mut buffer);
            if result > 0 {
                String::from_utf16_lossy(&buffer[..result as usize])
            } else {
                String::new()
            }
        }
    }

    /// 获取窗口所属进程名称
    pub fn get_process_name_from_window(hwnd: isize) -> String {
        unsafe {
            let hwnd_ptr = HWND(hwnd as *mut std::ffi::c_void);
            let mut process_id: u32 = 0;
            GetWindowThreadProcessId(hwnd_ptr, &mut process_id as *mut u32);

            if process_id == 0 {
                return String::new();
            }

            // 使用 tasklist 获取进程名
            match std::process::Command::new("tasklist")
                .args(&["/FI", &format!("PID eq {}", process_id), "/FO", "CSV", "/NH"])
                .output()
            {
                Ok(output) => {
                    let output_str = String::from_utf8_lossy(&output.stdout);
                    let parts: Vec<&str> = output_str.split(',').collect();
                    if !parts.is_empty() {
                        parts[0].trim().trim_matches('"').to_string()
                    } else {
                        String::new()
                    }
                }
                Err(_) => String::new(),
            }
        }
    }

    /// 检查窗口是否是工具窗口
    fn is_tool_window(hwnd: isize) -> bool {
        unsafe {
            let hwnd_ptr = HWND(hwnd as *mut std::ffi::c_void);
            let ex_style = GetWindowLongPtrW(hwnd_ptr, GWL_EXSTYLE) as u32;
            const WS_EX_TOOLWINDOW: u32 = 0x00000080;
            (ex_style & WS_EX_TOOLWINDOW) != 0
        }
    }
}
