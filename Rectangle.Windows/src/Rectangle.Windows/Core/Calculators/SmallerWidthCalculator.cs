using System;
using Rectangle.Windows.Services;

namespace Rectangle.Windows.Core.Calculators;

/// <summary>
/// 减小宽度计算器：使用 SizeOffset 配置减少宽度，保持居中
/// </summary>
public class SmallerWidthCalculator : IRectCalculator
{
    private readonly ConfigService? _configService;

    public SmallerWidthCalculator(ConfigService? configService = null)
    {
        _configService = configService;
    }

    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action)
    {
        var offset = (int)(_configService?.Load().SizeOffset ?? 30);
        var newWidth = Math.Max(currentWindow.Width - offset, 100); // 最小宽度 100

        // 保持居中
        var x = currentWindow.Left + (currentWindow.Width - newWidth) / 2;

        return new WindowRect(x, currentWindow.Top, newWidth, currentWindow.Height);
    }
}