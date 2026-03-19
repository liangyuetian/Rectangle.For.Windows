use crate::core::rect::{WorkArea, WindowRect};
use crate::core::action::WindowAction;

/// 布局计算器 trait
pub trait RectCalculator: Send + Sync {
    /// 计算窗口新位置和大小
    fn calculate(&self, work_area: &WorkArea, current_window: &WindowRect, action: WindowAction, gap: i32) -> WindowRect;
}

/// 计算器 trait 对象类型
pub type BoxedCalculator = Box<dyn RectCalculator>;
