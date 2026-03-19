use crate::core::action::WindowAction;
use crate::services::config::ConfigService;
use crate::services::logger::Logger;
use crate::services::window_manager::WindowManager;
use std::collections::HashMap;
use std::sync::Arc;
use windows::core::Result;
use windows::Win32::Foundation::{HWND, LPARAM, LRESULT, WPARAM};
use windows::Win32::UI::Shell::{
    Shell_NotifyIconW, NIF_ICON, NIF_MESSAGE, NIF_TIP, NIM_ADD, NIM_DELETE, NOTIFYICONDATAW,
};
use windows::Win32::UI::WindowsAndMessaging::{
    CreatePopupMenu, DefWindowProcW, InsertMenuW, SetForegroundWindow, ShowWindow, TrackPopupMenu,
    HICON, HMENU, MF_BYPOSITION, MF_STRING, SW_HIDE, TPM_BOTTOMALIGN, TPM_LEFTALIGN, WM_COMMAND,
    WM_RBUTTONUP, WM_USER, WS_OVERLAPPEDWINDOW,
};

const TRAY_ICON_ID: u32 = 1001;
const WM_TRAY_MESSAGE: u32 = WM_USER + 1;
const ID_MENU_SETTINGS: u32 = 2001;
const ID_MENU_EXIT: u32 = 2002;
const ID_MENU_LEFT_HALF: u32 = 2101;
const ID_MENU_RIGHT_HALF: u32 = 2102;
const ID_MENU_TOP_HALF: u32 = 2103;
const ID_MENU_BOTTOM_HALF: u32 = 2104;
const ID_MENU_MAXIMIZE: u32 = 2105;
const ID_MENU_CENTER: u32 = 2106;

/// 托盘图标（WinUI 3 风格）
pub struct TrayIcon {
    config_service: ConfigService,
    window_manager: Arc<WindowManager>,
    hwnd: Option<HWND>,
    nid: Option<NOTIFYICONDATAW>,
    show_settings_callback: Option<Box<dyn Fn() + Send + Sync>>,
}

impl TrayIcon {
    /// 创建新的托盘图标
    pub fn new(
        config_service: ConfigService,
        window_manager: Arc<WindowManager>,
    ) -> Result<Self> {
        Ok(Self {
            config_service,
            window_manager,
            hwnd: None,
            nid: None,
            show_settings_callback: None,
        })
    }

    /// 设置显示设置回调
    pub fn set_show_settings_callback<F>(&mut self, callback: F)
    where
        F: Fn() + Send + Sync + 'static,
    {
        self.show_settings_callback = Some(Box::new(callback));
    }

    /// 初始化托盘图标
    pub fn initialize(&mut self) -> Result<()> {
        // 创建消息窗口接收托盘消息
        let hwnd = self.create_message_window()?;
        self.hwnd = Some(hwnd);

        let mut nid = NOTIFYICONDATAW {
            cbSize: std::mem::size_of::<NOTIFYICONDATAW>() as u32,
            hWnd: hwnd,
            uID: TRAY_ICON_ID,
            uFlags: NIF_ICON | NIF_MESSAGE | NIF_TIP,
            uCallbackMessage: WM_TRAY_MESSAGE,
            ..Default::default()
        };

        // 设置提示文本
        let tooltip: Vec<u16> = "Rectangle 窗口管理器\0".encode_utf16().collect();
        nid.szTip[..tooltip.len().min(128)]
            .copy_from_slice(&tooltip[..tooltip.len().min(128)]);

        unsafe {
            // 加载应用图标
            nid.hIcon = windows::Win32::UI::WindowsAndMessaging::LoadIconW(
                None,
                windows::Win32::UI::WindowsAndMessaging::IDI_APPLICATION,
            )?;

            Shell_NotifyIconW(NIM_ADD, &nid)?;
        }

        self.nid = Some(nid);
        Logger::info("TrayIcon", "托盘图标初始化成功");

        Ok(())
    }

    /// 创建消息窗口
    fn create_message_window(&self) -> Result<HWND> {
        unsafe {
            let class_name: Vec<u16> = "RectangleTrayWindow\0".encode_utf16().collect();

            let wnd_class = windows::Win32::UI::WindowsAndMessaging::WNDCLASSW {
                lpfnWndProc: Some(Self::window_proc),
                hInstance: windows::Win32::System::LibraryLoader::GetModuleHandleW(None)?,
                lpszClassName: windows::core::PCWSTR(class_name.as_ptr()),
                ..Default::default()
            };

            windows::Win32::UI::WindowsAndMessaging::RegisterClassW(&wnd_class);

            let hwnd = windows::Win32::UI::WindowsAndMessaging::CreateWindowExW(
                windows::Win32::UI::WindowsAndMessaging::WINDOW_EX_STYLE(0),
                windows::core::PCWSTR(class_name.as_ptr()),
                windows::core::PCWSTR(class_name.as_ptr()),
                WS_OVERLAPPEDWINDOW,
                0,
                0,
                0,
                0,
                windows::Win32::UI::WindowsAndMessaging::HWND_MESSAGE,
                None,
                wnd_class.hInstance,
                Some(self as *const _ as *const std::ffi::c_void),
            )?;

            Ok(hwnd)
        }
    }

    /// 显示托盘菜单
    fn show_menu(&self) {
        unsafe {
            let hwnd = match self.hwnd {
                Some(h) => h,
                None => return,
            };

            let hmenu = CreatePopupMenu().unwrap_or(HMENU(std::ptr::null_mut()));
            if hmenu.0.is_null() {
                return;
            }

            // 布局菜单
            let _ = InsertMenuW(
                hmenu,
                0,
                MF_BYPOSITION | MF_STRING,
                ID_MENU_LEFT_HALF as usize,
                windows::core::PCWSTR(
                    "左半屏\0".encode_utf16().collect::<Vec<_>>().as_ptr(),
                ),
            );
            let _ = InsertMenuW(
                hmenu,
                1,
                MF_BYPOSITION | MF_STRING,
                ID_MENU_RIGHT_HALF as usize,
                windows::core::PCWSTR(
                    "右半屏\0".encode_utf16().collect::<Vec<_>>().as_ptr(),
                ),
            );
            let _ = InsertMenuW(
                hmenu,
                2,
                MF_BYPOSITION | MF_STRING,
                ID_MENU_TOP_HALF as usize,
                windows::core::PCWSTR(
                    "上半屏\0".encode_utf16().collect::<Vec<_>>().as_ptr(),
                ),
            );
            let _ = InsertMenuW(
                hmenu,
                3,
                MF_BYPOSITION | MF_STRING,
                ID_MENU_BOTTOM_HALF as usize,
                windows::core::PCWSTR(
                    "下半屏\0".encode_utf16().collect::<Vec<_>>().as_ptr(),
                ),
            );
            let _ = InsertMenuW(
                hmenu,
                4,
                MF_BYPOSITION | MF_STRING,
                ID_MENU_MAXIMIZE as usize,
                windows::core::PCWSTR(
                    "最大化\0".encode_utf16().collect::<Vec<_>>().as_ptr(),
                ),
            );
            let _ = InsertMenuW(
                hmenu,
                5,
                MF_BYPOSITION | MF_STRING,
                ID_MENU_CENTER as usize,
                windows::core::PCWSTR(
                    "居中\0".encode_utf16().collect::<Vec<_>>().as_ptr(),
                ),
            );
            let _ = InsertMenuW(
                hmenu,
                6,
                MF_BYPOSITION | MF_STRING,
                0,
                windows::core::PCWSTR(std::ptr::null()),
            );
            let _ = InsertMenuW(
                hmenu,
                7,
                MF_BYPOSITION | MF_STRING,
                ID_MENU_SETTINGS as usize,
                windows::core::PCWSTR(
                    "设置...\0".encode_utf16().collect::<Vec<_>>().as_ptr(),
                ),
            );
            let _ = InsertMenuW(
                hmenu,
                8,
                MF_BYPOSITION | MF_STRING,
                ID_MENU_EXIT as usize,
                windows::core::PCWSTR(
                    "退出\0".encode_utf16().collect::<Vec<_>>().as_ptr(),
                ),
            );

            SetForegroundWindow(hwnd);

            let mut point = windows::Win32::Foundation::POINT::default();
            let _ = windows::Win32::UI::WindowsAndMessaging::GetCursorPos(&mut point);

            TrackPopupMenu(
                hmenu,
                TPM_LEFTALIGN | TPM_BOTTOMALIGN,
                point.x,
                point.y,
                0,
                hwnd,
                None,
            );

            windows::Win32::UI::WindowsAndMessaging::DestroyMenu(hmenu);
        }
    }

    /// 处理菜单命令
    fn handle_command(&self, id: u32) -> bool {
        match id {
            ID_MENU_SETTINGS => {
                Logger::info("TrayIcon", "打开设置");
                if let Some(ref callback) = self.show_settings_callback {
                    callback();
                }
                true
            }
            ID_MENU_EXIT => {
                Logger::info("TrayIcon", "退出应用");
                unsafe {
                    windows::Win32::UI::WindowsAndMessaging::PostQuitMessage(0);
                }
                true
            }
            ID_MENU_LEFT_HALF => {
                self.window_manager.execute(WindowAction::LeftHalf, true);
                true
            }
            ID_MENU_RIGHT_HALF => {
                self.window_manager.execute(WindowAction::RightHalf, true);
                true
            }
            ID_MENU_TOP_HALF => {
                self.window_manager.execute(WindowAction::TopHalf, true);
                true
            }
            ID_MENU_BOTTOM_HALF => {
                self.window_manager.execute(WindowAction::BottomHalf, true);
                true
            }
            ID_MENU_MAXIMIZE => {
                self.window_manager.execute(WindowAction::Maximize, true);
                true
            }
            ID_MENU_CENTER => {
                self.window_manager.execute(WindowAction::Center, true);
                true
            }
            _ => false,
        }
    }

    /// 窗口过程
    unsafe extern "system" fn window_proc(
        hwnd: HWND,
        msg: u32,
        wparam: WPARAM,
        lparam: LPARAM,
    ) -> LRESULT {
        let user_data = windows::Win32::UI::WindowsAndMessaging::GetWindowLongPtrW(
            hwnd,
            windows::Win32::UI::WindowsAndMessaging::GWLP_USERDATA,
        );

        if user_data != 0 {
            let tray_icon = &*(user_data as *const TrayIcon);

            match msg {
                WM_TRAY_MESSAGE => {
                    let event = (lparam.0 & 0xFFFF) as u32;
                    match event {
                        WM_RBUTTONUP => {
                            tray_icon.show_menu();
                        }
                        _ => {}
                    }
                    return LRESULT(0);
                }
                WM_COMMAND => {
                    let id = (wparam.0 & 0xFFFF) as u32;
                    if tray_icon.handle_command(id) {
                        return LRESULT(0);
                    }
                }
                _ => {}
            }
        }

        DefWindowProcW(hwnd, msg, wparam, lparam)
    }
}

impl Drop for TrayIcon {
    fn drop(&mut self) {
        if let Some(ref nid) = self.nid {
            unsafe {
                let _ = Shell_NotifyIconW(NIM_DELETE, nid);
            }
        }
    }
}
