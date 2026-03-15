using Rectangle.Windows.WinUI.Services;

namespace Rectangle.Windows.WinUI.Core.Calculators;

// ============================================
// 双倍/减半尺寸计算器 (8个)
// ============================================

public class DoubleHeightUpCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var newHeight = Math.Min(currentWindow.Height * 2, workArea.Height - gap * 2);
        var y = currentWindow.Bottom - newHeight;
        y = Math.Max(workArea.Top + gap, y);
        return new WindowRect(currentWindow.X, y, currentWindow.Width, newHeight);
    }
}

public class DoubleHeightDownCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var newHeight = Math.Min(currentWindow.Height * 2, workArea.Height - gap * 2);
        var y = currentWindow.Top;
        if (y + newHeight > workArea.Bottom - gap)
            y = workArea.Bottom - gap - newHeight;
        return new WindowRect(currentWindow.X, y, currentWindow.Width, newHeight);
    }
}

public class DoubleWidthLeftCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var newWidth = Math.Min(currentWindow.Width * 2, workArea.Width - gap * 2);
        var x = currentWindow.Right - newWidth;
        x = Math.Max(workArea.Left + gap, x);
        return new WindowRect(x, currentWindow.Y, newWidth, currentWindow.Height);
    }
}

public class DoubleWidthRightCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var newWidth = Math.Min(currentWindow.Width * 2, workArea.Width - gap * 2);
        var x = currentWindow.Left;
        if (x + newWidth > workArea.Right - gap)
            x = workArea.Right - gap - newWidth;
        return new WindowRect(x, currentWindow.Y, newWidth, currentWindow.Height);
    }
}

public class HalveHeightUpCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var newHeight = Math.Max(currentWindow.Height / 2, 100);
        var y = currentWindow.Bottom - newHeight;
        return new WindowRect(currentWindow.X, y, currentWindow.Width, newHeight);
    }
}

public class HalveHeightDownCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var newHeight = Math.Max(currentWindow.Height / 2, 100);
        return new WindowRect(currentWindow.X, currentWindow.Y, currentWindow.Width, newHeight);
    }
}

public class HalveWidthLeftCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var newWidth = Math.Max(currentWindow.Width / 2, 200);
        var x = currentWindow.Right - newWidth;
        return new WindowRect(x, currentWindow.Y, newWidth, currentWindow.Height);
    }
}

public class HalveWidthRightCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var newWidth = Math.Max(currentWindow.Width / 2, 200);
        return new WindowRect(currentWindow.X, currentWindow.Y, newWidth, currentWindow.Height);
    }
}

// ============================================
// 单独调整宽度/高度计算器 (4个)
// ============================================

public class LargerWidthCalculator : IRectCalculator
{
    private readonly ConfigService? _configService;

    public LargerWidthCalculator(ConfigService? configService = null)
    {
        _configService = configService;
    }

    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var offset = (int)(_configService?.Load().SizeOffset ?? 30);
        var newWidth = Math.Min(currentWindow.Width + offset, workArea.Width - gap * 2);
        var x = currentWindow.Left - (newWidth - currentWindow.Width) / 2;
        x = Math.Max(workArea.Left + gap, Math.Min(x, workArea.Right - newWidth - gap));
        return new WindowRect(x, currentWindow.Top, newWidth, currentWindow.Height);
    }
}

public class SmallerWidthCalculator : IRectCalculator
{
    private readonly ConfigService? _configService;

    public SmallerWidthCalculator(ConfigService? configService = null)
    {
        _configService = configService;
    }

    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var offset = (int)(_configService?.Load().SizeOffset ?? 30);
        var newWidth = Math.Max(currentWindow.Width - offset, 200);
        var x = currentWindow.Left + (currentWindow.Width - newWidth) / 2;
        return new WindowRect(x, currentWindow.Top, newWidth, currentWindow.Height);
    }
}

public class LargerHeightCalculator : IRectCalculator
{
    private readonly ConfigService? _configService;

    public LargerHeightCalculator(ConfigService? configService = null)
    {
        _configService = configService;
    }

    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var offset = (int)(_configService?.Load().SizeOffset ?? 30);
        var newHeight = Math.Min(currentWindow.Height + offset, workArea.Height - gap * 2);
        var y = currentWindow.Top - (newHeight - currentWindow.Height) / 2;
        y = Math.Max(workArea.Top + gap, Math.Min(y, workArea.Bottom - newHeight - gap));
        return new WindowRect(currentWindow.Left, y, currentWindow.Width, newHeight);
    }
}

public class SmallerHeightCalculator : IRectCalculator
{
    private readonly ConfigService? _configService;

    public SmallerHeightCalculator(ConfigService? configService = null)
    {
        _configService = configService;
    }

    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var offset = (int)(_configService?.Load().SizeOffset ?? 30);
        var newHeight = Math.Max(currentWindow.Height - offset, 100);
        var y = currentWindow.Top + (currentWindow.Height - newHeight) / 2;
        return new WindowRect(currentWindow.Left, y, currentWindow.Width, newHeight);
    }
}

// ============================================
// 指定尺寸计算器
// ============================================

public class SpecifiedCalculator : IRectCalculator
{
    private readonly ConfigService? _configService;

    public SpecifiedCalculator(ConfigService? configService = null)
    {
        _configService = configService;
    }

    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var config = _configService?.Load();
        var width = config?.SpecifiedWidth ?? 1680;
        var height = config?.SpecifiedHeight ?? 1050;

        if (width > workArea.Width - gap * 2) width = workArea.Width - gap * 2;
        if (height > workArea.Height - gap * 2) height = workArea.Height - gap * 2;

        var x = workArea.Left + (workArea.Width - width) / 2;
        var y = workArea.Top + (workArea.Height - height) / 2;

        return new WindowRect(x, y, width, height);
    }
}

// ============================================
// Todo 模式计算器 (2个)
// ============================================

public class LeftTodoCalculator : IRectCalculator
{
    private readonly ConfigService? _configService;

    public LeftTodoCalculator(ConfigService? configService = null)
    {
        _configService = configService;
    }

    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var config = _configService?.Load();
        var todoWidth = config?.TodoSidebarWidth ?? 400;

        var availableWidth = workArea.Width - todoWidth - gap * 3;
        return new WindowRect(
            workArea.Left + gap,
            workArea.Top + gap,
            availableWidth,
            workArea.Height - gap * 2
        );
    }
}

public class RightTodoCalculator : IRectCalculator
{
    private readonly ConfigService? _configService;

    public RightTodoCalculator(ConfigService? configService = null)
    {
        _configService = configService;
    }

    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var config = _configService?.Load();
        var todoWidth = config?.TodoSidebarWidth ?? 400;

        var availableWidth = workArea.Width - todoWidth - gap * 3;
        var x = workArea.Left + todoWidth + gap * 2;
        return new WindowRect(
            x,
            workArea.Top + gap,
            availableWidth,
            workArea.Height - gap * 2
        );
    }
}
