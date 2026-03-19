use crate::services::config::ConfigService;
use crate::services::window_manager::WindowManager;
use std::sync::Arc;
use windows::core::Result;

/// 吸附区域设置页面
pub struct SnapAreasPage;

impl SnapAreasPage {
    /// 创建新的吸附区域页面
    pub fn new(
        _config_service: ConfigService,
        _window_manager: Arc<WindowManager>,
    ) -> Result<Self> {
        Ok(Self)
    }
}
