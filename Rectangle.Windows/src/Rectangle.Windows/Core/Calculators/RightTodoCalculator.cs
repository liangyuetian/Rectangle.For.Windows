using Rectangle.Windows.Core;

namespace Rectangle.Windows.Core.Calculators;

/// <summary>
/// 右侧 Todo 模式计算器
/// 为其他窗口预留右侧 Todo 侧边栏空间
/// </summary>
public class RightTodoCalculator : IRectCalculator
{
    private readonly int _todoSidebarWidth;

    public RightTodoCalculator(int todoSidebarWidth)
    {
        _todoSidebarWidth = todoSidebarWidth;
    }

    public WindowRect Calculate(WorkArea workArea, WindowRect currentRect, WindowAction action)
    {
        // 计算右侧 Todo 区域后的可用空间
        var availableWidth = workArea.Width - _todoSidebarWidth;

        // 返回可用区域（全高）
        return new WindowRect(
            workArea.Left,
            workArea.Top,
            availableWidth,
            workArea.Height
        );
    }
}
