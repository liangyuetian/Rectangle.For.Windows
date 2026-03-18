using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Rectangle.Windows.WinUI.Core;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Rectangle.Windows.WinUI.Services;

public unsafe class Win32WindowService
{
    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW")]
    private static extern nint GetWindowLongPtr(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsWindow(void* hWnd);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsWindowVisible(void* hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

    [StructLayout(LayoutKind.Sequential)]
    private struct WINDOWPLACEMENT
    {
        public uint length;
        public uint flags;
        public uint showCmd;
        public POINT ptMinPosition;
        public POINT ptMaxPosition;
        public global::Windows.Win32.Foundation.RECT rcNormalPosition;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int x, y;
    }

    public nint GetForegroundWindowHandle()
        => (nint)PInvoke.GetForegroundWindow().Value;

    public (int X, int Y, int Width, int Height) GetWindowRect(nint hwnd)
    {
        PInvoke.GetWindowRect((HWND)hwnd, out var rect);
        return (rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top);
    }

    /// <summary>
    /// 从最大化状态直接设置窗口尺寸，使用 SetWindowPlacement 实现单次过渡，避免先恢复再设置的闪屏。
    /// </summary>
    public bool SetWindowRectFromMaximized(nint hwnd, int x, int y, int width, int height)
    {
        if (hwnd == 0) return false;
        if (!IsWindow((void*)hwnd)) return false;

        var wp = new WINDOWPLACEMENT
        {
            length = (uint)Marshal.SizeOf<WINDOWPLACEMENT>(),
            showCmd = 1, // SW_SHOWNORMAL
            rcNormalPosition = new global::Windows.Win32.Foundation.RECT { left = x, top = y, right = x + width, bottom = y + height }
        };
        return SetWindowPlacement((IntPtr)hwnd, ref wp);
    }

    public bool SetWindowRect(nint hwnd, int x, int y, int width, int height)
    {
        Logger.Debug("Win32WindowService", $"SetWindowRect hwnd={hwnd} -> ({x}, {y}, {width}, {height})");
        if (hwnd == 0) return false;

        var hWnd = (HWND)hwnd;
        if (!IsWindow(hWnd.Value)) return false;

        var style = (uint)GetWindowLongPtr((nint)hwnd, -16);
        if ((style & 0x01000000) != 0 || (style & 0x20000000) != 0)
        {
            PInvoke.ShowWindow(hWnd, SHOW_WINDOW_CMD.SW_RESTORE);
            System.Threading.Thread.Sleep(50);
        }

        var result = PInvoke.SetWindowPos(hWnd, HWND.Null, x, y, width, height,
            SET_WINDOW_POS_FLAGS.SWP_NOZORDER | SET_WINDOW_POS_FLAGS.SWP_FRAMECHANGED | SET_WINDOW_POS_FLAGS.SWP_ASYNCWINDOWPOS);

        if (!result)
        {
            Logger.Warning("Win32WindowService", $"SetWindowRect 失败，错误码: {Marshal.GetLastWin32Error()}，重试...");
            result = PInvoke.SetWindowPos(hWnd, HWND.Null, x, y, width, height,
                SET_WINDOW_POS_FLAGS.SWP_NOZORDER | SET_WINDOW_POS_FLAGS.SWP_FRAMECHANGED);
        }

        return result;
    }

    /// <summary>
    /// 使用 Windows 原生最大化（SW_MAXIMIZE），按一下最大化、再按一下恢复。
    /// </summary>
    public void ShowWindowMaximize(nint hwnd)
    {
        if (hwnd == 0) return;
        PInvoke.ShowWindow((HWND)hwnd, SHOW_WINDOW_CMD.SW_MAXIMIZE);
    }

    /// <summary>
    /// 使用 Windows 原生恢复（SW_RESTORE）。
    /// </summary>
    public void ShowWindowRestore(nint hwnd)
    {
        if (hwnd == 0) return;
        PInvoke.ShowWindow((HWND)hwnd, SHOW_WINDOW_CMD.SW_RESTORE);
    }

    public WorkArea GetWorkAreaFromWindow(nint hwnd)
    {
        var hMonitor = PInvoke.MonitorFromWindow((HWND)hwnd, MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST);
        var mi = new MONITORINFO { cbSize = (uint)Marshal.SizeOf<MONITORINFO>() };
        PInvoke.GetMonitorInfo(hMonitor, ref mi);
        var r = mi.rcWork;
        return new WorkArea(r.left, r.top, r.right, r.bottom);
    }

    public List<WorkArea> GetMonitorWorkAreas()
    {
        var workAreas = new List<WorkArea>();
        var handle = GCHandle.Alloc(workAreas);
        try
        {
            PInvoke.EnumDisplayMonitors(new HDC(), (RECT?)null,
                (hMonitor, _, _, dwData) =>
                {
                    var mi = new MONITORINFO { cbSize = (uint)Marshal.SizeOf<MONITORINFO>() };
                    PInvoke.GetMonitorInfo(hMonitor, ref mi);
                    var r = mi.rcWork;
                    ((List<WorkArea>)GCHandle.FromIntPtr((nint)dwData).Target!)
                        .Add(new WorkArea(r.left, r.top, r.right, r.bottom));
                    return true;
                },
                (LPARAM)(nint)GCHandle.ToIntPtr(handle));
        }
        finally { handle.Free(); }
        return workAreas;
    }

    public nint GetMonitorFromWindow(nint hwnd)
        => (nint)PInvoke.MonitorFromWindow((HWND)hwnd, MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST).Value;

    public string GetProcessNameFromWindow(nint hwnd)
    {
        uint processId;
        PInvoke.GetWindowThreadProcessId((HWND)hwnd, &processId);
        if (processId == 0) return "未知";
        try { return System.Diagnostics.Process.GetProcessById((int)processId).ProcessName; }
        catch { return "未知"; }
    }

    public WorkArea GetWorkAreaFromCursor()
    {
        PInvoke.GetCursorPos(out var pt);
        var hMonitor = PInvoke.MonitorFromPoint(pt, MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST);
        var mi = new MONITORINFO { cbSize = (uint)Marshal.SizeOf<MONITORINFO>() };
        PInvoke.GetMonitorInfo(hMonitor, ref mi);
        var r = mi.rcWork;
        return new WorkArea(r.left, r.top, r.right, r.bottom);
    }
}
