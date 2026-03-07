using Rectangle.Windows.Core;

namespace Rectangle.Windows.Core.Calculators;

public class TopLeftSixthCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action)
    {
        var width = workArea.Width / 3;
        var height = workArea.Height / 2;
        return new WindowRect(workArea.Left, workArea.Top, width, height);
    }
}
