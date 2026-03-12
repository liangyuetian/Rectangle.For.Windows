using System;

namespace Rectangle.Windows.Core.Calculators;

/// <summary>
/// 向下双倍高度计算器：保持顶部位置，高度翻倍
/// </summary>
public class DoubleHeightDownCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action)
    {
        var newHeight = Math.Min(currentWindow.Height * 2, workArea.Height);

        // 确保不超出工作区底部边界
        if (currentWindow.Top + newHeight > workArea.Bottom)
        {
            newHeight = workArea.Bottom - currentWindow.Top;
        }

        return new WindowRect(currentWindow.X, currentWindow.Top, currentWindow.Width, newHeight);
    }
}