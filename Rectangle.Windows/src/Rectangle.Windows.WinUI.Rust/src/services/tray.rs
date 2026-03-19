use crate::core::action::WindowAction;
use crate::services::config::ConfigService;
use crate::services::logger::Logger;
use crate::services::window_manager::WindowManager;
use std::collections::HashMap;
use std::sync::Arc;
use windows::Win32::Foundation::{HWND, LPARAM, LRESULT, WPARAM};
use windows::Win32::Graphics::Gdi::HICON;
use windows::Win32::UI::Shell::{Shell_NotifyIconW, NIF_ICON, NIF_MESSAGE, NIF_TIP, NIM_ADD, NIM_DELETE, NIM_MODIFY, NOTIFYICONDATAW};
use windows::Win32::UI::WindowsAndMessaging::{
    CreatePopupMenu, DefWindowProcW, DestroyMenu, InsertMenuW, SetForegroundWindow, TrackPopupMenu,
    HMENU, MF_BYPOSITION, MF_STRING, TPM_BOTTOMALIGN, TPM_LEFTALIGN, WM_COMMAND, WM_RBUTTONUP,
    WM_USER,
};

const TRAY_ICON_ID: u32 = 1001;
const WM_TRAY_MESSAGE: u32 = WM_USER + 1;
const ID_MENU_SETTINGS: u32 = 2001;
const ID_MENU_EXIT: u32 = 2002;
const ID_MENU_LEFT_HALF: u32 = 2101;
const ID_MENU_RIGHT_HALF: u32 = 2102;
const ID_MENU_MAXIMIZE: u32 = 2103;
const ID_MENU_CENTER: u32 = 2104;

/// 托盘图标服务
pub struct TrayIconService {
    config_service: ConfigService,
    window_manager: Arc<WindowManager>,
    hwnd: Option<HWND>,
    nid: Option<NOTIFYICONDATAW>,
}

impl TrayIconService {
    /// 创建新的托盘图标服务
    pub fn new(config_service: ConfigService, window_manager: Arc<WindowManager>) -> Self {
        Self {
            config_service,
            window_manager,
            hwnd: None,
            nid: None,
        }
    }

    /// 初始化托盘图标
    pub fn initialize(&mut self, hwnd: HWND) -> Result<(), Box<dyn std::error::Error>> {
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
        nid.szTip[..tooltip.len().min(128)].copy_from_slice(&tooltip[..tooltip.len().min(128)]);

        unsafe {
            // 尝试加载应用图标，如果失败则使用默认图标
            nid.hIcon = windows::Win32::UI::WindowsAndMessaging::LoadIconW(
                None,
                windows::Win32::UI::WindowsAndMessaging::IDI_APPLICATION,
            )?;

            Shell_NotifyIconW(NIM_ADD, &nid)?;
        }

        self.nid = Some(nid);
        Logger::info("TrayIconService", "托盘图标初始化成功");

        Ok(())
    }

    /// 显示托盘菜单
    pub fn show_menu(&self) {
        unsafe {
            let hwnd = match self.hwnd {
                Some(h) => h,
                None => return,
            };

            let hmenu = CreatePopupMenu().unwrap_or(HMENU(std::ptr::null_mut()));
            if hmenu.0.is_null() {
                return;
            }

            // 插入菜单项
            let _ = InsertMenuW(hmenu, 0, MF_BYPOSITION | MF_STRING, ID_MENU_LEFT_HALF as usize, windows::core::PCWSTR("左半屏\0".encode_utf16().collect::<Vec<_>>().as_ptr()));
            let _ = InsertMenuW(hmenu, 1, MF_BYPOSITION | MF_STRING, ID_MENU_RIGHT_HALF as usize, windows::core::PCWSTR("右半屏\0".encode_utf16().collect::<Vec<_>>().as_ptr()));
            let _ = InsertMenuW(hmenu, 2, MF_BYPOSITION | MF_STRING, ID_MENU_MAXIMIZE as usize, windows::core::PCWSTR("最大化\0".encode_utf16().collect::<Vec<_>>().as_ptr()));
            let _ = InsertMenuW(hmenu, 3, MF_BYPOSITION | MF_STRING, ID_MENU_CENTER as usize, windows::core::PCWSTR("居中\0".encode_utf16().collect::<Vec<_>>().as_ptr()));
            let _ = InsertMenuW(hmenu, 4, MF_BYPOSITION | MF_STRING, 0, windows::core::PCWSTR(std::ptr::null())); // 分隔线
            let _ = InsertMenuW(hmenu, 5, MF_BYPOSITION | MF_STRING, ID_MENU_SETTINGS as usize, windows::core::PCWSTR("设置\0".encode_utf16().collect::<Vec<_>>().as_ptr()));
            let _ = InsertMenuW(hmenu, 6, MF_BYPOSITION | MF_STRING, ID_MENU_EXIT as usize, windows::core::PCWSTR("退出\0".encode_utf16().collect::<Vec<_>>().as_ptr()));

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

            DestroyMenu(hmenu);
        }
    }

    /// 处理菜单命令
    pub fn handle_command(&self, id: u32) -> bool {
        match id {
            ID_MENU_SETTINGS => {
                Logger::info("TrayIconService", "打开设置");
                // TODO: 打开设置窗口
                true
            }
            ID_MENU_EXIT => {
                Logger::info("TrayIconService", "退出应用");
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

    /// 处理托盘消息
    pub fn handle_tray_message(&self, lparam: LPARAM) {
        let msg = (lparam.0 & 0xFFFF) as u32;
        match msg {
            WM_RBUTTONUP => {
                self.show_menu();
            }
            _ => {}
        }
    }
}

impl Drop for TrayIconService {
    fn drop(&mut self) {
        if let Some(ref nid) = self.nid {
            unsafe {
                let _ = Shell_NotifyIconW(NIM_DELETE, nid);
            }
        }
    }
}
