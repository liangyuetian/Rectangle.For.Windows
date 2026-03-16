using Rectangle.Windows.WinUI.Services;

namespace Rectangle.Windows.WinUI.Core.Calculators;

public class TopHalfCalculator : IRectCalculator
{
    private readonly ConfigService? _configService;

    public TopHalfCalculator(ConfigService? configService = null) => _configService = configService;

    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var ratio = GetVerticalRatio();
        var totalHeight = workArea.Height - gap;
        var height = (int)(totalHeight * ratio / 100.0);
        return new WindowRect(workArea.Left, workArea.Top, workArea.Width, height);
    }

    private int GetVerticalRatio() =>
        _configService?.Load().VerticalSplitRatio is int r && r >= 1 && r <= 99 ? r : 50;
}

public class BottomHalfCalculator : IRectCalculator
{
    private readonly ConfigService? _configService;

    public BottomHalfCalculator(ConfigService? configService = null) => _configService = configService;

    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var ratio = GetVerticalRatio();
        var totalHeight = workArea.Height - gap;
        var height = (int)(totalHeight * (100 - ratio) / 100.0);
        var y = workArea.Top + (int)(totalHeight * ratio / 100.0) + gap;
        return new WindowRect(workArea.Left, y, workArea.Width, height);
    }

    private int GetVerticalRatio() =>
        _configService?.Load().VerticalSplitRatio is int r && r >= 1 && r <= 99 ? r : 50;
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
