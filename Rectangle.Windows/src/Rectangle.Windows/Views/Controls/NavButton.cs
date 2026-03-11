using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Rectangle.Windows.Views.Controls;

/// <summary>
/// 导航按钮
/// </summary>
public class NavButton : Control
{
    public int Index { get; }

    private bool _isSelected;
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
                Invalidate();
            }
        }
    }

    private readonly string _icon;
    private readonly string _text;
    private bool _isHovered;

    private static readonly Color NormalColor = Color.Transparent;
    private static readonly Color HoverColor = Color.FromArgb(45, 45, 45);
    private static readonly Color SelectedColor = Color.FromArgb(55, 55, 55);

    private static readonly Font IconFont = new("Segoe UI Emoji", 11F);
    private static readonly Font TextFont = new("Microsoft YaHei UI", 9F);

    public NavButton(string icon, string text, int index)
    {
        _icon = icon;
        _text = text;
        Index = index;
        Height = 40;
        Cursor = Cursors.Hand;
        DoubleBuffered = true;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var bgColor = _isSelected ? SelectedColor : (_isHovered ? HoverColor : NormalColor);
        var rect = new System.Drawing.Rectangle(0, 0, Width - 1, Height - 1);

        using var path = SettingsTheme.CreateRoundedRect(rect, 6);
        using var brush = new SolidBrush(bgColor);
        g.FillPath(brush, path);

        if (_isSelected)
        {
            using var accentBrush = new SolidBrush(SettingsTheme.AccentColor);
            g.FillRectangle(accentBrush, 0, 8, 3, Height - 16);
        }

        using var textBrush = new SolidBrush(Color.White);
        g.DrawString(_icon, IconFont, textBrush, 12, 10);
        g.DrawString(_text, TextFont, textBrush, 38, 11);
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
