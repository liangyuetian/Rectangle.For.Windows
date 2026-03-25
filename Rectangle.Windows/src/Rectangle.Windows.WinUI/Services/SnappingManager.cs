using System;
using System.Drawing;
using System.Runtime.InteropServices;
using Rectangle.Windows.WinUI.Core;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Rectangle.Windows.WinUI.Services;

/// <summary>
/// 拖拽吸附管理器
/// 管理窗口拖拽吸附的完整流程
/// </summary>
public class SnappingManager : IDisposable
{
    private readonly MouseHookService _mouseHook;
    private readonly Win32WindowService _win32;
    private readonly ConfigService _configService;
    private readonly DragState _dragState;
    private readonly WindowHistory _history;
    private WindowManager? _windowManager;
    private Views.SnapPreviewWindow? _previewWindow;

    // 配置
    private bool _isEnabled = true;
    private int _edgeMarginTop = 5;
    private int _edgeMarginBottom = 5;
    private int _edgeMarginLeft = 5;
    private int _edgeMarginRight = 5;
    private int _cornerSize = 20;
    private int _snapModifiers = 0;

    // 缓存配置
    private AppConfig? _cachedConfig;
    private DateTime _configLastLoadTime = DateTime.MinValue;
    private readonly TimeSpan _configCacheDuration = TimeSpan.FromSeconds(5);

    // 性能优化：帧率限制
    private DateTime _lastUpdateTime = DateTime.MinValue;
    private readonly int _updateIntervalMs = 16; // ~60fps
    private SnapArea? _lastSnapArea;

    // 常量定义
    private const int MinimumDragDistance = 5;
    private const int ValidDragDistance = 10;
    private const int ValidDragDurationMs = 100;
    private const int PositionChangeThreshold = 5;

    // 事件
    public event EventHandler<SnapEventArgs>? SnapTriggered;
    public event EventHandler? DragStarted;
    public event EventHandler? DragEnded;

    public SnappingManager(Win32WindowService win32, ConfigService configService, WindowHistory history)
    {
        _win32 = win32;
        _configService = configService;
        _history = history;
        _mouseHook = new MouseHookService();
        _dragState = new DragState();

        // 订阅鼠标事件
        _mouseHook.MouseDown += OnMouseDown;
        _mouseHook.MouseUp += OnMouseUp;
        _mouseHook.MouseMove += OnMouseMove;
    }

    /// <summary>
    /// 设置 WindowManager 实例（用于执行吸附操作）
    /// </summary>
    public void SetWindowManager(WindowManager windowManager)
    {
        _windowManager = windowManager;
    }

    /// <summary>
    /// 启用拖拽吸附
    /// </summary>
    public bool Enable()
    {
        if (_isEnabled) return true;

        LoadConfig();

        // 检查是否启用了拖拽吸附
        var config = GetCachedConfig();
        if (config?.DragToSnap == false)
        {
            Logger.Info("SnappingManager", "拖拽吸附已禁用（配置）");
            return false;
        }

        _isEnabled = true;
        Logger.Info("SnappingManager", "拖拽吸附已启用");
        return true;
    }

    /// <summary>
    /// 获取缓存的配置，如果缓存过期则重新加载
    /// </summary>
    private AppConfig? GetCachedConfig()
    {
        var now = DateTime.Now;
        if (_cachedConfig == null || (now - _configLastLoadTime) > _configCacheDuration)
        {
            _cachedConfig = _configService?.Load();
            _configLastLoadTime = now;
        }
        return _cachedConfig;
    }

    /// <summary>
    /// 清除配置缓存，强制下次重新加载
    /// </summary>
    public void InvalidateConfigCache()
    {
        _cachedConfig = null;
        _configLastLoadTime = DateTime.MinValue;
    }

    /// <summary>
    /// 禁用拖拽吸附
    /// </summary>
    public void Disable()
    {
        if (!_isEnabled) return;

        _isEnabled = false;
        HideSnapPreview();
        _dragState.Reset();
        Logger.Info("SnappingManager", "拖拽吸附已禁用");
    }

    /// <summary>
    /// 加载配置
    /// </summary>
    private void LoadConfig()
    {
        var config = _configService?.Load();
        if (config == null) return;

        _edgeMarginTop = config.SnapEdgeMarginTop;
        _edgeMarginBottom = config.SnapEdgeMarginBottom;
        _edgeMarginLeft = config.SnapEdgeMarginLeft;
        _edgeMarginRight = config.SnapEdgeMarginRight;
        _cornerSize = config.CornerSnapAreaSize;
        _snapModifiers = config.SnapModifiers;
    }

    /// <summary>
    /// 鼠标按下事件
    /// </summary>
    private void OnMouseDown(object? sender, MouseHookEventArgs e)
    {
        // 只处理左键
        if (e.Button != MouseButton.Left)
            return;

        // 获取光标下的窗口
        var hwnd = GetWindowUnderCursor(e.X, e.Y);
        if (hwnd == 0) return;

        // 检查是否是有效窗口
        if (!IsValidWindowForDragging(hwnd))
            return;

        // 获取窗口矩形
        var (x, y, w, h) = _win32.GetWindowRect(hwnd);

        // 开始拖拽
        _dragState.Reset();
        _dragState.IsDragging = true;
        _dragState.DraggedWindow = hwnd;
        _dragState.InitialMousePos = new Point(e.X, e.Y);
        _dragState.InitialWindowRect = new WindowRect(x, y, w, h);
        _dragState.DragStartTime = DateTime.Now;
        _dragState.DragButton = MouseButton.Left;

        // Unsnap 恢复：检测窗口是否被程序调整过
        // 如果是，从历史记录获取原始尺寸保存到 OriginalRect
        var config = GetCachedConfig();
        if (config?.UnsnapRestore == true && _history.IsProgramAdjusted(hwnd))
        {
            if (_history.TryGetRestoreRect(hwnd, out var restoreRect))
            {
                _dragState.OriginalRect = new WindowRect(
                    restoreRect.X, restoreRect.Y, restoreRect.W, restoreRect.H);
                Logger.Info("SnappingManager", $"Unsnap: 检测到已吸附窗口，保存原始位置 ({restoreRect.X}, {restoreRect.Y}, {restoreRect.W}, {restoreRect.H})");
            }
        }
        else
        {
            // 保存当前位置作为原始位置
            _dragState.OriginalRect = new WindowRect(x, y, w, h);
        }

        DragStarted?.Invoke(this, EventArgs.Empty);

        Logger.Debug("SnappingManager", $"开始拖拽窗口: {hwnd}");
    }

    /// <summary>
    /// 鼠标移动事件
    /// </summary>
    private void OnMouseMove(object? sender, MouseHookEventArgs e)
    {
        if (!_dragState.IsDragging) return;

        // 帧率限制：检查是否应该更新
        var now = DateTime.Now;
        var elapsed = (now - _lastUpdateTime).TotalMilliseconds;
        if (elapsed < _updateIntervalMs)
            return;
        _lastUpdateTime = now;

        _dragState.CurrentMousePos = new Point(e.X, e.Y);

        // 检查拖拽距离，如果太小可能是点击而不是拖拽
        if (_dragState.GetDragDistance() < MinimumDragDistance)
            return;

        // 计算吸附区域
        var snapArea = CalculateSnapArea(new Point(e.X, e.Y));

        // 检测吸附区域是否变化
        bool snapAreaChanged = !SnapAreaEquals(snapArea, _lastSnapArea);

        if (snapArea != null)
        {
            _dragState.CurrentSnapArea = snapArea;

            // 显示预览窗口
            if (snapAreaChanged)
            {
                ShowSnapPreview(snapArea);
                Logger.Debug("SnappingManager", $"检测到吸附区域: {snapArea.Name}");
            }
        }
        else
        {
            _dragState.CurrentSnapArea = null;

            // 隐藏预览窗口
            if (_lastSnapArea != null)
            {
                HideSnapPreview();
            }
        }

        _lastSnapArea = snapArea;
    }

    /// <summary>
    /// 显示吸附预览窗口
    /// </summary>
    private void ShowSnapPreview(SnapArea snapArea)
    {
        if (_windowManager == null) return;

        var hwnd = _dragState.DraggedWindow;
        if (hwnd == 0) return;

        // 获取目标工作区
        var workArea = GetWorkAreaFromPoint(_dragState.CurrentMousePos);
        if (!workArea.HasValue) return;

        // 计算预览窗口位置
        var calculator = new CalculatorFactory(_configService).GetCalculator(snapArea.Action);
        if (calculator == null) return;

        var config = GetCachedConfig();
        var previewRect = calculator.Calculate(workArea.Value, default, snapArea.Action, config?.GapSize ?? 0);

        _previewWindow ??= new Views.SnapPreviewWindow();
        _previewWindow.ShowPreview(previewRect);
        Logger.Debug("SnappingManager", $"预览窗口位置: ({previewRect.X}, {previewRect.Y}, {previewRect.Width}, {previewRect.Height})");
    }

    /// <summary>
    /// 隐藏吸附预览窗口
    /// </summary>
    private void HideSnapPreview()
    {
        try
        {
            _previewWindow?.HidePreview();
        }
        catch { }
    }

    /// <summary>
    /// 比较两个吸附区域是否相同
    /// </summary>
    private static bool SnapAreaEquals(SnapArea? a, SnapArea? b)
    {
        if (a == null && b == null) return true;
        if (a == null || b == null) return false;
        return a.Action == b.Action && a.Type == b.Type;
    }

    /// <summary>
    /// 鼠标释放事件
    /// </summary>
    private void OnMouseUp(object? sender, MouseHookEventArgs e)
    {
        if (!_dragState.IsDragging) return;

        var hwnd = _dragState.DraggedWindow;

        // 检查是否是有效拖拽（持续时间 > 100ms 或距离 > 10px）
        bool isValidDrag = _dragState.GetDragDurationMs() > ValidDragDurationMs ||
                          _dragState.GetDragDistance() > ValidDragDistance;

        var config = GetCachedConfig();

        if (isValidDrag && _dragState.CurrentSnapArea != null)
        {
            // 执行吸附
            ExecuteSnap(_dragState.CurrentSnapArea);
        }
        else if (isValidDrag && config?.UnsnapRestore == true && _dragState.OriginalRect.HasValue)
        {
            // Unsnap 恢复：拖拽结束但未吸附，恢复原始尺寸
            var originalRect = _dragState.OriginalRect.Value;

            // 检查窗口当前位置是否与原始位置不同（说明用户确实拖拽了）
            var (currentX, currentY, currentW, currentH) = _win32.GetWindowRect(hwnd);
            bool positionChanged = Math.Abs(currentX - originalRect.X) > PositionChangeThreshold ||
                                   Math.Abs(currentY - originalRect.Y) > PositionChangeThreshold;

            if (positionChanged)
            {
                // 恢复窗口到原始尺寸
                _win32.SetWindowRect(hwnd, originalRect.X, originalRect.Y, originalRect.Width, originalRect.Height);

                // 清除程序调整标记
                _history.ClearProgramAdjustedMark(hwnd);

                Logger.Info("SnappingManager", $"Unsnap 恢复: 恢复窗口到原始位置 ({originalRect.X}, {originalRect.Y}, {originalRect.Width}, {originalRect.Height})");
            }
        }

        DragEnded?.Invoke(this, EventArgs.Empty);

        Logger.Debug("SnappingManager", $"结束拖拽窗口: {hwnd}");

        HideSnapPreview();
        _dragState.Reset();
    }

    /// <summary>
    /// 计算吸附区域
    /// </summary>
    private SnapArea? CalculateSnapArea(Point cursorPos)
    {
        // 获取光标所在的屏幕
        var workArea = GetWorkAreaFromPoint(cursorPos);
        if (!workArea.HasValue) return null;

        // 检查屏幕边缘
        var snapArea = CheckScreenEdges(cursorPos, workArea);
        if (snapArea != null) return snapArea;

        // 检查屏幕角落
        snapArea = CheckScreenCorners(cursorPos, workArea);
        if (snapArea != null) return snapArea;

        return null;
    }

    /// <summary>
    /// 检查屏幕边缘吸附
    /// </summary>
    private SnapArea? CheckScreenEdges(Point cursorPos, WorkArea? workArea)
    {
        if (workArea == null) return null;

        // 左边缘
        if (cursorPos.X >= workArea.Value.Left && cursorPos.X <= workArea.Value.Left + _edgeMarginLeft)
        {
            return new SnapArea
            {
                Bounds = new System.Drawing.Rectangle(workArea.Value.Left, workArea.Value.Top, _edgeMarginLeft, workArea.Value.Height),
                Action = WindowAction.LeftHalf,
                Type = SnapAreaType.Edge,
                Name = "Left Edge"
            };
        }

        // 右边缘
        if (cursorPos.X >= workArea.Value.Right - _edgeMarginRight && cursorPos.X <= workArea.Value.Right)
        {
            return new SnapArea
            {
                Bounds = new System.Drawing.Rectangle(workArea.Value.Right - _edgeMarginRight, workArea.Value.Top, _edgeMarginRight, workArea.Value.Height),
                Action = WindowAction.RightHalf,
                Type = SnapAreaType.Edge,
                Name = "Right Edge"
            };
        }

        // 上边缘
        if (cursorPos.Y >= workArea.Value.Top && cursorPos.Y <= workArea.Value.Top + _edgeMarginTop)
        {
            return new SnapArea
            {
                Bounds = new System.Drawing.Rectangle(workArea.Value.Left, workArea.Value.Top, workArea.Value.Width, _edgeMarginTop),
                Action = WindowAction.Maximize,
                Type = SnapAreaType.Edge,
                Name = "Top Edge"
            };
        }

        // 下边缘
        if (cursorPos.Y >= workArea.Value.Bottom - _edgeMarginBottom && cursorPos.Y <= workArea.Value.Bottom)
        {
            return new SnapArea
            {
                Bounds = new System.Drawing.Rectangle(workArea.Value.Left, workArea.Value.Bottom - _edgeMarginBottom, workArea.Value.Width, _edgeMarginBottom),
                Action = WindowAction.BottomHalf,
                Type = SnapAreaType.Edge,
                Name = "Bottom Edge"
            };
        }

        return null;
    }

    /// <summary>
    /// 检查屏幕角落吸附
    /// </summary>
    private SnapArea? CheckScreenCorners(Point cursorPos, WorkArea? workArea)
    {
        if (workArea == null) return null;

        // 左上角
        if (cursorPos.X <= workArea.Value.Left + _cornerSize &&
            cursorPos.Y <= workArea.Value.Top + _cornerSize)
        {
            return new SnapArea
            {
                Bounds = new System.Drawing.Rectangle(workArea.Value.Left, workArea.Value.Top, _cornerSize, _cornerSize),
                Action = WindowAction.TopLeft,
                Type = SnapAreaType.Corner,
                Name = "Top Left Corner"
            };
        }

        // 右上角
        if (cursorPos.X >= workArea.Value.Right - _cornerSize &&
            cursorPos.Y <= workArea.Value.Top + _cornerSize)
        {
            return new SnapArea
            {
                Bounds = new System.Drawing.Rectangle(workArea.Value.Right - _cornerSize, workArea.Value.Top, _cornerSize, _cornerSize),
                Action = WindowAction.TopRight,
                Type = SnapAreaType.Corner,
                Name = "Top Right Corner"
            };
        }

        // 左下角
        if (cursorPos.X <= workArea.Value.Left + _cornerSize &&
            cursorPos.Y >= workArea.Value.Bottom - _cornerSize)
        {
            return new SnapArea
            {
                Bounds = new System.Drawing.Rectangle(workArea.Value.Left, workArea.Value.Bottom - _cornerSize, _cornerSize, _cornerSize),
                Action = WindowAction.BottomLeft,
                Type = SnapAreaType.Corner,
                Name = "Bottom Left Corner"
            };
        }

        // 右下角
        if (cursorPos.X >= workArea.Value.Right - _cornerSize &&
            cursorPos.Y >= workArea.Value.Bottom - _cornerSize)
        {
            return new SnapArea
            {
                Bounds = new System.Drawing.Rectangle(workArea.Value.Right - _cornerSize, workArea.Value.Bottom - _cornerSize, _cornerSize, _cornerSize),
                Action = WindowAction.BottomRight,
                Type = SnapAreaType.Corner,
                Name = "Bottom Right Corner"
            };
        }

        return null;
    }

    /// <summary>
    /// 获取点所在的屏幕工作区
    /// </summary>
    private WorkArea? GetWorkAreaFromPoint(Point point)
    {
        try
        {
            var pt = new System.Drawing.Point(point.X, point.Y);
            var hMonitor = PInvoke.MonitorFromPoint(pt, MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST);
            var mi = new MONITORINFO();
            mi.cbSize = (uint)Marshal.SizeOf<MONITORINFO>();

            if (PInvoke.GetMonitorInfo(hMonitor, ref mi))
            {
                var rcWork = mi.rcWork;
                return new WorkArea(rcWork.left, rcWork.top, rcWork.right, rcWork.bottom);
            }
        }
        catch (Exception ex)
        {
            Logger.Warning("SnappingManager", $"获取屏幕工作区失败: {ex.Message}");
        }

        return null;
    }

    /// <summary>
    /// 执行吸附
    /// </summary>
    private void ExecuteSnap(SnapArea snapArea)
    {
        var hwnd = _dragState.DraggedWindow;
        if (hwnd == 0) return;

        // 保存原始位置（用于恢复）
        if (_dragState.OriginalRect.HasValue)
        {
            var orig = _dragState.OriginalRect.Value;
            _history.SaveRestoreRect(hwnd,
                orig.X,
                orig.Y,
                orig.Width,
                orig.Height);
        }

        // 标记窗口为由程序调整
        _history.MarkAsProgramAdjusted(hwnd);

        // 执行窗口操作
        if (_windowManager != null)
        {
            _windowManager.Execute(snapArea.Action, hwnd, forceDirectAction: true);
            Logger.Info("SnappingManager", $"执行吸附: {snapArea.Name} -> {snapArea.Action}");
        }
        else
        {
            // 如果没有 WindowManager，触发事件让外部处理
            SnapTriggered?.Invoke(this, new SnapEventArgs
            {
                WindowHandle = hwnd,
                Action = snapArea.Action,
                SnapArea = snapArea
            });
            Logger.Debug("SnappingManager", $"触发吸附事件: {snapArea.Name} -> {snapArea.Action}");
        }
    }

    /// <summary>
    /// 检查窗口是否适合拖拽
    /// </summary>
    private unsafe bool IsValidWindowForDragging(nint hwnd)
    {
        // 检查窗口是否可见
        if (!PInvoke.IsWindowVisible(new HWND((void*)hwnd)))
            return false;

        // 窗口样式常量
        const int GWL_STYLE = -16;
        const int GWL_EXSTYLE = -20;
        const uint WS_CAPTION = 0x00C00000;
        const uint WS_EX_TOOLWINDOW = 0x00000080;
        const uint WS_EX_NOACTIVATE = 0x08000000;

        // 检查窗口是否有标题栏（排除桌面、任务栏等）
        var style = (uint)GetWindowLong(hwnd, GWL_STYLE);
        var exStyle = (uint)GetWindowLong(hwnd, GWL_EXSTYLE);

        // 排除无标题栏窗口（如桌面、某些工具窗口）
        if ((style & WS_CAPTION) != WS_CAPTION)
            return false;

        // 排除弹出式菜单、工具提示等
        if ((exStyle & WS_EX_TOOLWINDOW) == WS_EX_TOOLWINDOW)
            return false;

        // 排除无激活窗口（如某些系统窗口）
        if ((exStyle & WS_EX_NOACTIVATE) == WS_EX_NOACTIVATE)
            return false;

        return true;
    }

    [DllImport("user32.dll", EntryPoint = "GetWindowLongW")]
    private static extern nint GetWindowLong32(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW")]
    private static extern nint GetWindowLong64(IntPtr hWnd, int nIndex);

    private static nint GetWindowLong(nint hWnd, int nIndex)
    {
        if (nint.Size == 8)
            return GetWindowLong64((nint)hWnd, nIndex);
        else
            return GetWindowLong32((nint)hWnd, nIndex);
    }

    /// <summary>
    /// 获取光标下的窗口
    /// </summary>
    private unsafe nint GetWindowUnderCursor(int x, int y)
    {
        var pt = new System.Drawing.Point(x, y);
        var hwnd = PInvoke.WindowFromPoint(pt);

        if ((nint)hwnd.Value == 0) return 0;

        // 获取根窗口
        const uint GA_ROOT = 2;
        var rootHwnd = PInvoke.GetAncestor(hwnd, (GET_ANCESTOR_FLAGS)GA_ROOT);

        return (nint)rootHwnd.Value != 0 ? (nint)rootHwnd.Value : (nint)hwnd.Value;
    }

    public void Dispose()
    {
        Disable();
        try
        {
            _previewWindow?.Dispose();
            _previewWindow = null;
        }
        catch { }
        _mouseHook?.Dispose();
    }
}

/// <summary>
/// 吸附事件参数
/// </summary>
public class SnapEventArgs : EventArgs
{
    public nint WindowHandle { get; set; }
    public WindowAction Action { get; set; }
    public SnapArea SnapArea { get; set; } = new SnapArea();
}

/// <summary>
/// 吸附区域
/// </summary>
public class SnapArea
{
    public System.Drawing.Rectangle Bounds { get; set; }
    public WindowAction Action { get; set; }
    public SnapAreaType Type { get; set; }
    public string Name { get; set; } = "";
}

/// <summary>
/// 吸附区域类型
/// </summary>
public enum SnapAreaType
{
    Edge,
    Corner
}

/// <summary>
/// 拖拽状态
/// </summary>
public class DragState
{
    public bool IsDragging { get; set; }
    public nint DraggedWindow { get; set; }
    public Point InitialMousePos { get; set; }
    public Point CurrentMousePos { get; set; }
    public WindowRect InitialWindowRect { get; set; }
    public WindowRect? OriginalRect { get; set; }
    public DateTime DragStartTime { get; set; }
    public MouseButton DragButton { get; set; }
    public SnapArea? CurrentSnapArea { get; set; }

    public void Reset()
    {
        IsDragging = false;
        DraggedWindow = 0;
        InitialMousePos = Point.Empty;
        CurrentMousePos = Point.Empty;
        InitialWindowRect = default;
        OriginalRect = null;
        DragStartTime = DateTime.MinValue;
        DragButton = MouseButton.None;
        CurrentSnapArea = null;
    }

    public double GetDragDistance()
    {
        int dx = CurrentMousePos.X - InitialMousePos.X;
        int dy = CurrentMousePos.Y - InitialMousePos.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    public double GetDragDurationMs()
    {
        return (DateTime.Now - DragStartTime).TotalMilliseconds;
    }
}
