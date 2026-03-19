use crate::core::rect::{WorkArea, WindowRect};
use crate::core::action::WindowAction;
use crate::core::calculator::RectCalculator;
use crate::services::config::ConfigService;

/// 左侧 Todo 模式计算器
pub struct LeftTodoCalculator {
    config_service: Option<ConfigService>,
}

impl LeftTodoCalculator {
    pub fn new(config_service: Option<&ConfigService>) -> Self {
        Self { config_service: config_service.cloned() }
    }
}

impl RectCalculator for LeftTodoCalculator {
    fn calculate(&self, work_area: &WorkArea, _current_window: &WindowRect, _action: WindowAction, gap: i32) -> WindowRect {
        let todo_width = if let Some(ref service) = self.config_service {
            let config = service.load();
            config.todo_sidebar_width
        } else {
            400
        };

        let available_width = work_area.width() - todo_width - gap * 3;
        WindowRect::new(
            work_area.left + gap,
            work_area.top + gap,
            available_width,
            work_area.height() - gap * 2
        )
    }
}

/// 右侧 Todo 模式计算器
pub struct RightTodoCalculator {
    config_service: Option<ConfigService>,
}

impl RightTodoCalculator {
    pub fn new(config_service: Option<&ConfigService>) -> Self {
        Self { config_service: config_service.cloned() }
    }
}

impl RectCalculator for RightTodoCalculator {
    fn calculate(&self, work_area: &WorkArea, _current_window: &WindowRect, _action: WindowAction, gap: i32) -> WindowRect {
        let todo_width = if let Some(ref service) = self.config_service {
            let config = service.load();
            config.todo_sidebar_width
        } else {
            400
        };

        let available_width = work_area.width() - todo_width - gap * 3;
        let x = work_area.left + todo_width + gap * 2;
        WindowRect::new(
            x,
            work_area.top + gap,
            available_width,
            work_area.height() - gap * 2
        )
    }
}
