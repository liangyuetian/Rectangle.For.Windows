using Rectangle.Windows.Core.Calculators;
using Rectangle.Windows.Services;
using System.Collections.Generic;

namespace Rectangle.Windows.Core;

public class CalculatorFactory
{
    private readonly Dictionary<WindowAction, IRectCalculator> _calculators;
    private readonly ConfigService? _configService;

    public CalculatorFactory(ConfigService? configService = null)
    {
        _configService = configService;
        _calculators = new()
        {
            // 半屏
            [WindowAction.LeftHalf] = new LeftHalfCalculator(),
            [WindowAction.RightHalf] = new RightHalfCalculator(),
            [WindowAction.TopHalf] = new TopHalfCalculator(),
            [WindowAction.BottomHalf] = new BottomHalfCalculator(),
            // 四角
            [WindowAction.TopLeft] = new TopLeftCalculator(),
            [WindowAction.TopRight] = new TopRightCalculator(),
            [WindowAction.BottomLeft] = new BottomLeftCalculator(),
            [WindowAction.BottomRight] = new BottomRightCalculator(),
            // 三分之一
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
            [WindowAction.FirstThreeFourths] = new FirstThreeFourthsCalculator(),
            [WindowAction.CenterThreeFourths] = new CenterThreeFourthsCalculator(),
            [WindowAction.LastThreeFourths] = new LastThreeFourthsCalculator(),
            // 六等分
            [WindowAction.TopLeftSixth] = new TopLeftSixthCalculator(),
            [WindowAction.TopCenterSixth] = new TopCenterSixthCalculator(),
            [WindowAction.TopRightSixth] = new TopRightSixthCalculator(),
            [WindowAction.BottomLeftSixth] = new BottomLeftSixthCalculator(),
            [WindowAction.BottomCenterSixth] = new BottomCenterSixthCalculator(),
            [WindowAction.BottomRightSixth] = new BottomRightSixthCalculator(),
            // 九等分
            [WindowAction.TopLeftNinth] = new TopLeftNinthCalculator(),
            [WindowAction.TopCenterNinth] = new TopCenterNinthCalculator(),
            [WindowAction.TopRightNinth] = new TopRightNinthCalculator(),
            [WindowAction.MiddleLeftNinth] = new MiddleLeftNinthCalculator(),
            [WindowAction.MiddleCenterNinth] = new MiddleCenterNinthCalculator(),
            [WindowAction.MiddleRightNinth] = new MiddleRightNinthCalculator(),
            [WindowAction.BottomLeftNinth] = new BottomLeftNinthCalculator(),
            [WindowAction.BottomCenterNinth] = new BottomCenterNinthCalculator(),
            [WindowAction.BottomRightNinth] = new BottomRightNinthCalculator(),
            // 八等分
            [WindowAction.TopLeftEighth] = new TopLeftEighthCalculator(),
            [WindowAction.TopCenterLeftEighth] = new TopCenterLeftEighthCalculator(),
            [WindowAction.TopCenterRightEighth] = new TopCenterRightEighthCalculator(),
            [WindowAction.TopRightEighth] = new TopRightEighthCalculator(),
            [WindowAction.BottomLeftEighth] = new BottomLeftEighthCalculator(),
            [WindowAction.BottomCenterLeftEighth] = new BottomCenterLeftEighthCalculator(),
            [WindowAction.BottomCenterRightEighth] = new BottomCenterRightEighthCalculator(),
            [WindowAction.BottomRightEighth] = new BottomRightEighthCalculator(),
            // 角落三分之一
            [WindowAction.TopLeftThird] = new TopLeftThirdCalculator(),
            [WindowAction.TopRightThird] = new TopRightThirdCalculator(),
            [WindowAction.BottomLeftThird] = new BottomLeftThirdCalculator(),
            [WindowAction.BottomRightThird] = new BottomRightThirdCalculator(),
            // 垂直三分之一
            [WindowAction.TopVerticalThird] = new TopVerticalThirdCalculator(),
            [WindowAction.MiddleVerticalThird] = new MiddleVerticalThirdCalculator(),
            [WindowAction.BottomVerticalThird] = new BottomVerticalThirdCalculator(),
            [WindowAction.TopVerticalTwoThirds] = new TopVerticalTwoThirdsCalculator(),
            [WindowAction.BottomVerticalTwoThirds] = new BottomVerticalTwoThirdsCalculator(),
            // 居中显著
            [WindowAction.CenterProminently] = new CenterProminentlyCalculator(),
            // 最大化
            [WindowAction.Maximize] = new MaximizeCalculator(),
            // 居中
            [WindowAction.Center] = new CenterCalculator(),
            [WindowAction.CenterHalf] = new CenterHalfCalculator(),
            [WindowAction.AlmostMaximize] = new AlmostMaximizeCalculator(configService),
            [WindowAction.MaximizeHeight] = new MaximizeHeightCalculator(),
            [WindowAction.Larger] = new LargerCalculator(),
            [WindowAction.Smaller] = new SmallerCalculator(),
            // 移动到边缘
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
