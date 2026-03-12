using System;
using System.Drawing;
using System.Windows.Forms;

namespace Rectangle.Windows.Views;

/// <summary>
/// 预览窗口（Footprint）
/// 拖拽时显示目标位置的半透明预览
/// </summary>
public class FootprintWindow : Form
{
    private readonly Timer _fadeTimer;
    private float _opacity = 0.3f;
    private float _targetOpacity = 0.3f;
    private bool _isFading = false;

    public FootprintWindow()
    {
        // 设置窗口样式
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.Manual;
        ShowInTaskbar = false;
        TopMost = true;
        
        // 透明背景
        BackColor = Color.FromArgb(0, 120, 215); // Windows 蓝色
        TransparencyKey = Color.Empty;
        Opacity = 0.3;
        
        // 无边框，不获取焦点
        SetStyle(ControlStyles.SupportsTransparentBackColor, true);
        
        // 淡入淡出定时器
        _fadeTimer = new Timer();
        _fadeTimer.Interval = 16; // ~60fps
        _fadeTimer.Tick += OnFadeTick;
        
        // 双缓冲防止闪烁
        DoubleBuffered = true;
    }

    /// <summary>
    /// 显示预览窗口
    /// </summary>
    public void ShowPreview(Rectangle bounds, string? areaName = null)
    {
        if (InvokeRequired)
        {
            Invoke(() => ShowPreview(bounds, areaName));
            return;
        }

        // 设置位置和大小
        Bounds = bounds;
        
        // 显示窗口
        if (!Visible)
        {
            Show();
            _targetOpacity = 0.3f;
            _opacity = 0.0f;
            Opacity = 0;
            _isFading = true;
            _fadeTimer.Start();
        }
        
        Invalidate(); // 重绘
    }

    /// <summary>
    /// 隐藏预览窗口
    /// </summary>
    public void HidePreview()
    {
        if (InvokeRequired)
        {
            Invoke(HidePreview);
            return;
        }

        if (Visible)
        {
            _targetOpacity = 0.0f;
            _isFading = true;
            _fadeTimer.Start();
        }
    }

    /// <summary>
    /// 淡入淡出动画
    /// </summary>
    private void OnFadeTick(object? sender, EventArgs e)
    {
        if (!_isFading) return;

        float speed = 0.05f;
        
        if (_opacity < _targetOpacity)
        {
            _opacity += speed;
            if (_opacity >= _targetOpacity)
            {
                _opacity = _targetOpacity;
                _isFading = false;
                
                if (_opacity <= 0)
                {
                    Hide();
                    _fadeTimer.Stop();
                }
            }
        }
        else if (_opacity > _targetOpacity)
        {
            _opacity -= speed;
            if (_opacity <= _targetOpacity)
            {
                _opacity = _targetOpacity;
                _isFading = false;
                
                if (_opacity <= 0)
                {
                    Hide();
                    _fadeTimer.Stop();
                }
            }
        }
        else
        {
            _isFading = false;
            if (_opacity <= 0)
            {
                Hide();
            }
            _fadeTimer.Stop();
        }

        Opacity = _opacity;
    }

    /// <summary>
    /// 自定义绘制
    /// </summary>
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        
        // 绘制半透明填充
        using (var brush = new SolidBrush(Color.FromArgb(80, BackColor)))
        {
            g.FillRectangle(brush, ClientRectangle);
        }
        
        // 绘制边框
        using (var pen = new Pen(Color.FromArgb(200, BackColor), 2))
        {
            g.DrawRectangle(pen, 1, 1, Width - 2, Height - 2);
        }
    }

    /// <summary>
    /// 防止窗口获取焦点
    /// </summary>
    protected override bool ShowWithoutActivation => true;

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _fadeTimer?.Stop();
        _fadeTimer?.Dispose();
        base.OnFormClosing(e);
    }
}
