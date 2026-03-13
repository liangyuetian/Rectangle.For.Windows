namespace Rectangle.Windows.Core.Calculators;

/// <summary>
/// 下部垂直三分之一计算器（占据屏幕下部 1/3 高度，全宽）
/// </summary>
public class BottomVerticalThirdCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action)
    {
        var height = workArea.Height / 3;
        return new WindowRect(workArea.Left, workArea.Top + height * 2, workArea.Width, height);
    }
}