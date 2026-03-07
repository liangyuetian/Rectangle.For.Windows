namespace Rectangle.Windows.WinUI.Core;

public class CalculatorFactory
{
    private readonly Dictionary<WindowAction, IRectCalculator> _calculators = new()
    {
        [WindowAction.LeftHalf] = new Calculators.LeftHalfCalculator(),
        [WindowAction.RightHalf] = new Calculators.RightHalfCalculator(),
        [WindowAction.TopHalf] = new Calculators.TopHalfCalculator(),
        [WindowAction.BottomHalf] = new Calculators.BottomHalfCalculator(),
        [WindowAction.TopLeft] = new Calculators.TopLeftCalculator(),
        [WindowAction.TopRight] = new Calculators.TopRightCalculator(),
        [WindowAction.BottomLeft] = new Calculators.BottomLeftCalculator(),
        [WindowAction.BottomRight] = new Calculators.BottomRightCalculator(),
        [WindowAction.Maximize] = new Calculators.MaximizeCalculator(),
        [WindowAction.Center] = new Calculators.CenterCalculator(),
    };

    public IRectCalculator? GetCalculator(WindowAction action)
    {
        return _calculators.TryGetValue(action, out var c) ? c : null;
    }
}
