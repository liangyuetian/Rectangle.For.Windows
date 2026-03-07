using Rectangle.Windows.Core;

namespace Rectangle.Windows.Core.Calculators;

public class FirstFourthCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action)
    {
        var width = workArea.Width / 4;
        return new WindowRect(workArea.Left, workArea.Top, width, workArea.Height);
    }
}
