namespace Rectangle.Windows.Core.Calculators;

public class MaximizeCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action)
    {
        return new WindowRect(workArea.Left, workArea.Top, workArea.Width, workArea.Height);
    }
}