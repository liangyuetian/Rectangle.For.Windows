using Rectangle.Windows.Core;

namespace Rectangle.Windows.Core.Calculators;

public class MaximizeHeightCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action)
    {
        return new WindowRect(currentWindow.X, workArea.Top, currentWindow.Width, workArea.Height);
    }
}
