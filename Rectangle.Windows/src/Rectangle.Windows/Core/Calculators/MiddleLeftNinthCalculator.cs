namespace Rectangle.Windows.Core.Calculators;

/// <summary>
/// 左中九等分计算器
/// </summary>
public class MiddleLeftNinthCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action)
    {
        var width = workArea.Width / 3;
        var height = workArea.Height / 3;
        return new WindowRect(workArea.Left, workArea.Top + height, width, height);
    }
}