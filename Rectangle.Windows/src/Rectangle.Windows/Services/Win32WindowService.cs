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
        Logger.Debug("SetWindowRect", $"尝试移动窗口 hwnd={hwnd} 到 ({x}, {y}, {width}, {height})");
        
        // 验证窗口句柄是否有效
        if (hwnd == 0)
        {
            Logger.Warning("SetWindowRect", "错误：窗口句柄为 0");
            return false;
        }

        var hWnd = (HWND)hwnd;
        
        // 检查窗口是否存在
        if (!IsWindowInternal(hWnd))
        {
            Logger.Warning("SetWindowRect", $"错误：窗口句柄无效或窗口已关闭 hwnd={hwnd}");
            return false;
        }

        // 检查窗口是否可见
        if (!PInvoke.IsWindowVisible(hWnd))
        {
            Logger.Warning("SetWindowRect", $"警告：窗口不可见 hwnd={hwnd}");
        }
        
        // 先确保窗口处于正常状态（非最大化/最小化）
        var style = (uint)GetWindowLong(hWnd, -16); // GWL_STYLE = -16
        
        // WS_MAXIMIZE = 0x01000000, 如果窗口已最大化，先还原
        if ((style & 0x01000000) != 0)
        {
            Logger.Debug("SetWindowRect", "窗口已最大化，先还原");
            PInvoke.ShowWindowAsync(hWnd, SHOW_WINDOW_CMD.SW_RESTORE); // SW_RESTORE = 9
        }

        // WS_MINIMIZE = 0x20000000, 如果窗口已最小化，先还原
        if ((style & 0x20000000) != 0)
        {
            Logger.Debug("SetWindowRect", "窗口已最小化，先还原");
            PInvoke.ShowWindowAsync(hWnd, SHOW_WINDOW_CMD.SW_RESTORE); // SW_RESTORE = 9
        }

        // SWP_FRAMECHANGED: 强制更新窗口框架（对 Electron 应用很重要）
        // SWP_ASYNCWINDOWPOS: 异步设置，避免某些应用阻塞
        var result = PInvoke.SetWindowPos(
            hWnd, 
            HWND.Null, 
            x, y, width, height, 
            SET_WINDOW_POS_FLAGS.SWP_NOZORDER | SET_WINDOW_POS_FLAGS.SWP_FRAMECHANGED | SET_WINDOW_POS_FLAGS.SWP_ASYNCWINDOWPOS);
        
        Logger.Debug("SetWindowRect", $"SetWindowPos 结果: {result}");
        
        if (!result)
        {
            // 获取错误码
            var errorCode = Marshal.GetLastWin32Error();
            Logger.Error("SetWindowRect", $"SetWindowPos 失败，错误码: {errorCode}");
            
            // 如果失败，尝试不带 SWP_ASYNCWINDOWPOS 再试一次
            result = PInvoke.SetWindowPos(
                hWnd, 
                HWND.Null, 
                x, y, width, height, 
                SET_WINDOW_POS_FLAGS.SWP_NOZORDER | SET_WINDOW_POS_FLAGS.SWP_FRAMECHANGED);
            Logger.Debug("SetWindowRect", $"重试结果: {result}");
            
            if (!result)
            {
                errorCode = Marshal.GetLastWin32Error();
                Logger.Error("SetWindowRect", $"重试失败，错误码: {errorCode}");
            }
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
            Logger.Warning("Win32WindowService", $"获取进程名失败: {ex.Message}");
            return "未知";
        }
    }

    /// <summary>
    /// 将光标移动到指定位置
    /// </summary>
    public bool SetCursorPos(int x, int y)
    {
        return PInvoke.SetCursorPos(x, y);
    }

    /// <summary>
    /// 将光标移动到窗口中心
    /// </summary>
    public bool MoveCursorToWindowCenter(nint hwnd)
    {
        var (x, y, w, h) = GetWindowRect(hwnd);
        var centerX = x + w / 2;
        var centerY = y + h / 2;
        
        return SetCursorPos(centerX, centerY);
    }

    private bool IsWindowInternal(HWND hWnd)
    {
        return PInvoke.IsWindow(hWnd);
    }
}
