namespace Rectangle.Windows.WinUI.Core;

/// <summary>
/// 重复执行模式
/// </summary>
public enum SubsequentExecutionMode
{
    None = 0,
    CycleSize = 1,
    CyclePosition = 2,
    CycleDisplay = 3
}

/// <summary>
/// Todo 侧边栏位置
/// </summary>
public enum TodoSidebarSide
{
    Left = 0,
    Right = 1
}

/// <summary>
/// 窗口动作类型
/// </summary>
public enum WindowActionType
{
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
    Special
}

/// <summary>
/// 显示器信息
/// </summary>
public class DisplayInfo
{
    public int Index { get; set; }
    public string Name { get; set; } = "";
    public int Width { get; set; }
    public int Height { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public bool IsPrimary { get; set; }
    public int Dpi { get; set; }
}
