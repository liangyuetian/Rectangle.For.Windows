using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Rectangle.Windows.Views;

/// <summary>
/// 拖拽吸附预览窗口（Footprint）
/// 在拖拽窗口时显示将要吸附到的位置预览
/// </summary>
public class FootprintWindow : Form
{
    private static FootprintWindow? _instance;
    private static readonly object _lock = new();

    /// <summary>
    /// 预览窗口的透明度（0.0 - 1.0）
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public float Alpha { get; set; } = 0.3f;

    /// <summary>
    /// 边框宽度
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int BorderWidth { get; set; } = 2;

    /// <summary>
    /// 预览区域颜色
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color FillColor { get; set; } = Color.FromArgb(0, 120, 212);

    /// <summary>
    /// 边框颜色
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color BorderColor { get; set; } = Color.FromArgb(0, 120, 212);

    /// <summary>
    /// 获取单例实例
    /// </summary>
    public static FootprintWindow Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new FootprintWindow();
                }
            }
            return _instance;
        }
    }

    private FootprintWindow()
    {
        InitializeWindow();
    }

    private void InitializeWindow()
    {
        // 窗口样式：无边框、置顶、不显示在任务栏
        FormBorderStyle = FormBorderStyle.None;
        TopMost = true;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.Manual;

        // 允许透明
        AllowTransparency = true;
        TransparencyKey = Color.Empty;
        BackColor = Color.FromArgb(1, 1, 1); // 用于透明背景

        // 不抢焦点 - 通过 ShowWithoutActivation 属性实现
        // ShowActivated = false; // 在 .NET 9 中可能不可用

        // 双缓冲避免闪烁
        DoubleBuffered = true;

        // 设置窗口扩展样式
        Load += (s, e) => SetWindowStyle();
    }

    private void SetWindowStyle()
    {
        // WS_EX_TOOLWINDOW: 不在任务栏显示，不在Alt+Tab中显示
        // WS_EX_LAYERED: 支持透明度
        // WS_EX_TRANSPARENT: 鼠标点击穿透
        const int WS_EX_TOOLWINDOW = 0x00000080;
        const int WS_EX_LAYERED = 0x00080000;
        const int WS_EX_TRANSPARENT = 0x00000020;

        var exStyle = WS_EX_TOOLWINDOW | WS_EX_LAYERED | WS_EX_TRANSPARENT;
        NativeMethods.SetWindowLong(Handle, -20, exStyle);
    }

    /// <summary>
    /// 显示预览窗口在指定位置
    /// </summary>
    /// <param name="x">X 坐标</param>
    /// <param name="y">Y 坐标</param>
    /// <param name="width">宽度</param>
    /// <param name="height">高度</param>
    public void ShowPreview(int x, int y, int width, int height)
    {
        if (InvokeRequired)
        {
            Invoke(new Action(() => ShowPreview(x, y, width, height)));
            return;
        }

        SetBounds(x, y, width, height);

        if (!Visible)
        {
            Show();
        }

        Refresh();
    }

    /// <summary>
    /// 显示预览窗口（使用 System.Drawing.Rectangle）
    /// </summary>
    public void ShowPreview(System.Drawing.Rectangle bounds)
    {
        ShowPreview(bounds.X, bounds.Y, bounds.Width, bounds.Height);
    }

    /// <summary>
    /// 隐藏预览窗口
    /// </summary>
    public new void Hide()
    {
        if (InvokeRequired)
        {
            Invoke(new Action(Hide));
            return;
        }

        if (Visible)
        {
            base.Hide();
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var rect = new System.Drawing.Rectangle(0, 0, Width - 1, Height - 1);

        // 绘制半透明填充
        var fillColor = Color.FromArgb((int)(Alpha * 255), FillColor);
        using var fillBrush = new SolidBrush(fillColor);
        using var fillPath = CreateRoundedRectangle(rect, 4);
        g.FillPath(fillBrush, fillPath);

        // 绘制边框
        using var borderPen = new Pen(BorderColor, BorderWidth);
        using var borderPath = CreateRoundedRectangle(rect, 4);
        g.DrawPath(borderPen, borderPath);
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        // 不绘制背景，保持透明
    }

    protected override CreateParams CreateParams
    {
        get
        {
            var cp = base.CreateParams;
            // WS_EX_LAYERED: 支持透明度
            cp.ExStyle |= 0x00080000;
            return cp;
        }
    }

    /// <summary>
    /// 显示窗口时不激活（不抢焦点）
    /// </summary>
    protected override bool ShowWithoutActivation => true;

    private static GraphicsPath CreateRoundedRectangle(System.Drawing.Rectangle rect, int radius)
    {
        var path = new GraphicsPath();
        var diameter = radius * 2;

        path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
        path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
        path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();

        return path;
    }

    /// <summary>
    /// 配置预览窗口样式
    /// </summary>
    public void Configure(float? alpha = null, int? borderWidth = null, Color? fillColor = null, Color? borderColor = null)
    {
        if (alpha.HasValue) Alpha = alpha.Value;
        if (borderWidth.HasValue) BorderWidth = borderWidth.Value;
        if (fillColor.HasValue) FillColor = fillColor.Value;
        if (borderColor.HasValue) BorderColor = borderColor.Value;
    }

    /// <summary>
    /// 清理资源
    /// </summary>
    public new void Dispose()
    {
        if (Visible)
        {
            Hide();
        }
        base.Dispose();
        _instance = null;
    }
}

/// <summary>
/// Windows API 方法
/// </summary>
internal static class NativeMethods
{
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    internal static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    internal static extern int GetWindowLong(IntPtr hWnd, int nIndex);
}