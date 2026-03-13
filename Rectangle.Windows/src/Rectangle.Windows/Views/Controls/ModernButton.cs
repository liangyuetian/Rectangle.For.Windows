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
    private bool _isPressed;
    private static readonly Color NormalColor = SettingsTheme.CardColor;
    private static readonly Color HoverColor = SettingsTheme.CardHoverColor;
    private static readonly Color PressedColor = SettingsTheme.BackgroundColor;
    private static readonly Color BorderColor = SettingsTheme.BorderColor;

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

        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        var rect = new System.Drawing.Rectangle(0, 0, Width - 1, Height - 1);
        using var path = SettingsTheme.CreateRoundedRect(rect, 4);

        var bgColor = _isPressed ? PressedColor : (_isHovered ? HoverColor : NormalColor);
        using var brush = new SolidBrush(bgColor);
        using var pen = new Pen(_isHovered ? Color.FromArgb(80, 80, 80) : BorderColor, 1);

        g.FillPath(brush, path);
        g.DrawPath(pen, path);

        using var textBrush = new SolidBrush(SettingsTheme.TextColor);
        var textSize = g.MeasureString(Text, Font);
        g.DrawString(Text, Font, textBrush,
            (Width - textSize.Width) / 2,
            (Height - textSize.Height) / 2 + 1); // visually center correction
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        _isPressed = true;
        Invalidate();
        base.OnMouseDown(e);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        _isPressed = false;
        Invalidate();
        base.OnMouseUp(e);
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
