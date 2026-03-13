namespace Rectangle.Windows.Core.Calculators;

/// <summary>
/// 上中左八等分计算器
/// </summary>
public class TopCenterLeftEighthCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action)
    {
        var width = workArea.Width / 4;
        var height = workArea.Height / 2;
        return new WindowRect(workArea.Left + width, workArea.Top, width, height);
    }
}