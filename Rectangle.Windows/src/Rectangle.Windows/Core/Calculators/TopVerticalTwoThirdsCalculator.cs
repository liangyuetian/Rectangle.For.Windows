namespace Rectangle.Windows.Core.Calculators;

/// <summary>
/// 上部垂直三分之二计算器（占据屏幕上部 2/3 高度，全宽）
/// </summary>
public class TopVerticalTwoThirdsCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action)
    {
        var height = workArea.Height * 2 / 3;
        return new WindowRect(workArea.Left, workArea.Top, workArea.Width, height);
    }
}