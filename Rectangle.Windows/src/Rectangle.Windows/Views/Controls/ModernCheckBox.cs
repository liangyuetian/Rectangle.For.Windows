using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Rectangle.Windows.Views.Controls;

/// <summary>
/// 现代复选框（iOS 风格开关）
/// </summary>
public class ModernCheckBox : Panel
{
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool Checked { get; set; }
    public event EventHandler? CheckedChanged;

    private readonly Label _titleLabel;
    private readonly Label _descLabel;
    private readonly Panel _switchBox;

    private static readonly Color CheckedColor = Color.FromArgb(0, 120, 212);
    private static readonly Color UncheckedColor = Color.FromArgb(60, 60, 60);

    public ModernCheckBox(string title, string description)
    {
        Width = 460;
        Height = 50;
        Cursor = Cursors.Hand;

        _titleLabel = new Label
        {
            Text = title,
            ForeColor = Color.White,
            Location = new Point(0, 5),
            AutoSize = true
        };
        Controls.Add(_titleLabel);

        _descLabel = new Label
        {
            Text = description,
            ForeColor = Color.FromArgb(150, 150, 150),
            Font = new Font("Microsoft YaHei UI", 8F),
            Location = new Point(0, 25),
            AutoSize = true
        };
        Controls.Add(_descLabel);

        _switchBox = new Panel
        {
            Size = new Size(44, 22),
            Location = new Point(416, 14)
        };
        _switchBox.Paint += SwitchBox_Paint;
        _switchBox.Click += Toggle;
        Controls.Add(_switchBox);

        _titleLabel.Click += Toggle;
        _descLabel.Click += Toggle;
        Click += Toggle;
    }

    private void Toggle(object? sender, EventArgs e)
    {
        Checked = !Checked;
        _switchBox.Invalidate();
        CheckedChanged?.Invoke(this, EventArgs.Empty);
    }

    private void SwitchBox_Paint(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var rect = new System.Drawing.Rectangle(0, 0, 43, 21);
        using var path = SettingsTheme.CreateRoundedRect(rect, 10);

        var bgColor = Checked ? CheckedColor : UncheckedColor;
        using var brush = new SolidBrush(bgColor);
        g.FillPath(brush, path);

        var circleX = Checked ? 24 : 3;
        using var circleBrush = new SolidBrush(Color.White);
        g.FillEllipse(circleBrush, circleX, 3, 16, 16);
    }
}
