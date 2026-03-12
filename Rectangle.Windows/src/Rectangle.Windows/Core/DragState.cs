using System;

namespace Rectangle.Windows.Core;

/// <summary>
/// 拖拽状态
/// 记录窗口拖拽过程中的状态信息
/// </summary>
public class DragState
{
    /// <summary>
    /// 是否正在拖拽
    /// </summary>
    public bool IsDragging { get; set; }
    
    /// <summary>
    /// 被拖拽的窗口句柄
    /// </summary>
    public nint DraggedWindow { get; set; }
    
    /// <summary>
    /// 拖拽开始时的鼠标位置
    /// </summary>
    public System.Drawing.Point InitialMousePos { get; set; }
    
    /// <summary>
    /// 拖拽开始时的窗口位置
    /// </summary>
    public WindowRect InitialWindowRect { get; set; }
    
    /// <summary>
    /// 拖拽开始时间
    /// </summary>
    public DateTime DragStartTime { get; set; }
    
    /// <summary>
    /// 当前鼠标位置
    /// </summary>
    public System.Drawing.Point CurrentMousePos { get; set; }
    
    /// <summary>
    /// 当前吸附区域（如果有）
    /// </summary>
    public SnapArea? CurrentSnapArea { get; set; }
    
    /// <summary>
    /// 是否已触发吸附
    /// </summary>
    public bool HasSnapped { get; set; }
    
    /// <summary>
    /// 原始窗口位置（用于 Unsnap 恢复）
    /// </summary>
    public WindowRect? OriginalRect { get; set; }
    
    /// <summary>
    /// 拖拽的按钮（左键/右键）
    /// </summary>
    public MouseButton DragButton { get; set; }

    /// <summary>
    /// 重置状态
    /// </summary>
    public void Reset()
    {
        IsDragging = false;
        DraggedWindow = 0;
        InitialMousePos = System.Drawing.Point.Empty;
        InitialWindowRect = new WindowRect();
        DragStartTime = DateTime.MinValue;
        CurrentMousePos = System.Drawing.Point.Empty;
        CurrentSnapArea = null;
        HasSnapped = false;
        OriginalRect = null;
        DragButton = MouseButton.None;
    }

    /// <summary>
    /// 计算拖拽偏移量
    /// </summary>
    public System.Drawing.Point GetDragOffset()
    {
        return new System.Drawing.Point(
            CurrentMousePos.X - InitialMousePos.X,
            CurrentMousePos.Y - InitialMousePos.Y);
    }

    /// <summary>
    /// 计算拖拽持续时间（毫秒）
    /// </summary>
    public int GetDragDurationMs()
    {
        return (int)(DateTime.Now - DragStartTime).TotalMilliseconds;
    }

    /// <summary>
    /// 计算拖拽距离（像素）
    /// </summary>
    public double GetDragDistance()
    {
        var offset = GetDragOffset();
        return Math.Sqrt(offset.X * offset.X + offset.Y * offset.Y);
    }

    /// <summary>
    /// 根据初始位置和偏移量计算新位置
    /// </summary>
    public WindowRect CalculateNewRect()
    {
        var offset = GetDragOffset();
        return new WindowRect(
            InitialWindowRect.X + offset.X,
            InitialWindowRect.Y + offset.Y,
            InitialWindowRect.Width,
            InitialWindowRect.Height);
    }
}

/// <summary>
/// 鼠标按钮
/// </summary>
public enum MouseButton
{
    None,
    Left,
    Right,
    Middle
}

/// <summary>
/// 吸附区域
/// </summary>
public class SnapArea
{
    /// <summary>
    /// 区域边界
    /// </summary>
    public System.Drawing.Rectangle Bounds { get; set; }
    
    /// <summary>
    /// 对应的窗口操作
    /// </summary>
    public WindowAction Action { get; set; }
    
    /// <summary>
    /// 区域类型
    /// </summary>
    public SnapAreaType Type { get; set; }
    
    /// <summary>
    /// 区域名称（用于日志）
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 检查点是否在区域内
    /// </summary>
    public bool Contains(System.Drawing.Point point)
    {
        return Bounds.Contains(point);
    }

    /// <summary>
    /// 获取区域的中心点
    /// </summary>
    public System.Drawing.Point GetCenter()
    {
        return new System.Drawing.Point(
            Bounds.Left + Bounds.Width / 2,
            Bounds.Top + Bounds.Height / 2);
    }
}

/// <summary>
/// 吸附区域类型
/// </summary>
public enum SnapAreaType
{
    /// <summary>
    /// 屏幕边缘
    /// </summary>
    Edge,
    
    /// <summary>
    /// 屏幕角落
    /// </summary>
    Corner,
    
    /// <summary>
    /// 自定义区域
    /// </summary>
    Custom
}
