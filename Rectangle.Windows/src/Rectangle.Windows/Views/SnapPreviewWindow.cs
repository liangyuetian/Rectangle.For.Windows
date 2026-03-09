using Rectangle.Windows.Core;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Rectangle.Windows.Views;

public class SnapPreviewWindow : Form
{
    private static readonly Color PreviewColor = Color.FromArgb(0, 120, 212);
    private static readonly Color BorderColor = Color.FromArgb(0, 100, 200);
    private const int BorderWidth = 3;

    public SnapPreviewWindow()
    {
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.Manual;
        ShowInTaskbar = false;
        TopMost = true;
        Opacity = 0.4;
        BackColor = PreviewColor;
        DoubleBuffered = true;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        using var borderPen = new Pen(BorderColor, BorderWidth);
        e.Graphics.DrawRectangle(borderPen, BorderWidth / 2, BorderWidth / 2, 
            Width - BorderWidth, Height - BorderWidth);
    }

    public void ShowPreview(WindowRect rect)
    {
        SetBounds(rect.X, rect.Y, rect.Width, rect.Height);
        Invalidate();
        Show();
    }

    public void HidePreview()
    {
        Hide();
    }
}
