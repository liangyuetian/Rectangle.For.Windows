namespace Rectangle.Windows.Core.Calculators;

public class TopHalfCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action)
    {
        var height = workArea.Height / 2;
        return new WindowRect(workArea.Left, workArea.Top, workArea.Width, height);
    }
}