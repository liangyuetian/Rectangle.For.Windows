use crate::core::rect::{WorkArea, WindowRect};
use crate::core::action::WindowAction;
use crate::core::calculator::RectCalculator;

/// 第一三分之一计算器
pub struct FirstThirdCalculator;

impl FirstThirdCalculator {
    pub fn new() -> Self {
        Self
    }
}

impl RectCalculator for FirstThirdCalculator {
    fn calculate(&self, work_area: &WorkArea, _current_window: &WindowRect, _action: WindowAction, gap: i32) -> WindowRect {
        let width = (work_area.width() - gap * 2) / 3;
        WindowRect::new(work_area.left, work_area.top, width, work_area.height())
    }
}

/// 中间三分之一计算器
pub struct CenterThirdCalculator;

impl CenterThirdCalculator {
    pub fn new() -> Self {
        Self
    }
}

impl RectCalculator for CenterThirdCalculator {
    fn calculate(&self, work_area: &WorkArea, _current_window: &WindowRect, _action: WindowAction, gap: i32) -> WindowRect {
        let width = (work_area.width() - gap * 2) / 3;
        let x = work_area.left + width + gap;
        WindowRect::new(x, work_area.top, width, work_area.height())
    }
}

/// 最后三分之一计算器
pub struct LastThirdCalculator;

impl LastThirdCalculator {
    pub fn new() -> Self {
        Self
    }
}

impl RectCalculator for LastThirdCalculator {
    fn calculate(&self, work_area: &WorkArea, _current_window: &WindowRect, _action: WindowAction, gap: i32) -> WindowRect {
        let width = (work_area.width() - gap * 2) / 3;
        let x = work_area.left + width * 2 + gap * 2;
        WindowRect::new(x, work_area.top, width, work_area.height())
    }
}

/// 前三分之二计算器
pub struct FirstTwoThirdsCalculator;

impl FirstTwoThirdsCalculator {
    pub fn new() -> Self {
        Self
    }
}

impl RectCalculator for FirstTwoThirdsCalculator {
    fn calculate(&self, work_area: &WorkArea, _current_window: &WindowRect, _action: WindowAction, gap: i32) -> WindowRect {
        let width = (work_area.width() - gap * 2) / 3 * 2 + gap;
        WindowRect::new(work_area.left, work_area.top, width, work_area.height())
    }
}

/// 中间三分之二计算器
pub struct CenterTwoThirdsCalculator;

impl CenterTwoThirdsCalculator {
    pub fn new() -> Self {
        Self
    }
}

impl RectCalculator for CenterTwoThirdsCalculator {
    fn calculate(&self, work_area: &WorkArea, _current_window: &WindowRect, _action: WindowAction, gap: i32) -> WindowRect {
        let width = (work_area.width() - gap * 2) / 3 * 2;
        let x = work_area.left + (work_area.width() - width) / 2;
        WindowRect::new(x, work_area.top, width, work_area.height())
    }
}

/// 后三分之二计算器
pub struct LastTwoThirdsCalculator;

impl LastTwoThirdsCalculator {
    pub fn new() -> Self {
        Self
    }
}

impl RectCalculator for LastTwoThirdsCalculator {
    fn calculate(&self, work_area: &WorkArea, _current_window: &WindowRect, _action: WindowAction, gap: i32) -> WindowRect {
        let width = (work_area.width() - gap * 2) / 3 * 2 + gap;
        let x = work_area.right - width;
        WindowRect::new(x, work_area.top, width, work_area.height())
    }
}
