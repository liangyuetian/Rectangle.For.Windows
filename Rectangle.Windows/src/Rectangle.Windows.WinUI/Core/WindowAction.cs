namespace Rectangle.Windows.WinUI.Core;

public enum WindowAction
{
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
    // 移动到边缘
    MoveLeft,
    MoveRight,
    MoveUp,
    MoveDown,
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

    // 撤销/重做
    Undo,
    Redo
}
