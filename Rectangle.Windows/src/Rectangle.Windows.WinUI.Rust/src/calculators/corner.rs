use crate::core::rect::{WorkArea, WindowRect};
use crate::core::action::WindowAction;
use crate::core::calculator::RectCalculator;

/// 左上计算器
pub struct TopLeftCalculator;

impl TopLeftCalculator {
    pub fn new() -> Self {
        Self
    }
}

impl RectCalculator for TopLeftCalculator {
    fn calculate(&self, work_area: &WorkArea, _current_window: &WindowRect, _action: WindowAction, gap: i32) -> WindowRect {
        let width = (work_area.width() - gap) / 2;
        let height = (work_area.height() - gap) / 2;
        WindowRect::new(work_area.left, work_area.top, width, height)
    }
}

/// 右上计算器
pub struct TopRightCalculator;

impl TopRightCalculator {
    pub fn new() -> Self {
        Self
    }
}

impl RectCalculator for TopRightCalculator {
    fn calculate(&self, work_area: &WorkArea, _current_window: &WindowRect, _action: WindowAction, gap: i32) -> WindowRect {
        let width = (work_area.width() - gap) / 2;
        let height = (work_area.height() - gap) / 2;
        let x = work_area.left + width + gap;
        WindowRect::new(x, work_area.top, width, height)
    }
}

/// 左下计算器
pub struct BottomLeftCalculator;

impl BottomLeftCalculator {
    pub fn new() -> Self {
        Self
    }
}

impl RectCalculator for BottomLeftCalculator {
    fn calculate(&self, work_area: &WorkArea, _current_window: &WindowRect, _action: WindowAction, gap: i32) -> WindowRect {
        let width = (work_area.width() - gap) / 2;
        let height = (work_area.height() - gap) / 2;
        let y = work_area.top + height + gap;
        WindowRect::new(work_area.left, y, width, height)
    }
}

/// 右下计算器
pub struct BottomRightCalculator;

impl BottomRightCalculator {
    pub fn new() -> Self {
        Self
    }
}

impl RectCalculator for BottomRightCalculator {
    fn calculate(&self, work_area: &WorkArea, _current_window: &WindowRect, _action: WindowAction, gap: i32) -> WindowRect {
        let width = (work_area.width() - gap) / 2;
        let height = (work_area.height() - gap) / 2;
        let x = work_area.left + width + gap;
        let y = work_area.top + height + gap;
        WindowRect::new(x, y, width, height)
    }
}
