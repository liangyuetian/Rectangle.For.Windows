using Rectangle.Windows.Core;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Rectangle.Windows.Views;

public class SnapPreviewWindow : Form
{
    private readonly SolidBrush _previewBrush;
    private readonly Pen _borderPen;

    public SnapPreviewWindow()
    {
        // 设置窗口样式
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.Manual;
        ShowInTaskbar = false;
        TopMost = true;
        Opacity = 0.5;
        BackColor = Color.FromArgb(0, 120, 212);

        _previewBrush = new SolidBrush(Color.FromArgb(0, 120, 212));
        _borderPen = new Pen(Color.FromArgb(0, 100, 200), 2);
    }

    public void ShowPreview(WindowRect rect)
    {
        SetBounds(rect.X, rect.Y, rect.Width, rect.Height);
        Show();
    }

    public void HidePreview()
    {
        Hide();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _previewBrush?.Dispose();
            _borderPen?.Dispose();
        }
        base.Dispose(disposing);
    }
}
