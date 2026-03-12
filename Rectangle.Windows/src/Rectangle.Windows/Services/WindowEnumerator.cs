using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace Rectangle.Windows.Services;

/// <summary>
/// 窗口枚举器：枚举系统中所有可见窗口
/// </summary>
public static class WindowEnumerator
{
    // P/Invoke delegates and methods
    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsIconic(IntPtr hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongW")]
    private static extern nint GetWindowLong32(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW")]
    private static extern nint GetWindowLong64(IntPtr hWnd, int nIndex);

    private static nint GetWindowLong(IntPtr hWnd, int nIndex)
    {
        if (IntPtr.Size == 8)
            return GetWindowLong64(hWnd, nIndex);
        else
            return GetWindowLong32(hWnd, nIndex);
    }

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    /// <summary>
    /// 枚举所有可见窗口
    /// </summary>
    /// <param name="excludeMinimized">是否排除最小化的窗口</param>
    /// <param name="excludeToolWindows">是否排除工具窗口</param>
    /// <returns>窗口句柄列表</returns>
    public static List<nint> EnumerateVisibleWindows(bool excludeMinimized = true, bool excludeToolWindows = true)
    {
        var windows = new List<nint>();

        EnumWindows((hwnd, lParam) =>
        {
            if (IsWindowValid(hwnd, excludeMinimized, excludeToolWindows))
            {
                windows.Add(hwnd);
            }
            return true;
        }, IntPtr.Zero);

        return windows;
    }

    /// <summary>
    /// 枚举指定进程的所有窗口
    /// </summary>
    /// <param name="processName">进程名称</param>
    /// <param name="excludeMinimized">是否排除最小化的窗口</param>
    /// <returns>窗口句柄列表</returns>
    public static List<nint> EnumerateWindowsByProcess(string processName, bool excludeMinimized = true)
    {
        var windows = new List<nint>();

        EnumWindows((hwnd, lParam) =>
        {
            if (IsWindowValid(hwnd, excludeMinimized, true))
            {
                var process = GetProcessNameFromWindow(hwnd);
                if (process.Equals(processName, StringComparison.OrdinalIgnoreCase))
                {
                    windows.Add(hwnd);
                }
            }
            return true;
        }, IntPtr.Zero);

        return windows;
    }

    /// <summary>
    /// 检查窗口是否有效
    /// </summary>
    private static bool IsWindowValid(IntPtr hwnd, bool excludeMinimized, bool excludeToolWindows)
    {
        // 检查窗口是否可见
        if (!IsWindowVisible(hwnd))
            return false;

        // 检查窗口是否有标题
        var title = GetWindowTitleInternal(hwnd);
        if (string.IsNullOrEmpty(title))
            return false;

        // 排除最小化窗口
        if (excludeMinimized && IsIconic(hwnd))
            return false;

        // 排除工具窗口
        if (excludeToolWindows && IsToolWindowInternal(hwnd))
            return false;

        // 排除空窗口
        var (x, y, w, h) = GetWindowRectInternal(hwnd);
        if (w <= 0 || h <= 0)
            return false;

        return true;
    }

    /// <summary>
    /// 获取窗口标题（内部使用）
    /// </summary>
    private static string GetWindowTitleInternal(IntPtr hwnd)
    {
        var sb = new StringBuilder(256);
        var length = GetWindowText(hwnd, sb, 256);
        return length > 0 ? sb.ToString() : string.Empty;
    }

    /// <summary>
    /// 检查窗口是否是工具窗口（内部使用）
    /// </summary>
    private static bool IsToolWindowInternal(IntPtr hwnd)
    {
        var exStyle = GetWindowLong(hwnd, -20); // GWL_EXSTYLE = -20
        const int WS_EX_TOOLWINDOW = 0x00000080;
        return (exStyle.ToInt64() & WS_EX_TOOLWINDOW) != 0;
    }

    /// <summary>
    /// 获取窗口矩形（内部使用）
    /// </summary>
    private static (int X, int Y, int Width, int Height) GetWindowRectInternal(IntPtr hwnd)
    {
        GetWindowRect(hwnd, out var rect);
        return (rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top);
    }

    /// <summary>
    /// 获取窗口所属进程名称
    /// </summary>
    public static string GetProcessNameFromWindow(nint hwnd)
    {
        try
        {
            unsafe
            {
                uint processId;
                PInvoke.GetWindowThreadProcessId(new HWND(hwnd), &processId);

                if (processId == 0) return string.Empty;

                using var process = System.Diagnostics.Process.GetProcessById((int)processId);
                return process.ProcessName;
            }
        }
        catch
        {
            return string.Empty;
        }
    }
}