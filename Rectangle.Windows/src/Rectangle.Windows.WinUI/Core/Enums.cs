namespace Rectangle.Windows.WinUI.Core;

/// <summary>
/// 重复执行模式
/// </summary>
public enum SubsequentExecutionMode
{
    /// <summary>
    /// 无操作
    /// </summary>
    None = 0,

    /// <summary>
    /// 循环切换大小
    /// </summary>
    CycleSize = 1,

    /// <summary>
    /// 循环切换位置
    /// </summary>
    CyclePosition = 2,

    /// <summary>
    /// 在显示器之间循环
    /// </summary>
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
/// 拖拽状态
/// </summary>
public enum DragState
{
    None = 0,
    Started = 1,
    InProgress = 2,
    Ended = 3
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
/// 工作区域信息
/// </summary>
public struct WorkArea
{
    public int Left { get; set; }
    public int Top { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public int Right => Left + Width;
    public int Bottom => Top + Height;

    public WorkArea(int left, int top, int width, int height)
    {
        Left = left;
        Top = top;
        Width = width;
        Height = height;
    }
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
