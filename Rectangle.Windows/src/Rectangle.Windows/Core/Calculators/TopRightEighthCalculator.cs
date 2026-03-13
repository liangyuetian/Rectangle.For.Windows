namespace Rectangle.Windows.Core.Calculators;

/// <summary>
/// 右上角八等分计算器
/// </summary>
public class TopRightEighthCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action)
    {
        var width = workArea.Width / 4;
        var height = workArea.Height / 2;
        return new WindowRect(workArea.Left + width * 3, workArea.Top, width, height);
    }
}