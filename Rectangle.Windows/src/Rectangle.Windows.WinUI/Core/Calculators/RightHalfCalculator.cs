using Rectangle.Windows.WinUI.Services;

namespace Rectangle.Windows.WinUI.Core.Calculators;

public class RightHalfCalculator : IRectCalculator
{
    private readonly ConfigService? _configService;

    public RightHalfCalculator(ConfigService? configService = null) => _configService = configService;

    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var ratio = GetHorizontalRatio();
        var totalWidth = workArea.Width - gap;
        var width = (int)(totalWidth * (100 - ratio) / 100.0);
        var x = workArea.Left + (int)(totalWidth * ratio / 100.0) + gap;
        return new WindowRect(x, workArea.Top, width, workArea.Height);
    }

    private int GetHorizontalRatio() =>
        _configService?.Load().HorizontalSplitRatio is int r && r >= 1 && r <= 99 ? r : 50;
}
