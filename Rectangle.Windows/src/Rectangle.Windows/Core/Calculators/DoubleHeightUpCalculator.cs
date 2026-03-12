using System;

namespace Rectangle.Windows.Core.Calculators;

/// <summary>
/// 向上双倍高度计算器：保持底部位置，高度翻倍
/// </summary>
public class DoubleHeightUpCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action)
    {
        var newHeight = Math.Min(currentWindow.Height * 2, workArea.Height);
        var y = currentWindow.Bottom - newHeight;

        // 确保不超出工作区边界
        y = Math.Max(workArea.Top, y);

        return new WindowRect(currentWindow.X, y, currentWindow.Width, newHeight);
    }
}