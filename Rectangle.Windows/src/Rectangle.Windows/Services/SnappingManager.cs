using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Rectangle.Windows.Core;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Rectangle.Windows.Services;

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

    // 配置
    private bool _isEnabled = true;
    private int _edgeMarginTop = 5;
    private int _edgeMarginBottom = 5;
    private int _edgeMarginLeft = 5;
    private int _edgeMarginRight = 5;
    private int _cornerSize = 20;
    private int _snapModifiers = 0;

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
        var config = _configService?.Load();
        if (config?.DragToSnap == false)
        {
            Console.WriteLine("[SnappingManager] 拖拽吸附已禁用（配置）");
            return false;
        }

        if (_mouseHook.InstallHook())
        {
            _isEnabled = true;
            Console.WriteLine("[SnappingManager] 拖拽吸附已启用");
            return true;
        }

        return false;
    }

    /// <summary>
    /// 禁用拖拽吸附
    /// </summary>
    public void Disable()
    {
        if (!_isEnabled) return;
        
        _mouseHook.UninstallHook();
        _isEnabled = false;
        _dragState.Reset();
        Console.WriteLine("[SnappingManager] 拖拽吸附已禁用");
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
        if (e.Button != System.Windows.Forms.MouseButtons.Left)
            return;

        // 获取光标下的窗口
        var hwnd = MouseHookService.GetWindowUnderCursor();
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
        _dragState.InitialMousePos = e.Point;
        _dragState.InitialWindowRect = new WindowRect(x, y, w, h);
        _dragState.DragStartTime = DateTime.Now;
        _dragState.DragButton = MouseButton.Left;

        // Unsnap 恢复：检测窗口是否被程序调整过
        // 如果是，从历史记录获取原始尺寸保存到 OriginalRect
        var config = _configService?.Load();
        if (config?.UnsnapRestore == true && _history.IsProgramAdjusted(hwnd))
        {
            if (_history.TryGetRestoreRect(hwnd, out var restoreRect))
            {
                _dragState.OriginalRect = new WindowRect(
                    restoreRect.X, restoreRect.Y, restoreRect.W, restoreRect.H);
                Console.WriteLine($"[SnappingManager] Unsnap: 检测到已吸附窗口，保存原始位置 ({restoreRect.X}, {restoreRect.Y}, {restoreRect.W}, {restoreRect.H})");
            }
        }
        else
        {
            // 保存当前位置作为原始位置
            _dragState.OriginalRect = new WindowRect(x, y, w, h);
        }

        DragStarted?.Invoke(this, EventArgs.Empty);

        Console.WriteLine($"[SnappingManager] 开始拖拽窗口: {hwnd}");
    }

    /// <summary>
    /// 鼠标移动事件
    /// </summary>
    private void OnMouseMove(object? sender, MouseHookEventArgs e)
    {
        if (!_dragState.IsDragging) return;

        _dragState.CurrentMousePos = e.Point;

        // 检查拖拽距离，如果太小可能是点击而不是拖拽
        if (_dragState.GetDragDistance() < 5)
            return;

        // 计算吸附区域
        var snapArea = CalculateSnapArea(e.Point);
        
        if (snapArea != null)
        {
            // 显示吸附预览（后续实现）
            _dragState.CurrentSnapArea = snapArea;
            Console.WriteLine($"[SnappingManager] 检测到吸附区域: {snapArea.Name}");
        }
        else
        {
            _dragState.CurrentSnapArea = null;
        }
    }

    /// <summary>
    /// 鼠标释放事件
    /// </summary>
    private void OnMouseUp(object? sender, MouseHookEventArgs e)
    {
        if (!_dragState.IsDragging) return;

        var hwnd = _dragState.DraggedWindow;

        // 检查是否是有效拖拽（持续时间 > 100ms 或距离 > 10px）
        bool isValidDrag = _dragState.GetDragDurationMs() > 100 ||
                          _dragState.GetDragDistance() > 10;

        var config = _configService?.Load();

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
            bool positionChanged = Math.Abs(currentX - originalRect.X) > 5 ||
                                   Math.Abs(currentY - originalRect.Y) > 5;

            if (positionChanged)
            {
                // 恢复窗口到原始尺寸
                _win32.SetWindowRect(hwnd, originalRect.X, originalRect.Y, originalRect.Width, originalRect.Height);

                // 清除程序调整标记
                _history.ClearProgramAdjustedMark(hwnd);

                Console.WriteLine($"[SnappingManager] Unsnap 恢复: 恢复窗口到原始位置 ({originalRect.X}, {originalRect.Y}, {originalRect.Width}, {originalRect.Height})");
            }
        }

        DragEnded?.Invoke(this, EventArgs.Empty);

        Console.WriteLine($"[SnappingManager] 结束拖拽窗口: {hwnd}");

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
            var hMonitor = PInvoke.MonitorFromPoint(point, MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST);
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
            Console.WriteLine($"[SnappingManager] 获取屏幕工作区失败: {ex.Message}");
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
            _windowManager.Execute(snapArea.Action, hwnd);
            Console.WriteLine($"[SnappingManager] 执行吸附: {snapArea.Name} -> {snapArea.Action}");
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
            Console.WriteLine($"[SnappingManager] 触发吸附事件: {snapArea.Name} -> {snapArea.Action}");
        }
    }

    // P/Invoke for IsWindowVisible
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    /// <summary>
    /// 检查窗口是否适合拖拽
    /// </summary>
    private bool IsValidWindowForDragging(nint hwnd)
    {
        // 检查窗口是否可见
        if (!IsWindowVisible((IntPtr)hwnd))
            return false;

        // 检查窗口是否有标题栏（排除桌面等）
        // 这里可以添加更多检查

        return true;
    }

    public void Dispose()
    {
        Disable();
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
