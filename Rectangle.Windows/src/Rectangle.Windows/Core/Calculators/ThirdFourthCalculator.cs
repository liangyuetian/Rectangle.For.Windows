using Rectangle.Windows.Core;

namespace Rectangle.Windows.Core.Calculators;

public class ThirdFourthCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action)
    {
        var width = workArea.Width / 4;
        var x = workArea.Left + 2 * width;
        return new WindowRect(x, workArea.Top, width, workArea.Height);
    }
}
