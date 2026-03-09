using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Rectangle.Windows.Core;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Rectangle.Windows.Services;

public unsafe class Win32WindowService
{
    public nint GetForegroundWindowHandle()
    {
        return PInvoke.GetForegroundWindow().Value;
    }

    public (int X, int Y, int Width, int Height) GetWindowRect(nint hwnd)
    {
        PInvoke.GetWindowRect((HWND)hwnd, out var rect);
        return (rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top);
    }

    public bool SetWindowRect(nint hwnd, int x, int y, int width, int height)
    {
        Console.WriteLine($"[SetWindowRect] 尝试移动窗口 hwnd={hwnd} 到 ({x}, {y}, {width}, {height})");
        
        // 先确保窗口处于正常状态（非最大化/最小化）
        var style = (uint)GetWindowLong((HWND)hwnd, -16); // GWL_STYLE = -16
        
        // WS_MAXIMIZE = 0x01000000, 如果窗口已最大化，先还原
        if ((style & 0x01000000) != 0)
        {
            Console.WriteLine("[SetWindowRect] 窗口已最大化，先还原");
            PInvoke.ShowWindow((HWND)hwnd, (SHOW_WINDOW_CMD)9); // SW_RESTORE = 9
        }

        // SWP_FRAMECHANGED: 强制更新窗口框架（对 Electron 应用很重要）
        // SWP_ASYNCWINDOWPOS: 异步设置，避免某些应用阻塞
        var result = PInvoke.SetWindowPos(
            (HWND)hwnd, 
            HWND.Null, 
            x, y, width, height, 
            SET_WINDOW_POS_FLAGS.SWP_NOZORDER | SET_WINDOW_POS_FLAGS.SWP_FRAMECHANGED | SET_WINDOW_POS_FLAGS.SWP_ASYNCWINDOWPOS);
        
        Console.WriteLine($"[SetWindowRect] SetWindowPos 结果: {result}");
        
        if (!result)
        {
            // 如果失败，尝试不带 SWP_ASYNCWINDOWPOS 再试一次
            result = PInvoke.SetWindowPos(
                (HWND)hwnd, 
                HWND.Null, 
                x, y, width, height, 
                SET_WINDOW_POS_FLAGS.SWP_NOZORDER | SET_WINDOW_POS_FLAGS.SWP_FRAMECHANGED);
            Console.WriteLine($"[SetWindowRect] 重试结果: {result}");
        }
        
        return result;
    }

    [DllImport("user32.dll", EntryPoint = "GetWindowLongW")]
    private static extern nint GetWindowLong32(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW")]
    private static extern nint GetWindowLong64(IntPtr hWnd, int nIndex);

    private static nint GetWindowLong(HWND hWnd, int nIndex)
    {
        if (nint.Size == 8)
            return GetWindowLong64(hWnd.Value, nIndex);
        else
            return GetWindowLong32(hWnd.Value, nIndex);
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
                    var target = GCHandle.FromIntPtr(dwData).Target;
                    if (target is List<WorkArea> list)
                    {
                        list.Add(new WorkArea(rcWork.left, rcWork.top, rcWork.right, rcWork.bottom));
                    }
                    return true;
                },
                (LPARAM)GCHandle.ToIntPtr(handle));
        }
        finally
        {
            handle.Free();
        }

        return workAreas;
    }

    public nint GetMonitorFromWindow(nint hwnd)
    {
        return PInvoke.MonitorFromWindow((HWND)hwnd, MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST).Value;
    }

    public WorkArea? GetNextMonitorWorkArea(nint hwnd)
    {
        var workAreas = GetMonitorWorkAreas();
        if (workAreas.Count <= 1) return null;

        var currentMonitor = GetMonitorFromWindow(hwnd);
        for (int i = 0; i < workAreas.Count; i++)
        {
            var monitorHandle = PInvoke.MonitorFromPoint(
                new System.Drawing.Point(workAreas[i].Left + 1, workAreas[i].Top + 1),
                MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST);
            
            if (monitorHandle.Value == currentMonitor)
            {
                var nextIndex = (i + 1) % workAreas.Count;
                return workAreas[nextIndex];
            }
        }
        return null;
    }

    public WorkArea? GetPreviousMonitorWorkArea(nint hwnd)
    {
        var workAreas = GetMonitorWorkAreas();
        if (workAreas.Count <= 1) return null;

        var currentMonitor = GetMonitorFromWindow(hwnd);
        for (int i = 0; i < workAreas.Count; i++)
        {
            var monitorHandle = PInvoke.MonitorFromPoint(
                new System.Drawing.Point(workAreas[i].Left + 1, workAreas[i].Top + 1),
                MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST);
            
            if (monitorHandle.Value == currentMonitor)
            {
                var prevIndex = (i - 1 + workAreas.Count) % workAreas.Count;
                return workAreas[prevIndex];
            }
        }
        return null;
    }

    public string GetProcessNameFromWindow(nint hwnd)
    {
        uint processId;
        PInvoke.GetWindowThreadProcessId((HWND)hwnd, &processId);
        
        if (processId == 0) return "未知";

        try
        {
            using var process = System.Diagnostics.Process.GetProcessById((int)processId);
            return process.ProcessName;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Win32WindowService] 获取进程名失败: {ex.Message}");
            return "未知";
        }
    }
}
