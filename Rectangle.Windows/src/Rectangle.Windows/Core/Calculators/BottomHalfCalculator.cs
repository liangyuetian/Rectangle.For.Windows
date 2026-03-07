namespace Rectangle.Windows.Core.Calculators;

public class BottomHalfCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action)
    {
        var height = workArea.Height / 2;
        return new WindowRect(workArea.Left, workArea.Top + height, workArea.Width, height);
    }
}