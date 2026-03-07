using Rectangle.Windows.Core;

namespace Rectangle.Windows.Core.Calculators;

public class SmallerCalculator : IRectCalculator
{
    private const double ScaleFactor = 0.9;

    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action)
    {
        var newWidth = (int)(currentWindow.Width * ScaleFactor);
        var newHeight = (int)(currentWindow.Height * ScaleFactor);
        
        var newX = currentWindow.X + (currentWindow.Width - newWidth) / 2;
        var newY = currentWindow.Y + (currentWindow.Height - newHeight) / 2;
        
        if (newX < workArea.Left) newX = workArea.Left;
        if (newY < workArea.Top) newY = workArea.Top;
        if (newX + newWidth > workArea.Right) newX = workArea.Right - newWidth;
        if (newY + newHeight > workArea.Bottom) newY = workArea.Bottom - newHeight;
        
        return new WindowRect(newX, newY, newWidth, newHeight);
    }
}
