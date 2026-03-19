use crate::services::config::ConfigService;
use crate::services::hotkey::HotkeyManager;
use crate::services::logger::Logger;
use crate::services::window_manager::WindowManager;
use crate::ui::main_window::MainWindow;
use crate::ui::tray_icon::TrayIcon;
use std::cell::RefCell;
use std::rc::Rc;
use std::sync::Arc;
use windows::core::Result;
use windows::Win32::System::Com::{CoInitializeEx, COINIT_APARTMENTTHREADED};

/// WinUI 3 应用程序
pub struct WinUIApp {
    config_service: ConfigService,
    window_manager: Arc<WindowManager>,
    hotkey_manager: Option<HotkeyManager>,
    tray_icon: Option<TrayIcon>,
    main_window: Rc<RefCell<Option<MainWindow>>>,
}

impl WinUIApp {
    /// 创建新的 WinUI 应用
    pub fn new() -> Result<Self> {
        // 初始化 WinRT
        unsafe {
            CoInitializeEx(None, COINIT_APARTMENTTHREADED)?;
        }

        // 初始化日志
        Logger::init();
        Logger::info("WinUIApp", "初始化 WinUI 3 应用");

        // 创建服务
        let config_service = ConfigService::new();
        let window_manager = Arc::new(WindowManager::new(config_service.clone()));
        let hotkey_manager = HotkeyManager::new(config_service.clone(), window_manager.clone());

        Ok(Self {
            config_service,
            window_manager,
            hotkey_manager: Some(hotkey_manager),
            tray_icon: None,
            main_window: Rc::new(RefCell::new(None)),
        })
    }

    /// 运行应用
    pub fn run(&mut self) -> Result<()> {
        Logger::info("WinUIApp", "启动应用");

        // 初始化热键
        if let Some(mut hotkey_manager) = self.hotkey_manager.take() {
            if let Err(e) = hotkey_manager.initialize() {
                Logger::warning("WinUIApp", &format!("热键初始化失败: {:?}", e));
            }
        }

        // 创建主窗口（设置窗口）
        let main_window = MainWindow::new(
            self.config_service.clone(),
            self.window_manager.clone(),
        )?;
        *self.main_window.borrow_mut() = Some(main_window);

        // 创建托盘图标
        let mut tray_icon = TrayIcon::new(
            self.config_service.clone(),
            self.window_manager.clone(),
        )?;

        // 设置显示设置回调
        let main_window_ref = self.main_window.clone();
        tray_icon.set_show_settings_callback(move || {
            if let Ok(mut window) = main_window_ref.try_borrow_mut() {
                if let Some(ref mut w) = *window {
                    w.show();
                }
            }
        });

        tray_icon.initialize()?;
        self.tray_icon = Some(tray_icon);

        // 运行消息循环
        self.run_message_loop()?;

        Logger::info("WinUIApp", "应用退出");
        Ok(())
    }

    /// 显示设置窗口
    pub fn show_settings(&self) {
        if let Ok(mut window) = self.main_window.try_borrow_mut() {
            if let Some(ref mut w) = *window {
                w.show();
            }
        }
    }

    /// 运行消息循环
    fn run_message_loop(&self) -> Result<()> {
        unsafe {
            let mut msg = windows::Win32::UI::WindowsAndMessaging::MSG::default();
            while windows::Win32::UI::WindowsAndMessaging::GetMessageW(&mut msg, None, 0, 0)
                .as_bool()
            {
                windows::Win32::UI::WindowsAndMessaging::TranslateMessage(&msg);
                windows::Win32::UI::WindowsAndMessaging::DispatchMessageW(&msg);
            }
        }
        Ok(())
    }
}

impl Drop for WinUIApp {
    fn drop(&mut self) {
        Logger::info("WinUIApp", "清理资源");
    }
}
