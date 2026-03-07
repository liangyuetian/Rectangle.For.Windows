namespace Rectangle.Windows.WinUI.Core;

public interface IRectCalculator
{
    WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0);
}
