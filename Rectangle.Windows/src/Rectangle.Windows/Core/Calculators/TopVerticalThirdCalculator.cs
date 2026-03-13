namespace Rectangle.Windows.Core.Calculators;

/// <summary>
/// 上部垂直三分之一计算器（占据屏幕上部 1/3 高度，全宽）
/// </summary>
public class TopVerticalThirdCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action)
    {
        var height = workArea.Height / 3;
        return new WindowRect(workArea.Left, workArea.Top, workArea.Width, height);
    }
}