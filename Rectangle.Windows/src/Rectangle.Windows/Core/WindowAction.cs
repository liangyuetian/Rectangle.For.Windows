namespace Rectangle.Windows.Core;

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
    // 四等分（子菜单）
    FirstFourth,
    SecondFourth,
    ThirdFourth,
    LastFourth,
    FirstThreeFourths,
    CenterThreeFourths,
    LastThreeFourths,
    // 六等分（子菜单）
    TopLeftSixth,
    TopCenterSixth,
    TopRightSixth,
    BottomLeftSixth,
    BottomCenterSixth,
    BottomRightSixth,
    // 九等分（子菜单）
    TopLeftNinth,
    TopCenterNinth,
    TopRightNinth,
    MiddleLeftNinth,
    MiddleCenterNinth,
    MiddleRightNinth,
    BottomLeftNinth,
    BottomCenterNinth,
    BottomRightNinth,
    // 八等分（子菜单）
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
    // 移动到边缘（子菜单）
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
    // 多窗口管理
    TileAll,
    CascadeAll,
    ReverseAll,
    TileActiveApp,
    CascadeActiveApp,
    // Todo 模式
    LeftTodo,
    RightTodo,
    // 撤销重做
    Undo,
    Redo
}