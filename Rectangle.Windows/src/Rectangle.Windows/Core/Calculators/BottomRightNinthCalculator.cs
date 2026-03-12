namespace Rectangle.Windows.Core.Calculators;

/// <summary>
/// 右下角九等分计算器
/// </summary>
public class BottomRightNinthCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action)
    {
        var width = workArea.Width / 3;
        var height = workArea.Height / 3;
        return new WindowRect(workArea.Left + width * 2, workArea.Top + height * 2, width, height);
    }
}