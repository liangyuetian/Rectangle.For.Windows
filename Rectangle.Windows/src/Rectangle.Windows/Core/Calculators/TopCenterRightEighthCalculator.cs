namespace Rectangle.Windows.Core.Calculators;

/// <summary>
/// 上中右八等分计算器
/// </summary>
public class TopCenterRightEighthCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action)
    {
        var width = workArea.Width / 4;
        var height = workArea.Height / 2;
        return new WindowRect(workArea.Left + width * 2, workArea.Top, width, height);
    }
}