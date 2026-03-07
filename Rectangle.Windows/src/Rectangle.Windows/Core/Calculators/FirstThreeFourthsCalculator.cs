using Rectangle.Windows.Core;

namespace Rectangle.Windows.Core.Calculators;

public class FirstThreeFourthsCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action)
    {
        var width = 3 * workArea.Width / 4;
        return new WindowRect(workArea.Left, workArea.Top, width, workArea.Height);
    }
}
