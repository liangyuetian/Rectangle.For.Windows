use crate::core::action::WindowAction;
use chrono::{DateTime, Local};
use serde::{Deserialize, Serialize};
use std::collections::HashMap;
use std::collections::HashSet;

/// 记录程序对窗口的操作信息
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct RectangleAction {
    pub action: WindowAction,
    pub count: i32,
    pub last_execution_time: DateTime<Local>,
    pub rect: (i32, i32, i32, i32),
}

/// 窗口历史记录管理
pub struct WindowHistory {
    /// 用户原始窗口位置（用于 Restore 恢复到最初位置）
    restore_rects: HashMap<isize, (i32, i32, i32, i32)>,
    /// 程序最后一次对窗口的操作（用于重复执行检测）
    last_actions: HashMap<isize, RectangleAction>,
    /// 由程序调整过的窗口集合（位置变化不应被记录为用户操作）
    program_adjusted_windows: HashSet<isize>,
    /// 重复执行超时时间（秒）
    repeat_timeout_seconds: f64,
}

impl WindowHistory {
    pub fn new() -> Self {
        Self {
            restore_rects: HashMap::new(),
            last_actions: HashMap::new(),
            program_adjusted_windows: HashSet::new(),
            repeat_timeout_seconds: 2.0,
        }
    }

    /// 保存恢复点（总是覆盖）
    pub fn save_restore_rect(&mut self, hwnd: isize, x: i32, y: i32, w: i32, h: i32) {
        self.restore_rects.insert(hwnd, (x, y, w, h));
    }

    /// 只在没有恢复点时保存（保留最初位置）
    pub fn save_restore_rect_if_not_exists(&mut self, hwnd: isize, x: i32, y: i32, w: i32, h: i32) {
        if !self.restore_rects.contains_key(&hwnd) {
            self.restore_rects.insert(hwnd, (x, y, w, h));
        }
    }

    /// 获取恢复点
    pub fn try_get_restore_rect(&self, hwnd: isize) -> Option<(i32, i32, i32, i32)> {
        self.restore_rects.get(&hwnd).copied()
    }

    /// 是否有恢复点
    pub fn has_restore_rect(&self, hwnd: isize) -> bool {
        self.restore_rects.contains_key(&hwnd)
    }

    /// 记录操作
    pub fn record_action(&mut self, hwnd: isize, action: WindowAction, x: i32, y: i32, w: i32, h: i32) {
        let now = Local::now();
        if let Some(last) = self.last_actions.get(&hwnd) {
            let elapsed = (now - last.last_execution_time).num_seconds() as f64;
            if last.action == action && elapsed <= self.repeat_timeout_seconds {
                let new_action = RectangleAction {
                    action,
                    count: last.count + 1,
                    last_execution_time: now,
                    rect: (x, y, w, h),
                };
                self.last_actions.insert(hwnd, new_action);
                return;
            }
        }
        self.last_actions.insert(hwnd, RectangleAction {
            action,
            count: 1,
            last_execution_time: now,
            rect: (x, y, w, h),
        });
    }

    /// 获取最后操作
    pub fn try_get_last_action(&self, hwnd: isize) -> Option<&RectangleAction> {
        self.last_actions.get(&hwnd)
    }

    /// 移除最后操作
    pub fn remove_last_action(&mut self, hwnd: isize) {
        self.last_actions.remove(&hwnd);
    }

    /// 检测窗口是否被用户手动移动（与程序最后操作位置不同）
    pub fn is_window_moved_externally(&self, hwnd: isize, x: i32, y: i32, w: i32, h: i32) -> bool {
        if let Some(last) = self.last_actions.get(&hwnd) {
            let (rx, ry, rw, rh) = last.rect;
            return (rx - x).abs() > 2 || (ry - y).abs() > 2
                || (rw - w).abs() > 2 || (rh - h).abs() > 2;
        }
        false
    }

    /// 标记为程序调整的窗口
    pub fn mark_as_program_adjusted(&mut self, hwnd: isize) {
        self.program_adjusted_windows.insert(hwnd);
    }

    /// 清除程序调整标记
    pub fn clear_program_adjusted_mark(&mut self, hwnd: isize) {
        self.program_adjusted_windows.remove(&hwnd);
    }

    /// 是否为程序调整的窗口
    pub fn is_program_adjusted(&self, hwnd: isize) -> bool {
        self.program_adjusted_windows.contains(&hwnd)
    }

    /// 移除窗口记录
    pub fn remove_window(&mut self, hwnd: isize) {
        self.restore_rects.remove(&hwnd);
        self.last_actions.remove(&hwnd);
        self.program_adjusted_windows.remove(&hwnd);
    }

    /// 清空所有记录
    pub fn clear(&mut self) {
        self.restore_rects.clear();
        self.last_actions.clear();
        self.program_adjusted_windows.clear();
    }
}

impl Default for WindowHistory {
    fn default() -> Self {
        Self::new()
    }
}
