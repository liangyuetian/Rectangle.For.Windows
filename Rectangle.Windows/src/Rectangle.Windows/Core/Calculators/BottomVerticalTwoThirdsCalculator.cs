namespace Rectangle.Windows.Core.Calculators;

/// <summary>
/// 下部垂直三分之二计算器（占据屏幕下部 2/3 高度，全宽）
/// </summary>
public class BottomVerticalTwoThirdsCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action)
    {
        var height = workArea.Height * 2 / 3;
        return new WindowRect(workArea.Left, workArea.Top + workArea.Height / 3, workArea.Width, height);
    }
}