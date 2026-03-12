using System;

namespace Rectangle.Windows.Core.Calculators;

/// <summary>
/// 向下减半高度计算器：保持顶部位置，高度减半
/// </summary>
public class HalveHeightDownCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action)
    {
        var newHeight = Math.Max(currentWindow.Height / 2, 100); // 最小高度 100

        return new WindowRect(currentWindow.X, currentWindow.Top, currentWindow.Width, newHeight);
    }
}