using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Rectangle.Windows.Views.Controls;

/// <summary>
/// 现代卡片
/// </summary>
public class ModernCard : Panel
{
    private bool _isHovered;

    public ModernCard()
    {
        DoubleBuffered = true;
        Padding = new Padding(16);
        
        // Wire events to controls so hovering children highlights the card
        WireHoverEvents(this);
        ControlAdded += (s, e) => WireHoverEvents(e.Control);
    }

    private void WireHoverEvents(Control? target)
    {
        if (target is null) return;
        target.MouseEnter += (s, e) => { _isHovered = true; Invalidate(); };
        target.MouseLeave += (s, e) => { _isHovered = false; Invalidate(); };
        foreach (Control c in target.Controls)
        {
            WireHoverEvents(c);
        }
        target.ControlAdded += (s, e) => WireHoverEvents(e.Control);
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

        var rect = new System.Drawing.Rectangle(0, 0, Width - 1, Height - 1);
        using var path = SettingsTheme.CreateRoundedRect(rect, 8);
        using var brush = new SolidBrush(_isHovered ? SettingsTheme.CardHoverColor : SettingsTheme.CardColor);
        using var pen = new Pen(SettingsTheme.BorderColor, 1);

        g.FillPath(brush, path);
        
        // Draw subtle highlight top edge
        if (!_isHovered) 
        {
            using var highlightPen = new Pen(Color.FromArgb(15, 255, 255, 255), 1);
            g.DrawArc(highlightPen, rect.X, rect.Y, 16, 16, 180, 90);
            g.DrawLine(highlightPen, rect.X + 8, rect.Y, rect.Right - 8, rect.Y);
            g.DrawArc(highlightPen, rect.Right - 16, rect.Y, 16, 16, 270, 90);
        }
        
        g.DrawPath(pen, path);
    }
}
