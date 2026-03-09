using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Rectangle.Windows.Views.Controls;

/// <summary>
/// 现代卡片
/// </summary>
public class ModernCard : Panel
{
    public ModernCard()
    {
        DoubleBuffered = true;
        Padding = new Padding(16);
    }

    public void SetContent(Control content)
    {
        content.Dock = DockStyle.Fill;
        Controls.Add(content);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        using var path = SettingsTheme.CreateRoundedRect(rect, 8);
        using var brush = new SolidBrush(SettingsTheme.CardColor);
        using var pen = new Pen(SettingsTheme.BorderColor, 1);

        g.FillPath(brush, path);
        g.DrawPath(pen, path);
    }
}
