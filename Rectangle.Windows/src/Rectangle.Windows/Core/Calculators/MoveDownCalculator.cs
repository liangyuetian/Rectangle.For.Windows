using Rectangle.Windows.Core;

namespace Rectangle.Windows.Core.Calculators;

public class MoveDownCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action)
    {
        var y = workArea.Bottom - currentWindow.Height;
        return new WindowRect(currentWindow.X, y, currentWindow.Width, currentWindow.Height);
    }
}
