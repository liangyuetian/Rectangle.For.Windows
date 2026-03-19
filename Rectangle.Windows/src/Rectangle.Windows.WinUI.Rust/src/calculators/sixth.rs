use crate::core::rect::{WorkArea, WindowRect};
use crate::core::action::WindowAction;
use crate::core::calculator::RectCalculator;

/// 左上六分之一计算器
pub struct TopLeftSixthCalculator;

impl TopLeftSixthCalculator {
    pub fn new() -> Self {
        Self
    }
}

impl RectCalculator for TopLeftSixthCalculator {
    fn calculate(&self, work_area: &WorkArea, _current_window: &WindowRect, _action: WindowAction, gap: i32) -> WindowRect {
        let width = (work_area.width() - gap * 2) / 3;
        let height = (work_area.height() - gap) / 2;
        WindowRect::new(work_area.left, work_area.top, width, height)
    }
}

/// 上中六分之一计算器
pub struct TopCenterSixthCalculator;

impl TopCenterSixthCalculator {
    pub fn new() -> Self {
        Self
    }
}

impl RectCalculator for TopCenterSixthCalculator {
    fn calculate(&self, work_area: &WorkArea, _current_window: &WindowRect, _action: WindowAction, gap: i32) -> WindowRect {
        let width = (work_area.width() - gap * 2) / 3;
        let height = (work_area.height() - gap) / 2;
        let x = work_area.left + width + gap;
        WindowRect::new(x, work_area.top, width, height)
    }
}

/// 右上六分之一计算器
pub struct TopRightSixthCalculator;

impl TopRightSixthCalculator {
    pub fn new() -> Self {
        Self
    }
}

impl RectCalculator for TopRightSixthCalculator {
    fn calculate(&self, work_area: &WorkArea, _current_window: &WindowRect, _action: WindowAction, gap: i32) -> WindowRect {
        let width = (work_area.width() - gap * 2) / 3;
        let height = (work_area.height() - gap) / 2;
        let x = work_area.left + width * 2 + gap * 2;
        WindowRect::new(x, work_area.top, width, height)
    }
}

/// 左下六分之一计算器
pub struct BottomLeftSixthCalculator;

impl BottomLeftSixthCalculator {
    pub fn new() -> Self {
        Self
    }
}

impl RectCalculator for BottomLeftSixthCalculator {
    fn calculate(&self, work_area: &WorkArea, _current_window: &WindowRect, _action: WindowAction, gap: i32) -> WindowRect {
        let width = (work_area.width() - gap * 2) / 3;
        let height = (work_area.height() - gap) / 2;
        let y = work_area.top + height + gap;
        WindowRect::new(work_area.left, y, width, height)
    }
}

/// 下中六分之一计算器
pub struct BottomCenterSixthCalculator;

impl BottomCenterSixthCalculator {
    pub fn new() -> Self {
        Self
    }
}

impl RectCalculator for BottomCenterSixthCalculator {
    fn calculate(&self, work_area: &WorkArea, _current_window: &WindowRect, _action: WindowAction, gap: i32) -> WindowRect {
        let width = (work_area.width() - gap * 2) / 3;
        let height = (work_area.height() - gap) / 2;
        let x = work_area.left + width + gap;
        let y = work_area.top + height + gap;
        WindowRect::new(x, y, width, height)
    }
}

/// 右下六分之一计算器
pub struct BottomRightSixthCalculator;

impl BottomRightSixthCalculator {
    pub fn new() -> Self {
        Self
    }
}

impl RectCalculator for BottomRightSixthCalculator {
    fn calculate(&self, work_area: &WorkArea, _current_window: &WindowRect, _action: WindowAction, gap: i32) -> WindowRect {
        let width = (work_area.width() - gap * 2) / 3;
        let height = (work_area.height() - gap) / 2;
        let x = work_area.left + width * 2 + gap * 2;
        let y = work_area.top + height + gap;
        WindowRect::new(x, y, width, height)
    }
}
