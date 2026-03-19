use serde::{Deserialize, Serialize};

/// 窗口动作类型
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, Serialize, Deserialize)]
pub enum WindowAction {
    // 半屏
    LeftHalf,
    RightHalf,
    CenterHalf,
    TopHalf,
    BottomHalf,

    // 四角
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight,

    // 三分之一
    FirstThird,
    CenterThird,
    LastThird,
    FirstTwoThirds,
    CenterTwoThirds,
    LastTwoThirds,

    // 四等分
    FirstFourth,
    SecondFourth,
    ThirdFourth,
    LastFourth,
    FirstThreeFourths,
    CenterThreeFourths,
    LastThreeFourths,

    // 六等分
    TopLeftSixth,
    TopCenterSixth,
    TopRightSixth,
    BottomLeftSixth,
    BottomCenterSixth,
    BottomRightSixth,

    // 九等分
    TopLeftNinth,
    TopCenterNinth,
    TopRightNinth,
    MiddleLeftNinth,
    MiddleCenterNinth,
    MiddleRightNinth,
    BottomLeftNinth,
    BottomCenterNinth,
    BottomRightNinth,

    // 八等分
    TopLeftEighth,
    TopCenterLeftEighth,
    TopCenterRightEighth,
    TopRightEighth,
    BottomLeftEighth,
    BottomCenterLeftEighth,
    BottomCenterRightEighth,
    BottomRightEighth,

    // 角落三分之一
    TopLeftThird,
    TopRightThird,
    BottomLeftThird,
    BottomRightThird,

    // 垂直三分之一
    TopVerticalThird,
    MiddleVerticalThird,
    BottomVerticalThird,
    TopVerticalTwoThirds,
    BottomVerticalTwoThirds,

    // 居中显著
    CenterProminently,

    // 移动到边缘
    MoveLeft,
    MoveRight,
    MoveUp,
    MoveDown,

    // 双倍/减半尺寸
    DoubleHeightUp,
    DoubleHeightDown,
    DoubleWidthLeft,
    DoubleWidthRight,
    HalveHeightUp,
    HalveHeightDown,
    HalveWidthLeft,
    HalveWidthRight,

    // 单独调整宽度/高度
    LargerWidth,
    SmallerWidth,
    LargerHeight,
    SmallerHeight,

    // 指定尺寸
    Specified,

    // 最大化与缩放
    Maximize,
    AlmostMaximize,
    MaximizeHeight,
    Larger,
    Smaller,
    Center,
    Restore,

    // 显示器
    NextDisplay,
    PreviousDisplay,

    // Todo 模式
    LeftTodo,
    RightTodo,

    // 撤销/重做
    Undo,
    Redo,
}

impl WindowAction {
    /// 获取动作的分类
    pub fn category(&self) -> WindowActionType {
        match self {
            WindowAction::LeftHalf | WindowAction::RightHalf | WindowAction::CenterHalf |
            WindowAction::TopHalf | WindowAction::BottomHalf => WindowActionType::HalfScreen,

            WindowAction::TopLeft | WindowAction::TopRight |
            WindowAction::BottomLeft | WindowAction::BottomRight => WindowActionType::Corner,

            WindowAction::FirstThird | WindowAction::CenterThird | WindowAction::LastThird |
            WindowAction::FirstTwoThirds | WindowAction::CenterTwoThirds | WindowAction::LastTwoThirds |
            WindowAction::TopLeftThird | WindowAction::TopRightThird |
            WindowAction::BottomLeftThird | WindowAction::BottomRightThird |
            WindowAction::TopVerticalThird | WindowAction::MiddleVerticalThird | WindowAction::BottomVerticalThird |
            WindowAction::TopVerticalTwoThirds | WindowAction::BottomVerticalTwoThirds => WindowActionType::Third,

            WindowAction::FirstFourth | WindowAction::SecondFourth | WindowAction::ThirdFourth | WindowAction::LastFourth |
            WindowAction::FirstThreeFourths | WindowAction::CenterThreeFourths | WindowAction::LastThreeFourths => WindowActionType::Fourth,

            WindowAction::TopLeftSixth | WindowAction::TopCenterSixth | WindowAction::TopRightSixth |
            WindowAction::BottomLeftSixth | WindowAction::BottomCenterSixth | WindowAction::BottomRightSixth => WindowActionType::Sixth,

            WindowAction::TopLeftNinth | WindowAction::TopCenterNinth | WindowAction::TopRightNinth |
            WindowAction::MiddleLeftNinth | WindowAction::MiddleCenterNinth | WindowAction::MiddleRightNinth |
            WindowAction::BottomLeftNinth | WindowAction::BottomCenterNinth | WindowAction::BottomRightNinth => WindowActionType::Ninth,

            WindowAction::TopLeftEighth | WindowAction::TopCenterLeftEighth | WindowAction::TopCenterRightEighth |
            WindowAction::TopRightEighth | WindowAction::BottomLeftEighth | WindowAction::BottomCenterLeftEighth |
            WindowAction::BottomCenterRightEighth | WindowAction::BottomRightEighth => WindowActionType::Eighth,

            WindowAction::Maximize | WindowAction::AlmostMaximize | WindowAction::MaximizeHeight |
            WindowAction::Larger | WindowAction::Smaller | WindowAction::Center | WindowAction::Restore => WindowActionType::Maximize,

            WindowAction::MoveLeft | WindowAction::MoveRight | WindowAction::MoveUp | WindowAction::MoveDown => WindowActionType::Move,

            WindowAction::NextDisplay | WindowAction::PreviousDisplay => WindowActionType::Display,

            WindowAction::LeftTodo | WindowAction::RightTodo => WindowActionType::Special,

            WindowAction::Undo | WindowAction::Redo => WindowActionType::Restore,

            WindowAction::DoubleHeightUp | WindowAction::DoubleHeightDown | WindowAction::DoubleWidthLeft |
            WindowAction::DoubleWidthRight | WindowAction::HalveHeightUp | WindowAction::HalveHeightDown |
            WindowAction::HalveWidthLeft | WindowAction::HalveWidthRight | WindowAction::LargerWidth |
            WindowAction::SmallerWidth | WindowAction::LargerHeight | WindowAction::SmallerHeight |
            WindowAction::Specified | WindowAction::CenterProminently => WindowActionType::Resize,
        }
    }
}

/// 窗口动作分类
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum WindowActionType {
    None,
    HalfScreen,
    Corner,
    Third,
    Fourth,
    Sixth,
    Eighth,
    Ninth,
    Maximize,
    Move,
    Resize,
    Display,
    Restore,
    Special,
}

/// 重复执行模式
#[derive(Debug, Clone, Copy, PartialEq, Eq, Serialize, Deserialize)]
pub enum SubsequentExecutionMode {
    None = 0,
    CycleSize = 1,
    CyclePosition = 2,
    CycleDisplay = 3,
}

/// Todo 侧边栏位置
#[derive(Debug, Clone, Copy, PartialEq, Eq, Serialize, Deserialize)]
pub enum TodoSidebarSide {
    Left = 0,
    Right = 1,
}
