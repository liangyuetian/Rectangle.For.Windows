use crate::services::config::ConfigService;
use crate::services::window_manager::WindowManager;
use std::sync::Arc;
use windows::core::Result;

/// 通用设置页面
pub struct GeneralSettingsPage;

impl GeneralSettingsPage {
    /// 创建新的通用设置页面
    pub fn new(
        _config_service: ConfigService,
        _window_manager: Arc<WindowManager>,
    ) -> Result<Self> {
        Ok(Self)
    }
}
