using Rectangle.Windows.Services;

namespace Rectangle.Windows.Core.Calculators;

/// <summary>
/// 指定尺寸计算器：将窗口设置为指定的宽度和高度，并居中
/// </summary>
public class SpecifiedCalculator : IRectCalculator
{
    private readonly ConfigService? _configService;

    public SpecifiedCalculator(ConfigService? configService = null)
    {
        _configService = configService;
    }

    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action)
    {
        var config = _configService?.Load();
        var width = config?.SpecifiedWidth ?? 1680;
        var height = config?.SpecifiedHeight ?? 1050;

        // 确保不超出工作区
        if (width > workArea.Width) width = workArea.Width;
        if (height > workArea.Height) height = workArea.Height;

        // 居中
        var x = workArea.Left + (workArea.Width - width) / 2;
        var y = workArea.Top + (workArea.Height - height) / 2;

        return new WindowRect(x, y, width, height);
    }
}