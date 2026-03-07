using Rectangle.Windows.Core;

namespace Rectangle.Windows.Core.Calculators;

public class AlmostMaximizeCalculator : IRectCalculator
{
    private const int Margin = 10;

    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action)
    {
        return new WindowRect(
            workArea.Left + Margin,
            workArea.Top + Margin,
            workArea.Width - 2 * Margin,
            workArea.Height - 2 * Margin);
    }
}
