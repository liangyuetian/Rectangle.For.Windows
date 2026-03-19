use crate::core::rect::{WorkArea, WindowRect};
use crate::core::action::WindowAction;
use crate::core::calculator::RectCalculator;
use crate::services::config::ConfigService;

/// 左半屏计算器
pub struct LeftHalfCalculator {
    config_service: Option<ConfigService>,
}

impl LeftHalfCalculator {
    pub fn new(config_service: Option<&ConfigService>) -> Self {
        Self { config_service: config_service.cloned() }
    }

    fn get_horizontal_ratio(&self) -> i32 {
        if let Some(ref service) = self.config_service {
            let config = service.load();
            if let Some(ratio) = config.horizontal_split_ratio {
                if ratio >= 1 && ratio <= 99 {
                    return ratio;
                }
            }
        }
        50
    }
}

impl RectCalculator for LeftHalfCalculator {
    fn calculate(&self, work_area: &WorkArea, _current_window: &WindowRect, _action: WindowAction, gap: i32) -> WindowRect {
        let ratio = self.get_horizontal_ratio();
        let total_width = work_area.width() - gap;
        let width = (total_width as f64 * ratio as f64 / 100.0) as i32;
        WindowRect::new(work_area.left, work_area.top, width, work_area.height())
    }
}

/// 右半屏计算器
pub struct RightHalfCalculator {
    config_service: Option<ConfigService>,
}

impl RightHalfCalculator {
    pub fn new(config_service: Option<&ConfigService>) -> Self {
        Self { config_service: config_service.cloned() }
    }

    fn get_horizontal_ratio(&self) -> i32 {
        if let Some(ref service) = self.config_service {
            let config = service.load();
            if let Some(ratio) = config.horizontal_split_ratio {
                if ratio >= 1 && ratio <= 99 {
                    return ratio;
                }
            }
        }
        50
    }
}

impl RectCalculator for RightHalfCalculator {
    fn calculate(&self, work_area: &WorkArea, _current_window: &WindowRect, _action: WindowAction, gap: i32) -> WindowRect {
        let ratio = self.get_horizontal_ratio();
        let total_width = work_area.width() - gap;
        let width = (total_width as f64 * (100 - ratio) as f64 / 100.0) as i32;
        let x = work_area.left + (total_width as f64 * ratio as f64 / 100.0) as i32 + gap;
        WindowRect::new(x, work_area.top, width, work_area.height())
    }
}

/// 上半屏计算器
pub struct TopHalfCalculator {
    config_service: Option<ConfigService>,
}

impl TopHalfCalculator {
    pub fn new(config_service: Option<&ConfigService>) -> Self {
        Self { config_service: config_service.cloned() }
    }

    fn get_vertical_ratio(&self) -> i32 {
        if let Some(ref service) = self.config_service {
            let config = service.load();
            if let Some(ratio) = config.vertical_split_ratio {
                if ratio >= 1 && ratio <= 99 {
                    return ratio;
                }
            }
        }
        50
    }
}

impl RectCalculator for TopHalfCalculator {
    fn calculate(&self, work_area: &WorkArea, _current_window: &WindowRect, _action: WindowAction, gap: i32) -> WindowRect {
        let ratio = self.get_vertical_ratio();
        let total_height = work_area.height() - gap;
        let height = (total_height as f64 * ratio as f64 / 100.0) as i32;
        WindowRect::new(work_area.left, work_area.top, work_area.width(), height)
    }
}

/// 下半屏计算器
pub struct BottomHalfCalculator {
    config_service: Option<ConfigService>,
}

impl BottomHalfCalculator {
    pub fn new(config_service: Option<&ConfigService>) -> Self {
        Self { config_service: config_service.cloned() }
    }

    fn get_vertical_ratio(&self) -> i32 {
        if let Some(ref service) = self.config_service {
            let config = service.load();
            if let Some(ratio) = config.vertical_split_ratio {
                if ratio >= 1 && ratio <= 99 {
                    return ratio;
                }
            }
        }
        50
    }
}

impl RectCalculator for BottomHalfCalculator {
    fn calculate(&self, work_area: &WorkArea, _current_window: &WindowRect, _action: WindowAction, gap: i32) -> WindowRect {
        let ratio = self.get_vertical_ratio();
        let total_height = work_area.height() - gap;
        let height = (total_height as f64 * (100 - ratio) as f64 / 100.0) as i32;
        let y = work_area.top + (total_height as f64 * ratio as f64 / 100.0) as i32 + gap;
        WindowRect::new(work_area.left, y, work_area.width(), height)
    }
}

/// 中间半屏计算器
pub struct CenterHalfCalculator;

impl CenterHalfCalculator {
    pub fn new() -> Self {
        Self
    }
}

impl RectCalculator for CenterHalfCalculator {
    fn calculate(&self, work_area: &WorkArea, _current_window: &WindowRect, _action: WindowAction, gap: i32) -> WindowRect {
        let width = (work_area.width() - gap) / 2;
        let height = work_area.height();
        let x = work_area.left + (work_area.width() - width) / 2;
        WindowRect::new(x, work_area.top, width, height)
    }
}
