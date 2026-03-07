using Rectangle.Windows.Core;

namespace Rectangle.Windows.Core.Calculators;

public class MoveRightCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action)
    {
        var x = workArea.Right - currentWindow.Width;
        return new WindowRect(x, currentWindow.Y, currentWindow.Width, currentWindow.Height);
    }
}
