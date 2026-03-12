using System;

namespace Rectangle.Windows.Core.Calculators;

/// <summary>
/// 向右减半宽度计算器：保持左边位置，宽度减半
/// </summary>
public class HalveWidthRightCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action)
    {
        var newWidth = Math.Max(currentWindow.Width / 2, 100); // 最小宽度 100

        return new WindowRect(currentWindow.Left, currentWindow.Y, newWidth, currentWindow.Height);
    }
}