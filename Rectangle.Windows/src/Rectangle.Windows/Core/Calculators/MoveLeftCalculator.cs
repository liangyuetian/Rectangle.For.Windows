using Rectangle.Windows.Core;

namespace Rectangle.Windows.Core.Calculators;

public class MoveLeftCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action)
    {
        return new WindowRect(workArea.Left, currentWindow.Y, currentWindow.Width, currentWindow.Height);
    }
}
