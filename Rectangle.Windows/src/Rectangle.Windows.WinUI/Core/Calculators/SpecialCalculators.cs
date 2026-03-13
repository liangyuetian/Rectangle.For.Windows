using Rectangle.Windows.WinUI.Services;

namespace Rectangle.Windows.WinUI.Core.Calculators;

/// <summary>
/// 重复执行计算器 - 处理连续按同一快捷键的行为
/// </summary>
public class RepeatedExecutionsCalculator : IRectCalculator
{
    private readonly ConfigService _configService;
    private readonly WindowHistory _history;
    private readonly CalculatorFactory _factory;
    private readonly ScreenDetectionService _screenService;

    public RepeatedExecutionsCalculator(ConfigService configService, WindowHistory history)
    {
        _configService = configService;
        _history = history;
        _factory = new CalculatorFactory();
        _screenService = new ScreenDetectionService(new Win32WindowService());
    }

    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var config = _configService.Load();
        var mode = config.SubsequentExecutionMode;

        // 获取窗口历史位置
        var lastRect = _history.GetLastRect(); // 需要实现

        return mode switch
        {
            SubsequentExecutionMode.CycleSize => CalculateCycleSize(workArea, currentWindow, action, gap),
            SubsequentExecutionMode.CyclePosition => CalculateCyclePosition(workArea, currentWindow, action, gap),
            SubsequentExecutionMode.CycleDisplay => CalculateCycleDisplay(currentWindow),
            _ => currentWindow
        };
    }

    /// <summary>
    /// 循环切换大小
    /// </summary>
    private WindowRect CalculateCycleSize(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap)
    {
        // 实现循环切换大小的逻辑
        // 例如：1/2 -> 2/3 -> 3/4 -> 1/2
        return currentWindow;
    }

    /// <summary>
    /// 循环切换位置
    /// </summary>
    private WindowRect CalculateCyclePosition(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap)
    {
        // 实现循环切换位置的逻辑
        // 例如：左半屏 -> 右半屏 -> 左半屏
        return currentWindow;
    }

    /// <summary>
    /// 循环切换显示器
    /// </summary>
    private WindowRect CalculateCycleDisplay(WindowRect currentWindow)
    {
        var displays = _screenService.GetAllWorkAreas();
        // 找到当前显示器并切换到下一个
        return currentWindow;
    }
}

/// <summary>
/// 指定尺寸计算器
/// </summary>
public class SpecifiedCalculator : IRectCalculator
{
    private readonly ConfigService _configService;

    public SpecifiedCalculator(ConfigService configService)
    {
        _configService = configService;
    }

    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var config = _configService.Load();

        // 居中显示指定尺寸的窗口
        int x = workArea.Left + (workArea.Width - config.SpecifiedWidth) / 2;
        int y = workArea.Top + (workArea.Height - config.SpecifiedHeight) / 2;

        return new WindowRect(x, y, config.SpecifiedWidth, config.SpecifiedHeight);
    }
}

/// <summary>
/// Todo 侧边栏计算器
/// </summary>
public class LeftTodoCalculator : IRectCalculator
{
    private readonly ConfigService _configService;

    public LeftTodoCalculator(ConfigService configService)
    {
        _configService = configService;
    }

    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var config = _configService.Load();
        var width = workArea.Width - config.TodoSidebarWidth - gap;

        return new WindowRect(
            workArea.Left + config.TodoSidebarWidth + gap,
            workArea.Top,
            width,
            workArea.Height);
    }
}

public class RightTodoCalculator : IRectCalculator
{
    private readonly ConfigService _configService;

    public RightTodoCalculator(ConfigService configService)
    {
        _configService = configService;
    }

    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var config = _configService.Load();
        var width = workArea.Width - config.TodoSidebarWidth - gap;

        return new WindowRect(
            workArea.Left,
            workArea.Top,
            width,
            workArea.Height);
    }
}

/// <summary>
/// 四等分 - 3/4 大小
/// </summary>
public class FirstThreeFourthsCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var width = (workArea.Width - gap * 3) / 4 * 3 + gap * 2;
        return new WindowRect(workArea.Left, workArea.Top, width, workArea.Height);
    }
}

public class CenterThreeFourthsCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var width = (workArea.Width - gap * 3) / 4 * 3 + gap * 2;
        var x = workArea.Left + (workArea.Width - width) / 2;
        return new WindowRect(x, workArea.Top, width, workArea.Height);
    }
}

public class LastThreeFourthsCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var width = (workArea.Width - gap * 3) / 4 * 3 + gap * 2;
        var x = workArea.Left + workArea.Width - width;
        return new WindowRect(x, workArea.Top, width, workArea.Height);
    }
}
