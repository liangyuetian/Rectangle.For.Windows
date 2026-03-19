use crate::core::rect::{WorkArea, WindowRect};
use crate::core::action::WindowAction;
use crate::core::drag_state::{DragState, SnapArea, SnapAreaType, MouseButton};
use crate::core::calculator_factory::CalculatorFactory;
use crate::services::config::ConfigService;
use crate::services::win32::Win32WindowService;
use crate::services::window_enumerator::WindowEnumerator;
use crate::services::window_manager::WindowManager;
use crate::services::logger::Logger;
use std::sync::{Arc, Mutex};
use std::time::{Duration, Instant};
use windows::Win32::Foundation::POINT;
use windows::Win32::UI::WindowsAndMessaging::{
    GetCursorPos, MonitorFromPoint, GetMonitorInfo, MONITORINFO, MONITOR_DEFAULTTONEAREST,
    IsWindowVisible, GetWindowLongPtrW, GWL_STYLE, GWL_EXSTYLE,
};

/// 吸附事件参数
#[derive(Debug, Clone)]
pub struct SnapEvent {
    pub window_handle: isize,
    pub action: WindowAction,
    pub snap_area: SnapArea,
}

/// 拖拽吸附管理器
pub struct SnappingManager {
    config_service: ConfigService,
    win32: Win32WindowService,
    window_manager: Option<Arc<WindowManager>>,
    drag_state: Arc<Mutex<DragState>>,
    is_enabled: bool,
    // 配置
    edge_margin_top: i32,
    edge_margin_bottom: i32,
    edge_margin_left: i32,
    edge_margin_right: i32,
    corner_size: i32,
    snap_modifiers: i32,
    // 性能优化
    last_update_time: Instant,
    update_interval_ms: u64,
    last_snap_area: Option<SnapArea>,
}

impl SnappingManager {
    /// 常量定义
    const MINIMUM_DRAG_DISTANCE: i32 = 5;
    const VALID_DRAG_DISTANCE: i32 = 10;
    const VALID_DRAG_DURATION_MS: u64 = 100;
    const POSITION_CHANGE_THRESHOLD: i32 = 5;

    pub fn new(config_service: ConfigService, window_manager: Option<Arc<WindowManager>>) -> Self {
        Self {
            config_service,
            win32: Win32WindowService::new(),
            window_manager,
            drag_state: Arc::new(Mutex::new(DragState::new())),
            is_enabled: false,
            edge_margin_top: 5,
            edge_margin_bottom: 5,
            edge_margin_left: 5,
            edge_margin_right: 5,
            corner_size: 20,
            snap_modifiers: 0,
            last_update_time: Instant::now(),
            update_interval_ms: 16, // ~60fps
            last_snap_area: None,
        }
    }

    /// 加载配置
    fn load_config(&mut self) {
        let config = self.config_service.load();
        self.edge_margin_top = config.snap_edge_margin_top;
        self.edge_margin_bottom = config.snap_edge_margin_bottom;
        self.edge_margin_left = config.snap_edge_margin_left;
        self.edge_margin_right = config.snap_edge_margin_right;
        self.corner_size = config.corner_snap_area_size;
        self.snap_modifiers = config.snap_modifiers;
    }

    /// 启用拖拽吸附
    pub fn enable(&mut self) -> bool {
        if self.is_enabled {
            return true;
        }

        self.load_config();

        // 检查是否启用了拖拽吸附
        let config = self.config_service.load();
        if !config.drag_to_snap {
            Logger::info("SnappingManager", "拖拽吸附已禁用（配置）");
            return false;
        }

        self.is_enabled = true;
        Logger::info("SnappingManager", "拖拽吸附已启用");
        true
    }

    /// 禁用拖拽吸附
    pub fn disable(&mut self) {
        if !self.is_enabled {
            return;
        }

        self.is_enabled = false;
        {
            let mut state = self.drag_state.lock().unwrap();
            state.reset();
        }
        Logger::info("SnappingManager", "拖拽吸附已禁用");
    }

    /// 处理鼠标按下事件
    pub fn on_mouse_down(&self, x: i32, y: i32) {
        if !self.is_enabled {
            return;
        }

        // 获取光标下的窗口
        let hwnd = WindowEnumerator::get_process_name_from_window

::get_window_under_cursor();
        if hwnd == 0 {
            return;
        }

        // 检查是否是有效窗口
        if !self.is_valid_window_for_dragging(hwnd) {
            return;
        }

        // 获取窗口矩形
        let (win_x, win_y, win_w, win_h) = match self.win32.get_window_rect(hwnd) {
            Some(rect) => rect,
            None => return,
        };

        // 开始拖拽
        {
            let mut state = self.drag_state.lock().unwrap();
            state.reset();
            state.is_dragging = true;
            state.dragged_window = hwnd;
            state.initial_mouse_x = x;
            state.initial_mouse_y = y;
            state.current_mouse_x = x;
            state.current_mouse_y = y;
            state.initial_window_rect = WindowRect::new(win_x, win_y, win_w, win_h);
            state.drag_start_time = Instant::now();
            state.drag_button = Some(MouseButton::Left);

            // 检查是否需要保存原始位置（用于 Unsnap 恢复）
            let config = self.config_service.load();
            if config.unsnap_restore {
                state.original_rect = Some(WindowRect::new(win_x, win_y, win_w, win_h));
            }
        }

        Logger::debug("SnappingManager", &format!("开始拖拽窗口: {}", hwnd));
    }

    /// 处理鼠标移动事件
    pub fn on_mouse_move(&self, x: i32, y: i32) {
        if !self.is_enabled {
            return;
        }

        {
            let mut state = self.drag_state.lock().unwrap();
            if !state.is_dragging {
                return;
            }

            // 帧率限制
            let now = Instant::now();
            let elapsed = now.duration_since(self.last_update_time).as_millis() as u64;
            if elapsed < self.update_interval_ms {
                return;
            }

            state.current_mouse_x = x;
            state.current_mouse_y = y;
        }

        // 检查拖拽距离
        let drag_distance = {
            let state = self.drag_state.lock().unwrap();
            state.get_drag_distance()
        };

        if drag_distance < Self::MINIMUM_DRAG_DISTANCE {
            return;
        }

        // 计算吸附区域
        let snap_area = self.calculate_snap_area(x, y);

        // 检测吸附区域是否变化
        let snap_area_changed = match (&snap_area, &self.last_snap_area) {
            (Some(a), Some(b)) => a.action != b.action,
            (None, None) => false,
            _ => true,
        };

        if snap_area.is_some() {
            let area = snap_area.clone().unwrap();
            {
                let mut state = self.drag_state.lock().unwrap();
                state.current_snap_area = Some(area);
            }

            if snap_area_changed {
                Logger::debug("SnappingManager", &format!("检测到吸附区域: {:?}", area.name));
            }
        } else {
            let mut state = self.drag_state.lock().unwrap();
            state.current_snap_area = None;
        }
    }

    /// 处理鼠标释放事件
    pub fn on_mouse_up(&self, x: i32, y: i32) {
        if !self.is_enabled {
            return;
        }

        let (hwnd, is_valid_drag, snap_area, original_rect) = {
            let state = self.drag_state.lock().unwrap();
            (
                state.dragged_window,
                state.get_drag_duration_ms() > Self::VALID_DRAG_DURATION_MS
                    || state.get_drag_distance() > Self::VALID_DRAG_DISTANCE,
                state.current_snap_area.clone(),
                state.original_rect,
            )
        };

        if hwnd == 0 {
            return;
        }

        // 执行吸附
        if is_valid_drag && snap_area.is_some() {
            self.execute_snap(&snap_area.unwrap(), hwnd);
        } else if is_valid_drag {
            // Unsnap 恢复
            let config = self.config_service.load();
            if config.unsnap_restore && original_rect.is_some() {
                let rect = original_rect.unwrap();
                self.win32.set_window_rect(hwnd, rect.x, rect.y, rect.width, rect.height);
                Logger::info("SnappingManager", &format!(
                    "Unsnap 恢复: 恢复窗口到原始位置 ({}, {}, {}, {})",
                    rect.x, rect.y, rect.width, rect.height
                ));
            }
        }

        Logger::debug("SnappingManager", &format!("结束拖拽窗口: {}", hwnd));

        // 重置状态
        {
            let mut state = self.drag_state.lock().unwrap();
            state.reset();
        }
    }

    /// 计算吸附区域
    fn calculate_snap_area(&self, x: i32, y: i32) -> Option<SnapArea> {
        // 获取光标所在的屏幕
        let work_area = self.get_work_area_from_point(x, y)?;

        // 检查屏幕边缘
        if let Some(snap_area) = self.check_screen_edges(x, y, &work_area) {
            return Some(snap_area);
        }

        // 检查屏幕角落
        if let Some(snap_area) = self.check_screen_corners(x, y, &work_area) {
            return Some(snap_area);
        }

        None
    }

    /// 获取点所在的屏幕工作区
    fn get_work_area_from_point(&self, x: i32, y: i32) -> Option<WorkArea> {
        unsafe {
            let point = POINT { x, y };
            let h_monitor = MonitorFromPoint(point, MONITOR_DEFAULTTONEAREST);

            let mut monitor_info = MONITORINFO {
                cbSize: std::mem::size_of::<MONITORINFO>() as u32,
                rcMonitor: windows::Win32::Foundation::RECT::default(),
                rcWork: windows::Win32::Foundation::RECT::default(),
                dwFlags: 0,
            };

            if GetMonitorInfo(h_monitor, &mut monitor_info).as_bool() {
                Some(WorkArea::new(
                    monitor_info.rcWork.left,
                    monitor_info.rcWork.top,
                    monitor_info.rcWork.right,
                    monitor_info.rcWork.bottom,
                ))
            } else {
                None
            }
        }
    }

    /// 检查屏幕边缘吸附
    fn check_screen_edges(&self, x: i32, y: i32, work_area: &WorkArea) -> Option<SnapArea> {
        // 左边缘
        if x >= work_area.left && x <= work_area.left + self.edge_margin_left {
            return Some(SnapArea::new(
                WindowAction::LeftHalf,
                SnapAreaType::Edge,
                "Left Edge",
            ));
        }

        // 右边缘
        if x >= work_area.right - self.edge_margin_right && x <= work_area.right {
            return Some(SnapArea::new(
                WindowAction::RightHalf,
                SnapAreaType::Edge,
                "Right Edge",
            ));
        }

        // 上边缘
        if y >= work_area.top && y <= work_area.top + self.edge_margin_top {
            return Some(SnapArea::new(
                WindowAction::Maximize,
                SnapAreaType::Edge,
                "Top Edge",
            ));
        }

        // 下边缘
        if y >= work_area.bottom - self.edge_margin_bottom && y <= work_area.bottom {
            return Some(SnapArea::new(
                WindowAction::BottomHalf,
                SnapAreaType::Edge,
                "Bottom Edge",
            ));
        }

        None
    }

    /// 检查屏幕角落吸附
    fn check_screen_corners(&self, x: i32, y: i32, work_area: &WorkArea) -> Option<SnapArea> {
        // 左上角
        if x <= work_area.left + self.corner_size && y <= work_area.top + self.corner_size {
            return Some(SnapArea::new(
                WindowAction::TopLeft,
                SnapAreaType::Corner,
                "Top Left Corner",
            ));
        }

        // 右上角
        if x >= work_area.right - self.corner_size && y <= work_area.top + self.corner_size {
            return Some(SnapArea::new(
                WindowAction::TopRight,
                SnapAreaType::Corner,
                "Top Right Corner",
            ));
        }

        // 左下角
        if x <= work_area.left + self.corner_size && y >= work_area.bottom - self.corner_size {
            return Some(SnapArea::new(
                WindowAction::BottomLeft,
                SnapAreaType::Corner,
                "Bottom Left Corner",
            ));
        }

        // 右下角
        if x >= work_area.right - self.corner_size && y >= work_area.bottom - self.corner_size {
            return Some(SnapArea::new(
                WindowAction::BottomRight,
                SnapAreaType::Corner,
                "Bottom Right Corner",
            ));
        }

        None
    }

    /// 执行吸附
    fn execute_snap(&self, snap_area: &SnapArea, hwnd: isize) {
        // 执行窗口操作
        if let Some(ref wm) = self.window_manager {
            wm.execute(snap_area.action, false);
            Logger::info("SnappingManager", &format!(
                "执行吸附: {:?} -> {:?}",
                snap_area.name, snap_area.action
            ));
        }
    }

    /// 检查窗口是否适合拖拽
    fn is_valid_window_for_dragging(&self, hwnd: isize) -> bool {
        unsafe {
            let hwnd_ptr = windows::Win32::Foundation::HWND(hwnd as *mut std::ffi::c_void);

            // 检查窗口是否可见
            if !IsWindowVisible(hwnd_ptr).as_bool() {
                return false;
            }

            const WS_CAPTION: u32 = 0x00C00000;
            const WS_EX_TOOLWINDOW: u32 = 0x00000080;
            const WS_EX_NOACTIVATE: u32 = 0x08000000;

            // 检查窗口样式
            let style = GetWindowLongPtrW(hwnd_ptr, GWL_STYLE) as u32;
            let ex_style = GetWindowLongPtrW(hwnd_ptr, GWL_EXSTYLE) as u32;

            // 排除无标题栏窗口
            if (style & WS_CAPTION) != WS_CAPTION {
                return false;
            }

            // 排除工具窗口
            if (ex_style & WS_EX_TOOLWINDOW) == WS_EX_TOOLWINDOW {
                return false;
            }

            // 排除无激活窗口
            if (ex_style & WS_EX_NOACTIVATE) == WS_EX_NOACTIVATE {
                return false;
            }

            true
        }
    }

    /// 检查是否已启用
    pub fn is_enabled(&self) -> bool {
        self.is_enabled
    }
}
