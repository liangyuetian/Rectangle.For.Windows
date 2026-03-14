namespace Rectangle.Windows.WinUI.Core.Calculators;

public class RightHalfCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var width = (workArea.Width - gap) / 2;
        var x = workArea.Left + width + gap;
        return new WindowRect(x, workArea.Top, width, workArea.Height);
    }
}
