namespace Rectangle.Windows.Core.Calculators;

/// <summary>
/// 居中显著计算器（比普通居中更大的窗口，占据屏幕 80% 宽度和高度）
/// </summary>
public class CenterProminentlyCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action)
    {
        var width = (int)(workArea.Width * 0.8);
        var height = (int)(workArea.Height * 0.8);
        var x = workArea.Left + (workArea.Width - width) / 2;
        var y = workArea.Top + (workArea.Height - height) / 2;
        return new WindowRect(x, y, width, height);
    }
}