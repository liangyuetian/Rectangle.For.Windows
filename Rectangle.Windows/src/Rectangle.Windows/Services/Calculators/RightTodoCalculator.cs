using Rectangle.Windows.Core;

namespace Rectangle.Windows.Services.Calculators;

/// <summary>
/// 右侧 Todo 侧边栏计算器
/// 将窗口放置在屏幕右侧，为 Todo 应用预留左侧空间
/// </summary>
public class RightTodoCalculator : IRectCalculator
{
    private readonly ConfigService _configService;

    public RightTodoCalculator(ConfigService configService)
    {
        _configService = configService;
    }

    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action)
    {
        var config = _configService.Load();
        var todoWidth = config.TodoSidebarWidth;
        var gap = config.GapSize;

        // 窗口占据右侧，扣除 Todo 侧边栏宽度
        var availableWidth = workArea.Width - todoWidth - gap;
        var windowWidth = availableWidth - gap * 2;
        var windowX = workArea.Left + todoWidth + gap;

        return new WindowRect(
            windowX,
            workArea.Top + gap,
            windowWidth,
            workArea.Height - gap * 2
        );
    }
}