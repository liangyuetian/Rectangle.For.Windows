using Rectangle.Windows.Core;

namespace Rectangle.Windows.Core.Calculators;

public class CenterHalfCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action)
    {
        var width = workArea.Width / 2;
        var x = workArea.Left + (workArea.Width - width) / 2;
        return new WindowRect(x, workArea.Top, width, workArea.Height);
    }
}
