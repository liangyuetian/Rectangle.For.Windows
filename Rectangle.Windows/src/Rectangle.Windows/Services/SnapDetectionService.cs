using Rectangle.Windows.Core;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace Rectangle.Windows.Services;

public sealed class SnapDetectionService : IDisposable
{
    private readonly Win32WindowService _win32;
    private readonly WindowManager _windowManager;
    private nint _mouseHook;
    private bool _isDragging;
    private nint _draggedWindow;
    private Point _dragStartPoint;
    private WindowAction? _pendingSnapAction;
    private readonly int _snapThreshold = 20;
    private static SnapDetectionService? _instance;

    private const uint WM_LBUTTONDOWN = 0x0201;
    private const uint WM_MOUSEMOVE = 0x0200;
    private const uint WM_LBUTTONUP = 0x0202;
    private const int WH_MOUSE_LL = 14;

    public event Action<WindowAction?>? SnapPreviewRequested;
    public event Action? SnapPreviewHidden;

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern nint SetWindowsHookEx(int idHook, HookProc lpfn, nint hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool UnhookWindowsHookEx(nint hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern nint CallNextHookEx(nint hhk, int nCode, nint wParam, nint lParam);

    [DllImport("user32.dll")]
    private static extern nint WindowFromPoint(POINT pt);

    [DllImport("user32.dll")]
    private static extern nint GetAncestor(nint hwnd, uint gaFlags);

    private delegate nint HookProc(int nCode, nint wParam, nint lParam);

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MSLLHOOKSTRUCT
    {
        public POINT pt;
        public uint mouseData;
        public uint flags;
        public uint time;
        public nint dwExtraInfo;
    }

    private readonly HookProc _hookCallback;

    public SnapDetectionService(Win32WindowService win32, WindowManager windowManager)
    {
        _win32 = win32;
        _windowManager = windowManager;
        _instance = this;
        _hookCallback = MouseHookCallback;
        InstallHook();
    }

    private void InstallHook()
    {
        _mouseHook = SetWindowsHookEx(WH_MOUSE_LL, _hookCallback, nint.Zero, 0);
    }

    private nint MouseHookCallback(int nCode, nint wParam, nint lParam)
    {
        if (nCode < 0)
        {
            return CallNextHookEx(_mouseHook, nCode, wParam, lParam);
        }

        var msg = (uint)wParam;
        var mouseStruct = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
        var point = new Point(mouseStruct.pt.X, mouseStruct.pt.Y);

        switch (msg)
        {
            case WM_LBUTTONDOWN:
                OnMouseDown(point);
                break;
            case WM_MOUSEMOVE:
                OnMouseMove(point);
                break;
            case WM_LBUTTONUP:
                OnMouseUp(point);
                break;
        }

        return CallNextHookEx(_mouseHook, nCode, wParam, lParam);
    }

    private void OnMouseDown(Point point)
    {
        _dragStartPoint = point;
        _isDragging = false;
        _draggedWindow = 0;
        _pendingSnapAction = null;

        // 检查是否在窗口标题栏区域
        var hwnd = WindowFromPoint(new POINT { X = point.X, Y = point.Y });
        if (hwnd != 0)
        {
            // 获取顶层窗口 (GA_ROOT = 2)
            var rootHwnd = GetAncestor(hwnd, 2);
            if (rootHwnd != 0)
            {
                var (x, y, w, h) = _win32.GetWindowRect(rootHwnd);
                // 标题栏高度约 30px
                if (point.Y >= y && point.Y <= y + 30 && point.X >= x && point.X <= x + w)
                {
                    _draggedWindow = rootHwnd;
                }
            }
        }
    }

    private void OnMouseMove(Point point)
    {
        if (_draggedWindow == 0) return;

        var dx = point.X - _dragStartPoint.X;
        var dy = point.Y - _dragStartPoint.Y;
        
        // 检查是否开始拖拽（移动超过 5px）
        if (!_isDragging && (Math.Abs(dx) > 5 || Math.Abs(dy) > 5))
        {
            _isDragging = true;
        }

        if (!_isDragging) return;

        // 检测吸附区域
        var workArea = _win32.GetWorkAreaFromWindow(_draggedWindow);
        var action = DetectSnapArea(point, workArea);

        if (action != _pendingSnapAction)
        {
            _pendingSnapAction = action;
            SnapPreviewRequested?.Invoke(action);
        }
    }

    private void OnMouseUp(Point point)
    {
        if (_isDragging && _pendingSnapAction.HasValue)
        {
            SnapPreviewHidden?.Invoke();
            _windowManager.Execute(_pendingSnapAction.Value);
        }
        else if (_pendingSnapAction.HasValue)
        {
            SnapPreviewHidden?.Invoke();
        }

        _isDragging = false;
        _draggedWindow = 0;
        _pendingSnapAction = null;
    }

    private WindowAction? DetectSnapArea(Point point, WorkArea workArea)
    {
        // 左边缘
        if (point.X <= workArea.Left + _snapThreshold)
        {
            // 左上角
            if (point.Y <= workArea.Top + _snapThreshold)
                return WindowAction.TopLeft;
            // 左下角
            if (point.Y >= workArea.Bottom - _snapThreshold)
                return WindowAction.BottomLeft;
            // 左半屏
            return WindowAction.LeftHalf;
        }

        // 右边缘
        if (point.X >= workArea.Right - _snapThreshold)
        {
            // 右上角
            if (point.Y <= workArea.Top + _snapThreshold)
                return WindowAction.TopRight;
            // 右下角
            if (point.Y >= workArea.Bottom - _snapThreshold)
                return WindowAction.BottomRight;
            // 右半屏
            return WindowAction.RightHalf;
        }

        // 上边缘
        if (point.Y <= workArea.Top + _snapThreshold)
        {
            return WindowAction.Maximize;
        }

        return null;
    }

    public void Dispose()
    {
        if (_mouseHook != 0)
        {
            UnhookWindowsHookEx(_mouseHook);
            _mouseHook = 0;
        }
        _instance = null;
    }
}
