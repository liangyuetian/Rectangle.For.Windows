using System;
using Rectangle.Windows.Services;

namespace Rectangle.Windows.Core.Calculators;

/// <summary>
/// 增大高度计算器：使用 SizeOffset 配置增加高度，保持居中
/// </summary>
public class LargerHeightCalculator : IRectCalculator
{
    private readonly ConfigService? _configService;

    public LargerHeightCalculator(ConfigService? configService = null)
    {
        _configService = configService;
    }

    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action)
    {
        var offset = (int)(_configService?.Load().SizeOffset ?? 30);
        var newHeight = Math.Min(currentWindow.Height + offset, workArea.Height);

        // 保持居中
        var y = currentWindow.Top - (newHeight - currentWindow.Height) / 2;

        // 确保不超出工作区边界
        y = Math.Max(workArea.Top, Math.Min(y, workArea.Bottom - newHeight));

        return new WindowRect(currentWindow.Left, y, currentWindow.Width, (int)newHeight);
    }
}