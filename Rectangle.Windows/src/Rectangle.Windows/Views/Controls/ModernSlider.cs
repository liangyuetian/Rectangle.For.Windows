using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Rectangle.Windows.Views.Controls;

/// <summary>
/// 现代滑块
/// </summary>
public class ModernSlider : Control
{
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int Value { get; set; }
    public int Minimum { get; }
    public int Maximum { get; }
    public event EventHandler? ValueChanged;

    private bool _isDragging;
    private bool _isHovered;
    private static readonly Color TrackColor = Color.FromArgb(60, 60, 60);
    private static readonly Color FillColor = SettingsTheme.AccentColor;
    private static readonly Color ThumbColor = Color.White;

    public ModernSlider(int min, int max)
    {
        Minimum = min;
        Maximum = max;
        Height = 30;
        DoubleBuffered = true;
        Cursor = Cursors.Hand;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        var availableWidth = Width - 60; // Leave room for text
        var trackY = Height / 2 - 2;
        var trackRect = new System.Drawing.Rectangle(8, trackY, availableWidth, 4);
        
        using var trackPath = SettingsTheme.CreateRoundedRect(trackRect, 2);
        using var trackBrush = new SolidBrush(TrackColor);
        g.FillPath(trackBrush, trackPath);

        var ratio = Maximum > Minimum ? (float)(Value - Minimum) / (Maximum - Minimum) : 0;
        var fillWidth = (int)(availableWidth * ratio);
        
        if (fillWidth > 0)
        {
            var fillRect = new System.Drawing.Rectangle(8, trackY, fillWidth, 4);
            using var fillPath = SettingsTheme.CreateRoundedRect(fillRect, 2);
            using var fillBrush = new SolidBrush(FillColor);
            g.FillPath(fillBrush, fillPath);
        }

        var thumbX = 8 + fillWidth - 8;
        if (thumbX < 8) thumbX = 8;
        
        // draw thumb shadow/border
        var thumbOuterRect = new System.Drawing.Rectangle(thumbX, Height / 2 - 8, 16, 16);
        using var thumbBorder = new SolidBrush(Color.FromArgb(50, 0, 0, 0));
        g.FillEllipse(thumbBorder, new System.Drawing.Rectangle(thumbX, Height / 2 - 7, 16, 16));
        
        using var thumbBrush = new SolidBrush(ThumbColor);
        g.FillEllipse(thumbBrush, thumbOuterRect);
        
        // draw hover inner detail
        if (_isHovered || _isDragging)
        {
            using var hoverBrush = new SolidBrush(Color.FromArgb(200, 200, 200));
            g.FillEllipse(hoverBrush, thumbX + 4, Height / 2 - 4, 8, 8);
        }

        using var valueBrush = new SolidBrush(SettingsTheme.SecondaryTextColor);
        g.DrawString($"{Value} px", Font, valueBrush, Width - 50, Height / 2 - Font.Height / 2);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        _isDragging = true;
        UpdateValue(e.X);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (_isDragging)
            UpdateValue(e.X);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        _isDragging = false;
        Invalidate();
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        _isHovered = true;
        Invalidate();
        base.OnMouseEnter(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        _isHovered = false;
        Invalidate();
        base.OnMouseLeave(e);
    }

    private void UpdateValue(int x)
    {
        var availableWidth = Width - 60;
        var ratio = Math.Clamp((float)(x - 8) / availableWidth, 0, 1);
        var newValue = (int)(Minimum + ratio * (Maximum - Minimum));
        if (newValue != Value)
        {
            Value = newValue;
            Invalidate();
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
