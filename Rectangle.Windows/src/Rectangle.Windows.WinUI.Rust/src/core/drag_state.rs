use crate::core::rect::WindowRect;
use crate::core::action::WindowAction;

/// 鼠标按钮
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum MouseButton {
    Left,
    Right,
    Middle,
}

/// 吸附区域类型
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum SnapAreaType {
    Edge,
    Corner,
    Center,
}

/// 吸附区域
#[derive(Debug, Clone)]
pub struct SnapArea {
    pub action: WindowAction,
    pub area_type: SnapAreaType,
    pub name: String,
}

impl SnapArea {
    pub fn new(action: WindowAction, area_type: SnapAreaType, name: impl Into<String>) -> Self {
        Self {
            action,
            area_type,
            name: name.into(),
        }
    }
}

/// 拖拽状态
#[derive(Debug, Clone)]
pub struct DragState {
    pub is_dragging: bool,
    pub dragged_window: isize,
    pub initial_mouse_x: i32,
    pub initial_mouse_y: i32,
    pub current_mouse_x: i32,
    pub current_mouse_y: i32,
    pub initial_window_rect: WindowRect,
    pub original_rect: Option<WindowRect>,
    pub current_snap_area: Option<SnapArea>,
    pub drag_button: Option<MouseButton>,
    pub drag_start_time: std::time::Instant,
}

impl DragState {
    pub fn new() -> Self {
        Self {
            is_dragging: false,
            dragged_window: 0,
            initial_mouse_x: 0,
            initial_mouse_y: 0,
            current_mouse_x: 0,
            current_mouse_y: 0,
            initial_window_rect: WindowRect::new(0, 0, 0, 0),
            original_rect: None,
            current_snap_area: None,
            drag_button: None,
            drag_start_time: std::time::Instant::now(),
        }
    }

    /// 重置状态
    pub fn reset(&mut self) {
        self.is_dragging = false;
        self.dragged_window = 0;
        self.initial_mouse_x = 0;
        self.initial_mouse_y = 0;
        self.current_mouse_x = 0;
        self.current_mouse_y = 0;
        self.initial_window_rect = WindowRect::new(0, 0, 0, 0);
        self.original_rect = None;
        self.current_snap_area = None;
        self.drag_button = None;
    }

    /// 获取拖拽距离
    pub fn get_drag_distance(&self) -> i32 {
        let dx = self.current_mouse_x - self.initial_mouse_x;
        let dy = self.current_mouse_y - self.initial_mouse_y;
        ((dx * dx + dy * dy) as f64).sqrt() as i32
    }

    /// 获取拖拽持续时间（毫秒）
    pub fn get_drag_duration_ms(&self) -> u64 {
        self.drag_start_time.elapsed().as_millis() as u64
    }
}

impl Default for DragState {
    fn default() -> Self {
        Self::new()
    }
}
