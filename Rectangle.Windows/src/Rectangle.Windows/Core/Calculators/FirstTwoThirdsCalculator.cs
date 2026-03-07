namespace Rectangle.Windows.Core.Calculators;

public class FirstTwoThirdsCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action)
    {
        var width = (workArea.Width * 2) / 3;
        return new WindowRect(workArea.Left, workArea.Top, width, workArea.Height);
    }
}