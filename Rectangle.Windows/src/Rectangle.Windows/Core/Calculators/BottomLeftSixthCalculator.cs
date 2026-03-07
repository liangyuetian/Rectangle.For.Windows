using Rectangle.Windows.Core;

namespace Rectangle.Windows.Core.Calculators;

public class BottomLeftSixthCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action)
    {
        var width = workArea.Width / 3;
        var height = workArea.Height / 2;
        var y = workArea.Top + height;
        return new WindowRect(workArea.Left, y, width, height);
    }
}
