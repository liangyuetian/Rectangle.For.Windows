namespace Rectangle.Windows.Core.Calculators;

public class LastThirdCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action)
    {
        var width = workArea.Width / 3;
        return new WindowRect(workArea.Left + (width * 2), workArea.Top, width, workArea.Height);
    }
}