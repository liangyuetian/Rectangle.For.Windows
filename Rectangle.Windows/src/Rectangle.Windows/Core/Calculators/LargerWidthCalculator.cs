using System;
using Rectangle.Windows.Services;

namespace Rectangle.Windows.Core.Calculators;

/// <summary>
/// 增大宽度计算器：使用 SizeOffset 配置增加宽度，保持居中
/// </summary>
public class LargerWidthCalculator : IRectCalculator
{
    private readonly ConfigService? _configService;

    public LargerWidthCalculator(ConfigService? configService = null)
    {
        _configService = configService;
    }

    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action)
    {
        var offset = (int)(_configService?.Load().SizeOffset ?? 30);
        var newWidth = Math.Min(currentWindow.Width + offset, workArea.Width);

        // 保持居中
        var x = currentWindow.Left - (newWidth - currentWindow.Width) / 2;

        // 确保不超出工作区边界
        x = Math.Max(workArea.Left, Math.Min(x, workArea.Right - newWidth));

        return new WindowRect(x, currentWindow.Top, (int)newWidth, currentWindow.Height);
    }
}