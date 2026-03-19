use crate::core::rect::{WorkArea, WindowRect};
use crate::core::action::WindowAction;
use crate::core::calculator::RectCalculator;

// ============================================
// 八等分计算器 (8个)
// ============================================

pub struct TopLeftEighthCalculator;

impl TopLeftEighthCalculator {
    pub fn new() -> Self {
        Self
    }
}

impl RectCalculator for TopLeftEighthCalculator {
    fn calculate(&self, work_area: &WorkArea, _current_window: &WindowRect, _action: WindowAction, gap: i32) -> WindowRect {
        let width = (work_area.width() - gap * 3) / 4;
        let height = (work_area.height() - gap) / 2;
        WindowRect::new(work_area.left + gap, work_area.top + gap, width, height)
    }
}

pub struct TopCenterLeftEighthCalculator;

impl TopCenterLeftEighthCalculator {
    pub fn new() -> Self {
        Self
    }
}

impl RectCalculator for TopCenterLeftEighthCalculator {
    fn calculate(&self, work_area: &WorkArea, _current_window: &WindowRect, _action: WindowAction, gap: i32) -> WindowRect {
        let width = (work_area.width() - gap * 3) / 4;
        let height = (work_area.height() - gap) / 2;
        let x = work_area.left + width + gap * 2;
        WindowRect::new(x, work_area.top + gap, width, height)
    }
}

pub struct TopCenterRightEighthCalculator;

impl TopCenterRightEighthCalculator {
    pub fn new() -> Self {
        Self
    }
}

impl RectCalculator for TopCenterRightEighthCalculator {
    fn calculate(&self, work_area: &WorkArea, _current_window: &WindowRect, _action: WindowAction, gap: i32) -> WindowRect {
        let width = (work_area.width() - gap * 3) / 4;
        let height = (work_area.height() - gap) / 2;
        let x = work_area.left + width * 2 + gap * 3;
        WindowRect::new(x, work_area.top + gap, width, height)
    }
}

pub struct TopRightEighthCalculator;

impl TopRightEighthCalculator {
    pub fn new() -> Self {
        Self
    }
}

impl RectCalculator for TopRightEighthCalculator {
    fn calculate(&self, work_area: &WorkArea, _current_window: &WindowRect, _action: WindowAction, gap: i32) -> WindowRect {
        let width = (work_area.width() - gap * 3) / 4;
        let height = (work_area.height() - gap) / 2;
        let x = work_area.left + width * 3 + gap * 4;
        WindowRect::new(x, work_area.top + gap, width, height)
    }
}

pub struct BottomLeftEighthCalculator;

impl BottomLeftEighthCalculator {
    pub fn new() -> Self {
        Self
    }
}

impl RectCalculator for BottomLeftEighthCalculator {
    fn calculate(&self, work_area: &WorkArea, _current_window: &WindowRect, _action: WindowAction, gap: i32) -> WindowRect {
        let width = (work_area.width() - gap * 3) / 4;
        let height = (work_area.height() - gap) / 2;
        let y = work_area.top + height + gap * 2;
        WindowRect::new(work_area.left + gap, y, width, height)
    }
}

pub struct BottomCenterLeftEighthCalculator;

impl BottomCenterLeftEighthCalculator {
    pub fn new() -> Self {
        Self
    }
}

impl RectCalculator for BottomCenterLeftEighthCalculator {
    fn calculate(&self, work_area: &WorkArea, _current_window: &WindowRect, _action: WindowAction, gap: i32) -> WindowRect {
        let width = (work_area.width() - gap * 3) / 4;
        let height = (work_area.height() - gap) / 2;
        let x = work_area.left + width + gap * 2;
        let y = work_area.top + height + gap * 2;
        WindowRect::new(x, y, width, height)
    }
}

pub struct BottomCenterRightEighthCalculator;

impl BottomCenterRightEighthCalculator {
    pub fn new() -> Self {
        Self
    }
}

impl RectCalculator for BottomCenterRightEighthCalculator {
    fn calculate(&self, work_area: &WorkArea, _current_window: &WindowRect, _action: WindowAction, gap: i32) -> WindowRect {
        let width = (work_area.width() - gap * 3) / 4;
        let height = (work_area.height() - gap) / 2;
        let x = work_area.left + width * 2 + gap * 3;
        let y = work_area.top + height + gap * 2;
        WindowRect::new(x, y, width, height)
    }
}

pub struct BottomRightEighthCalculator;

impl BottomRightEighthCalculator {
    pub fn new() -> Self {
        Self
    }
}

impl RectCalculator for BottomRightEighthCalculator {
    fn calculate(&self, work_area: &WorkArea, _current_window: &WindowRect, _action: WindowAction, gap: i32) -> WindowRect {
        let width = (work_area.width() - gap * 3) / 4;
        let height = (work_area.height() - gap) / 2;
        let x = work_area.left + width * 3 + gap * 4;
        let y = work_area.top + height + gap * 2;
        WindowRect::new(x, y, width, height)
    }
}
