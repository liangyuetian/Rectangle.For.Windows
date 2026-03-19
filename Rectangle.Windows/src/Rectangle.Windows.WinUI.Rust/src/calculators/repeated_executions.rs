use crate::core::rect::{WorkArea, WindowRect};
use crate::core::action::{WindowAction, SubsequentExecutionMode};
use crate::core::calculator::RectCalculator;
use crate::services::config::ConfigService;
use std::collections::HashMap;

/// 重复执行模式计算器
/// 支持循环大小、位置、显示器
pub struct RepeatedExecutionsCalculator {
    config_service: Option<ConfigService>,
    action_cycles: HashMap<WindowAction, Vec<WindowAction>>,
}

impl RepeatedExecutionsCalculator {
    pub fn new(config_service: Option<&ConfigService>) -> Self {
        let mut calculator = Self {
            config_service: config_service.cloned(),
            action_cycles: HashMap::new(),
        };
        calculator.init_cycles();
        calculator
    }

    /// 初始化循环模式
    fn init_cycles(&mut self) {
        // 水平三分屏循环：1/3 -> 1/2 -> 2/3
        self.action_cycles.insert(
            WindowAction::FirstThird,
            vec![
                WindowAction::FirstThird,
                WindowAction::CenterHalf,
                WindowAction::FirstTwoThirds,
            ],
        );
        self.action_cycles.insert(
            WindowAction::CenterThird,
            vec![
                WindowAction::CenterThird,
                WindowAction::CenterHalf,
                WindowAction::CenterTwoThirds,
            ],
        );
        self.action_cycles.insert(
            WindowAction::LastThird,
            vec![
                WindowAction::LastThird,
                WindowAction::CenterHalf,
                WindowAction::LastTwoThirds,
            ],
        );

        // 垂直三分屏循环
        self.action_cycles.insert(
            WindowAction::TopVerticalThird,
            vec![
                WindowAction::TopVerticalThird,
                WindowAction::CenterHalf,
                WindowAction::TopVerticalTwoThirds,
            ],
        );
        self.action_cycles.insert(
            WindowAction::BottomVerticalThird,
            vec![
                WindowAction::BottomVerticalThird,
                WindowAction::CenterHalf,
                WindowAction::BottomVerticalTwoThirds,
            ],
        );

        // 四角循环
        self.action_cycles.insert(
            WindowAction::TopLeft,
            vec![
                WindowAction::TopLeft,
                WindowAction::TopLeftThird,
            ],
        );
        self.action_cycles.insert(
            WindowAction::TopRight,
            vec![
                WindowAction::TopRight,
                WindowAction::TopRightThird,
            ],
        );
        self.action_cycles.insert(
            WindowAction::BottomLeft,
            vec![
                WindowAction::BottomLeft,
                WindowAction::BottomLeftThird,
            ],
        );
        self.action_cycles.insert(
            WindowAction::BottomRight,
            vec![
                WindowAction::BottomRight,
                WindowAction::BottomRightThird,
            ],
        );

        // 半屏循环
        self.action_cycles.insert(
            WindowAction::LeftHalf,
            vec![
                WindowAction::LeftHalf,
                WindowAction::CenterHalf,
            ],
        );
        self.action_cycles.insert(
            WindowAction::RightHalf,
            vec![
                WindowAction::RightHalf,
                WindowAction::CenterHalf,
            ],
        );
        self.action_cycles.insert(
            WindowAction::TopHalf,
            vec![
                WindowAction::TopHalf,
                WindowAction::CenterHalf,
            ],
        );
        self.action_cycles.insert(
            WindowAction::BottomHalf,
            vec![
                WindowAction::BottomHalf,
                WindowAction::CenterHalf,
            ],
        );
    }

    /// 获取下一个动作
    pub fn get_next_action(&self, current_action: WindowAction, execution_count: i32) -> WindowAction {
        if let Some(cycle) = self.action_cycles.get(&current_action) {
            let index = (execution_count as usize) % cycle.len();
            cycle[index.min(cycle.len() - 1)]
        } else {
            current_action
        }
    }

    /// 获取执行模式
    pub fn get_execution_mode(&self) -> SubsequentExecutionMode {
        if let Some(ref service) = self.config_service {
            let config = service.load();
            match config.subsequent_execution_mode {
                0 => SubsequentExecutionMode::None,
                2 => SubsequentExecutionMode::CyclePosition,
                3 => SubsequentExecutionMode::CycleDisplay,
                _ => SubsequentExecutionMode::CycleSize,
            }
        } else {
            SubsequentExecutionMode::CycleSize
        }
    }

    /// 检查是否支持循环
    pub fn supports_cycle(&self, action: WindowAction) -> bool {
        self.action_cycles.contains_key(&action)
    }

    /// 获取动作的循环列表
    pub fn get_action_cycle(&self, action: WindowAction) -> Option<&Vec<WindowAction>> {
        self.action_cycles.get(&action)
    }
}

/// 循环计算器 trait 实现
pub struct CycleSizeCalculator {
    base_calculator: Box<dyn RectCalculator>,
    cycle_index: usize,
    size_multipliers: Vec<f64>,
}

impl CycleSizeCalculator {
    pub fn new(base_calculator: Box<dyn RectCalculator>) -> Self {
        Self {
            base_calculator,
            cycle_index: 0,
            size_multipliers: vec![0.33, 0.5, 0.67],
        }
    }

    pub fn next_size(&mut self) -> f64 {
        let size = self.size_multipliers[self.cycle_index];
        self.cycle_index = (self.cycle_index + 1) % self.size_multipliers.len();
        size
    }
}

/// 循环位置计算器
pub struct CyclePositionCalculator {
    positions: Vec<(i32, i32)>,
    current_index: usize,
}

impl CyclePositionCalculator {
    pub fn new(work_area: &WorkArea, window_size: (i32, i32)) -> Self {
        let (w, h) = window_size;
        let positions = vec![
            (work_area.left, work_area.top), // 左上
            (work_area.right - w, work_area.top), // 右上
            (work_area.left, work_area.bottom - h), // 左下
            (work_area.right - w, work_area.bottom - h), // 右下
            (
                work_area.left + (work_area.width() - w) / 2,
                work_area.top + (work_area.height() - h) / 2,
            ), // 中心
        ];

        Self {
            positions,
            current_index: 0,
        }
    }

    pub fn next_position(&mut self) -> (i32, i32) {
        let pos = self.positions[self.current_index];
        self.current_index = (self.current_index + 1) % self.positions.len();
        pos
    }
}

/// 循环显示器计算器
pub struct CycleDisplayCalculator {
    display_count: i32,
    current_display: i32,
}

impl CycleDisplayCalculator {
    pub fn new(display_count: i32) -> Self {
        Self {
            display_count: display_count.max(1),
            current_display: 0,
        }
    }

    pub fn next_display(&mut self) -> i32 {
        let display = self.current_display;
        self.current_display = (self.current_display + 1) % self.display_count;
        display
    }

    pub fn previous_display(&mut self) -> i32 {
        self.current_display = if self.current_display == 0 {
            self.display_count - 1
        } else {
            self.current_display - 1
        };
        self.current_display
    }
}
