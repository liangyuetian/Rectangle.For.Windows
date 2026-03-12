using System;

namespace Rectangle.Windows.Core.Calculators;

/// <summary>
/// 向左减半宽度计算器：保持右边位置，宽度减半
/// </summary>
public class HalveWidthLeftCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action)
    {
        var newWidth = Math.Max(currentWindow.Width / 2, 100); // 最小宽度 100
        var x = currentWindow.Right - newWidth;

        return new WindowRect(x, currentWindow.Y, newWidth, currentWindow.Height);
    }
}