use crate::core::rect::{WorkArea, WindowRect};
use crate::core::action::WindowAction;
use crate::core::calculator::RectCalculator;
use crate::services::config::ConfigService;

/// 向左移动计算器
pub struct MoveLeftCalculator;

impl MoveLeftCalculator {
    pub fn new() -> Self {
        Self
    }
}

impl RectCalculator for MoveLeftCalculator {
    fn calculate(&self, work_area: &WorkArea, current_window: &WindowRect, _action: WindowAction, gap: i32) -> WindowRect {
        WindowRect::new(work_area.left, current_window.y, current_window.width, current_window.height)
    }
}

/// 向右移动计算器
pub struct MoveRightCalculator;

impl MoveRightCalculator {
    pub fn new() -> Self {
        Self
    }
}

impl RectCalculator for MoveRightCalculator {
    fn calculate(&self, work_area: &WorkArea, current_window: &WindowRect, _action: WindowAction, gap: i32) -> WindowRect {
        let x = work_area.right - current_window.width;
        WindowRect::new(x, current_window.y, current_window.width, current_window.height)
    }
}

/// 向上移动计算器
pub struct MoveUpCalculator;

impl MoveUpCalculator {
    pub fn new() -> Self {
        Self
    }
}

impl RectCalculator for MoveUpCalculator {
    fn calculate(&self, work_area: &WorkArea, current_window: &WindowRect, _action: WindowAction, gap: i32) -> WindowRect {
        WindowRect::new(current_window.x, work_area.top, current_window.width, current_window.height)
    }
}

/// 向下移动计算器
pub struct MoveDownCalculator;

impl MoveDownCalculator {
    pub fn new() -> Self {
        Self
    }
}

impl RectCalculator for MoveDownCalculator {
    fn calculate(&self, work_area: &WorkArea, current_window: &WindowRect, _action: WindowAction, gap: i32) -> WindowRect {
        let y = work_area.bottom - current_window.height;
        WindowRect::new(current_window.x, y, current_window.width, current_window.height)
    }
}
