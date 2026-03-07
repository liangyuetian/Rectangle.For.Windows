using Rectangle.Windows.Core;
using Rectangle.Windows.Core.Calculators;
using Xunit;

namespace Rectangle.Windows.Tests;

public class LeftHalfCalculatorTests
{
    [Fact]
    public void Calculate_ReturnsCorrectWidth()
    {
        // Arrange
        var calculator = new LeftHalfCalculator();
        var workArea = new WorkArea(0, 0, 1920, 1080);
        var currentWindow = new WindowRect(100, 100, 800, 600);

        // Act
        var result = calculator.Calculate(workArea, currentWindow, WindowAction.LeftHalf);

        // Assert
        Assert.Equal(960, result.Width);  // 1920 / 2 = 960
        Assert.Equal(0, result.X);        // Left edge
        Assert.Equal(0, result.Y);        // Top
        Assert.Equal(1080, result.Height); // Full height
    }

    [Fact]
    public void Calculate_WithNonZeroOrigin_ReturnsCorrectPosition()
    {
        // Arrange
        var calculator = new LeftHalfCalculator();
        var workArea = new WorkArea(100, 50, 2020, 1130);  // Work area offset
        var currentWindow = new WindowRect(500, 200, 800, 600);

        // Act
        var result = calculator.Calculate(workArea, currentWindow, WindowAction.LeftHalf);

        // Assert
        Assert.Equal(960, result.Width);  // (2020 - 100) / 2 = 960
        Assert.Equal(100, result.X);      // Work area left
        Assert.Equal(50, result.Y);       // Work area top
        Assert.Equal(1080, result.Height); // 1130 - 50 = 1080
    }
}

public class RightHalfCalculatorTests
{
    [Fact]
    public void Calculate_ReturnsCorrectPosition()
    {
        // Arrange
        var calculator = new RightHalfCalculator();
        var workArea = new WorkArea(0, 0, 1920, 1080);
        var currentWindow = new WindowRect(100, 100, 800, 600);

        // Act
        var result = calculator.Calculate(workArea, currentWindow, WindowAction.RightHalf);

        // Assert
        Assert.Equal(960, result.Width);   // 1920 / 2 = 960
        Assert.Equal(960, result.X);       // Right half starts at 960
        Assert.Equal(0, result.Y);
        Assert.Equal(1080, result.Height);
    }
}

public class TopHalfCalculatorTests
{
    [Fact]
    public void Calculate_ReturnsCorrectPosition()
    {
        // Arrange
        var calculator = new TopHalfCalculator();
        var workArea = new WorkArea(0, 0, 1920, 1080);
        var currentWindow = new WindowRect(100, 100, 800, 600);

        // Act
        var result = calculator.Calculate(workArea, currentWindow, WindowAction.TopHalf);

        // Assert
        Assert.Equal(1920, result.Width);
        Assert.Equal(540, result.Height);  // 1080 / 2 = 540
        Assert.Equal(0, result.X);
        Assert.Equal(0, result.Y);
    }
}

public class BottomHalfCalculatorTests
{
    [Fact]
    public void Calculate_ReturnsCorrectPosition()
    {
        // Arrange
        var calculator = new BottomHalfCalculator();
        var workArea = new WorkArea(0, 0, 1920, 1080);
        var currentWindow = new WindowRect(100, 100, 800, 600);

        // Act
        var result = calculator.Calculate(workArea, currentWindow, WindowAction.BottomHalf);

        // Assert
        Assert.Equal(1920, result.Width);
        Assert.Equal(540, result.Height);   // 1080 / 2 = 540
        Assert.Equal(0, result.X);
        Assert.Equal(540, result.Y);        // Starts at 540
    }
}

public class MaximizeCalculatorTests
{
    [Fact]
    public void Calculate_ReturnsFullWorkArea()
    {
        // Arrange
        var calculator = new MaximizeCalculator();
        var workArea = new WorkArea(0, 0, 1920, 1080);
        var currentWindow = new WindowRect(100, 100, 800, 600);

        // Act
        var result = calculator.Calculate(workArea, currentWindow, WindowAction.Maximize);

        // Assert
        Assert.Equal(0, result.X);
        Assert.Equal(0, result.Y);
        Assert.Equal(1920, result.Width);
        Assert.Equal(1080, result.Height);
    }
}

public class FirstThirdCalculatorTests
{
    [Fact]
    public void Calculate_ReturnsOneThirdWidth()
    {
        // Arrange
        var calculator = new FirstThirdCalculator();
        var workArea = new WorkArea(0, 0, 1920, 1080);

        // Act
        var result = calculator.Calculate(workArea, default, WindowAction.FirstThird);

        // Assert
        Assert.Equal(640, result.Width);  // 1920 / 3 = 640
        Assert.Equal(0, result.X);
        Assert.Equal(1080, result.Height);
    }
}

public class CenterThirdCalculatorTests
{
    [Fact]
    public void Calculate_ReturnsCenterThird()
    {
        // Arrange
        var calculator = new CenterThirdCalculator();
        var workArea = new WorkArea(0, 0, 1920, 1080);

        // Act
        var result = calculator.Calculate(workArea, default, WindowAction.CenterThird);

        // Assert
        Assert.Equal(640, result.Width);    // 1920 / 3 = 640
        Assert.Equal(640, result.X);        // Starts at 640
        Assert.Equal(1080, result.Height);
    }
}

public class LastThirdCalculatorTests
{
    [Fact]
    public void Calculate_ReturnsLastThird()
    {
        // Arrange
        var calculator = new LastThirdCalculator();
        var workArea = new WorkArea(0, 0, 1920, 1080);

        // Act
        var result = calculator.Calculate(workArea, default, WindowAction.LastThird);

        // Assert
        Assert.Equal(640, result.Width);     // 1920 / 3 = 640
        Assert.Equal(1280, result.X);        // Starts at 1920 - 640 = 1280
        Assert.Equal(1080, result.Height);
    }
}

public class TopLeftCalculatorTests
{
    [Fact]
    public void Calculate_ReturnsQuarterScreen()
    {
        // Arrange
        var calculator = new TopLeftCalculator();
        var workArea = new WorkArea(0, 0, 1920, 1080);

        // Act
        var result = calculator.Calculate(workArea, default, WindowAction.TopLeft);

        // Assert
        Assert.Equal(960, result.Width);   // 1920 / 2 = 960
        Assert.Equal(540, result.Height);  // 1080 / 2 = 540
        Assert.Equal(0, result.X);
        Assert.Equal(0, result.Y);
    }
}

public class CalculatorFactoryTests
{
    [Fact]
    public void GetCalculator_ReturnsCorrectCalculator()
    {
        // Arrange
        var factory = new CalculatorFactory();

        // Act & Assert
        Assert.IsType<LeftHalfCalculator>(factory.GetCalculator(WindowAction.LeftHalf));
        Assert.IsType<RightHalfCalculator>(factory.GetCalculator(WindowAction.RightHalf));
        Assert.IsType<TopHalfCalculator>(factory.GetCalculator(WindowAction.TopHalf));
        Assert.IsType<BottomHalfCalculator>(factory.GetCalculator(WindowAction.BottomHalf));
        Assert.IsType<MaximizeCalculator>(factory.GetCalculator(WindowAction.Maximize));
        Assert.IsType<CenterCalculator>(factory.GetCalculator(WindowAction.Center));
        Assert.IsType<FirstThirdCalculator>(factory.GetCalculator(WindowAction.FirstThird));
        Assert.IsType<TopLeftCalculator>(factory.GetCalculator(WindowAction.TopLeft));
    }

    [Fact]
    public void GetCalculator_ReturnsNullForUnsupportedAction()
    {
        // Arrange
        var factory = new CalculatorFactory();

        // Act
        var result = factory.GetCalculator((WindowAction)999);

        // Assert
        Assert.Null(result);
    }
}
