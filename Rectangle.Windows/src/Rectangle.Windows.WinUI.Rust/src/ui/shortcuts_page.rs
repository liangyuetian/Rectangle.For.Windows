use crate::services::config::ConfigService;
use crate::services::window_manager::WindowManager;
use std::sync::Arc;
use windows::core::Result;

/// 快捷键设置页面
pub struct ShortcutsPage;

impl ShortcutsPage {
    /// 创建新的快捷键页面
    pub fn new(
        _config_service: ConfigService,
        _window_manager: Arc<WindowManager>,
    ) -> Result<Self> {
        Ok(Self)
    }
}
