namespace Rectangle.Windows.WinUI.Core.Calculators;

/// <summary>
/// 八等分计算器
/// </summary>
public class TopLeftEighthCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var width = (workArea.Width - gap * 3) / 4;
        var height = (workArea.Height - gap) / 2;
        return new WindowRect(workArea.Left, workArea.Top, width, height);
    }
}

public class TopCenterLeftEighthCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var width = (workArea.Width - gap * 3) / 4;
        var height = (workArea.Height - gap) / 2;
        var x = workArea.Left + width + gap;
        return new WindowRect(x, workArea.Top, width, height);
    }
}

public class TopCenterRightEighthCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var width = (workArea.Width - gap * 3) / 4;
        var height = (workArea.Height - gap) / 2;
        var x = workArea.Left + width * 2 + gap * 2;
        return new WindowRect(x, workArea.Top, width, height);
    }
}

public class TopRightEighthCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var width = (workArea.Width - gap * 3) / 4;
        var height = (workArea.Height - gap) / 2;
        var x = workArea.Left + width * 3 + gap * 3;
        return new WindowRect(x, workArea.Top, width, height);
    }
}

public class BottomLeftEighthCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var width = (workArea.Width - gap * 3) / 4;
        var height = (workArea.Height - gap) / 2;
        var y = workArea.Top + height + gap;
        return new WindowRect(workArea.Left, y, width, height);
    }
}

public class BottomCenterLeftEighthCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var width = (workArea.Width - gap * 3) / 4;
        var height = (workArea.Height - gap) / 2;
        var x = workArea.Left + width + gap;
        var y = workArea.Top + height + gap;
        return new WindowRect(x, y, width, height);
    }
}

public class BottomCenterRightEighthCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var width = (workArea.Width - gap * 3) / 4;
        var height = (workArea.Height - gap) / 2;
        var x = workArea.Left + width * 2 + gap * 2;
        var y = workArea.Top + height + gap;
        return new WindowRect(x, y, width, height);
    }
}

public class BottomRightEighthCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var width = (workArea.Width - gap * 3) / 4;
        var height = (workArea.Height - gap) / 2;
        var x = workArea.Left + width * 3 + gap * 3;
        var y = workArea.Top + height + gap;
        return new WindowRect(x, y, width, height);
    }
}

/// <summary>
/// 九宫格计算器
/// </summary>
public class TopLeftNinthCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var width = (workArea.Width - gap * 2) / 3;
        var height = (workArea.Height - gap * 2) / 3;
        return new WindowRect(workArea.Left, workArea.Top, width, height);
    }
}

public class TopCenterNinthCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var width = (workArea.Width - gap * 2) / 3;
        var height = (workArea.Height - gap * 2) / 3;
        var x = workArea.Left + width + gap;
        return new WindowRect(x, workArea.Top, width, height);
    }
}

public class TopRightNinthCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var width = (workArea.Width - gap * 2) / 3;
        var height = (workArea.Height - gap * 2) / 3;
        var x = workArea.Left + width * 2 + gap * 2;
        return new WindowRect(x, workArea.Top, width, height);
    }
}

public class MiddleLeftNinthCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var width = (workArea.Width - gap * 2) / 3;
        var height = (workArea.Height - gap * 2) / 3;
        var y = workArea.Top + height + gap;
        return new WindowRect(workArea.Left, y, width, height);
    }
}

public class MiddleCenterNinthCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var width = (workArea.Width - gap * 2) / 3;
        var height = (workArea.Height - gap * 2) / 3;
        var x = workArea.Left + width + gap;
        var y = workArea.Top + height + gap;
        return new WindowRect(x, y, width, height);
    }
}

public class MiddleRightNinthCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var width = (workArea.Width - gap * 2) / 3;
        var height = (workArea.Height - gap * 2) / 3;
        var x = workArea.Left + width * 2 + gap * 2;
        var y = workArea.Top + height + gap;
        return new WindowRect(x, y, width, height);
    }
}

public class BottomLeftNinthCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var width = (workArea.Width - gap * 2) / 3;
        var height = (workArea.Height - gap * 2) / 3;
        var y = workArea.Top + height * 2 + gap * 2;
        return new WindowRect(workArea.Left, y, width, height);
    }
}

public class BottomCenterNinthCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var width = (workArea.Width - gap * 2) / 3;
        var height = (workArea.Height - gap * 2) / 3;
        var x = workArea.Left + width + gap;
        var y = workArea.Top + height * 2 + gap * 2;
        return new WindowRect(x, y, width, height);
    }
}

public class BottomRightNinthCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var width = (workArea.Width - gap * 2) / 3;
        var height = (workArea.Height - gap * 2) / 3;
        var x = workArea.Left + width * 2 + gap * 2;
        var y = workArea.Top + height * 2 + gap * 2;
        return new WindowRect(x, y, width, height);
    }
}

/// <summary>
/// 垂直三分屏计算器
/// </summary>
public class TopVerticalThirdCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var width = (workArea.Width - gap * 2) / 3;
        return new WindowRect(workArea.Left, workArea.Top, width, workArea.Height);
    }
}

public class MiddleVerticalThirdCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var width = (workArea.Width - gap * 2) / 3;
        var x = workArea.Left + width + gap;
        return new WindowRect(x, workArea.Top, width, workArea.Height);
    }
}

public class BottomVerticalThirdCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var width = (workArea.Width - gap * 2) / 3;
        var x = workArea.Left + width * 2 + gap * 2;
        return new WindowRect(x, workArea.Top, width, workArea.Height);
    }
}
