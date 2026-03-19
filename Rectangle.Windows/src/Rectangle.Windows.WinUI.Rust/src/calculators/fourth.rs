use crate::core::rect::{WorkArea, WindowRect};
use crate::core::action::WindowAction;
use crate::core::calculator::RectCalculator;

/// 第一四分之一计算器
pub struct FirstFourthCalculator;

impl FirstFourthCalculator {
    pub fn new() -> Self {
        Self
    }
}

impl RectCalculator for FirstFourthCalculator {
    fn calculate(&self, work_area: &WorkArea, _current_window: &WindowRect, _action: WindowAction, gap: i32) -> WindowRect {
        let width = (work_area.width() - gap * 3) / 4;
        WindowRect::new(work_area.left, work_area.top, width, work_area.height())
    }
}

/// 第二四分之一计算器
pub struct SecondFourthCalculator;

impl SecondFourthCalculator {
    pub fn new() -> Self {
        Self
    }
}

impl RectCalculator for SecondFourthCalculator {
    fn calculate(&self, work_area: &WorkArea, _current_window: &WindowRect, _action: WindowAction, gap: i32) -> WindowRect {
        let width = (work_area.width() - gap * 3) / 4;
        let x = work_area.left + width + gap;
        WindowRect::new(x, work_area.top, width, work_area.height())
    }
}

/// 第三四分之一计算器
pub struct ThirdFourthCalculator;

impl ThirdFourthCalculator {
    pub fn new() -> Self {
        Self
    }
}

impl RectCalculator for ThirdFourthCalculator {
    fn calculate(&self, work_area: &WorkArea, _current_window: &WindowRect, _action: WindowAction, gap: i32) -> WindowRect {
        let width = (work_area.width() - gap * 3) / 4;
        let x = work_area.left + width * 2 + gap * 2;
        WindowRect::new(x, work_area.top, width, work_area.height())
    }
}

/// 最后四分之一计算器
pub struct LastFourthCalculator;

impl LastFourthCalculator {
    pub fn new() -> Self {
        Self
    }
}

impl RectCalculator for LastFourthCalculator {
    fn calculate(&self, work_area: &WorkArea, _current_window: &WindowRect, _action: WindowAction, gap: i32) -> WindowRect {
        let width = (work_area.width() - gap * 3) / 4;
        let x = work_area.right - width;
        WindowRect::new(x, work_area.top, width, work_area.height())
    }
}

/// 前四分之三计算器
pub struct FirstThreeFourthsCalculator;

impl FirstThreeFourthsCalculator {
    pub fn new() -> Self {
        Self
    }
}

impl RectCalculator for FirstThreeFourthsCalculator {
    fn calculate(&self, work_area: &WorkArea, _current_window: &WindowRect, _action: WindowAction, gap: i32) -> WindowRect {
        let width = (work_area.width() - gap * 3) / 4 * 3 + gap * 2;
        WindowRect::new(work_area.left, work_area.top, width, work_area.height())
    }
}

/// 中间四分之三计算器
pub struct CenterThreeFourthsCalculator;

impl CenterThreeFourthsCalculator {
    pub fn new() -> Self {
        Self
    }
}

impl RectCalculator for CenterThreeFourthsCalculator {
    fn calculate(&self, work_area: &WorkArea, _current_window: &WindowRect, _action: WindowAction, gap: i32) -> WindowRect {
        let width = (work_area.width() - gap * 3) / 4 * 3 + gap * 2;
        let x = work_area.left + (work_area.width() - width) / 2;
        WindowRect::new(x, work_area.top, width, work_area.height())
    }
}

/// 后四分之三计算器
pub struct LastThreeFourthsCalculator;

impl LastThreeFourthsCalculator {
    pub fn new() -> Self {
        Self
    }
}

impl RectCalculator for LastThreeFourthsCalculator {
    fn calculate(&self, work_area: &WorkArea, _current_window: &WindowRect, _action: WindowAction, gap: i32) -> WindowRect {
        let width = (work_area.width() - gap * 3) / 4 * 3 + gap * 2;
        let x = work_area.right - width;
        WindowRect::new(x, work_area.top, width, work_area.height())
    }
}
