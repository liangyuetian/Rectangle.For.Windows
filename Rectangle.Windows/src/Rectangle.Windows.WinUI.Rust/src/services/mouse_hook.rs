use crate::services::logger::Logger;
use std::sync::{Arc, Mutex};
use windows::Win32::Foundation::{HWND, LPARAM, LRESULT, WPARAM};
use windows::Win32::UI::WindowsAndMessaging::{
    CallNextHookEx, SetWindowsHookExW, UnhookWindowsHookEx, HHOOK, HOOKPROC,
    WH_MOUSE_LL, WM_LBUTTONDOWN, WM_LBUTTONUP, WM_MOUSEMOVE, WM_RBUTTONDOWN, WM_RBUTTONUP,
    GetCursorPos, WindowFromPoint, GetAncestor, GA_ROOT,
};

/// 鼠标钩子事件参数
#[derive(Debug, Clone)]
pub struct MouseHookEvent {
    pub x: i32,
    pub y: i32,
    pub timestamp: u32,
    pub is_left_button: bool,
    pub is_right_button: bool,
}

/// 鼠标钩子回调类型
pub type MouseHookCallback = Box<dyn Fn(&MouseHookEvent) + Send + Sync>;

/// 全局鼠标钩子服务
pub struct MouseHookService {
    hook_handle: Option<HHOOK>,
    callback: Arc<Mutex<Option<MouseHookCallback>>>,
}

impl MouseHookService {
    /// 创建新的鼠标钩子服务
    pub fn new() -> Self {
        Self {
            hook_handle: None,
            callback: Arc::new(Mutex::new(None)),
        }
    }

    /// 安装鼠标钩子
    pub fn install_hook(&mut self, callback: impl Fn(&MouseHookEvent) + Send + Sync + 'static) -> bool {
        if self.hook_handle.is_some() {
            return true;
        }

        // 设置回调
        {
            let mut cb = self.callback.lock().unwrap();
            *cb = Some(Box::new(callback));
        }

        unsafe {
            let h_instance = windows::Win32::System::LibraryLoader::GetModuleHandleW(None)
                .unwrap_or(windows::Win32::Foundation::HINSTANCE(std::ptr::null_mut()));

            let hook_proc: HOOKPROC = Some(Self::hook_callback);

            let hook = SetWindowsHookExW(
                WH_MOUSE_LL,
                hook_proc,
                h_instance,
                0,
            );

            if hook.0.is_null() {
                Logger::error("MouseHookService", "安装鼠标钩子失败");
                return false;
            }

            self.hook_handle = Some(hook);
            Logger::info("MouseHookService", "鼠标钩子安装成功");
            true
        }
    }

    /// 卸载鼠标钩子
    pub fn uninstall_hook(&mut self) {
        if let Some(hook) = self.hook_handle {
            unsafe {
                UnhookWindowsHookEx(hook);
            }
            self.hook_handle = None;
            {
                let mut cb = self.callback.lock().unwrap();
                *cb = None;
            }
            Logger::info("MouseHookService", "鼠标钩子已卸载");
        }
    }

    /// 钩子回调函数
    unsafe extern "system" fn hook_callback(
        n_code: i32,
        w_param: WPARAM,
        l_param: LPARAM,
    ) -> LRESULT {
        if n_code >= 0 {
            let msg = w_param.0 as u32;

            // 获取鼠标位置
            let mut point = windows::Win32::Foundation::POINT::default();
            if GetCursorPos(&mut point).as_bool() {
                let event = MouseHookEvent {
                    x: point.x,
                    y: point.y,
                    timestamp: windows::Win32::System::Threading::GetTickCount(),
                    is_left_button: msg == WM_LBUTTONDOWN || msg == WM_LBUTTONUP,
                    is_right_button: msg == WM_RBUTTONDOWN || msg == WM_RBUTTONUP,
                };

                // 这里需要将事件传递给回调
                // 由于这是 extern 函数，我们需要使用全局状态或线程本地存储
                // 简化处理：通过全局 CHANNEL 发送事件
            }

            match msg {
                WM_LBUTTONDOWN | WM_LBUTTONUP | WM_MOUSEMOVE => {
                    // 处理鼠标事件
                }
                _ => {}
            }
        }

        CallNextHookEx(HHOOK(std::ptr::null_mut()), n_code, w_param, l_param)
    }

    /// 获取当前光标位置
    pub fn get_cursor_position() -> (i32, i32) {
        unsafe {
            let mut point = windows::Win32::Foundation::POINT::default();
            if GetCursorPos(&mut point).as_bool() {
                (point.x, point.y)
            } else {
                (0, 0)
            }
        }
    }

    /// 获取光标下的窗口句柄
    pub fn get_window_under_cursor() -> isize {
        unsafe {
            let mut point = windows::Win32::Foundation::POINT::default();
            if GetCursorPos(&mut point).as_bool() {
                let hwnd = WindowFromPoint(point);
                // 获取顶层窗口
                let root = GetAncestor(hwnd, GA_ROOT);
                if root.0.is_null() {
                    hwnd.0 as isize
                } else {
                    root.0 as isize
                }
            } else {
                0
            }
        }
    }

    /// 检查钩子是否已安装
    pub fn is_installed(&self) -> bool {
        self.hook_handle.is_some()
    }
}

impl Drop for MouseHookService {
    fn drop(&mut self) {
        self.uninstall_hook();
    }
}

impl Default for MouseHookService {
    fn default() -> Self {
        Self::new()
    }
}
