namespace Rectangle.Windows.Core.Calculators;

/// <summary>
/// 右上角三分之一计算器（占据屏幕右上角的 1/3 宽度和 1/3 高度）
/// </summary>
public class TopRightThirdCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action)
    {
        var width = workArea.Width / 3;
        var height = workArea.Height / 3;
        return new WindowRect(workArea.Left + width * 2, workArea.Top, width, height);
    }
}