namespace Rectangle.Windows.WinUI.Core.Calculators;

public class TopHalfCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var height = (workArea.Height - gap) / 2;
        return new WindowRect(workArea.Left, workArea.Top, workArea.Width, height);
    }
}

public class BottomHalfCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var height = (workArea.Height - gap) / 2;
        var y = workArea.Top + height + gap;
        return new WindowRect(workArea.Left, y, workArea.Width, height);
    }
}

public class TopLeftCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var width = (workArea.Width - gap) / 2;
        var height = (workArea.Height - gap) / 2;
        return new WindowRect(workArea.Left, workArea.Top, width, height);
    }
}

public class TopRightCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var width = (workArea.Width - gap) / 2;
        var height = (workArea.Height - gap) / 2;
        return new WindowRect(workArea.Left + width + gap, workArea.Top, width, height);
    }
}

public class BottomLeftCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var width = (workArea.Width - gap) / 2;
        var height = (workArea.Height - gap) / 2;
        return new WindowRect(workArea.Left, workArea.Top + height + gap, width, height);
    }
}

public class BottomRightCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var width = (workArea.Width - gap) / 2;
        var height = (workArea.Height - gap) / 2;
        return new WindowRect(workArea.Left + width + gap, workArea.Top + height + gap, width, height);
    }
}

public class MaximizeCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        return new WindowRect(workArea.Left, workArea.Top, workArea.Width, workArea.Height);
    }
}

public class CenterCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var x = workArea.Left + (workArea.Width - currentWindow.Width) / 2;
        var y = workArea.Top + (workArea.Height - currentWindow.Height) / 2;
        return new WindowRect(x, y, currentWindow.Width, currentWindow.Height);
    }
}
