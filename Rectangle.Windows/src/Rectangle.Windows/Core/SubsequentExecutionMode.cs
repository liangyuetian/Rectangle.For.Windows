namespace Rectangle.Windows.Core;

/// <summary>
/// 重复执行模式：定义连续按同一快捷键时的行为
/// </summary>
public enum SubsequentExecutionMode
{
    /// <summary>
    /// 无特殊操作：重复执行相同的操作
    /// </summary>
    None = 0,
    
    /// <summary>
    /// 循环尺寸：在不同尺寸之间循环
    /// 例如：1/3 → 2/3 → 1/3
    /// </summary>
    CycleSize = 1,
    
    /// <summary>
    /// 跨显示器：在不同显示器之间循环
    /// </summary>
    AcrossMonitor = 2,
    
    /// <summary>
    /// 调整大小：每次执行时增加/减少窗口尺寸
    /// </summary>
    Resize = 3
}
