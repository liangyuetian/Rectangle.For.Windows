using System.Drawing;
using System.Drawing.Drawing2D;

namespace Rectangle.Windows.Views.Controls;

/// <summary>
/// 设置界面主题颜色和通用工具方法
/// </summary>
public static class SettingsTheme
{
    public static readonly Color BackgroundColor = Color.FromArgb(24, 24, 24); // Deeper background for contrast
    public static readonly Color CardColor = Color.FromArgb(38, 38, 38);       // Slightly lighter cards
    public static readonly Color CardHoverColor = Color.FromArgb(45, 45, 45);  // Subtle hover state
    public static readonly Color AccentColor = Color.FromArgb(0, 120, 212);    // Windows 11 Blue
    public static readonly Color AccentHoverColor = Color.FromArgb(0, 100, 190); 
    public static readonly Color TextColor = Color.FromArgb(250, 250, 250);
    public static readonly Color SecondaryTextColor = Color.FromArgb(160, 160, 160);
    public static readonly Color BorderColor = Color.FromArgb(50, 50, 50);     // Dim border
    public static readonly Color InputBackColor = Color.FromArgb(32, 32, 32);  // Inputs slightly darker than cards
    public static readonly Color NavBackgroundColor = Color.FromArgb(20, 20, 20);// Nav darkest

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
