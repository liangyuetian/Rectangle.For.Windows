using System;

namespace Rectangle.Windows.Core.Calculators;

public class LargerCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action)
    {
        var scale = 1.1f;
        var newWidth = (int)(currentWindow.Width * scale);
        var newHeight = (int)(currentWindow.Height * scale);
        
        // 限制在工作区范围内
        newWidth = Math.Min(newWidth, workArea.Width);
        newHeight = Math.Min(newHeight, workArea.Height);
        
        var x = currentWindow.X - (newWidth - currentWindow.Width) / 2;
        var y = currentWindow.Y - (newHeight - currentWindow.Height) / 2;
        
        // 确保不超出工作区边界
        x = Math.Max(workArea.Left, Math.Min(x, workArea.Right - newWidth));
        y = Math.Max(workArea.Top, Math.Min(y, workArea.Bottom - newHeight));
        
        return new WindowRect(x, y, newWidth, newHeight);
    }
}
