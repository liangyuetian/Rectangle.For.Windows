using System;
using Rectangle.Windows.Services;

namespace Rectangle.Windows.Core.Calculators;

/// <summary>
/// 减小高度计算器：使用 SizeOffset 配置减少高度，保持居中
/// </summary>
public class SmallerHeightCalculator : IRectCalculator
{
    private readonly ConfigService? _configService;

    public SmallerHeightCalculator(ConfigService? configService = null)
    {
        _configService = configService;
    }

    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action)
    {
        var offset = (int)(_configService?.Load().SizeOffset ?? 30);
        var newHeight = Math.Max(currentWindow.Height - offset, 100); // 最小高度 100

        // 保持居中
        var y = currentWindow.Top + (currentWindow.Height - newHeight) / 2;

        return new WindowRect(currentWindow.Left, y, currentWindow.Width, newHeight);
    }
}