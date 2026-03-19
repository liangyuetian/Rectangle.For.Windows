use crate::core::rect::{WorkArea, WindowRect};
use crate::core::action::WindowAction;
use crate::core::calculator::RectCalculator;

// ============================================
// 角落三分之一计算器 (4个)
// ============================================

pub struct TopLeftThirdCalculator;

impl TopLeftThirdCalculator {
    pub fn new() -> Self {
        Self
    }
}

impl RectCalculator for TopLeftThirdCalculator {
    fn calculate(&self, work_area: &WorkArea, _current_window: &WindowRect, _action: WindowAction, gap: i32) -> WindowRect {
        let width = (work_area.width() - gap * 2) / 3;
        let height = (work_area.height() - gap * 2) / 3;
        WindowRect::new(work_area.left + gap, work_area.top + gap, width, height)
    }
}

pub struct TopRightThirdCalculator;

impl TopRightThirdCalculator {
    pub fn new() -> Self {
        Self
    }
}

impl RectCalculator for TopRightThirdCalculator {
    fn calculate(&self, work_area: &WorkArea, _current_window: &WindowRect, _action: WindowAction, gap: i32) -> WindowRect {
        let width = (work_area.width() - gap * 2) / 3;
        let height = (work_area.height() - gap * 2) / 3;
        let x = work_area.left + width * 2 + gap;
        WindowRect::new(x, work_area.top + gap, width, height)
    }
}

pub struct BottomLeftThirdCalculator;

impl BottomLeftThirdCalculator {
    pub fn new() -> Self {
        Self
    }
}

impl RectCalculator for BottomLeftThirdCalculator {
    fn calculate(&self, work_area: &WorkArea, _current_window: &WindowRect, _action: WindowAction, gap: i32) -> WindowRect {
        let width = (work_area.width() - gap * 2) / 3;
        let height = (work_area.height() - gap * 2) / 3;
        let y = work_area.top + height * 2 + gap;
        WindowRect::new(work_area.left + gap, y, width, height)
    }
}

pub struct BottomRightThirdCalculator;

impl BottomRightThirdCalculator {
    pub fn new() -> Self {
        Self
    }
}

impl RectCalculator for BottomRightThirdCalculator {
    fn calculate(&self, work_area: &WorkArea, _current_window: &WindowRect, _action: WindowAction, gap: i32) -> WindowRect {
        let width = (work_area.width() - gap * 2) / 3;
        let height = (work_area.height() - gap * 2) / 3;
        let x = work_area.left + width * 2 + gap;
        let y = work_area.top + height * 2 + gap;
        WindowRect::new(x, y, width, height)
    }
}

// ============================================
// 垂直三分之一计算器 (5个)
// ============================================

pub struct TopVerticalThirdCalculator;

impl TopVerticalThirdCalculator {
    pub fn new() -> Self {
        Self
    }
}

impl RectCalculator for TopVerticalThirdCalculator {
    fn calculate(&self, work_area: &WorkArea, _current_window: &WindowRect, _action: WindowAction, gap: i32) -> WindowRect {
        let height = (work_area.height() - gap * 2) / 3;
        WindowRect::new(work_area.left + gap, work_area.top + gap, work_area.width() - gap * 2, height)
    }
}

pub struct MiddleVerticalThirdCalculator;

impl MiddleVerticalThirdCalculator {
    pub fn new() -> Self {
        Self
    }
}

impl RectCalculator for MiddleVerticalThirdCalculator {
    fn calculate(&self, work_area: &WorkArea, _current_window: &WindowRect, _action: WindowAction, gap: i32) -> WindowRect {
        let height = (work_area.height() - gap * 2) / 3;
        let y = work_area.top + height + gap;
        WindowRect::new(work_area.left + gap, y, work_area.width() - gap * 2, height)
    }
}

pub struct BottomVerticalThirdCalculator;

impl BottomVerticalThirdCalculator {
    pub fn new() -> Self {
        Self
    }
}

impl RectCalculator for BottomVerticalThirdCalculator {
    fn calculate(&self, work_area: &WorkArea, _current_window: &WindowRect, _action: WindowAction, gap: i32) -> WindowRect {
        let height = (work_area.height() - gap * 2) / 3;
        let y = work_area.top + height * 2 + gap;
        WindowRect::new(work_area.left + gap, y, work_area.width() - gap * 2, height)
    }
}

pub struct TopVerticalTwoThirdsCalculator;

impl TopVerticalTwoThirdsCalculator {
    pub fn new() -> Self {
        Self
    }
}

impl RectCalculator for TopVerticalTwoThirdsCalculator {
    fn calculate(&self, work_area: &WorkArea, _current_window: &WindowRect, _action: WindowAction, gap: i32) -> WindowRect {
        let height = (work_area.height() - gap * 2) / 3 * 2 + gap;
        WindowRect::new(work_area.left + gap, work_area.top + gap, work_area.width() - gap * 2, height)
    }
}

pub struct BottomVerticalTwoThirdsCalculator;

impl BottomVerticalTwoThirdsCalculator {
    pub fn new() -> Self {
        Self
    }
}

impl RectCalculator for BottomVerticalTwoThirdsCalculator {
    fn calculate(&self, work_area: &WorkArea, _current_window: &WindowRect, _action: WindowAction, gap: i32) -> WindowRect {
        let height = (work_area.height() - gap * 2) / 3 * 2 + gap;
        let y = work_area.bottom - height - gap;
        WindowRect::new(work_area.left + gap, y, work_area.width() - gap * 2, height)
    }
}
