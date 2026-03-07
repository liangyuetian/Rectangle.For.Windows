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

    public nint GetForegroundWindowHandle()
    {
        return (nint)PInvoke.GetForegroundWindow().Value;
    }

    public (int X, int Y, int Width, int Height) GetWindowRect(nint hwnd)
    {
        PInvoke.GetWindowRect((HWND)hwnd, out var rect);
        return (rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top);
    }

    public bool SetWindowRect(nint hwnd, int x, int y, int width, int height)
    {
        Console.WriteLine($"[SetWindowRect] 尝试移动窗口 hwnd={hwnd} 到 ({x}, {y}, {width}, {height})");
        
        var style = (uint)GetWindowLongPtr((nint)hwnd, -16);
        
        if ((style & 0x01000000) != 0)
        {
            Console.WriteLine("[SetWindowRect] 窗口已最大化，先还原");
            PInvoke.ShowWindow((HWND)hwnd, SHOW_WINDOW_CMD.SW_RESTORE);
        }

        var result = PInvoke.SetWindowPos(
            (HWND)hwnd, 
            HWND.Null, 
            x, y, width, height, 
            SET_WINDOW_POS_FLAGS.SWP_NOZORDER | SET_WINDOW_POS_FLAGS.SWP_FRAMECHANGED | SET_WINDOW_POS_FLAGS.SWP_ASYNCWINDOWPOS);
        
        Console.WriteLine($"[SetWindowRect] SetWindowPos 结果: {result}");
        
        if (!result)
        {
            result = PInvoke.SetWindowPos(
                (HWND)hwnd, 
                HWND.Null, 
                x, y, width, height, 
                SET_WINDOW_POS_FLAGS.SWP_NOZORDER | SET_WINDOW_POS_FLAGS.SWP_FRAMECHANGED);
            Console.WriteLine($"[SetWindowRect] 重试结果: {result}");
        }
        
        return result;
    }

    public WorkArea GetWorkAreaFromWindow(nint hwnd)
    {
        var hMonitor = PInvoke.MonitorFromWindow((HWND)hwnd, MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST);
        var mi = new MONITORINFO();
        mi.cbSize = (uint)Marshal.SizeOf<MONITORINFO>();
        PInvoke.GetMonitorInfo(hMonitor, ref mi);
        var rcWork = mi.rcWork;
        return new WorkArea(rcWork.left, rcWork.top, rcWork.right, rcWork.bottom);
    }

    public List<WorkArea> GetMonitorWorkAreas()
    {
        var workAreas = new List<WorkArea>();
        
        GCHandle handle = GCHandle.Alloc(workAreas);
        try
        {
            var hdc = new HDC();
            RECT? rect = null;
            PInvoke.EnumDisplayMonitors(
                hdc,
                rect,
                (hMonitor, hdcMonitor, lprcMonitor, dwData) =>
                {
                    var mi = new MONITORINFO();
                    mi.cbSize = (uint)Marshal.SizeOf<MONITORINFO>();
                    PInvoke.GetMonitorInfo(hMonitor, ref mi);
                    var rcWork = mi.rcWork;
                    var list = (List<WorkArea>)GCHandle.FromIntPtr((nint)dwData).Target!;
                    list.Add(new WorkArea(rcWork.left, rcWork.top, rcWork.right, rcWork.bottom));
                    return true;
                },
                (LPARAM)(nint)GCHandle.ToIntPtr(handle));
        }
        finally
        {
            handle.Free();
        }

        return workAreas;
    }

    public nint GetMonitorFromWindow(nint hwnd)
    {
        return (nint)PInvoke.MonitorFromWindow((HWND)hwnd, MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST).Value;
    }

    public string GetProcessNameFromWindow(nint hwnd)
    {
        uint processId;
        PInvoke.GetWindowThreadProcessId((HWND)hwnd, &processId);
        
        if (processId == 0) return "未知";

        try
        {
            var process = System.Diagnostics.Process.GetProcessById((int)processId);
            return process.ProcessName;
        }
        catch
        {
            return "未知";
        }
    }
}
