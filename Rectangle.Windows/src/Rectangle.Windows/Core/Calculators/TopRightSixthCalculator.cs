using Rectangle.Windows.Core;

namespace Rectangle.Windows.Core.Calculators;

public class TopRightSixthCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action)
    {
        var width = workArea.Width / 3;
        var height = workArea.Height / 2;
        var x = workArea.Left + 2 * width;
        return new WindowRect(x, workArea.Top, width, height);
    }
}
