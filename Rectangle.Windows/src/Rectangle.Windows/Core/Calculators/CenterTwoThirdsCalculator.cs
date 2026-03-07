namespace Rectangle.Windows.Core.Calculators;

public class CenterTwoThirdsCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action)
    {
        var width = (workArea.Width * 2) / 3;
        var x = workArea.Left + (workArea.Width / 6);
        return new WindowRect(x, workArea.Top, width, workArea.Height);
    }
}