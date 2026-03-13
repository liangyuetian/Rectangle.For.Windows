namespace Rectangle.Windows.Core.Calculators;

/// <summary>
/// 左下角九等分计算器
/// </summary>
public class BottomLeftNinthCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action)
    {
        var width = workArea.Width / 3;
        var height = workArea.Height / 3;
        return new WindowRect(workArea.Left, workArea.Top + height * 2, width, height);
    }
}