namespace Rectangle.Windows.Core.Calculators;

/// <summary>
/// 左上角三分之一计算器（占据屏幕左上角的 1/3 宽度和 1/3 高度）
/// </summary>
public class TopLeftThirdCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action)
    {
        var width = workArea.Width / 3;
        var height = workArea.Height / 3;
        return new WindowRect(workArea.Left, workArea.Top, width, height);
    }
}