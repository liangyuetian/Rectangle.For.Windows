use crate::core::action::WindowAction;
use crate::services::config::{ConfigService, ShortcutConfig};
use crate::services::logger::Logger;
use crate::services::window_manager::WindowManager;
use std::collections::HashMap;
use std::sync::{Arc, Mutex};
use windows::Win32::Foundation::{HWND, LPARAM, LRESULT, WPARAM};
use windows::Win32::UI::Input::KeyboardAndMouse::{
    RegisterHotKey, UnregisterHotKey, MOD_ALT, MOD_CONTROL, MOD_SHIFT, MOD_WIN,
};
use windows::Win32::UI::WindowsAndMessaging::{
    CreateWindowExW, DefWindowProcW, DispatchMessageW, GetMessageW, MSG, WM_HOTKEY,
    WNDCLASSW, CS_HREDRAW, CS_VREDRAW, CW_USEDEFAULT, HWND_MESSAGE, MSG_DONTROUTE,
    WM_DESTROY, WS_OVERLAPPEDWINDOW,
};

/// 热键管理器
pub struct HotkeyManager {
    config_service: ConfigService,
    window_manager: Arc<WindowManager>,
    hotkeys: Arc<Mutex<HashMap<i32, (WindowAction, u32, u32)>>>, // id -> (action, mod, vk)
    next_id: Arc<Mutex<i32>>,
    hwnd: Option<HWND>,
}

impl HotkeyManager {
    /// 创建新的热键管理器
    pub fn new(config_service: ConfigService, window_manager: Arc<WindowManager>) -> Self {
        Self {
            config_service,
            window_manager,
            hotkeys: Arc::new(Mutex::new(HashMap::new())),
            next_id: Arc::new(Mutex::new(1)),
            hwnd: None,
        }
    }

    /// 初始化热键
    pub fn initialize(&mut self) -> Result<(), Box<dyn std::error::Error>> {
        // 创建消息窗口
        let hwnd = self.create_message_window()?;
        self.hwnd = Some(hwnd);

        // 加载配置并注册热键
        let config = self.config_service.load();
        let shortcuts = &config.shortcuts;

        let action_map = Self::build_action_map();

        for (name, shortcut) in shortcuts {
            if !shortcut.enabled || shortcut.key_code == 0 {
                continue;
            }

            if let Some(&action) = action_map.get(name.as_str()) {
                if let Err(e) = self.register(action, shortcut) {
                    Logger::warning("HotkeyManager", &format!("注册热键 {} 失败: {}", name, e));
                } else {
                    Logger::info("HotkeyManager", &format!("注册热键 {}: {:?}", name, action));
                }
            }
        }

        Logger::info("HotkeyManager", "热键管理器初始化完成");
        Ok(())
    }

    /// 注册热键
    fn register(&self, action: WindowAction, shortcut: &ShortcutConfig) -> Result<(), Box<dyn std::error::Error>> {
        let hwnd = self.hwnd.ok_or("窗口未创建")?;

        let mut modifiers = 0u32;
        if (shortcut.modifier_flags & 0x0002) != 0 { modifiers |= MOD_CONTROL.0; }
        if (shortcut.modifier_flags & 0x0001) != 0 { modifiers |= MOD_ALT.0; }
        if (shortcut.modifier_flags & 0x0004) != 0 { modifiers |= MOD_SHIFT.0; }
        if (shortcut.modifier_flags & 0x0008) != 0 { modifiers |= MOD_WIN.0; }

        let mut id = self.next_id.lock().unwrap();
        let hotkey_id = *id;
        *id += 1;

        unsafe {
            RegisterHotKey(
                hwnd,
                hotkey_id,
                windows::Win32::UI::Input::KeyboardAndMouse::HOT_KEY_MODIFIERS(modifiers),
                shortcut.key_code as u32,
            )?;
        }

        self.hotkeys.lock().unwrap().insert(hotkey_id, (action, shortcut.modifier_flags, shortcut.key_code as u32));

        Ok(())
    }

    /// 注销热键
    fn unregister(&self, id: i32) -> Result<(), Box<dyn std::error::Error>> {
        let hwnd = self.hwnd.ok_or("窗口未创建")?;

        unsafe {
            UnregisterHotKey(hwnd, id)?;
        }

        self.hotkeys.lock().unwrap().remove(&id);

        Ok(())
    }

    /// 创建消息窗口
    fn create_message_window(&self) -> Result<HWND, Box<dyn std::error::Error>> {
        unsafe {
            let class_name: Vec<u16> = "RectangleHotkeyWindow\0".encode_utf16().collect();

            let wnd_class = WNDCLASSW {
                lpfnWndProc: Some(window_proc),
                hInstance: windows::Win32::System::LibraryLoader::GetModuleHandleW(None)?,
                lpszClassName: windows::core::PCWSTR(class_name.as_ptr()),
                style: CS_HREDRAW | CS_VREDRAW,
                ..Default::default()
            };

            windows::Win32::UI::WindowsAndMessaging::RegisterClassW(&wnd_class);

            let hwnd = CreateWindowExW(
                windows::Win32::UI::WindowsAndMessaging::WINDOW_EX_STYLE(0),
                windows::core::PCWSTR(class_name.as_ptr()),
                windows::core::PCWSTR(class_name.as_ptr()),
                WS_OVERLAPPEDWINDOW,
                CW_USEDEFAULT,
                CW_USEDEFAULT,
                CW_USEDEFAULT,
                CW_USEDEFAULT,
                HWND_MESSAGE,
                None,
                wnd_class.hInstance,
                Some(self as *const _ as *const std::ffi::c_void),
            )?;

            Ok(hwnd)
        }
    }

    /// 构建动作映射
    fn build_action_map() -> HashMap<&'static str, WindowAction> {
        let mut map = HashMap::new();

        map.insert("LeftHalf", WindowAction::LeftHalf);
        map.insert("RightHalf", WindowAction::RightHalf);
        map.insert("TopHalf", WindowAction::TopHalf);
        map.insert("BottomHalf", WindowAction::BottomHalf);
        map.insert("CenterHalf", WindowAction::CenterHalf);

        map.insert("TopLeft", WindowAction::TopLeft);
        map.insert("TopRight", WindowAction::TopRight);
        map.insert("BottomLeft", WindowAction::BottomLeft);
        map.insert("BottomRight", WindowAction::BottomRight);

        map.insert("FirstThird", WindowAction::FirstThird);
        map.insert("CenterThird", WindowAction::CenterThird);
        map.insert("LastThird", WindowAction::LastThird);
        map.insert("FirstTwoThirds", WindowAction::FirstTwoThirds);
        map.insert("CenterTwoThirds", WindowAction::CenterTwoThirds);
        map.insert("LastTwoThirds", WindowAction::LastTwoThirds);

        map.insert("Maximize", WindowAction::Maximize);
        map.insert("AlmostMaximize", WindowAction::AlmostMaximize);
        map.insert("MaximizeHeight", WindowAction::MaximizeHeight);
        map.insert("Center", WindowAction::Center);
        map.insert("Larger", WindowAction::Larger);
        map.insert("Smaller", WindowAction::Smaller);
        map.insert("Restore", WindowAction::Restore);

        map.insert("MoveLeft", WindowAction::MoveLeft);
        map.insert("MoveRight", WindowAction::MoveRight);
        map.insert("MoveUp", WindowAction::MoveUp);
        map.insert("MoveDown", WindowAction::MoveDown);

        map.insert("FirstFourth", WindowAction::FirstFourth);
        map.insert("SecondFourth", WindowAction::SecondFourth);
        map.insert("ThirdFourth", WindowAction::ThirdFourth);
        map.insert("LastFourth", WindowAction::LastFourth);
        map.insert("FirstThreeFourths", WindowAction::FirstThreeFourths);
        map.insert("CenterThreeFourths", WindowAction::CenterThreeFourths);
        map.insert("LastThreeFourths", WindowAction::LastThreeFourths);

        map.insert("TopLeftSixth", WindowAction::TopLeftSixth);
        map.insert("TopCenterSixth", WindowAction::TopCenterSixth);
        map.insert("TopRightSixth", WindowAction::TopRightSixth);
        map.insert("BottomLeftSixth", WindowAction::BottomLeftSixth);
        map.insert("BottomCenterSixth", WindowAction::BottomCenterSixth);
        map.insert("BottomRightSixth", WindowAction::BottomRightSixth);

        map.insert("AlmostMaximize", WindowAction::AlmostMaximize);
        map.insert("MaximizeHeight", WindowAction::MaximizeHeight);
        map.insert("Larger", WindowAction::Larger);
        map.insert("Smaller", WindowAction::Smaller);

        map.insert("NextDisplay", WindowAction::NextDisplay);
        map.insert("PreviousDisplay", WindowAction::PreviousDisplay);

        map.insert("Undo", WindowAction::Undo);
        map.insert("Redo", WindowAction::Redo);

        map
    }

    /// 运行消息循环
    pub fn run_message_loop(&self) {
        unsafe {
            let mut msg = MSG::default();
            while GetMessageW(&mut msg, None, 0, 0).as_bool() {
                DispatchMessageW(&msg);
            }
        }
    }

    /// 处理热键消息
    fn handle_hotkey(&self, id: i32) {
        if let Some((action, _, _)) = self.hotkeys.lock().unwrap().get(&id) {
            Logger::info("HotkeyManager", &format!("触发热键: {:?}", action));
            self.window_manager.execute(*action, false);
        }
    }
}

/// 窗口过程
unsafe extern "system" fn window_proc(
    hwnd: HWND,
    msg: u32,
    wparam: WPARAM,
    lparam: LPARAM,
) -> LRESULT {
    match msg {
        WM_HOTKEY => {
            // 获取热键管理器指针
            let user_data = windows::Win32::UI::WindowsAndMessaging::GetWindowLongPtrW(
                hwnd,
                windows::Win32::UI::WindowsAndMessaging::GWLP_USERDATA,
            );

            if user_data != 0 {
                let manager = &*(user_data as *const HotkeyManager);
                manager.handle_hotkey(wparam.0 as i32);
            }

            LRESULT(0)
        }
        WM_DESTROY => {
            windows::Win32::UI::WindowsAndMessaging::PostQuitMessage(0);
            LRESULT(0)
        }
        _ => DefWindowProcW(hwnd, msg, wparam, lparam),
    }
}
