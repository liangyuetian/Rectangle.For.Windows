namespace Rectangle.Windows.WinUI.Core.Calculators;

// ============================================
// 四等分 (3/4宽度) 计算器 (3个)
// ============================================

public class FirstThreeFourthsCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var width = (workArea.Width - gap * 3) / 4 * 3 + gap * 2;
        return new WindowRect(workArea.Left + gap, workArea.Top + gap, width, workArea.Height - gap * 2);
    }
}

public class CenterThreeFourthsCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var width = (workArea.Width - gap * 3) / 4 * 3;
        var x = workArea.Left + (workArea.Width - width) / 2;
        return new WindowRect(x, workArea.Top + gap, width, workArea.Height - gap * 2);
    }
}

public class LastThreeFourthsCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var width = (workArea.Width - gap * 3) / 4 * 3 + gap * 2;
        var x = workArea.Left + workArea.Width - width - gap;
        return new WindowRect(x, workArea.Top + gap, width, workArea.Height - gap * 2);
    }
}

// ============================================
// 九等分计算器 (9个)
// ============================================

public class TopLeftNinthCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var width = (workArea.Width - gap * 2) / 3;
        var height = (workArea.Height - gap * 2) / 3;
        return new WindowRect(workArea.Left + gap, workArea.Top + gap, width, height);
    }
}

public class TopCenterNinthCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var width = (workArea.Width - gap * 2) / 3;
        var height = (workArea.Height - gap * 2) / 3;
        var x = workArea.Left + width + gap;
        return new WindowRect(x, workArea.Top + gap, width, height);
    }
}

public class TopRightNinthCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var width = (workArea.Width - gap * 2) / 3;
        var height = (workArea.Height - gap * 2) / 3;
        var x = workArea.Left + width * 2 + gap;
        return new WindowRect(x, workArea.Top + gap, width, height);
    }
}

public class MiddleLeftNinthCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var width = (workArea.Width - gap * 2) / 3;
        var height = (workArea.Height - gap * 2) / 3;
        var y = workArea.Top + height + gap;
        return new WindowRect(workArea.Left + gap, y, width, height);
    }
}

public class MiddleCenterNinthCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var width = (workArea.Width - gap * 2) / 3;
        var height = (workArea.Height - gap * 2) / 3;
        var x = workArea.Left + width + gap;
        var y = workArea.Top + height + gap;
        return new WindowRect(x, y, width, height);
    }
}

public class MiddleRightNinthCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var width = (workArea.Width - gap * 2) / 3;
        var height = (workArea.Height - gap * 2) / 3;
        var x = workArea.Left + width * 2 + gap;
        var y = workArea.Top + height + gap;
        return new WindowRect(x, y, width, height);
    }
}

public class BottomLeftNinthCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var width = (workArea.Width - gap * 2) / 3;
        var height = (workArea.Height - gap * 2) / 3;
        var y = workArea.Top + height * 2 + gap;
        return new WindowRect(workArea.Left + gap, y, width, height);
    }
}

public class BottomCenterNinthCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var width = (workArea.Width - gap * 2) / 3;
        var height = (workArea.Height - gap * 2) / 3;
        var x = workArea.Left + width + gap;
        var y = workArea.Top + height * 2 + gap;
        return new WindowRect(x, y, width, height);
    }
}

public class BottomRightNinthCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var width = (workArea.Width - gap * 2) / 3;
        var height = (workArea.Height - gap * 2) / 3;
        var x = workArea.Left + width * 2 + gap;
        var y = workArea.Top + height * 2 + gap;
        return new WindowRect(x, y, width, height);
    }
}

// ============================================
// 八等分计算器 (8个)
// ============================================

public class TopLeftEighthCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var width = (workArea.Width - gap * 3) / 4;
        var height = (workArea.Height - gap) / 2;
        return new WindowRect(workArea.Left + gap, workArea.Top + gap, width, height);
    }
}

public class TopCenterLeftEighthCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var width = (workArea.Width - gap * 3) / 4;
        var height = (workArea.Height - gap) / 2;
        var x = workArea.Left + width + gap * 2;
        return new WindowRect(x, workArea.Top + gap, width, height);
    }
}

public class TopCenterRightEighthCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var width = (workArea.Width - gap * 3) / 4;
        var height = (workArea.Height - gap) / 2;
        var x = workArea.Left + width * 2 + gap * 3;
        return new WindowRect(x, workArea.Top + gap, width, height);
    }
}

public class TopRightEighthCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var width = (workArea.Width - gap * 3) / 4;
        var height = (workArea.Height - gap) / 2;
        var x = workArea.Left + width * 3 + gap * 4;
        return new WindowRect(x, workArea.Top + gap, width, height);
    }
}

public class BottomLeftEighthCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var width = (workArea.Width - gap * 3) / 4;
        var height = (workArea.Height - gap) / 2;
        var y = workArea.Top + height + gap * 2;
        return new WindowRect(workArea.Left + gap, y, width, height);
    }
}

public class BottomCenterLeftEighthCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var width = (workArea.Width - gap * 3) / 4;
        var height = (workArea.Height - gap) / 2;
        var x = workArea.Left + width + gap * 2;
        var y = workArea.Top + height + gap * 2;
        return new WindowRect(x, y, width, height);
    }
}

public class BottomCenterRightEighthCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var width = (workArea.Width - gap * 3) / 4;
        var height = (workArea.Height - gap) / 2;
        var x = workArea.Left + width * 2 + gap * 3;
        var y = workArea.Top + height + gap * 2;
        return new WindowRect(x, y, width, height);
    }
}

public class BottomRightEighthCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var width = (workArea.Width - gap * 3) / 4;
        var height = (workArea.Height - gap) / 2;
        var x = workArea.Left + width * 3 + gap * 4;
        var y = workArea.Top + height + gap * 2;
        return new WindowRect(x, y, width, height);
    }
}

// ============================================
// 角落三分之一计算器 (4个)
// ============================================

public class TopLeftThirdCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var width = (workArea.Width - gap * 2) / 3;
        var height = (workArea.Height - gap * 2) / 3;
        return new WindowRect(workArea.Left + gap, workArea.Top + gap, width, height);
    }
}

public class TopRightThirdCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var width = (workArea.Width - gap * 2) / 3;
        var height = (workArea.Height - gap * 2) / 3;
        var x = workArea.Left + width * 2 + gap;
        return new WindowRect(x, workArea.Top + gap, width, height);
    }
}

public class BottomLeftThirdCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var width = (workArea.Width - gap * 2) / 3;
        var height = (workArea.Height - gap * 2) / 3;
        var y = workArea.Top + height * 2 + gap;
        return new WindowRect(workArea.Left + gap, y, width, height);
    }
}

public class BottomRightThirdCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var width = (workArea.Width - gap * 2) / 3;
        var height = (workArea.Height - gap * 2) / 3;
        var x = workArea.Left + width * 2 + gap;
        var y = workArea.Top + height * 2 + gap;
        return new WindowRect(x, y, width, height);
    }
}

// ============================================
// 垂直三分之一计算器 (5个)
// ============================================

public class TopVerticalThirdCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var height = (workArea.Height - gap * 2) / 3;
        return new WindowRect(workArea.Left + gap, workArea.Top + gap, workArea.Width - gap * 2, height);
    }
}

public class MiddleVerticalThirdCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var height = (workArea.Height - gap * 2) / 3;
        var y = workArea.Top + height + gap;
        return new WindowRect(workArea.Left + gap, y, workArea.Width - gap * 2, height);
    }
}

public class BottomVerticalThirdCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var height = (workArea.Height - gap * 2) / 3;
        var y = workArea.Top + height * 2 + gap;
        return new WindowRect(workArea.Left + gap, y, workArea.Width - gap * 2, height);
    }
}

public class TopVerticalTwoThirdsCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var height = (workArea.Height - gap * 2) / 3 * 2 + gap;
        return new WindowRect(workArea.Left + gap, workArea.Top + gap, workArea.Width - gap * 2, height);
    }
}

public class BottomVerticalTwoThirdsCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var height = (workArea.Height - gap * 2) / 3 * 2 + gap;
        var y = workArea.Top + workArea.Height - height - gap;
        return new WindowRect(workArea.Left + gap, y, workArea.Width - gap * 2, height);
    }
}

// ============================================
// 居中显著计算器
// ============================================

public class CenterProminentlyCalculator : IRectCalculator
{
    public WindowRect Calculate(WorkArea workArea, WindowRect currentWindow, WindowAction action, int gap = 0)
    {
        var width = (int)((workArea.Width - gap * 2) * 0.8);
        var height = (int)((workArea.Height - gap * 2) * 0.8);
        var x = workArea.Left + (workArea.Width - width) / 2;
        var y = workArea.Top + (workArea.Height - height) / 2;
        return new WindowRect(x, y, width, height);
    }
}
