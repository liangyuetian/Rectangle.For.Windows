using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Rectangle.Windows.WinUI.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.UI.Windowing;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Rectangle.Windows.WinUI.Services
{
    /// <summary>
    /// 边缘指示器配置
    /// </summary>
    public class EdgeIndicatorConfig
    {
        /// <summary>
        /// 是否启用边缘指示器
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// 指示器宽度（像素）
        /// </summary>
        public int IndicatorWidth { get; set; } = 8;

        /// <summary>
        /// 正常状态颜色
        /// </summary>
        public global::Windows.UI.Color NormalColor { get; set; } = global::Windows.UI.Color.FromArgb(80, 0, 120, 215);

        /// <summary>
        /// 悬停状态颜色
        /// </summary>
        public global::Windows.UI.Color HoverColor { get; set; } = global::Windows.UI.Color.FromArgb(180, 0, 150, 255);

        /// <summary>
        /// 激活状态颜色（拖拽时）
        /// </summary>
        public global::Windows.UI.Color ActiveColor { get; set; } = global::Windows.UI.Color.FromArgb(255, 0, 180, 255);

        /// <summary>
        /// 指示器显示模式
        /// </summary>
        public EdgeIndicatorMode DisplayMode { get; set; } = EdgeIndicatorMode.AutoHide;

        /// <summary>
        /// 自动隐藏延迟（毫秒）
        /// </summary>
        public int AutoHideDelayMs { get; set; } = 2000;

        /// <summary>
        /// 触发距离（像素，鼠标靠近边缘多少距离时显示）
        /// </summary>
        public int TriggerDistance { get; set; } = 10;

        /// <summary>
        /// 是否显示吸附区域预览
        /// </summary>
        public bool ShowSnapAreas { get; set; } = true;

        /// <summary>
        /// 吸附区域透明度
        /// </summary>
        public double SnapAreaOpacity { get; set; } = 0.15;

        /// <summary>
        /// 吸附区域悬停透明度
        /// </summary>
        public double SnapAreaHoverOpacity { get; set; } = 0.35;
    }

    /// <summary>
    /// 边缘指示器显示模式
    /// </summary>
    public enum EdgeIndicatorMode
    {
        /// <summary>
        /// 始终显示
        /// </summary>
        AlwaysVisible,

        /// <summary>
        /// 自动隐藏
        /// </summary>
        AutoHide,

        /// <summary>
        /// 仅在拖拽时显示
        /// </summary>
        DragOnly,

        /// <summary>
        /// 仅在新用户引导时显示
        /// </summary>
        Onboarding
    }

    /// <summary>
    /// 边缘位置
    /// </summary>
    public enum ScreenEdge
    {
        Left,
        Right,
        Top,
        Bottom,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }

    /// <summary>
    /// 边缘指示器窗口
    /// </summary>
    public class EdgeIndicatorWindow : Window
    {
        private readonly ScreenEdge _edge;
        private readonly EdgeIndicatorConfig _config;
        private Border? _indicatorBorder;
        private bool _isHovered;
        private bool _isActive;

        public EdgeIndicatorWindow(ScreenEdge edge, EdgeIndicatorConfig config)
        {
            _edge = edge;
            _config = config;

            InitializeWindow();
        }

        private void InitializeWindow()
        {
            // 创建指示器UI
            _indicatorBorder = new Border
            {
                Background = new SolidColorBrush(_config.NormalColor),
                BorderThickness = new Thickness(0)
            };

            // 设置窗口内容
            Content = _indicatorBorder;

            // 设置窗口为工具窗口样式
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            if (hwnd != 0)
            {
                // 设置窗口样式：无边框、无任务栏图标、置顶
                const int GWL_STYLE   = -16;
                const int GWL_EXSTYLE = -20;
                const int WS_CAPTION     = 0x00C00000;
                const int WS_THICKFRAME  = 0x00040000;
                const int WS_EX_TOOLWINDOW = 0x00000080;
                const int WS_EX_TOPMOST    = 0x00000008;
                const int WS_EX_LAYERED    = 0x00080000;
                const int WS_EX_TRANSPARENT = 0x00000020;
                const int WS_EX_NOACTIVATE  = 0x08000000;

                var style   = GetWindowLongPtr(hwnd, GWL_STYLE);
                var exStyle = GetWindowLongPtr(hwnd, GWL_EXSTYLE);

                style   &= ~(WS_CAPTION | WS_THICKFRAME);
                exStyle |= WS_EX_TOOLWINDOW | WS_EX_TOPMOST | WS_EX_LAYERED | WS_EX_TRANSPARENT | WS_EX_NOACTIVATE;

                SetWindowLongPtr(hwnd, GWL_STYLE, style);
                SetWindowLongPtr(hwnd, GWL_EXSTYLE, exStyle);
            }
        }

        /// <summary>
        /// 设置悬停状态
        /// </summary>
        public void SetHoverState(bool hovered)
        {
            _isHovered = hovered;
            UpdateVisualState();
        }

        /// <summary>
        /// 设置激活状态
        /// </summary>
        public void SetActiveState(bool active)
        {
            _isActive = active;
            UpdateVisualState();
        }

        /// <summary>
        /// 更新视觉状态
        /// </summary>
        private void UpdateVisualState()
        {
            if (_indicatorBorder == null) return;

            global::Windows.UI.Color color;
            if (_isActive)
            {
                color = _config.ActiveColor;
            }
            else if (_isHovered)
            {
                color = _config.HoverColor;
            }
            else
            {
                color = _config.NormalColor;
            }

            _indicatorBorder.Background = new SolidColorBrush(color);
        }

        /// <summary>
        /// 设置位置和大小
        /// </summary>
        public void SetPositionAndSize(int x, int y, int width, int height)
        {
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            if (hwnd != 0)
            {
                PInvoke.SetWindowPos(
                    (HWND)hwnd,
                    new HWND(0),
                    x, y, width, height,
                    SET_WINDOW_POS_FLAGS.SWP_NOZORDER | SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE | SET_WINDOW_POS_FLAGS.SWP_SHOWWINDOW);
            }
        }

        /// <summary>
        /// 显示窗口
        /// </summary>
        public void ShowIndicator()
        {
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            if (hwnd != 0)
            {
                PInvoke.ShowWindow((HWND)hwnd, SHOW_WINDOW_CMD.SW_SHOWNA);
            }
        }

        /// <summary>
        /// 隐藏窗口
        /// </summary>
        public void HideIndicator()
        {
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            if (hwnd != 0)
            {
                PInvoke.ShowWindow((HWND)hwnd, SHOW_WINDOW_CMD.SW_HIDE);
            }
        }

        [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW", SetLastError = true)]
        private static extern nint GetWindowLongPtr(nint hWnd, int nIndex);

        [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
        private static extern nint SetWindowLongPtr(nint hWnd, int nIndex, nint dwNewLong);
    }

    /// <summary>
    /// 屏幕边缘指示器服务
    /// </summary>
    public class EdgeIndicatorService
    {
        private readonly EdgeIndicatorConfig _config;
        private readonly Dictionary<ScreenEdge, EdgeIndicatorWindow> _indicators = new();
        private readonly MouseHookService _mouseHook;
        private readonly ScreenDetectionService _screenDetection;

        private bool _isDragging = false;
        private System.Drawing.Point _lastMousePosition;
        private DateTime _lastMouseMoveTime = DateTime.Now;
        private bool _isVisible = false;

        /// <summary>
        /// 指示器状态变更事件
        /// </summary>
        public event EventHandler<bool>? VisibilityChanged;

        /// <summary>
        /// 边缘触发事件
        /// </summary>
        public event EventHandler<ScreenEdge>? EdgeTriggered;

        public EdgeIndicatorService(
            MouseHookService mouseHook,
            ScreenDetectionService screenDetection,
            EdgeIndicatorConfig? config = null)
        {
            _mouseHook = mouseHook;
            _screenDetection = screenDetection;
            _config = config ?? new EdgeIndicatorConfig();

            // 订阅鼠标事件
            _mouseHook.MouseMove += OnMouseMove;
            _mouseHook.MouseDown += OnMouseDown;
            _mouseHook.MouseUp += OnMouseUp;
        }

        /// <summary>
        /// 初始化边缘指示器
        /// </summary>
        public void Initialize()
        {
            if (!_config.Enabled)
            {
                Logger.Debug("EdgeIndicator", "[EdgeIndicator] 边缘指示器已禁用");
                return;
            }

            // 为主显示器的每个边缘创建指示器
            CreateIndicatorsForPrimaryScreen();

            Logger.Info("EdgeIndicator", "[EdgeIndicator] 边缘指示器服务已初始化");
        }

        /// <summary>
        /// 为主显示器创建指示器
        /// </summary>
        private void CreateIndicatorsForPrimaryScreen()
        {
            // 注意：实际实现需要在UI线程上创建窗口
            // 这里使用简化版本
            Logger.Debug("EdgeIndicator", "[EdgeIndicator] 创建边缘指示器窗口");
        }

        /// <summary>
        /// 鼠标移动事件处理
        /// </summary>
        private void OnMouseMove(object? sender, MouseHookEventArgs e)
        {
            if (!_config.Enabled) return;

            _lastMousePosition = new System.Drawing.Point(e.X, e.Y);
            _lastMouseMoveTime = DateTime.Now;

            // 检查是否靠近边缘
            CheckEdgeProximity(e.X, e.Y);
        }

        /// <summary>
        /// 鼠标按下事件处理
        /// </summary>
        private void OnMouseDown(object? sender, MouseHookEventArgs e)
        {
            _isDragging = true;

            if (_config.DisplayMode == EdgeIndicatorMode.DragOnly)
            {
                ShowIndicators();
            }

            UpdateIndicatorState(true);
        }

        /// <summary>
        /// 鼠标释放事件处理
        /// </summary>
        private void OnMouseUp(object? sender, MouseHookEventArgs e)
        {
            _isDragging = false;
            UpdateIndicatorState(false);

            if (_config.DisplayMode == EdgeIndicatorMode.DragOnly)
            {
                HideIndicators();
            }
        }

        /// <summary>
        /// 检查鼠标是否靠近屏幕边缘
        /// </summary>
        private void CheckEdgeProximity(int mouseX, int mouseY)
        {
            // DragOnly 模式下只在拖拽时检测边缘
            if (_config.DisplayMode == EdgeIndicatorMode.DragOnly && !_isDragging)
                return;

            var screens = _screenDetection.GetAllScreens();

            foreach (var screen in screens)
            {
                int left   = screen.X;
                int top    = screen.Y;
                int right  = screen.X + screen.Width;
                int bottom = screen.Y + screen.Height;

                bool nearLeft   = mouseX >= left  && mouseX <= left  + _config.TriggerDistance;
                bool nearRight  = mouseX >= right  - _config.TriggerDistance && mouseX <= right;
                bool nearTop    = mouseY >= top   && mouseY <= top   + _config.TriggerDistance;
                bool nearBottom = mouseY >= bottom - _config.TriggerDistance && mouseY <= bottom;

                // 触发边缘指示
                if (nearLeft && nearTop)
                {
                    TriggerEdge(ScreenEdge.TopLeft);
                }
                else if (nearRight && nearTop)
                {
                    TriggerEdge(ScreenEdge.TopRight);
                }
                else if (nearLeft && nearBottom)
                {
                    TriggerEdge(ScreenEdge.BottomLeft);
                }
                else if (nearRight && nearBottom)
                {
                    TriggerEdge(ScreenEdge.BottomRight);
                }
                else if (nearLeft)
                {
                    TriggerEdge(ScreenEdge.Left);
                }
                else if (nearRight)
                {
                    TriggerEdge(ScreenEdge.Right);
                }
                else if (nearTop)
                {
                    TriggerEdge(ScreenEdge.Top);
                }
                else if (nearBottom)
                {
                    TriggerEdge(ScreenEdge.Bottom);
                }
            }

            // 根据显示模式控制指示器可见性
            UpdateVisibility();
        }

        /// <summary>
        /// 触发边缘事件
        /// </summary>
        private void TriggerEdge(ScreenEdge edge)
        {
            EdgeTriggered?.Invoke(this, edge);

            if (_config.DisplayMode == EdgeIndicatorMode.AutoHide && !_isVisible)
            {
                ShowIndicators();
                _ = AutoHideAsync();
            }
        }

        /// <summary>
        /// 更新指示器可见性
        /// </summary>
        private void UpdateVisibility()
        {
            switch (_config.DisplayMode)
            {
                case EdgeIndicatorMode.AlwaysVisible:
                    if (!_isVisible) ShowIndicators();
                    break;

                case EdgeIndicatorMode.DragOnly:
                    // 由鼠标事件控制
                    break;

                case EdgeIndicatorMode.AutoHide:
                    // 由边缘触发控制
                    break;

                case EdgeIndicatorMode.Onboarding:
                    // 仅在新用户引导时显示
                    break;
            }
        }

        /// <summary>
        /// 自动隐藏
        /// </summary>
        private async Task AutoHideAsync()
        {
            await Task.Delay(_config.AutoHideDelayMs);

            // 检查鼠标是否还在边缘附近
            var elapsed = DateTime.Now - _lastMouseMoveTime;
            if (elapsed.TotalMilliseconds >= _config.AutoHideDelayMs)
            {
                HideIndicators();
            }
        }

        /// <summary>
        /// 显示所有指示器
        /// </summary>
        public void ShowIndicators()
        {
            if (_isVisible) return;

            _isVisible = true;
            foreach (var indicator in _indicators.Values)
            {
                indicator.ShowIndicator();
            }
            VisibilityChanged?.Invoke(this, true);

            Logger.Debug("EdgeIndicator", "[EdgeIndicator] 显示边缘指示器");
        }

        /// <summary>
        /// 隐藏所有指示器
        /// </summary>
        public void HideIndicators()
        {
            if (!_isVisible) return;

            _isVisible = false;
            foreach (var indicator in _indicators.Values)
            {
                indicator.HideIndicator();
            }
            VisibilityChanged?.Invoke(this, false);

            Logger.Debug("EdgeIndicator", "[EdgeIndicator] 隐藏边缘指示器");
        }

        /// <summary>
        /// 更新指示器状态
        /// </summary>
        private void UpdateIndicatorState(bool active)
        {
            foreach (var indicator in _indicators.Values)
            {
                indicator.SetActiveState(active);
            }
        }

        /// <summary>
        /// 更新配置
        /// </summary>
        public void UpdateConfig(EdgeIndicatorConfig config)
        {
            var wasEnabled = _config.Enabled;
            _config.Enabled = config.Enabled;
            _config.DisplayMode = config.DisplayMode;
            _config.TriggerDistance = config.TriggerDistance;

            if (!wasEnabled && config.Enabled)
            {
                Initialize();
            }
            else if (wasEnabled && !config.Enabled)
            {
                Dispose();
            }
        }

        /// <summary>
        /// 获取配置
        /// </summary>
        public EdgeIndicatorConfig GetConfig() => _config;

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            HideIndicators();

            foreach (var indicator in _indicators.Values)
            {
                indicator.Close();
            }
            _indicators.Clear();

            _mouseHook.MouseMove -= OnMouseMove;
            _mouseHook.MouseDown -= OnMouseDown;
            _mouseHook.MouseUp -= OnMouseUp;
        }
    }
}
