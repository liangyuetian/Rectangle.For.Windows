use crate::core::rect::{WorkArea, WindowRect};
use crate::core::action::WindowAction;
use crate::core::calculator::RectCalculator;
use crate::services::config::ConfigService;

/// 最大化计算器
pub struct MaximizeCalculator;

impl MaximizeCalculator {
    pub fn new() -> Self {
        Self
    }
}

impl RectCalculator for MaximizeCalculator {
    fn calculate(&self, work_area: &WorkArea, _current_window: &WindowRect, _action: WindowAction, _gap: i32) -> WindowRect {
        WindowRect::new(work_area.left, work_area.top, work_area.width(), work_area.height())
    }
}

/// 几乎最大化计算器
pub struct AlmostMaximizeCalculator {
    config_service: Option<ConfigService>,
}

impl AlmostMaximizeCalculator {
    pub fn new(config_service: Option<&ConfigService>) -> Self {
        Self { config_service: config_service.cloned() }
    }
}

impl RectCalculator for AlmostMaximizeCalculator {
    fn calculate(&self, work_area: &WorkArea, _current_window: &WindowRect, _action: WindowAction, _gap: i32) -> WindowRect {
        let (width_ratio, height_ratio) = if let Some(ref service) = self.config_service {
            let config = service.load();
            (config.almost_maximize_width, config.almost_maximize_height)
        } else {
            (0.9, 0.9)
        };

        let width = (work_area.width() as f32 * width_ratio) as i32;
        let height = (work_area.height() as f32 * height_ratio) as i32;
        let x = work_area.left + (work_area.width() - width) / 2;
        let y = work_area.top + (work_area.height() - height) / 2;
        WindowRect::new(x, y, width, height)
    }
}

/// 最大化高度计算器
pub struct MaximizeHeightCalculator;

impl MaximizeHeightCalculator {
    pub fn new() -> Self {
        Self
    }
}

impl RectCalculator for MaximizeHeightCalculator {
    fn calculate(&self, work_area: &WorkArea, current_window: &WindowRect, _action: WindowAction, _gap: i32) -> WindowRect {
        WindowRect::new(current_window.x, work_area.top, current_window.width, work_area.height())
    }
}

/// 居中计算器
pub struct CenterCalculator;

impl CenterCalculator {
    pub fn new() -> Self {
        Self
    }
}

impl RectCalculator for CenterCalculator {
    fn calculate(&self, work_area: &WorkArea, current_window: &WindowRect, _action: WindowAction, _gap: i32) -> WindowRect {
        let x = work_area.left + (work_area.width() - current_window.width) / 2;
        let y = work_area.top + (work_area.height() - current_window.height) / 2;
        WindowRect::new(x, y, current_window.width, current_window.height)
    }
}

/// 居中显著计算器
pub struct CenterProminentlyCalculator {
    config_service: Option<ConfigService>,
}

impl CenterProminentlyCalculator {
    pub fn new(config_service: Option<&ConfigService>) -> Self {
        Self { config_service: config_service.cloned() }
    }
}

impl RectCalculator for CenterProminentlyCalculator {
    fn calculate(&self, work_area: &WorkArea, _current_window: &WindowRect, _action: WindowAction, _gap: i32) -> WindowRect {
        let ratio = 0.7; // 默认占屏幕 70%
        let width = (work_area.width() as f32 * ratio) as i32;
        let height = (work_area.height() as f32 * ratio) as i32;
        let x = work_area.left + (work_area.width() - width) / 2;
        let y = work_area.top + (work_area.height() - height) / 2;
        WindowRect::new(x, y, width, height)
    }
}

/// 放大计算器
pub struct LargerCalculator {
    config_service: Option<ConfigService>,
}

impl LargerCalculator {
    pub fn new(config_service: Option<&ConfigService>) -> Self {
        Self { config_service: config_service.cloned() }
    }

    fn get_size_offset(&self) -> i32 {
        if let Some(ref service) = self.config_service {
            let config = service.load();
            config.size_offset as i32
        } else {
            30
        }
    }
}

impl RectCalculator for LargerCalculator {
    fn calculate(&self, work_area: &WorkArea, current_window: &WindowRect, _action: WindowAction, _gap: i32) -> WindowRect {
        let offset = self.get_size_offset();
        let new_width = (current_window.width + offset * 2).min(work_area.width());
        let new_height = (current_window.height + offset * 2).min(work_area.height());
        let x = (current_window.x - offset).max(work_area.left);
        let y = (current_window.y - offset).max(work_area.top);
        WindowRect::new(x, y, new_width, new_height)
    }
}

/// 缩小计算器
pub struct SmallerCalculator {
    config_service: Option<ConfigService>,
}

impl SmallerCalculator {
    pub fn new(config_service: Option<&ConfigService>) -> Self {
        Self { config_service: config_service.cloned() }
    }

    fn get_size_offset(&self) -> i32 {
        if let Some(ref service) = self.config_service {
            let config = service.load();
            config.size_offset as i32
        } else {
            30
        }
    }

    fn get_minimum_size(&self) -> (i32, i32) {
        if let Some(ref service) = self.config_service {
            let config = service.load();
            (config.minimum_window_width as i32, config.minimum_window_height as i32)
        } else {
            (100, 100)
        }
    }
}

impl RectCalculator for SmallerCalculator {
    fn calculate(&self, _work_area: &WorkArea, current_window: &WindowRect, _action: WindowAction, _gap: i32) -> WindowRect {
        let offset = self.get_size_offset();
        let (min_width, min_height) = self.get_minimum_size();
        let new_width = (current_window.width - offset * 2).max(min_width);
        let new_height = (current_window.height - offset * 2).max(min_height);
        let x = current_window.x + (current_window.width - new_width) / 2;
        let y = current_window.y + (current_window.height - new_height) / 2;
        WindowRect::new(x, y, new_width, new_height)
    }
}

/// 增加宽度计算器
pub struct LargerWidthCalculator {
    config_service: Option<ConfigService>,
}

impl LargerWidthCalculator {
    pub fn new(config_service: Option<&ConfigService>) -> Self {
        Self { config_service: config_service.cloned() }
    }

    fn get_size_offset(&self) -> i32 {
        if let Some(ref service) = self.config_service {
            let config = service.load();
            config.size_offset as i32
        } else {
            30
        }
    }
}

impl RectCalculator for LargerWidthCalculator {
    fn calculate(&self, work_area: &WorkArea, current_window: &WindowRect, _action: WindowAction, _gap: i32) -> WindowRect {
        let offset = self.get_size_offset();
        let new_width = (current_window.width + offset).min(work_area.width());
        WindowRect::new(current_window.x, current_window.y, new_width, current_window.height)
    }
}

/// 减小宽度计算器
pub struct SmallerWidthCalculator {
    config_service: Option<ConfigService>,
}

impl SmallerWidthCalculator {
    pub fn new(config_service: Option<&ConfigService>) -> Self {
        Self { config_service: config_service.cloned() }
    }

    fn get_size_offset(&self) -> i32 {
        if let Some(ref service) = self.config_service {
            let config = service.load();
            config.size_offset as i32
        } else {
            30
        }
    }

    fn get_minimum_width(&self) -> i32 {
        if let Some(ref service) = self.config_service {
            let config = service.load();
            config.minimum_window_width as i32
        } else {
            100
        }
    }
}

impl RectCalculator for SmallerWidthCalculator {
    fn calculate(&self, _work_area: &WorkArea, current_window: &WindowRect, _action: WindowAction, _gap: i32) -> WindowRect {
        let offset = self.get_size_offset();
        let min_width = self.get_minimum_width();
        let new_width = (current_window.width - offset).max(min_width);
        WindowRect::new(current_window.x, current_window.y, new_width, current_window.height)
    }
}

/// 增加高度计算器
pub struct LargerHeightCalculator {
    config_service: Option<ConfigService>,
}

impl LargerHeightCalculator {
    pub fn new(config_service: Option<&ConfigService>) -> Self {
        Self { config_service: config_service.cloned() }
    }

    fn get_size_offset(&self) -> i32 {
        if let Some(ref service) = self.config_service {
            let config = service.load();
            config.size_offset as i32
        } else {
            30
        }
    }
}

impl RectCalculator for LargerHeightCalculator {
    fn calculate(&self, work_area: &WorkArea, current_window: &WindowRect, _action: WindowAction, _gap: i32) -> WindowRect {
        let offset = self.get_size_offset();
        let new_height = (current_window.height + offset).min(work_area.height());
        WindowRect::new(current_window.x, current_window.y, current_window.width, new_height)
    }
}

/// 减小高度计算器
pub struct SmallerHeightCalculator {
    config_service: Option<ConfigService>,
}

impl SmallerHeightCalculator {
    pub fn new(config_service: Option<&ConfigService>) -> Self {
        Self { config_service: config_service.cloned() }
    }

    fn get_size_offset(&self) -> i32 {
        if let Some(ref service) = self.config_service {
            let config = service.load();
            config.size_offset as i32
        } else {
            30
        }
    }

    fn get_minimum_height(&self) -> i32 {
        if let Some(ref service) = self.config_service {
            let config = service.load();
            config.minimum_window_height as i32
        } else {
            100
        }
    }
}

impl RectCalculator for SmallerHeightCalculator {
    fn calculate(&self, _work_area: &WorkArea, current_window: &WindowRect, _action: WindowAction, _gap: i32) -> WindowRect {
        let offset = self.get_size_offset();
        let min_height = self.get_minimum_height();
        let new_height = (current_window.height - offset).max(min_height);
        WindowRect::new(current_window.x, current_window.y, current_window.width, new_height)
    }
}

/// 双倍高度向上计算器
pub struct DoubleHeightUpCalculator;

impl DoubleHeightUpCalculator {
    pub fn new() -> Self {
        Self
    }
}

impl RectCalculator for DoubleHeightUpCalculator {
    fn calculate(&self, work_area: &WorkArea, current_window: &WindowRect, _action: WindowAction, _gap: i32) -> WindowRect {
        let new_height = (current_window.height * 2).min(work_area.height());
        let y = current_window.y - (new_height - current_window.height);
        let y = y.max(work_area.top);
        WindowRect::new(current_window.x, y, current_window.width, new_height)
    }
}

/// 双倍高度向下计算器
pub struct DoubleHeightDownCalculator;

impl DoubleHeightDownCalculator {
    pub fn new() -> Self {
        Self
    }
}

impl RectCalculator for DoubleHeightDownCalculator {
    fn calculate(&self, work_area: &WorkArea, current_window: &WindowRect, _action: WindowAction, _gap: i32) -> WindowRect {
        let new_height = (current_window.height * 2).min(work_area.height() - (current_window.y - work_area.top));
        WindowRect::new(current_window.x, current_window.y, current_window.width, new_height)
    }
}

/// 双倍宽度向左计算器
pub struct DoubleWidthLeftCalculator;

impl DoubleWidthLeftCalculator {
    pub fn new() -> Self {
        Self
    }
}

impl RectCalculator for DoubleWidthLeftCalculator {
    fn calculate(&self, work_area: &WorkArea, current_window: &WindowRect, _action: WindowAction, _gap: i32) -> WindowRect {
        let new_width = (current_window.width * 2).min(work_area.width());
        let x = current_window.x - (new_width - current_window.width);
        let x = x.max(work_area.left);
        WindowRect::new(x, current_window.y, new_width, current_window.height)
    }
}

/// 双倍宽度向右计算器
pub struct DoubleWidthRightCalculator;

impl DoubleWidthRightCalculator {
    pub fn new() -> Self {
        Self
    }
}

impl RectCalculator for DoubleWidthRightCalculator {
    fn calculate(&self, work_area: &WorkArea, current_window: &WindowRect, _action: WindowAction, _gap: i32) -> WindowRect {
        let new_width = (current_window.width * 2).min(work_area.width() - (current_window.x - work_area.left));
        WindowRect::new(current_window.x, current_window.y, new_width, current_window.height)
    }
}

/// 减半高度向上计算器
pub struct HalveHeightUpCalculator;

impl HalveHeightUpCalculator {
    pub fn new() -> Self {
        Self
    }
}

impl RectCalculator for HalveHeightUpCalculator {
    fn calculate(&self, work_area: &WorkArea, current_window: &WindowRect, _action: WindowAction, _gap: i32) -> WindowRect {
        let new_height = current_window.height / 2;
        let y = current_window.y + current_window.height - new_height;
        WindowRect::new(current_window.x, y, current_window.width, new_height)
    }
}

/// 减半高度向下计算器
pub struct HalveHeightDownCalculator;

impl HalveHeightDownCalculator {
    pub fn new() -> Self {
        Self
    }
}

impl RectCalculator for HalveHeightDownCalculator {
    fn calculate(&self, work_area: &WorkArea, current_window: &WindowRect, _action: WindowAction, _gap: i32) -> WindowRect {
        let new_height = current_window.height / 2;
        WindowRect::new(current_window.x, current_window.y, current_window.width, new_height)
    }
}

/// 减半宽度向左计算器
pub struct HalveWidthLeftCalculator;

impl HalveWidthLeftCalculator {
    pub fn new() -> Self {
        Self
    }
}

impl RectCalculator for HalveWidthLeftCalculator {
    fn calculate(&self, work_area: &WorkArea, current_window: &WindowRect, _action: WindowAction, _gap: i32) -> WindowRect {
        let new_width = current_window.width / 2;
        let x = current_window.x + current_window.width - new_width;
        WindowRect::new(x, current_window.y, new_width, current_window.height)
    }
}

/// 减半宽度向右计算器
pub struct HalveWidthRightCalculator;

impl HalveWidthRightCalculator {
    pub fn new() -> Self {
        Self
    }
}

impl RectCalculator for HalveWidthRightCalculator {
    fn calculate(&self, work_area: &WorkArea, current_window: &WindowRect, _action: WindowAction, _gap: i32) -> WindowRect {
        let new_width = current_window.width / 2;
        WindowRect::new(current_window.x, current_window.y, new_width, current_window.height)
    }
}

/// 指定尺寸计算器
pub struct SpecifiedCalculator {
    config_service: Option<ConfigService>,
}

impl SpecifiedCalculator {
    pub fn new(config_service: Option<&ConfigService>) -> Self {
        Self { config_service: config_service.cloned() }
    }
}

impl RectCalculator for SpecifiedCalculator {
    fn calculate(&self, work_area: &WorkArea, current_window: &WindowRect, _action: WindowAction, _gap: i32) -> WindowRect {
        let (specified_width, specified_height) = if let Some(ref service) = self.config_service {
            let config = service.load();
            (config.specified_width, config.specified_height)
        } else {
            (1680, 1050)
        };

        let width = specified_width.min(work_area.width());
        let height = specified_height.min(work_area.height());
        let x = work_area.left + (work_area.width() - width) / 2;
        let y = work_area.top + (work_area.height() - height) / 2;
        WindowRect::new(x, y, width, height)
    }
}
