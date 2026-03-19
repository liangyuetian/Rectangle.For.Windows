use crate::services::config::ConfigService;
use crate::services::window_manager::WindowManager;
use std::collections::HashMap;
use std::sync::Arc;
use windows::core::Result;
use windows::Win32::Foundation::{HWND, LPARAM, LRESULT, WPARAM};
use windows::Win32::System::LibraryLoader::GetModuleHandleW;
use windows::Win32::UI::WindowsAndMessaging::{
    CreateWindowExW, DefWindowProcW, GetWindowLongPtrW, RegisterClassW, SetWindowLongPtrW,
    ShowWindow, WNDCLASSW, CS_HREDRAW, CS_VREDRAW, CW_USEDEFAULT, GWLP_USERDATA, SW_HIDE, SW_SHOW,
    WM_CLOSE, WM_COMMAND, WM_CREATE, WM_DESTROY, WM_SIZE,
    WS_OVERLAPPEDWINDOW, WS_VISIBLE, WS_CHILD, WS_BORDER,
    ES_AUTOHSCROLL, BS_PUSHBUTTON,
    WINDOW_EX_STYLE, WS_EX_CLIENTEDGE,
    HMENU,
};

const IDC_OPEN_CONFIG: i32 = 100;

/// 主窗口（设置窗口）
pub struct MainWindow {
    hwnd: Option<HWND>,
    config_service: ConfigService,
    window_manager: Arc<WindowManager>,
}

impl MainWindow {
    /// 创建新的主窗口
    pub fn new(
        config_service: ConfigService,
        window_manager: Arc<WindowManager>,
    ) -> Result<Self> {
        Ok(Self {
            hwnd: None,
            config_service,
            window_manager,
        })
    }

    /// 创建窗口
    fn create_window(&mut self) -> Result<HWND> {
        unsafe {
            let class_name: Vec<u16> = "RectangleMainWindow\0".encode_utf16().collect();

            let wnd_class = WNDCLASSW {
                lpfnWndProc: Some(Self::window_proc),
                hInstance: GetModuleHandleW(None)?,
                lpszClassName: windows::core::PCWSTR(class_name.as_ptr()),
                style: CS_HREDRAW | CS_VREDRAW,
                hbrBackground: windows::Win32::Graphics::Gdi::HBRUSH((16 + 1) as _), // COLOR_BTNFACE + 1
                ..Default::default()
            };

            RegisterClassW(&wnd_class);

            let hwnd = CreateWindowExW(
                WINDOW_EX_STYLE(0),
                windows::core::PCWSTR(class_name.as_ptr()),
                windows::core::PCWSTR("Rectangle 设置\0".encode_utf16().collect::<Vec<_>>().as_ptr()),
                WS_OVERLAPPEDWINDOW & !WS_VISIBLE,
                CW_USEDEFAULT,
                CW_USEDEFAULT,
                800,
                600,
                None,
                None,
                wnd_class.hInstance,
                Some(self as *const _ as *const std::ffi::c_void),
            )?;

            SetWindowLongPtrW(
                hwnd,
                GWLP_USERDATA,
                self as *const _ as isize,
            );

            self.hwnd = Some(hwnd);
            self.create_controls(hwnd)?;

            Ok(hwnd)
        }
    }

    /// 创建控件
    fn create_controls(&mut self, hwnd: HWND) -> Result<()> {
        unsafe {
            let h_instance = GetModuleHandleW(None)?;

            // 创建说明文本
            let info_text: Vec<u16> = "Rectangle 窗口管理器\n\n快捷键配置请直接编辑 config.json 文件\n\n配置文件位置:\0".encode_utf16().collect();

            CreateWindowExW(
                WS_EX_CLIENTEDGE,
                windows::core::PCWSTR("STATIC\0".encode_utf16().collect::<Vec<_>>().as_ptr()),
                windows::core::PCWSTR(info_text.as_ptr()),
                WS_CHILD | WS_VISIBLE,
                20, 20, 740, 80,
                hwnd,
                None,
                h_instance,
                None,
            )?;

            // 创建配置路径显示
            let config_path = self.config_service.config_path().to_string_lossy().to_string() + "\0";
            CreateWindowExW(
                WS_EX_CLIENTEDGE,
                windows::core::PCWSTR("EDIT\0".encode_utf16().collect::<Vec<_>>().as_ptr()),
                windows::core::PCWSTR(config_path.encode_utf16().collect::<Vec<_>>().as_ptr()),
                WS_CHILD | WS_VISIBLE | WS_BORDER | ES_AUTOHSCROLL,
                20, 110, 740, 25,
                hwnd,
                None,
                h_instance,
                None,
            )?;

            // 创建快捷键说明
            let shortcuts_text: Vec<u16> = "\n常用快捷键:\n\
• Ctrl+Alt+←/→ - 左/右半屏          • Ctrl+Alt+↑/↓ - 上/下半屏\n\
• Ctrl+Alt+U/I/J/K - 左上/右上/左下/右下四角\n\
• Ctrl+Alt+D/F/G - 第一/中间/最后三分之一\n\
• Ctrl+Alt+E/R/T - 前两/中间两/后三分之二\n\
• Ctrl+Alt+Enter - 最大化            • Ctrl+Alt+C - 居中窗口\n\
• Ctrl+Alt+=/- - 放大/缩小           • Ctrl+Alt+Backspace - 恢复窗口\n\
• Ctrl+Alt+Z - 撤销                  • Ctrl+Alt+Shift+Z - 重做\n\
• Ctrl+Alt+Win+←/→ - 移动到上一/下一显示器\n\n\
提示: 修改 config.json 后重启应用生效\0".encode_utf16().collect();

            CreateWindowExW(
                WS_EX_CLIENTEDGE,
                windows::core::PCWSTR("STATIC\0".encode_utf16().collect::<Vec<_>>().as_ptr()),
                windows::core::PCWSTR(shortcuts_text.as_ptr()),
                WS_CHILD | WS_VISIBLE,
                20, 150, 740, 320,
                hwnd,
                None,
                h_instance,
                None,
            )?;

            // 创建打开配置文件按钮
            CreateWindowExW(
                WINDOW_EX_STYLE(0),
                windows::core::PCWSTR("BUTTON\0".encode_utf16().collect::<Vec<_>>().as_ptr()),
                windows::core::PCWSTR("打开配置文件\0".encode_utf16().collect::<Vec<_>>().as_ptr()),
                WS_CHILD | WS_VISIBLE | BS_PUSHBUTTON,
                500, 490, 120, 30,
                hwnd,
                windows::core::HMENU(IDC_OPEN_CONFIG as usize),
                h_instance,
                None,
            )?;

            // 创建确定按钮
            CreateWindowExW(
                WINDOW_EX_STYLE(0),
                windows::core::PCWSTR("BUTTON\0".encode_utf16().collect::<Vec<_>>().as_ptr()),
                windows::core::PCWSTR("确定\0".encode_utf16().collect::<Vec<_>>().as_ptr()),
                WS_CHILD | WS_VISIBLE | BS_PUSHBUTTON,
                640, 490, 80, 30,
                hwnd,
                windows::core::HMENU(1 as usize),
                h_instance,
                None,
            )?;
        }

        Ok(())
    }

    /// 显示窗口
    pub fn show(&mut self) {
        if self.hwnd.is_none() {
            if let Err(e) = self.create_window() {
                log::error!("创建窗口失败: {:?}", e);
                return;
            }
        }

        if let Some(hwnd) = self.hwnd {
            unsafe {
                ShowWindow(hwnd, SW_SHOW);
                self.center_window(hwnd);
            }
        }
    }

    /// 居中窗口
    unsafe fn center_window(&self, hwnd: HWND) {
        use windows::Win32::Graphics::Gdi::{GetMonitorInfo, MONITORINFO};
        use windows::Win32::UI::WindowsAndMessaging::{
            GetWindowRect, MonitorFromWindow, MONITOR_DEFAULTTONEAREST,
            SetWindowPos, SWP_NOSIZE, SWP_NOZORDER,
        };
        use windows::Win32::Foundation::RECT;

        let mut rc = RECT::default();
        if GetWindowRect(hwnd, &mut rc).as_bool() {
            let h_monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);
            let mut monitor_info = MONITORINFO {
                cbSize: std::mem::size_of::<MONITORINFO>() as u32,
                rcMonitor: RECT::default(),
                rcWork: RECT::default(),
                dwFlags: 0,
            };
            if GetMonitorInfo(h_monitor, &mut monitor_info).as_bool() {
                let width = rc.right - rc.left;
                let height = rc.bottom - rc.top;
                let x = monitor_info.rcWork.left + (monitor_info.rcWork.right - monitor_info.rcWork.left - width) / 2;
                let y = monitor_info.rcWork.top + (monitor_info.rcWork.bottom - monitor_info.rcWork.top - height) / 2;
                SetWindowPos(hwnd, None, x, y, 0, 0, SWP_NOSIZE | SWP_NOZORDER);
            }
        }
    }

    /// 隐藏窗口
    pub fn hide(&self) {
        if let Some(hwnd) = self.hwnd {
            unsafe {
                ShowWindow(hwnd, SW_HIDE);
            }
        }
    }

    /// 打开配置文件
    fn open_config_file(&self) {
        let config_path = self.config_service.config_path();
        if config_path.exists() {
            let _ = std::process::Command::new("notepad.exe")
                .arg(config_path)
                .spawn();
        }
    }

    /// 窗口过程
    unsafe extern "system" fn window_proc(
        hwnd: HWND,
        msg: u32,
        wparam: WPARAM,
        lparam: LPARAM,
    ) -> LRESULT {
        let user_data = GetWindowLongPtrW(hwnd, GWLP_USERDATA);

        if user_data != 0 {
            let window = &mut *(user_data as *mut MainWindow);

            match msg {
                WM_CREATE => {
                    return LRESULT(0);
                }
                WM_SIZE => {
                    return LRESULT(0);
                }
                WM_COMMAND => {
                    let id = (wparam.0 & 0xFFFF) as i32;
                    match id {
                        1 => { // 确定按钮
                            window.hide();
                            return LRESULT(0);
                        }
                        IDC_OPEN_CONFIG => { // 打开配置文件
                            window.open_config_file();
                            return LRESULT(0);
                        }
                        _ => {}
                    }
                }
                WM_CLOSE => {
                    window.hide();
                    return LRESULT(0);
                }
                WM_DESTROY => {
                    window.hwnd = None;
                    return LRESULT(0);
                }
                _ => {}
            }
        }

        DefWindowProcW(hwnd, msg, wparam, lparam)
    }
}
