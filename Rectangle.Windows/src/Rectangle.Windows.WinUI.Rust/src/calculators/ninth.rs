use crate::core::rect::{WorkArea, WindowRect};
use crate::core::action::WindowAction;
use crate::core::calculator::RectCalculator;

// ============================================
// 九等分计算器 (9个)
// ============================================

pub struct TopLeftNinthCalculator;

impl TopLeftNinthCalculator {
    pub fn new() -> Self {
        Self
    }
}

impl RectCalculator for TopLeftNinthCalculator {
    fn calculate(&self, work_area: &WorkArea, _current_window: &WindowRect, _action: WindowAction, gap: i32) -> WindowRect {
        let width = (work_area.width() - gap * 2) / 3;
        let height = (work_area.height() - gap * 2) / 3;
        WindowRect::new(work_area.left + gap, work_area.top + gap, width, height)
    }
}

pub struct TopCenterNinthCalculator;

impl TopCenterNinthCalculator {
    pub fn new() -> Self {
        Self
    }
}

impl RectCalculator for TopCenterNinthCalculator {
    fn calculate(&self, work_area: &WorkArea, _current_window: &WindowRect, _action: WindowAction, gap: i32) -> WindowRect {
        let width = (work_area.width() - gap * 2) / 3;
        let height = (work_area.height() - gap * 2) / 3;
        let x = work_area.left + width + gap;
        WindowRect::new(x, work_area.top + gap, width, height)
    }
}

pub struct TopRightNinthCalculator;

impl TopRightNinthCalculator {
    pub fn new() -> Self {
        Self
    }
}

impl RectCalculator for TopRightNinthCalculator {
    fn calculate(&self, work_area: &WorkArea, _current_window: &WindowRect, _action: WindowAction, gap: i32) -> WindowRect {
        let width = (work_area.width() - gap * 2) / 3;
        let height = (work_area.height() - gap * 2) / 3;
        let x = work_area.left + width * 2 + gap;
        WindowRect::new(x, work_area.top + gap, width, height)
    }
}

pub struct MiddleLeftNinthCalculator;

impl MiddleLeftNinthCalculator {
    pub fn new() -> Self {
        Self
    }
}

impl RectCalculator for MiddleLeftNinthCalculator {
    fn calculate(&self, work_area: &WorkArea, _current_window: &WindowRect, _action: WindowAction, gap: i32) -> WindowRect {
        let width = (work_area.width() - gap * 2) / 3;
        let height = (work_area.height() - gap * 2) / 3;
        let y = work_area.top + height + gap;
        WindowRect::new(work_area.left + gap, y, width, height)
    }
}

pub struct MiddleCenterNinthCalculator;

impl MiddleCenterNinthCalculator {
    pub fn new() -> Self {
        Self
    }
}

impl RectCalculator for MiddleCenterNinthCalculator {
    fn calculate(&self, work_area: &WorkArea, _current_window: &WindowRect, _action: WindowAction, gap: i32) -> WindowRect {
        let width = (work_area.width() - gap * 2) / 3;
        let height = (work_area.height() - gap * 2) / 3;
        let x = work_area.left + width + gap;
        let y = work_area.top + height + gap;
        WindowRect::new(x, y, width, height)
    }
}

pub struct MiddleRightNinthCalculator;

impl MiddleRightNinthCalculator {
    pub fn new() -> Self {
        Self
    }
}

impl RectCalculator for MiddleRightNinthCalculator {
    fn calculate(&self, work_area: &WorkArea, _current_window: &WindowRect, _action: WindowAction, gap: i32) -> WindowRect {
        let width = (work_area.width() - gap * 2) / 3;
        let height = (work_area.height() - gap * 2) / 3;
        let x = work_area.left + width * 2 + gap;
        let y = work_area.top + height + gap;
        WindowRect::new(x, y, width, height)
    }
}

pub struct BottomLeftNinthCalculator;

impl BottomLeftNinthCalculator {
    pub fn new() -> Self {
        Self
    }
}

impl RectCalculator for BottomLeftNinthCalculator {
    fn calculate(&self, work_area: &WorkArea, _current_window: &WindowRect, _action: WindowAction, gap: i32) -> WindowRect {
        let width = (work_area.width() - gap * 2) / 3;
        let height = (work_area.height() - gap * 2) / 3;
        let y = work_area.top + height * 2 + gap;
        WindowRect::new(work_area.left + gap, y, width, height)
    }
}

pub struct BottomCenterNinthCalculator;

impl BottomCenterNinthCalculator {
    pub fn new() -> Self {
        Self
    }
}

impl RectCalculator for BottomCenterNinthCalculator {
    fn calculate(&self, work_area: &WorkArea, _current_window: &WindowRect, _action: WindowAction, gap: i32) -> WindowRect {
        let width = (work_area.width() - gap * 2) / 3;
        let height = (work_area.height() - gap * 2) / 3;
        let x = work_area.left + width + gap;
        let y = work_area.top + height * 2 + gap;
        WindowRect::new(x, y, width, height)
    }
}

pub struct BottomRightNinthCalculator;

impl BottomRightNinthCalculator {
    pub fn new() -> Self {
        Self
    }
}

impl RectCalculator for BottomRightNinthCalculator {
    fn calculate(&self, work_area: &WorkArea, _current_window: &WindowRect, _action: WindowAction, gap: i32) -> WindowRect {
        let width = (work_area.width() - gap * 2) / 3;
        let height = (work_area.height() - gap * 2) / 3;
        let x = work_area.left + width * 2 + gap;
        let y = work_area.top + height * 2 + gap;
        WindowRect::new(x, y, width, height)
    }
}
