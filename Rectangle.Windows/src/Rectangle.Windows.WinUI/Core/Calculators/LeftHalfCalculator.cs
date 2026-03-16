using Rectangle.Windows.WinUI.Services;

namespace Rectangle.Windows.WinUI.Core.Calculators;

public class LeftHalfCalculator : IRectCalculator
{
    private readonly ConfigService? _configService;

    public LeftHalfCalculator(ConfigService? configService = null) => _configService = configService;

    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var ratio = GetHorizontalRatio();
        var totalWidth = workArea.Width - gap;
        var width = (int)(totalWidth * ratio / 100.0);
        return new WindowRect(workArea.Left, workArea.Top, width, workArea.Height);
    }

    private int GetHorizontalRatio() =>
        _configService?.Load().HorizontalSplitRatio is var r && r >= 1 && r <= 99 ? r : 50;
}
