using Rectangle.Windows.WinUI.Core.Calculators;

namespace Rectangle.Windows.WinUI.Core;

public class CalculatorFactory
{
    private readonly Dictionary<WindowAction, IRectCalculator> _calculators;

    public CalculatorFactory()
    {
        _calculators = new Dictionary<WindowAction, IRectCalculator>
        {
            // 半屏
            [WindowAction.LeftHalf] = new LeftHalfCalculator(),
            [WindowAction.RightHalf] = new RightHalfCalculator(),
            [WindowAction.TopHalf] = new TopHalfCalculator(),
            [WindowAction.BottomHalf] = new BottomHalfCalculator(),
            [WindowAction.CenterHalf] = new CenterHalfCalculator(),

            // 四角
            [WindowAction.TopLeft] = new TopLeftCalculator(),
            [WindowAction.TopRight] = new TopRightCalculator(),
            [WindowAction.BottomLeft] = new BottomLeftCalculator(),
            [WindowAction.BottomRight] = new BottomRightCalculator(),

            // 三分屏
            [WindowAction.FirstThird] = new FirstThirdCalculator(),
            [WindowAction.CenterThird] = new CenterThirdCalculator(),
            [WindowAction.LastThird] = new LastThirdCalculator(),
            [WindowAction.FirstTwoThirds] = new FirstTwoThirdsCalculator(),
            [WindowAction.CenterTwoThirds] = new CenterTwoThirdsCalculator(),
            [WindowAction.LastTwoThirds] = new LastTwoThirdsCalculator(),

            // 四等分
            [WindowAction.FirstFourth] = new FirstFourthCalculator(),
            [WindowAction.SecondFourth] = new SecondFourthCalculator(),
            [WindowAction.ThirdFourth] = new ThirdFourthCalculator(),
            [WindowAction.LastFourth] = new LastFourthCalculator(),

            // 六等分
            [WindowAction.TopLeftSixth] = new TopLeftSixthCalculator(),
            [WindowAction.TopCenterSixth] = new TopCenterSixthCalculator(),
            [WindowAction.TopRightSixth] = new TopRightSixthCalculator(),
            [WindowAction.BottomLeftSixth] = new BottomLeftSixthCalculator(),
            [WindowAction.BottomCenterSixth] = new BottomCenterSixthCalculator(),
            [WindowAction.BottomRightSixth] = new BottomRightSixthCalculator(),

            // 最大化与缩放
            [WindowAction.Maximize] = new MaximizeCalculator(),
            [WindowAction.AlmostMaximize] = new AlmostMaximizeCalculator(),
            [WindowAction.MaximizeHeight] = new MaximizeHeightCalculator(),
            [WindowAction.Larger] = new LargerCalculator(),
            [WindowAction.Smaller] = new SmallerCalculator(),
            [WindowAction.Center] = new CenterCalculator(),

            // 移动
            [WindowAction.MoveLeft] = new MoveLeftCalculator(),
            [WindowAction.MoveRight] = new MoveRightCalculator(),
            [WindowAction.MoveUp] = new MoveUpCalculator(),
            [WindowAction.MoveDown] = new MoveDownCalculator(),
        };
    }

    public IRectCalculator? GetCalculator(WindowAction action)
    {
        return _calculators.TryGetValue(action, out var c) ? c : null;
    }
}
