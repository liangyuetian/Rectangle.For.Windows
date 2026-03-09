using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Rectangle.Windows.Views.Controls;

/// <summary>
/// 吸附区域预览
/// </summary>
public class SnapAreaPreview : Control
{
    private static readonly Color ScreenColor = Color.FromArgb(35, 35, 35);
    private static readonly Color BorderColor = Color.FromArgb(80, 80, 80);
    private static readonly Color ZoneColor = Color.FromArgb(0, 120, 212);

    public SnapAreaPreview()
    {
        DoubleBuffered = true;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        // 屏幕背景
        var screenRect = new Rectangle(20, 20, Width - 40, Height - 40);
        using var screenBrush = new SolidBrush(ScreenColor);
        using var screenPen = new Pen(BorderColor, 2);
        g.FillRectangle(screenBrush, screenRect);
        g.DrawRectangle(screenPen, screenRect);

        // 吸附区域标注
        var zones = new[]
        {
            (new Rectangle(20, 20, 30, 40), "左上"),
            (new Rectangle(Width / 2 - 15, 20, 30, 30), "上"),
            (new Rectangle(Width - 70, 20, 30, 40), "右上"),
            (new Rectangle(20, Height / 2 - 15, 30, 30), "左"),
            (new Rectangle(Width - 70, Height / 2 - 15, 30, 30), "右"),
            (new Rectangle(20, Height - 80, 30, 40), "左下"),
            (new Rectangle(Width / 2 - 15, Height - 70, 30, 30), "下"),
            (new Rectangle(Width - 70, Height - 80, 30, 40), "右下")
        };

        using var zoneBrush = new SolidBrush(Color.FromArgb(40, ZoneColor));
        using var zonePen = new Pen(Color.FromArgb(100, ZoneColor), 1);
        using var textBrush = new SolidBrush(Color.FromArgb(120, 120, 120));

        foreach (var (rect, label) in zones)
        {
            g.FillRectangle(zoneBrush, rect);
            g.DrawRectangle(zonePen, rect);
        }
    }
}
