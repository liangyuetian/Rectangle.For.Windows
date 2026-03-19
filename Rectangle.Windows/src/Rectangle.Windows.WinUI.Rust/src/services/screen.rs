use crate::core::rect::WorkArea;
use windows::Win32::Foundation::{LPARAM, RECT};
use windows::Win32::Graphics::Gdi::{HDC, HMONITOR, MONITORINFO};
use windows::Win32::UI::WindowsAndMessaging::{EnumDisplayMonitors, GetMonitorInfo};

/// 显示器检测服务
pub struct ScreenDetectionService;

impl ScreenDetectionService {
    /// 创建新的显示器检测服务
    pub fn new() -> Self {
        Self
    }

    /// 获取所有显示器工作区
    pub fn get_all_work_areas(&self) -> Vec<WorkArea> {
        let mut work_areas: Vec<WorkArea> = Vec::new();

        unsafe {
            let callback: unsafe extern "system" fn(HMONITOR, HDC, *mut RECT, LPARAM) -> i32 =
                monitor_enum_callback;
            EnumDisplayMonitors(
                None,
                None,
                Some(callback),
                LPARAM(&mut work_areas as *mut _ as isize),
            );
        }

        work_areas
    }

    /// 获取主显示器工作区
    pub fn get_primary_work_area(&self) -> Option<WorkArea> {
        // 第一个显示器通常是主显示器
        self.get_all_work_areas().into_iter().next()
    }

    /// 获取显示器数量
    pub fn get_display_count(&self) -> usize {
        self.get_all_work_areas().len()
    }
}

impl Default for ScreenDetectionService {
    fn default() -> Self {
        Self::new()
    }
}

/// 显示器枚举回调
unsafe extern "system" fn monitor_enum_callback(
    h_monitor: HMONITOR,
    _hdc: HDC,
    _rect: *mut RECT,
    lparam: LPARAM,
) -> i32 {
    let work_areas = &mut *(lparam.0 as *mut Vec<WorkArea>);

    let mut monitor_info = MONITORINFO {
        cbSize: std::mem::size_of::<MONITORINFO>() as u32,
        rcMonitor: RECT::default(),
        rcWork: RECT::default(),
        dwFlags: 0,
    };

    if GetMonitorInfo(h_monitor, &mut monitor_info).as_bool() {
        work_areas.push(WorkArea::new(
            monitor_info.rcWork.left,
            monitor_info.rcWork.top,
            monitor_info.rcWork.right,
            monitor_info.rcWork.bottom,
        ));
    }

    1 // 继续枚举
}
