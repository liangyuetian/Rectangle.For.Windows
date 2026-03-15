using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Rectangle.Windows.WinUI.Core;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Windows.UI;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Rectangle.Windows.WinUI.Views;

/// <summary>
/// 拖拽吸附预览窗口（Footprint）
/// 在拖拽窗口时显示将要吸附到的位置预览，支持淡入淡出动画
/// </summary>
public sealed partial class FootprintWindow : Window, IDisposable
{
    private static volatile FootprintWindow? _instance;
    private static readonly object _lock = new();

    private DispatcherTimer? _fadeTimer;
    private float _currentAlpha;
    private float _targetAlpha;
    private bool _isFadingIn;
    private int _animationDuration = 150; // 默认动画时长（毫秒）
    private bool _enableFade = true;
    private Stopwatch? _fadeStopwatch;
    private Border? _previewBorder;
    private bool _isDisposed;

    // Win32 API
    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW")]
    private static extern nint GetWindowLongPtr(nint hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW")]
    private static extern nint SetWindowLongPtr(nint hWnd, int nIndex, nint dwNewLong);

    [DllImport("user32.dll")]
    private static extern bool SetLayeredWindowAttributes(nint hwnd, uint crKey, byte bAlpha, uint dwFlags);

    /// <summary>
    /// 预览窗口的透明度（0.0 - 1.0）
    /// </summary>
    public float Alpha { get; set; } = 0.3f;

    /// <summary>
    /// 边框宽度
    /// </summary>
    public int BorderWidth { get; set; } = 2;

    /// <summary>
    /// 预览区域颜色
    /// </summary>
    public Color FillColor { get; set; } = ColorHelper.FromArgb(255, 0, 120, 212);

    /// <summary>
    /// 边框颜色
    /// </summary>
    public Color BorderColor { get; set; } = ColorHelper.FromArgb(255, 0, 120, 212);

    /// <summary>
    /// 是否启用淡入淡出动画
    /// </summary>
    public bool EnableFade
    {
        get => _enableFade;
        set => _enableFade = value;
    }

    /// <summary>
    /// 动画时长（毫秒）
    /// </summary>
    public int AnimationDuration
    {
        get => _animationDuration;
        set => _animationDuration = Math.Max(50, Math.Min(500, value));
    }

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
        // 创建预览边框
        _previewBorder = new Border
        {
            CornerRadius = new CornerRadius(4),
            Background = new SolidColorBrush(Color.FromArgb((byte)(Alpha * 255), FillColor.R, FillColor.G, FillColor.B)),
            BorderBrush = new SolidColorBrush(BorderColor),
            BorderThickness = new Thickness(BorderWidth)
        };

        this.Content = _previewBorder;

        // 设置窗口样式
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        SetWindowStyles(hwnd);
    }

    private void SetWindowStyles(nint hwnd)
    {
        const int GWL_EXSTYLE = -20;
        const int WS_EX_TOOLWINDOW = 0x00000080;
        const int WS_EX_NOACTIVATE = unchecked((int)0x08000000);
        const int WS_EX_TRANSPARENT = 0x00000020;
        const int WS_EX_LAYERED = unchecked((int)0x00080000);
        const uint LWA_ALPHA = 0x00000002;

        var exStyle = GetWindowLongPtr(hwnd, GWL_EXSTYLE);
        SetWindowLongPtr(hwnd, GWL_EXSTYLE,
            exStyle | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE | WS_EX_TRANSPARENT | WS_EX_LAYERED);

        // 设置初始透明度
        SetLayeredWindowAttributes(hwnd, 0, 0, LWA_ALPHA);
    }

    private void UpdateWindowOpacity()
    {
        const uint LWA_ALPHA = 0x00000002;
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        var opacity = (byte)(_currentAlpha * 255);
        SetLayeredWindowAttributes(hwnd, 0, opacity, LWA_ALPHA);
    }

    /// <summary>
    /// 显示预览窗口在指定位置
    /// </summary>
    public void ShowPreview(int x, int y, int width, int height)
    {
        // 参数验证
        if (width <= 0 || height <= 0)
        {
            throw new ArgumentException("宽度和高度必须大于 0");
        }

        if (_isDisposed) return;

        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);

        // 设置窗口位置和大小
        PInvoke.SetWindowPos(new HWND(hwnd), new HWND(0),
            x, y, width, height,
            SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE | SET_WINDOW_POS_FLAGS.SWP_SHOWWINDOW);

        // 检查窗口是否可见
        var isVisible = PInvoke.IsWindowVisible(new HWND(hwnd));

        if (!isVisible)
        {
            if (_enableFade)
            {
                StartFadeIn();
            }
            else
            {
                _currentAlpha = Alpha;
                UpdateWindowOpacity();
                PInvoke.ShowWindow(new HWND(hwnd), SHOW_WINDOW_CMD.SW_SHOWNA);
            }
        }
        else
        {
            // 更新透明度
            _currentAlpha = _enableFade ? _targetAlpha : Alpha;
            UpdateWindowOpacity();
        }
    }

    /// <summary>
    /// 显示预览窗口（使用 WindowRect）
    /// </summary>
    public void ShowPreview(WindowRect rect)
    {
        ShowPreview(rect.X, rect.Y, rect.Width, rect.Height);
    }

    /// <summary>
    /// 隐藏预览窗口
    /// </summary>
    public void HidePreview()
    {
        if (_isDisposed) return;

        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        var isVisible = PInvoke.IsWindowVisible(new HWND(hwnd));

        if (isVisible)
        {
            if (_enableFade)
            {
                StartFadeOut();
            }
            else
            {
                PInvoke.ShowWindow(new HWND(hwnd), SHOW_WINDOW_CMD.SW_HIDE);
            }
        }
    }

    /// <summary>
    /// 立即隐藏（无动画）
    /// </summary>
    public void HideImmediate()
    {
        if (_isDisposed) return;

        StopFadeTimer();
        _currentAlpha = 0;
        UpdateWindowOpacity();

        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        PInvoke.ShowWindow(new HWND(hwnd), SHOW_WINDOW_CMD.SW_HIDE);
    }

    private void StartFadeIn()
    {
        StopFadeTimer();

        _currentAlpha = 0;
        _targetAlpha = Alpha;
        _isFadingIn = true;

        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        PInvoke.ShowWindow(new HWND(hwnd), SHOW_WINDOW_CMD.SW_SHOWNA);

        _fadeStopwatch = Stopwatch.StartNew();

        _fadeTimer = new DispatcherTimer();
        _fadeTimer.Interval = TimeSpan.FromMilliseconds(16); // ~60fps
        _fadeTimer.Tick += FadeTimer_Tick;
        _fadeTimer.Start();
    }

    private void StartFadeOut()
    {
        StopFadeTimer();

        _currentAlpha = Alpha;
        _targetAlpha = 0;
        _isFadingIn = false;

        _fadeStopwatch = Stopwatch.StartNew();

        _fadeTimer = new DispatcherTimer();
        _fadeTimer.Interval = TimeSpan.FromMilliseconds(16); // ~60fps
        _fadeTimer.Tick += FadeTimer_Tick;
        _fadeTimer.Start();
    }

    private void FadeTimer_Tick(object? sender, object e)
    {
        if (_fadeStopwatch == null) return;

        var elapsed = _fadeStopwatch.Elapsed.TotalMilliseconds;
        var progress = Math.Min(1.0, elapsed / _animationDuration);

        // 使用 EaseOutQuad 缓动函数
        var easedProgress = 1 - (1 - progress) * (1 - progress);

        var startAlpha = _isFadingIn ? 0 : Alpha;
        var endAlpha = _isFadingIn ? Alpha : 0;

        _currentAlpha = (float)(startAlpha + (endAlpha - startAlpha) * easedProgress);

        // 更新窗口透明度
        UpdateWindowOpacity();

        if (progress >= 1.0)
        {
            StopFadeTimer();

            if (!_isFadingIn)
            {
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
                PInvoke.ShowWindow(new HWND(hwnd), SHOW_WINDOW_CMD.SW_HIDE);
            }
        }
    }

    private void StopFadeTimer()
    {
        if (_fadeTimer != null)
        {
            _fadeTimer.Stop();
            _fadeTimer.Tick -= FadeTimer_Tick;
            _fadeTimer = null;
        }

        if (_fadeStopwatch != null)
        {
            _fadeStopwatch.Stop();
            _fadeStopwatch = null;
        }
    }

    /// <summary>
    /// 配置预览窗口样式
    /// </summary>
    public void Configure(
        float? alpha = null,
        int? borderWidth = null,
        Color? fillColor = null,
        Color? borderColor = null,
        bool? enableFade = null,
        int? animationDuration = null)
    {
        if (_isDisposed) return;

        if (alpha.HasValue) Alpha = alpha.Value;
        if (borderWidth.HasValue) BorderWidth = borderWidth.Value;
        if (fillColor.HasValue) FillColor = fillColor.Value;
        if (borderColor.HasValue) BorderColor = borderColor.Value;
        if (enableFade.HasValue) EnableFade = enableFade.Value;
        if (animationDuration.HasValue) AnimationDuration = animationDuration.Value;

        // 更新 UI
        if (_previewBorder != null)
        {
            _previewBorder.Background = new SolidColorBrush(
                Color.FromArgb((byte)(Alpha * 255), FillColor.R, FillColor.G, FillColor.B));
            _previewBorder.BorderBrush = new SolidColorBrush(BorderColor);
            _previewBorder.BorderThickness = new Thickness(BorderWidth);
        }
    }

    /// <summary>
    /// 清理资源
    /// </summary>
    public void Dispose()
    {
        if (_isDisposed) return;

        StopFadeTimer();

        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        if (PInvoke.IsWindowVisible(new HWND(hwnd)))
        {
            PInvoke.ShowWindow(new HWND(hwnd), SHOW_WINDOW_CMD.SW_HIDE);
        }

        lock (_lock)
        {
            _instance = null;
        }

        _isDisposed = true;
        this.Close();
    }
}
