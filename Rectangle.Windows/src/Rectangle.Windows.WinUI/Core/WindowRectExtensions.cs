using Rectangle.Windows.WinUI.Services;

namespace Rectangle.Windows.WinUI.Core;

/// <summary>
/// WindowRect 扩展方法
/// </summary>
public static class WindowRectExtensions
{
    /// <summary>
    /// 应用最小窗口尺寸限制
    /// </summary>
    public static WindowRect ApplyMinimumSize(this WindowRect rect, ConfigService? configService)
    {
        if (configService == null) return rect;

        var config = configService.Load();

        float minWidth = config.MinimumWindowWidth;
        float minHeight = config.MinimumWindowHeight;

        // 如果配置为 0，表示无限制
        if (minWidth <= 0 && minHeight <= 0) return rect;

        int newWidth = rect.Width;
        int newHeight = rect.Height;

        // 应用最小宽度限制
        if (minWidth > 0 && rect.Width < minWidth)
        {
            newWidth = (int)minWidth;
        }

        // 应用最小高度限制
        if (minHeight > 0 && rect.Height < minHeight)
        {
            newHeight = (int)minHeight;
        }

        // 如果尺寸改变，保持窗口中心位置不变
        if (newWidth != rect.Width || newHeight != rect.Height)
        {
            int centerX = rect.X + rect.Width / 2;
            int centerY = rect.Y + rect.Height / 2;

            int newX = centerX - newWidth / 2;
            int newY = centerY - newHeight / 2;

            return new WindowRect(newX, newY, newWidth, newHeight);
        }

        return rect;
    }

    /// <summary>
    /// 确保窗口在屏幕工作区内
    /// </summary>
    public static WindowRect ClampToWorkArea(this WindowRect rect, WorkArea workArea)
    {
        int x = rect.X;
        int y = rect.Y;
        int width = rect.Width;
        int height = rect.Height;

        // 确保不超出右边界
        if (x + width > workArea.Right)
        {
            x = workArea.Right - width;
        }

        // 确保不超出下边界
        if (y + height > workArea.Bottom)
        {
            y = workArea.Bottom - height;
        }

        // 确保不超出左边界
        if (x < workArea.Left)
        {
            x = workArea.Left;
        }

        // 确保不超出上边界
        if (y < workArea.Top)
        {
            y = workArea.Top;
        }

        // 如果窗口比工作区还大，调整大小
        if (width > workArea.Width)
        {
            width = workArea.Width;
            x = workArea.Left;
        }

        if (height > workArea.Height)
        {
            height = workArea.Height;
            y = workArea.Top;
        }

        return new WindowRect(x, y, width, height);
    }
}
