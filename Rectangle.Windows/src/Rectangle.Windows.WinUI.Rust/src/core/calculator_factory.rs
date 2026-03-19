use crate::core::action::WindowAction;
use crate::core::calculator::{BoxedCalculator, RectCalculator};
use crate::services::config::ConfigService;
use std::collections::HashMap;

/// 显示器信息
#[derive(Debug, Clone)]
pub struct DisplayInfo {
    pub index: i32,
    pub name: String,
    pub width: i32,
    pub height: i32,
    pub x: i32,
    pub y: i32,
    pub is_primary: bool,
    pub dpi: i32,
}

/// 计算器工厂
pub struct CalculatorFactory {
    calculators: HashMap<WindowAction, BoxedCalculator>,
}

impl CalculatorFactory {
    pub fn new(config_service: Option<&ConfigService>) -> Self {
        let mut calculators: HashMap<WindowAction, BoxedCalculator> = HashMap::new();

        // 半屏计算器
        calculators.insert(WindowAction::LeftHalf, Box::new(crate::calculators::half::LeftHalfCalculator::new(config_service)));
        calculators.insert(WindowAction::RightHalf, Box::new(crate::calculators::half::RightHalfCalculator::new(config_service)));
        calculators.insert(WindowAction::TopHalf, Box::new(crate::calculators::half::TopHalfCalculator::new(config_service)));
        calculators.insert(WindowAction::BottomHalf, Box::new(crate::calculators::half::BottomHalfCalculator::new(config_service)));
        calculators.insert(WindowAction::CenterHalf, Box::new(crate::calculators::half::CenterHalfCalculator::new()));

        // 四角计算器
        calculators.insert(WindowAction::TopLeft, Box::new(crate::calculators::corner::TopLeftCalculator::new()));
        calculators.insert(WindowAction::TopRight, Box::new(crate::calculators::corner::TopRightCalculator::new()));
        calculators.insert(WindowAction::BottomLeft, Box::new(crate::calculators::corner::BottomLeftCalculator::new()));
        calculators.insert(WindowAction::BottomRight, Box::new(crate::calculators::corner::BottomRightCalculator::new()));

        // 三分屏计算器
        calculators.insert(WindowAction::FirstThird, Box::new(crate::calculators::third::FirstThirdCalculator::new()));
        calculators.insert(WindowAction::CenterThird, Box::new(crate::calculators::third::CenterThirdCalculator::new()));
        calculators.insert(WindowAction::LastThird, Box::new(crate::calculators::third::LastThirdCalculator::new()));
        calculators.insert(WindowAction::FirstTwoThirds, Box::new(crate::calculators::third::FirstTwoThirdsCalculator::new()));
        calculators.insert(WindowAction::CenterTwoThirds, Box::new(crate::calculators::third::CenterTwoThirdsCalculator::new()));
        calculators.insert(WindowAction::LastTwoThirds, Box::new(crate::calculators::third::LastTwoThirdsCalculator::new()));

        // 四等分计算器
        calculators.insert(WindowAction::FirstFourth, Box::new(crate::calculators::fourth::FirstFourthCalculator::new()));
        calculators.insert(WindowAction::SecondFourth, Box::new(crate::calculators::fourth::SecondFourthCalculator::new()));
        calculators.insert(WindowAction::ThirdFourth, Box::new(crate::calculators::fourth::ThirdFourthCalculator::new()));
        calculators.insert(WindowAction::LastFourth, Box::new(crate::calculators::fourth::LastFourthCalculator::new()));
        calculators.insert(WindowAction::FirstThreeFourths, Box::new(crate::calculators::fourth::FirstThreeFourthsCalculator::new()));
        calculators.insert(WindowAction::CenterThreeFourths, Box::new(crate::calculators::fourth::CenterThreeFourthsCalculator::new()));
        calculators.insert(WindowAction::LastThreeFourths, Box::new(crate::calculators::fourth::LastThreeFourthsCalculator::new()));

        // 六等分计算器
        calculators.insert(WindowAction::TopLeftSixth, Box::new(crate::calculators::sixth::TopLeftSixthCalculator::new()));
        calculators.insert(WindowAction::TopCenterSixth, Box::new(crate::calculators::sixth::TopCenterSixthCalculator::new()));
        calculators.insert(WindowAction::TopRightSixth, Box::new(crate::calculators::sixth::TopRightSixthCalculator::new()));
        calculators.insert(WindowAction::BottomLeftSixth, Box::new(crate::calculators::sixth::BottomLeftSixthCalculator::new()));
        calculators.insert(WindowAction::BottomCenterSixth, Box::new(crate::calculators::sixth::BottomCenterSixthCalculator::new()));
        calculators.insert(WindowAction::BottomRightSixth, Box::new(crate::calculators::sixth::BottomRightSixthCalculator::new()));

        // 最大化/居中计算器
        calculators.insert(WindowAction::Maximize, Box::new(crate::calculators::misc::MaximizeCalculator::new()));
        calculators.insert(WindowAction::AlmostMaximize, Box::new(crate::calculators::misc::AlmostMaximizeCalculator::new(config_service)));
        calculators.insert(WindowAction::MaximizeHeight, Box::new(crate::calculators::misc::MaximizeHeightCalculator::new()));
        calculators.insert(WindowAction::Center, Box::new(crate::calculators::misc::CenterCalculator::new()));
        calculators.insert(WindowAction::Larger, Box::new(crate::calculators::misc::LargerCalculator::new(config_service)));
        calculators.insert(WindowAction::Smaller, Box::new(crate::calculators::misc::SmallerCalculator::new(config_service)));
        calculators.insert(WindowAction::CenterProminently, Box::new(crate::calculators::misc::CenterProminentlyCalculator::new(config_service)));

        // 移动计算器
        calculators.insert(WindowAction::MoveLeft, Box::new(crate::calculators::r#move::MoveLeftCalculator::new()));
        calculators.insert(WindowAction::MoveRight, Box::new(crate::calculators::r#move::MoveRightCalculator::new()));
        calculators.insert(WindowAction::MoveUp, Box::new(crate::calculators::r#move::MoveUpCalculator::new()));
        calculators.insert(WindowAction::MoveDown, Box::new(crate::calculators::r#move::MoveDownCalculator::new()));

        // 调整尺寸计算器
        calculators.insert(WindowAction::LargerWidth, Box::new(crate::calculators::misc::LargerWidthCalculator::new(config_service)));
        calculators.insert(WindowAction::SmallerWidth, Box::new(crate::calculators::misc::SmallerWidthCalculator::new(config_service)));
        calculators.insert(WindowAction::LargerHeight, Box::new(crate::calculators::misc::LargerHeightCalculator::new(config_service)));
        calculators.insert(WindowAction::SmallerHeight, Box::new(crate::calculators::misc::SmallerHeightCalculator::new(config_service)));

        // 双倍/减半尺寸计算器
        calculators.insert(WindowAction::DoubleHeightUp, Box::new(crate::calculators::misc::DoubleHeightUpCalculator::new()));
        calculators.insert(WindowAction::DoubleHeightDown, Box::new(crate::calculators::misc::DoubleHeightDownCalculator::new()));
        calculators.insert(WindowAction::DoubleWidthLeft, Box::new(crate::calculators::misc::DoubleWidthLeftCalculator::new()));
        calculators.insert(WindowAction::DoubleWidthRight, Box::new(crate::calculators::misc::DoubleWidthRightCalculator::new()));
        calculators.insert(WindowAction::HalveHeightUp, Box::new(crate::calculators::misc::HalveHeightUpCalculator::new()));
        calculators.insert(WindowAction::HalveHeightDown, Box::new(crate::calculators::misc::HalveHeightDownCalculator::new()));
        calculators.insert(WindowAction::HalveWidthLeft, Box::new(crate::calculators::misc::HalveWidthLeftCalculator::new()));
        calculators.insert(WindowAction::HalveWidthRight, Box::new(crate::calculators::misc::HalveWidthRightCalculator::new()));

        // 指定尺寸计算器
        calculators.insert(WindowAction::Specified, Box::new(crate::calculators::misc::SpecifiedCalculator::new(config_service)));

        // 九等分计算器
        calculators.insert(WindowAction::TopLeftNinth, Box::new(crate::calculators::ninth::TopLeftNinthCalculator::new()));
        calculators.insert(WindowAction::TopCenterNinth, Box::new(crate::calculators::ninth::TopCenterNinthCalculator::new()));
        calculators.insert(WindowAction::TopRightNinth, Box::new(crate::calculators::ninth::TopRightNinthCalculator::new()));
        calculators.insert(WindowAction::MiddleLeftNinth, Box::new(crate::calculators::ninth::MiddleLeftNinthCalculator::new()));
        calculators.insert(WindowAction::MiddleCenterNinth, Box::new(crate::calculators::ninth::MiddleCenterNinthCalculator::new()));
        calculators.insert(WindowAction::MiddleRightNinth, Box::new(crate::calculators::ninth::MiddleRightNinthCalculator::new()));
        calculators.insert(WindowAction::BottomLeftNinth, Box::new(crate::calculators::ninth::BottomLeftNinthCalculator::new()));
        calculators.insert(WindowAction::BottomCenterNinth, Box::new(crate::calculators::ninth::BottomCenterNinthCalculator::new()));
        calculators.insert(WindowAction::BottomRightNinth, Box::new(crate::calculators::ninth::BottomRightNinthCalculator::new()));

        // 八等分计算器
        calculators.insert(WindowAction::TopLeftEighth, Box::new(crate::calculators::eighth::TopLeftEighthCalculator::new()));
        calculators.insert(WindowAction::TopCenterLeftEighth, Box::new(crate::calculators::eighth::TopCenterLeftEighthCalculator::new()));
        calculators.insert(WindowAction::TopCenterRightEighth, Box::new(crate::calculators::eighth::TopCenterRightEighthCalculator::new()));
        calculators.insert(WindowAction::TopRightEighth, Box::new(crate::calculators::eighth::TopRightEighthCalculator::new()));
        calculators.insert(WindowAction::BottomLeftEighth, Box::new(crate::calculators::eighth::BottomLeftEighthCalculator::new()));
        calculators.insert(WindowAction::BottomCenterLeftEighth, Box::new(crate::calculators::eighth::BottomCenterLeftEighthCalculator::new()));
        calculators.insert(WindowAction::BottomCenterRightEighth, Box::new(crate::calculators::eighth::BottomCenterRightEighthCalculator::new()));
        calculators.insert(WindowAction::BottomRightEighth, Box::new(crate::calculators::eighth::BottomRightEighthCalculator::new()));

        // 角落三分之一计算器
        calculators.insert(WindowAction::TopLeftThird, Box::new(crate::calculators::extended::TopLeftThirdCalculator::new()));
        calculators.insert(WindowAction::TopRightThird, Box::new(crate::calculators::extended::TopRightThirdCalculator::new()));
        calculators.insert(WindowAction::BottomLeftThird, Box::new(crate::calculators::extended::BottomLeftThirdCalculator::new()));
        calculators.insert(WindowAction::BottomRightThird, Box::new(crate::calculators::extended::BottomRightThirdCalculator::new()));

        // 垂直三分之一计算器
        calculators.insert(WindowAction::TopVerticalThird, Box::new(crate::calculators::extended::TopVerticalThirdCalculator::new()));
        calculators.insert(WindowAction::MiddleVerticalThird, Box::new(crate::calculators::extended::MiddleVerticalThirdCalculator::new()));
        calculators.insert(WindowAction::BottomVerticalThird, Box::new(crate::calculators::extended::BottomVerticalThirdCalculator::new()));
        calculators.insert(WindowAction::TopVerticalTwoThirds, Box::new(crate::calculators::extended::TopVerticalTwoThirdsCalculator::new()));
        calculators.insert(WindowAction::BottomVerticalTwoThirds, Box::new(crate::calculators::extended::BottomVerticalTwoThirdsCalculator::new()));

        // 显示器切换计算器
        calculators.insert(WindowAction::NextDisplay, Box::new(crate::calculators::display::NextDisplayCalculator::new()));
        calculators.insert(WindowAction::PreviousDisplay, Box::new(crate::calculators::display::PreviousDisplayCalculator::new()));

        // Todo 模式计算器
        calculators.insert(WindowAction::LeftTodo, Box::new(crate::calculators::todo::LeftTodoCalculator::new(config_service)));
        calculators.insert(WindowAction::RightTodo, Box::new(crate::calculators::todo::RightTodoCalculator::new(config_service)));

        Self { calculators }
    }

    /// 获取计算器
    pub fn get_calculator(&self, action: WindowAction) -> Option<&BoxedCalculator> {
        self.calculators.get(&action)
    }
}
