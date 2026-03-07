using Rectangle.Windows.Core;

namespace Rectangle.Windows.Core.Calculators;

public class LastThreeFourthsCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action)
    {
        var width = 3 * workArea.Width / 4;
        var x = workArea.Left + workArea.Width / 4;
        return new WindowRect(x, workArea.Top, width, workArea.Height);
    }
}
