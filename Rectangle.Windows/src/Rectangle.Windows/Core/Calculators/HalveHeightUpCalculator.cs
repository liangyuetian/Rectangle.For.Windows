using System;

namespace Rectangle.Windows.Core.Calculators;

/// <summary>
/// 向上减半高度计算器：保持底部位置，高度减半
/// </summary>
public class HalveHeightUpCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action)
    {
        var newHeight = Math.Max(currentWindow.Height / 2, 100); // 最小高度 100
        var y = currentWindow.Bottom - newHeight;

        return new WindowRect(currentWindow.X, y, currentWindow.Width, newHeight);
    }
}