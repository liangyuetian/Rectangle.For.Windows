namespace Rectangle.Windows.Core.Calculators;

/// <summary>
/// 下中左八等分计算器
/// </summary>
public class BottomCenterLeftEighthCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action)
    {
        var width = workArea.Width / 4;
        var height = workArea.Height / 2;
        return new WindowRect(workArea.Left + width, workArea.Top + height, width, height);
    }
}