namespace Rectangle.Windows.WinUI.Core.Calculators;

public class LeftHalfCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var width = (workArea.Width - gap) / 2;
        return new WindowRect(workArea.Left, workArea.Top, width, workArea.Height);
    }
}
