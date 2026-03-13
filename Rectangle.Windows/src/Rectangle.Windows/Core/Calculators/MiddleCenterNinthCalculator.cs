namespace Rectangle.Windows.Core.Calculators;

/// <summary>
/// 正中九等分计算器
/// </summary>
public class MiddleCenterNinthCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action)
    {
        var width = workArea.Width / 3;
        var height = workArea.Height / 3;
        return new WindowRect(workArea.Left + width, workArea.Top + height, width, height);
    }
}