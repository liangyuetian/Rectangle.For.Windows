namespace Rectangle.Windows.WinUI.Core.Calculators;

/// <summary>
/// 三分屏计算器
/// </summary>
public class FirstThirdCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var width = (workArea.Width - gap * 2) / 3;
        return new WindowRect(workArea.Left, workArea.Top, width, workArea.Height);
    }
}

public class CenterThirdCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var width = (workArea.Width - gap * 2) / 3;
        var x = workArea.Left + width + gap;
        return new WindowRect(x, workArea.Top, width, workArea.Height);
    }
}

public class LastThirdCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var width = (workArea.Width - gap * 2) / 3;
        var x = workArea.Left + width * 2 + gap * 2;
        return new WindowRect(x, workArea.Top, width, workArea.Height);
    }
}

public class FirstTwoThirdsCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var width = (workArea.Width - gap * 2) / 3 * 2 + gap;
        return new WindowRect(workArea.Left, workArea.Top, width, workArea.Height);
    }
}

public class CenterTwoThirdsCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var third = (workArea.Width - gap * 2) / 3;
        var width = third * 2;
        var x = workArea.Left + (workArea.Width - width) / 2;
        return new WindowRect(x, workArea.Top, width, workArea.Height);
    }
}

public class LastTwoThirdsCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var width = (workArea.Width - gap * 2) / 3 * 2 + gap;
        var x = workArea.Left + workArea.Width - width;
        return new WindowRect(x, workArea.Top, width, workArea.Height);
    }
}

/// <summary>
/// 四等分计算器
/// </summary>
public class FirstFourthCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var width = (workArea.Width - gap * 3) / 4;
        return new WindowRect(workArea.Left, workArea.Top, width, workArea.Height);
    }
}

public class SecondFourthCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var width = (workArea.Width - gap * 3) / 4;
        var x = workArea.Left + width + gap;
        return new WindowRect(x, workArea.Top, width, workArea.Height);
    }
}

public class ThirdFourthCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var width = (workArea.Width - gap * 3) / 4;
        var x = workArea.Left + width * 2 + gap * 2;
        return new WindowRect(x, workArea.Top, width, workArea.Height);
    }
}

public class LastFourthCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var width = (workArea.Width - gap * 3) / 4;
        var x = workArea.Left + width * 3 + gap * 3;
        return new WindowRect(x, workArea.Top, width, workArea.Height);
    }
}

/// <summary>
/// 六等分计算器
/// </summary>
public class TopLeftSixthCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var width = (workArea.Width - gap * 2) / 3;
        var height = (workArea.Height - gap) / 2;
        return new WindowRect(workArea.Left, workArea.Top, width, height);
    }
}

public class TopCenterSixthCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var width = (workArea.Width - gap * 2) / 3;
        var height = (workArea.Height - gap) / 2;
        var x = workArea.Left + width + gap;
        return new WindowRect(x, workArea.Top, width, height);
    }
}

public class TopRightSixthCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var width = (workArea.Width - gap * 2) / 3;
        var height = (workArea.Height - gap) / 2;
        var x = workArea.Left + width * 2 + gap * 2;
        return new WindowRect(x, workArea.Top, width, height);
    }
}

public class BottomLeftSixthCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var width = (workArea.Width - gap * 2) / 3;
        var height = (workArea.Height - gap) / 2;
        var y = workArea.Top + height + gap;
        return new WindowRect(workArea.Left, y, width, height);
    }
}

public class BottomCenterSixthCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var width = (workArea.Width - gap * 2) / 3;
        var height = (workArea.Height - gap) / 2;
        var x = workArea.Left + width + gap;
        var y = workArea.Top + height + gap;
        return new WindowRect(x, y, width, height);
    }
}

public class BottomRightSixthCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var width = (workArea.Width - gap * 2) / 3;
        var height = (workArea.Height - gap) / 2;
        var x = workArea.Left + width * 2 + gap * 2;
        var y = workArea.Top + height + gap;
        return new WindowRect(x, y, width, height);
    }
}

/// <summary>
/// 移动计算器
/// </summary>
public class MoveLeftCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var step = 30;
        var x = Math.Max(workArea.Left, currentWindow.X - step);
        return new WindowRect(x, currentWindow.Y, currentWindow.Width, currentWindow.Height);
    }
}

public class MoveRightCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var step = 30;
        var x = Math.Min(workArea.Right - currentWindow.Width, currentWindow.X + step);
        return new WindowRect(x, currentWindow.Y, currentWindow.Width, currentWindow.Height);
    }
}

public class MoveUpCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var step = 30;
        var y = Math.Max(workArea.Top, currentWindow.Y - step);
        return new WindowRect(currentWindow.X, y, currentWindow.Width, currentWindow.Height);
    }
}

public class MoveDownCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var step = 30;
        var y = Math.Min(workArea.Bottom - currentWindow.Height, currentWindow.Y + step);
        return new WindowRect(currentWindow.X, y, currentWindow.Width, currentWindow.Height);
    }
}

/// <summary>
/// 缩放计算器
/// </summary>
public class LargerCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var step = 30;
        var newWidth = Math.Min(currentWindow.Width + step * 2, workArea.Width);
        var newHeight = Math.Min(currentWindow.Height + step * 2, workArea.Height);
        var x = Math.Max(workArea.Left, Math.Min(currentWindow.X - step, workArea.Right - newWidth));
        var y = Math.Max(workArea.Top, Math.Min(currentWindow.Y - step, workArea.Bottom - newHeight));
        return new WindowRect(x, y, newWidth, newHeight);
    }
}

public class SmallerCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var step = 30;
        var newWidth = Math.Max(currentWindow.Width - step * 2, 200);
        var newHeight = Math.Max(currentWindow.Height - step * 2, 100);
        return new WindowRect(currentWindow.X + step, currentWindow.Y + step, newWidth, newHeight);
    }
}

/// <summary>
/// 其他计算器
/// </summary>
public class CenterHalfCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var width = (workArea.Width - gap) / 2;
        var x = workArea.Left + (workArea.Width - width) / 2;
        return new WindowRect(x, workArea.Top, width, workArea.Height);
    }
}

public class AlmostMaximizeCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var margin = 20;
        return new WindowRect(workArea.Left + margin, workArea.Top + margin,
            workArea.Width - margin * 2, workArea.Height - margin * 2);
    }
}

public class MaximizeHeightCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        return new WindowRect(currentWindow.X, workArea.Top, currentWindow.Width, workArea.Height);
    }
}
