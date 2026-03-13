using Rectangle.Windows.Core;

namespace Rectangle.Windows.Core.Calculators;

/// <summary>
/// 左侧 Todo 模式计算器
/// 为其他窗口预留左侧 Todo 侧边栏空间
/// </summary>
public class LeftTodoCalculator : IRectCalculator
{
    private readonly int _todoSidebarWidth;

    public LeftTodoCalculator(int todoSidebarWidth)
    {
        _todoSidebarWidth = todoSidebarWidth;
    }

    public WindowRect Calculate(WorkArea workArea, WindowRect currentRect, WindowAction action)
    {
        // 计算左侧 Todo 区域后的可用空间
        var availableX = workArea.Left + _todoSidebarWidth;
        var availableWidth = workArea.Width - _todoSidebarWidth;

        // 返回可用区域（全高）
        return new WindowRect(
            availableX,
            workArea.Top,
            availableWidth,
            workArea.Height
        );
    }
}
