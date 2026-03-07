using Rectangle.Windows.Core;

namespace Rectangle.Windows.Core.Calculators;

public class MoveUpCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action)
    {
        return new WindowRect(currentWindow.X, workArea.Top, currentWindow.Width, currentWindow.Height);
    }
}
