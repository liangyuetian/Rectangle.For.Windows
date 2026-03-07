namespace Rectangle.Windows.Core;

public interface IRectCalculator
{
    WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action);
}