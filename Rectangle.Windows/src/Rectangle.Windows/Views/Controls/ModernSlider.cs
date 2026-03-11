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
    private static readonly Color TrackColor = Color.FromArgb(60, 60, 60);
    private static readonly Color FillColor = Color.FromArgb(0, 120, 212);
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

        var trackY = Height / 2 - 2;
        var trackRect = new System.Drawing.Rectangle(8, trackY, Width - 16, 4);
        using var trackBrush = new SolidBrush(TrackColor);
        g.FillRectangle(trackBrush, trackRect);

        var ratio = (float)(Value - Minimum) / (Maximum - Minimum);
        var fillWidth = (int)((Width - 16) * ratio);
        using var fillBrush = new SolidBrush(FillColor);
        g.FillRectangle(fillBrush, 8, trackY, fillWidth, 4);

        var thumbX = 8 + fillWidth - 8;
        using var thumbBrush = new SolidBrush(ThumbColor);
        g.FillEllipse(thumbBrush, thumbX, Height / 2 - 8, 16, 16);

        using var valueBrush = new SolidBrush(Color.FromArgb(150, 150, 150));
        g.DrawString($"{Value} px", Font, valueBrush, Width - 45, 7);
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
    }

    private void UpdateValue(int x)
    {
        var ratio = Math.Clamp((float)(x - 8) / (Width - 16), 0, 1);
        var newValue = (int)(Minimum + ratio * (Maximum - Minimum));
        if (newValue != Value)
        {
            Value = newValue;
            Invalidate();
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
