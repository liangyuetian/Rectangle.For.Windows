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
        
        // 验证窗口句柄是否有效
        if (hwnd == 0)
        {
            Console.WriteLine("[SetWindowRect] 错误：窗口句柄为 0");
            return false;
        }

        var hWnd = (HWND)hwnd;
        
        // 检查窗口是否存在
        if (!IsWindowInternal(hWnd))
        {
            Console.WriteLine($"[SetWindowRect] 错误：窗口句柄无效或窗口已关闭 hwnd={hwnd}");
            return false;
        }

        // 检查窗口是否可见
        if (!IsWindowVisibleInternal(hWnd))
        {
            Console.WriteLine($"[SetWindowRect] 警告：窗口不可见 hwnd={hwnd}");
        }
        
        var style = (uint)GetWindowLongPtr((nint)hwnd, -16);
        
        // WS_MAXIMIZE = 0x01000000, 如果窗口已最大化，先还原
        if ((style & 0x01000000) != 0)
        {
            Console.WriteLine("[SetWindowRect] 窗口已最大化，先还原");
            PInvoke.ShowWindow(hWnd, SHOW_WINDOW_CMD.SW_RESTORE);
            System.Threading.Thread.Sleep(50); // 等待窗口还原完成
        }

        // WS_MINIMIZE = 0x20000000, 如果窗口已最小化，先还原
        if ((style & 0x20000000) != 0)
        {
            Console.WriteLine("[SetWindowRect] 窗口已最小化，先还原");
            PInvoke.ShowWindow(hWnd, SHOW_WINDOW_CMD.SW_RESTORE);
            System.Threading.Thread.Sleep(50); // 等待窗口还原完成
        }

        var result = PInvoke.SetWindowPos(
            hWnd, 
            HWND.Null, 
            x, y, width, height, 
            SET_WINDOW_POS_FLAGS.SWP_NOZORDER | SET_WINDOW_POS_FLAGS.SWP_FRAMECHANGED | SET_WINDOW_POS_FLAGS.SWP_ASYNCWINDOWPOS);
        
        Console.WriteLine($"[SetWindowRect] SetWindowPos 结果: {result}");
        
        if (!result)
        {
            // 获取错误码
            var errorCode = Marshal.GetLastWin32Error();
            Console.WriteLine($"[SetWindowRect] SetWindowPos 失败，错误码: {errorCode}");
            
            result = PInvoke.SetWindowPos(
                hWnd, 
                HWND.Null, 
                x, y, width, height, 
                SET_WINDOW_POS_FLAGS.SWP_NOZORDER | SET_WINDOW_POS_FLAGS.SWP_FRAMECHANGED);
            Console.WriteLine($"[SetWindowRect] 重试结果: {result}");
            
            if (!result)
            {
                errorCode = Marshal.GetLastWin32Error();
                Console.WriteLine($"[SetWindowRect] 重试失败，错误码: {errorCode}");
            }
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

    // P/Invoke for IsWindow
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsWindow(void* hWnd);

    // P/Invoke for IsWindowVisible
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsWindowVisible(void* hWnd);

    private bool IsWindowInternal(HWND hWnd)
    {
        return IsWindow(hWnd.Value);
    }

    private bool IsWindowVisibleInternal(HWND hWnd)
    {
        return IsWindowVisible(hWnd.Value);
    }
}
