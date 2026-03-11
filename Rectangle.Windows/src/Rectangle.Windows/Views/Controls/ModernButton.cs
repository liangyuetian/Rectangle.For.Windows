using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Rectangle.Windows.Views.Controls;

/// <summary>
/// 现代按钮
/// </summary>
public class ModernButton : Control
{
    private bool _isHovered;
    private static readonly Color NormalColor = Color.FromArgb(55, 55, 55);
    private static readonly Color HoverColor = Color.FromArgb(65, 65, 65);
    private static readonly Color BorderColor = Color.FromArgb(80, 80, 80);

    public ModernButton(string text)
    {
        Text = text;
        Height = 32;
        Cursor = Cursors.Hand;
        DoubleBuffered = true;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var rect = new System.Drawing.Rectangle(0, 0, Width - 1, Height - 1);
        using var path = SettingsTheme.CreateRoundedRect(rect, 6);

        var bgColor = _isHovered ? HoverColor : NormalColor;
        using var brush = new SolidBrush(bgColor);
        using var pen = new Pen(BorderColor, 1);

        g.FillPath(brush, path);
        g.DrawPath(pen, path);

        using var textBrush = new SolidBrush(Color.White);
        var textSize = g.MeasureString(Text, Font);
        g.DrawString(Text, Font, textBrush,
            (Width - textSize.Width) / 2,
            (Height - textSize.Height) / 2);
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        _isHovered = true;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        _isHovered = false;
        Invalidate();
    }
}
