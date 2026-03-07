namespace Rectangle.Windows.Core.Calculators;

public class TopLeftCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action)
    {
        var width = workArea.Width / 2;
        var height = workArea.Height / 2;
        return new WindowRect(workArea.Left, workArea.Top, width, height);
    }
}