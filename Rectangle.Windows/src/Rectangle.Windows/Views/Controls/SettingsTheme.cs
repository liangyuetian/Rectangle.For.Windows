using System.Drawing;
using System.Drawing.Drawing2D;

namespace Rectangle.Windows.Views.Controls;

/// <summary>
/// 设置界面主题颜色和通用工具方法
/// </summary>
public static class SettingsTheme
{
    public static readonly Color BackgroundColor = Color.FromArgb(32, 32, 32);
    public static readonly Color CardColor = Color.FromArgb(45, 45, 45);
    public static readonly Color CardHoverColor = Color.FromArgb(55, 55, 55);
    public static readonly Color AccentColor = Color.FromArgb(0, 120, 212);
    public static readonly Color TextColor = Color.FromArgb(255, 255, 255);
    public static readonly Color SecondaryTextColor = Color.FromArgb(180, 180, 180);
    public static readonly Color BorderColor = Color.FromArgb(60, 60, 60);
    public static readonly Color InputBackColor = Color.FromArgb(38, 38, 38);
    public static readonly Color NavBackgroundColor = Color.FromArgb(28, 28, 28);

    public static GraphicsPath CreateRoundedRect(System.Drawing.Rectangle rect, int radius)
    {
        var path = new GraphicsPath();
        var d = radius * 2;
        path.AddArc(rect.X, rect.Y, d, d, 180, 90);
        path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
        path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }
}
