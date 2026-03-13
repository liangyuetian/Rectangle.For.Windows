using System;

namespace Rectangle.Windows.Core.Calculators;

/// <summary>
/// 向左双倍宽度计算器：保持右边位置，宽度翻倍
/// </summary>
public class DoubleWidthLeftCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action)
    {
        var newWidth = Math.Min(currentWindow.Width * 2, workArea.Width);
        var x = currentWindow.Right - newWidth;

        // 确保不超出工作区边界
        x = Math.Max(workArea.Left, x);

        return new WindowRect(x, currentWindow.Y, newWidth, currentWindow.Height);
    }
}