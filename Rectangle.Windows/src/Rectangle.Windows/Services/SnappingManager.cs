using Rectangle.Windows.Core;
using Rectangle.Windows.Views;
using System;
using System.Drawing;
using System.Windows.Forms;

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
    private readonly FootprintWindow _footprint;
    
    // 配置
    private bool _isEnabled = true;
    private int _edgeMargin = 5;  // 边缘吸附区域大小（像素）
    private int _cornerSize = 20; // 角落吸附区域大小（像素）
    
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
        _footprint = new FootprintWindow();
        
        // 订阅鼠标事件
        _mouseHook.MouseDown += OnMouseDown;
        _mouseHook.MouseUp += OnMouseUp;
        _mouseHook.MouseMove += OnMouseMove;
    }

    /// <summary>
    /// 启用拖拽吸附
    /// </summary>
    public bool Enable()
    {
        if (_isEnabled) return true;
        
        LoadConfig();
        
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
        if (config?.SnapAreas != null)
        {
            // 可以在这里加载更多配置
        }
    }

    /// <summary>
    /// 鼠标按下事件
    /// </summary>
    private void OnMouseDown(object? sender, MouseHookEventArgs e)
    {
        // 只处理左键
        if (e.DragButton != System.Windows.Forms.MouseButtons.Left)
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
        _dragState.OriginalRect = new WindowRect(x, y, w, h);
        
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
            _dragState.CurrentSnapArea = snapArea;
            
            // 显示预览窗口
            var previewRect = CalculatePreviewRect(snapArea);
            _footprint.ShowPreview(previewRect, snapArea.Name);
            
            Console.WriteLine($"[SnappingManager] 检测到吸附区域: {snapArea.Name}");
        }
        else
        {
            _dragState.CurrentSnapArea = null;
            _footprint.HidePreview();
        }
    }

    /// <summary>
    /// 鼠标释放事件
    /// </summary>
    private void OnMouseUp(object? sender, MouseHookEventArgs e)
    {
        if (!_dragState.IsDragging) return;

        // 检查是否是有效拖拽（持续时间 > 100ms 或距离 > 10px）
        bool isValidDrag = _dragState.GetDragDurationMs() > 100 || 
                          _dragState.GetDragDistance() > 10;

        if (isValidDrag && _dragState.CurrentSnapArea != null)
        {
            // 执行吸附
            ExecuteSnap(_dragState.CurrentSnapArea);
        }

        DragEnded?.Invoke(this, EventArgs.Empty);
        
        Console.WriteLine($"[SnappingManager] 结束拖拽窗口: {_dragState.DraggedWindow}");
        
        _dragState.Reset();
    }

    /// <summary>
    /// 计算吸附区域
    /// </summary>
    private SnapArea? CalculateSnapArea(Point cursorPos)
    {
        // 获取光标所在的屏幕
        var workArea = GetWorkAreaFromPoint(cursorPos);
        if (workArea == null) return null;

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
    private SnapArea? CheckScreenEdges(Point cursorPos, WorkArea workArea)
    {
        // 左边缘
        if (cursorPos.X >= workArea.Left && cursorPos.X <= workArea.Left + _edgeMargin)
        {
            return new SnapArea
            {
                Bounds = new Rectangle(workArea.Left, workArea.Top, _edgeMargin, workArea.Height),
                Action = WindowAction.LeftHalf,
                Type = SnapAreaType.Edge,
                Name = "Left Edge"
            };
        }

        // 右边缘
        if (cursorPos.X >= workArea.Right - _edgeMargin && cursorPos.X <= workArea.Right)
        {
            return new SnapArea
            {
                Bounds = new Rectangle(workArea.Right - _edgeMargin, workArea.Top, _edgeMargin, workArea.Height),
                Action = WindowAction.RightHalf,
                Type = SnapAreaType.Edge,
                Name = "Right Edge"
            };
        }

        // 上边缘
        if (cursorPos.Y >= workArea.Top && cursorPos.Y <= workArea.Top + _edgeMargin)
        {
            return new SnapArea
            {
                Bounds = new Rectangle(workArea.Left, workArea.Top, workArea.Width, _edgeMargin),
                Action = WindowAction.TopHalf,
                Type = SnapAreaType.Edge,
                Name = "Top Edge"
            };
        }

        // 下边缘
        if (cursorPos.Y >= workArea.Bottom - _edgeMargin && cursorPos.Y <= workArea.Bottom)
        {
            return new SnapArea
            {
                Bounds = new Rectangle(workArea.Left, workArea.Bottom - _edgeMargin, workArea.Width, _edgeMargin),
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
    private SnapArea? CheckScreenCorners(Point cursorPos, WorkArea workArea)
    {
        // 左上角
        if (cursorPos.X <= workArea.Left + _cornerSize && 
            cursorPos.Y <= workArea.Top + _cornerSize)
        {
            return new SnapArea
            {
                Bounds = new Rectangle(workArea.Left, workArea.Top, _cornerSize, _cornerSize),
                Action = WindowAction.TopLeft,
                Type = SnapAreaType.Corner,
                Name = "Top Left Corner"
            };
        }

        // 右上角
        if (cursorPos.X >= workArea.Right - _cornerSize && 
            cursorPos.Y <= workArea.Top + _cornerSize)
        {
            return new SnapArea
            {
                Bounds = new Rectangle(workArea.Right - _cornerSize, workArea.Top, _cornerSize, _cornerSize),
                Action = WindowAction.TopRight,
                Type = SnapAreaType.Corner,
                Name = "Top Right Corner"
            };
        }

        // 左下角
        if (cursorPos.X <= workArea.Left + _cornerSize && 
            cursorPos.Y >= workArea.Bottom - _cornerSize)
        {
            return new SnapArea
            {
                Bounds = new Rectangle(workArea.Left, workArea.Bottom - _cornerSize, _cornerSize, _cornerSize),
                Action = WindowAction.BottomLeft,
                Type = SnapAreaType.Corner,
                Name = "Bottom Left Corner"
            };
        }

        // 右下角
        if (cursorPos.X >= workArea.Right - _cornerSize && 
            cursorPos.Y >= workArea.Bottom - _cornerSize)
        {
            return new SnapArea
            {
                Bounds = new Rectangle(workArea.Right - _cornerSize, workArea.Bottom - _cornerSize, _cornerSize, _cornerSize),
                Action = WindowAction.BottomRight,
                Type = SnapAreaType.Corner,
                Name = "Bottom Right Corner"
            };
        }

        return null;
    }

    /// <summary>
    /// 计算预览窗口位置
    /// </summary>
    private Rectangle CalculatePreviewRect(SnapArea snapArea)
    {
        var workArea = GetWorkAreaFromPoint(_dragState.CurrentMousePos);
        if (workArea == null) return Rectangle.Empty;

        // 根据操作类型计算预览位置
        switch (snapArea.Action)
        {
            case WindowAction.LeftHalf:
                return new Rectangle(workArea.Left, workArea.Top, workArea.Width / 2, workArea.Height);
            
            case WindowAction.RightHalf:
                return new Rectangle(workArea.Left + workArea.Width / 2, workArea.Top, workArea.Width / 2, workArea.Height);
            
            case WindowAction.TopHalf:
                return new Rectangle(workArea.Left, workArea.Top, workArea.Width, workArea.Height / 2);
            
            case WindowAction.BottomHalf:
                return new Rectangle(workArea.Left, workArea.Top + workArea.Height / 2, workArea.Width, workArea.Height / 2);
            
            case WindowAction.TopLeft:
                return new Rectangle(workArea.Left, workArea.Top, workArea.Width / 2, workArea.Height / 2);
            
            case WindowAction.TopRight:
                return new Rectangle(workArea.Left + workArea.Width / 2, workArea.Top, workArea.Width / 2, workArea.Height / 2);
            
            case WindowAction.BottomLeft:
                return new Rectangle(workArea.Left, workArea.Top + workArea.Height / 2, workArea.Width / 2, workArea.Height / 2);
            
            case WindowAction.BottomRight:
                return new Rectangle(workArea.Left + workArea.Width / 2, workArea.Top + workArea.Height / 2, workArea.Width / 2, workArea.Height / 2);
            
            default:
                return snapArea.Bounds;
        }
    }

    /// <summary>
    /// 获取点所在的屏幕工作区
    /// </summary>
    private WorkArea? GetWorkAreaFromPoint(Point point)
    {
        try
        {
            var hMonitor = PInvoke.MonitorFromPoint(point, PInvoke.MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST);
            var mi = new Windows.Win32.Graphics.Gdi.MONITORINFO();
            mi.cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf<Windows.Win32.Graphics.Gdi.MONITORINFO>();
            
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
        if (_dragState.OriginalRect != null)
        {
            _history.SaveRestoreRect(hwnd, 
                _dragState.OriginalRect.X, 
                _dragState.OriginalRect.Y, 
                _dragState.OriginalRect.Width, 
                _dragState.OriginalRect.Height);
        }

        // 触发吸附事件
        SnapTriggered?.Invoke(this, new SnapEventArgs
        {
            WindowHandle = hwnd,
            Action = snapArea.Action,
            SnapArea = snapArea
        });

        Console.WriteLine($"[SnappingManager] 执行吸附: {snapArea.Name} -> {snapArea.Action}");
    }

    /// <summary>
    /// 检查窗口是否适合拖拽
    /// </summary>
    private bool IsValidWindowForDragging(nint hwnd)
    {
        // 检查窗口是否可见
        if (!PInvoke.IsWindowVisible(new Windows.Win32.Foundation.HWND(hwnd)))
            return false;

        // 检查窗口是否有标题栏（排除桌面等）
        // 这里可以添加更多检查

        return true;
    }

    public void Dispose()
    {
        Disable();
        _mouseHook?.Dispose();
        _footprint?.Dispose();
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
