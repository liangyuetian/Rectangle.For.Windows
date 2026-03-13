using Xunit;
using Rectangle.Windows.Core;

namespace Rectangle.Windows.Tests;

/// <summary>
/// WindowRect 单元测试
/// </summary>
public class WindowRectTests
{
    [Fact]
    public void Constructor_ShouldSetProperties()
    {
        var rect = new WindowRect(10, 20, 800, 600);

        Assert.Equal(10, rect.X);
        Assert.Equal(20, rect.Y);
        Assert.Equal(800, rect.Width);
        Assert.Equal(600, rect.Height);
    }

    [Fact]
    public void Deconstruct_ShouldReturnCorrectValues()
    {
        var rect = new WindowRect(10, 20, 800, 600);
        var (x, y, w, h) = rect;

        Assert.Equal(10, x);
        Assert.Equal(20, y);
        Assert.Equal(800, w);
        Assert.Equal(600, h);
    }

    [Fact]
    public void ToRectangle_ShouldConvertCorrectly()
    {
        var rect = new WindowRect(10, 20, 800, 600);
        var result = rect.ToRectangle();

        Assert.Equal(10, result.X);
        Assert.Equal(20, result.Y);
        Assert.Equal(800, result.Width);
        Assert.Equal(600, result.Height);
    }

    [Fact]
    public void FromRectangle_ShouldCreateCorrectly()
    {
        var rectangle = new System.Drawing.Rectangle(10, 20, 800, 600);
        var rect = WindowRect.FromRectangle(rectangle);

        Assert.Equal(10, rect.X);
        Assert.Equal(20, rect.Y);
        Assert.Equal(800, rect.Width);
        Assert.Equal(600, rect.Height);
    }

    [Fact]
    public void Center_ShouldCalculateCorrectly()
    {
        var rect = new WindowRect(100, 100, 800, 600);
        var center = rect.Center;

        Assert.Equal(500, center.X);  // 100 + 800 / 2
        Assert.Equal(400, center.Y);  // 100 + 600 / 2
    }

    [Fact]
    public void Right_ShouldCalculateCorrectly()
    {
        var rect = new WindowRect(100, 100, 800, 600);

        Assert.Equal(900, rect.Right);  // 100 + 800
    }

    [Fact]
    public void Bottom_ShouldCalculateCorrectly()
    {
        var rect = new WindowRect(100, 100, 800, 600);

        Assert.Equal(700, rect.Bottom);  // 100 + 600
    }

    [Fact]
    public void Area_ShouldCalculateCorrectly()
    {
        var rect = new WindowRect(0, 0, 800, 600);

        Assert.Equal(480000, rect.Area);  // 800 * 600
    }

    [Fact]
    public void Contains_ShouldReturnTrueForContainedPoint()
    {
        var rect = new WindowRect(100, 100, 800, 600);

        Assert.True(rect.Contains(200, 200));
        Assert.True(rect.Contains(100, 100));  // 边界
        Assert.True(rect.Contains(899, 699));  // 边界
    }

    [Fact]
    public void Contains_ShouldReturnFalseForOutsidePoint()
    {
        var rect = new WindowRect(100, 100, 800, 600);

        Assert.False(rect.Contains(50, 200));   // 左边
        Assert.False(rect.Contains(200, 50));   // 上边
        Assert.False(rect.Contains(1000, 200)); // 右边
        Assert.False(rect.Contains(200, 800));  // 下边
    }

    [Fact]
    public void IntersectsWith_ShouldReturnTrueForIntersecting()
    {
        var rect1 = new WindowRect(100, 100, 800, 600);
        var rect2 = new WindowRect(500, 500, 800, 600);

        Assert.True(rect1.IntersectsWith(rect2));
    }

    [Fact]
    public void IntersectsWith_ShouldReturnFalseForNonIntersecting()
    {
        var rect1 = new WindowRect(100, 100, 100, 100);
        var rect2 = new WindowRect(500, 500, 100, 100);

        Assert.False(rect1.IntersectsWith(rect2));
    }

    [Fact]
    public void Equals_ShouldReturnTrueForSameValues()
    {
        var rect1 = new WindowRect(100, 200, 800, 600);
        var rect2 = new WindowRect(100, 200, 800, 600);

        Assert.True(rect1.Equals(rect2));
        Assert.True(rect1 == rect2);
    }

    [Fact]
    public void Equals_ShouldReturnFalseForDifferentValues()
    {
        var rect1 = new WindowRect(100, 200, 800, 600);
        var rect2 = new WindowRect(100, 200, 800, 601);

        Assert.False(rect1.Equals(rect2));
        Assert.True(rect1 != rect2);
    }

    [Fact]
    public void GetHashCode_ShouldBeConsistent()
    {
        var rect1 = new WindowRect(100, 200, 800, 600);
        var rect2 = new WindowRect(100, 200, 800, 600);

        Assert.Equal(rect1.GetHashCode(), rect2.GetHashCode());
    }

    [Fact]
    public void ToString_ShouldReturnCorrectFormat()
    {
        var rect = new WindowRect(100, 200, 800, 600);
        var result = rect.ToString();

        Assert.Contains("100", result);
        Assert.Contains("200", result);
        Assert.Contains("800", result);
        Assert.Contains("600", result);
    }
}
