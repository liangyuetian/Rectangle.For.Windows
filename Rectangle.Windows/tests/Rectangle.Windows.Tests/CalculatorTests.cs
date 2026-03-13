using Xunit;
using Rectangle.Windows.Core;
using Rectangle.Windows.Core.Calculators;

namespace Rectangle.Windows.Tests;

/// <summary>
/// 计算器单元测试
/// </summary>
public class CalculatorTests
{
    private readonly WorkArea _testWorkArea = new(0, 0, 1920, 1080);

    [Fact]
    public void LeftHalfCalculator_ShouldReturnLeftHalf()
    {
        var calculator = new LeftHalfCalculator();
        var result = calculator.Calculate(_testWorkArea, default, WindowAction.LeftHalf);

        Assert.Equal(0, result.X);
        Assert.Equal(0, result.Y);
        Assert.Equal(960, result.Width);
        Assert.Equal(1080, result.Height);
    }

    [Fact]
    public void RightHalfCalculator_ShouldReturnRightHalf()
    {
        var calculator = new RightHalfCalculator();
        var result = calculator.Calculate(_testWorkArea, default, WindowAction.RightHalf);

        Assert.Equal(960, result.X);
        Assert.Equal(0, result.Y);
        Assert.Equal(960, result.Width);
        Assert.Equal(1080, result.Height);
    }

    [Fact]
    public void TopHalfCalculator_ShouldReturnTopHalf()
    {
        var calculator = new TopHalfCalculator();
        var result = calculator.Calculate(_testWorkArea, default, WindowAction.TopHalf);

        Assert.Equal(0, result.X);
        Assert.Equal(0, result.Y);
        Assert.Equal(1920, result.Width);
        Assert.Equal(540, result.Height);
    }

    [Fact]
    public void BottomHalfCalculator_ShouldReturnBottomHalf()
    {
        var calculator = new BottomHalfCalculator();
        var result = calculator.Calculate(_testWorkArea, default, WindowAction.BottomHalf);

        Assert.Equal(0, result.X);
        Assert.Equal(540, result.Y);
        Assert.Equal(1920, result.Width);
        Assert.Equal(540, result.Height);
    }

    [Fact]
    public void CenterCalculator_ShouldReturnCentered()
    {
        var calculator = new CenterCalculator();
        var result = calculator.Calculate(_testWorkArea, default, WindowAction.Center);

        // 默认居中保持原窗口大小或使用默认大小
        Assert.True(result.X >= 0);
        Assert.True(result.Y >= 0);
        Assert.True(result.Width > 0);
        Assert.True(result.Height > 0);
    }

    [Fact]
    public void FirstThirdCalculator_ShouldReturnFirstThird()
    {
        var calculator = new FirstThirdCalculator();
        var result = calculator.Calculate(_testWorkArea, default, WindowAction.FirstThird);

        Assert.Equal(0, result.X);
        Assert.Equal(0, result.Y);
        Assert.Equal(640, result.Width);  // 1920 / 3
        Assert.Equal(1080, result.Height);
    }

    [Fact]
    public void CenterThirdCalculator_ShouldReturnCenterThird()
    {
        var calculator = new CenterThirdCalculator();
        var result = calculator.Calculate(_testWorkArea, default, WindowAction.CenterThird);

        Assert.Equal(640, result.X);  // 1920 / 3
        Assert.Equal(0, result.Y);
        Assert.Equal(640, result.Width);
        Assert.Equal(1080, result.Height);
    }

    [Fact]
    public void LastThirdCalculator_ShouldReturnLastThird()
    {
        var calculator = new LastThirdCalculator();
        var result = calculator.Calculate(_testWorkArea, default, WindowAction.LastThird);

        Assert.Equal(1280, result.X);  // 1920 * 2 / 3
        Assert.Equal(0, result.Y);
        Assert.Equal(640, result.Width);
        Assert.Equal(1080, result.Height);
    }

    [Fact]
    public void TopLeftCalculator_ShouldReturnTopLeft()
    {
        var calculator = new TopLeftCalculator();
        var result = calculator.Calculate(_testWorkArea, default, WindowAction.TopLeft);

        Assert.Equal(0, result.X);
        Assert.Equal(0, result.Y);
        Assert.Equal(960, result.Width);   // 1920 / 2
        Assert.Equal(540, result.Height);  // 1080 / 2
    }

    [Fact]
    public void TopRightCalculator_ShouldReturnTopRight()
    {
        var calculator = new TopRightCalculator();
        var result = calculator.Calculate(_testWorkArea, default, WindowAction.TopRight);

        Assert.Equal(960, result.X);
        Assert.Equal(0, result.Y);
        Assert.Equal(960, result.Width);
        Assert.Equal(540, result.Height);
    }

    [Fact]
    public void BottomLeftCalculator_ShouldReturnBottomLeft()
    {
        var calculator = new BottomLeftCalculator();
        var result = calculator.Calculate(_testWorkArea, default, WindowAction.BottomLeft);

        Assert.Equal(0, result.X);
        Assert.Equal(540, result.Y);
        Assert.Equal(960, result.Width);
        Assert.Equal(540, result.Height);
    }

    [Fact]
    public void BottomRightCalculator_ShouldReturnBottomRight()
    {
        var calculator = new BottomRightCalculator();
        var result = calculator.Calculate(_testWorkArea, default, WindowAction.BottomRight);

        Assert.Equal(960, result.X);
        Assert.Equal(540, result.Y);
        Assert.Equal(960, result.Width);
        Assert.Equal(540, result.Height);
    }

    [Fact]
    public void MaximizeCalculator_ShouldReturnFullWorkArea()
    {
        var calculator = new MaximizeCalculator();
        var result = calculator.Calculate(_testWorkArea, default, WindowAction.Maximize);

        Assert.Equal(0, result.X);
        Assert.Equal(0, result.Y);
        Assert.Equal(1920, result.Width);
        Assert.Equal(1080, result.Height);
    }

    [Fact]
    public void FirstFourthCalculator_ShouldReturnFirstFourth()
    {
        var calculator = new FirstFourthCalculator();
        var result = calculator.Calculate(_testWorkArea, default, WindowAction.FirstFourth);

        Assert.Equal(0, result.X);
        Assert.Equal(0, result.Y);
        Assert.Equal(480, result.Width);  // 1920 / 4
        Assert.Equal(540, result.Height); // 1080 / 2
    }

    [Fact]
    public void FirstTwoThirdsCalculator_ShouldReturnFirstTwoThirds()
    {
        var calculator = new FirstTwoThirdsCalculator();
        var result = calculator.Calculate(_testWorkArea, default, WindowAction.FirstTwoThirds);

        Assert.Equal(0, result.X);
        Assert.Equal(0, result.Y);
        Assert.Equal(1280, result.Width);  // 1920 * 2 / 3
        Assert.Equal(1080, result.Height);
    }

    [Fact]
    public void LastTwoThirdsCalculator_ShouldReturnLastTwoThirds()
    {
        var calculator = new LastTwoThirdsCalculator();
        var result = calculator.Calculate(_testWorkArea, default, WindowAction.LastTwoThirds);

        Assert.Equal(640, result.X);       // 1920 / 3
        Assert.Equal(0, result.Y);
        Assert.Equal(1280, result.Width);  // 1920 * 2 / 3
        Assert.Equal(1080, result.Height);
    }

    [Fact]
    public void TopLeftNinthCalculator_ShouldReturnTopLeftNinth()
    {
        var calculator = new TopLeftNinthCalculator();
        var result = calculator.Calculate(_testWorkArea, default, WindowAction.TopLeftNinth);

        Assert.Equal(0, result.X);
        Assert.Equal(0, result.Y);
        Assert.Equal(640, result.Width);   // 1920 / 3
        Assert.Equal(360, result.Height);  // 1080 / 3
    }

    [Fact]
    public void CenterProminentlyCalculator_ShouldReturnLargerCenter()
    {
        var calculator = new CenterProminentlyCalculator();
        var result = calculator.Calculate(_testWorkArea, default, WindowAction.CenterProminently);

        // 80% of work area
        var expectedWidth = (int)(1920 * 0.8);
        var expectedHeight = (int)(1080 * 0.8);
        var expectedX = (1920 - expectedWidth) / 2;
        var expectedY = (1080 - expectedHeight) / 2;

        Assert.Equal(expectedX, result.X);
        Assert.Equal(expectedY, result.Y);
        Assert.Equal(expectedWidth, result.Width);
        Assert.Equal(expectedHeight, result.Height);
    }
}
