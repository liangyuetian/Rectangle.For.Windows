namespace Rectangle.Windows.Core.Calculators;

public class CenterCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action)
    {
        var x = workArea.Left + (workArea.Width - currentWindow.Width) / 2;
        var y = workArea.Top + (workArea.Height - currentWindow.Height) / 2;
        return new WindowRect(x, y, currentWindow.Width, currentWindow.Height);
    }
}