using System;

namespace Rectangle.Windows.Core.Calculators;

/// <summary>
/// 向右双倍宽度计算器：保持左边位置，宽度翻倍
/// </summary>
public class DoubleWidthRightCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action)
    {
        var newWidth = Math.Min(currentWindow.Width * 2, workArea.Width);

        // 确保不超出工作区右边界
        if (currentWindow.Left + newWidth > workArea.Right)
        {
            newWidth = workArea.Right - currentWindow.Left;
        }

        return new WindowRect(currentWindow.Left, currentWindow.Y, newWidth, currentWindow.Height);
    }
}