using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace Rectangle.Windows.WinUI.Services
{
    /// <summary>
    /// 窗口枚举器：枚举系统中所有可见窗口
    /// </summary>
    public static class WindowEnumerator
    {
        private static unsafe string GetWindowTitleInternal(IntPtr hwnd)
        {
            int length = PInvoke.GetWindowTextLength(new HWND(hwnd));
            if (length == 0) return string.Empty;

            char* buf = stackalloc char[length + 1];
            int result = PInvoke.GetWindowText(new HWND(hwnd), buf, length + 1);
            return result > 0 ? new string(buf, 0, result) : string.Empty;
        }

        private static bool IsToolWindowInternal(IntPtr hwnd)
        {
            var exStyle = GetWindowLong(hwnd, -20);
            const int WS_EX_TOOLWINDOW = 0x00000080;
            return (exStyle.ToInt64() & WS_EX_TOOLWINDOW) != 0;
        }

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

        private static (int X, int Y, int Width, int Height) GetWindowRectInternal(IntPtr hwnd)
        {
            PInvoke.GetWindowRect(new HWND(hwnd), out var rect);
            return (rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top);
        }

        private static bool IsWindowValid(IntPtr hwnd, bool excludeMinimized, bool excludeToolWindows)
        {
            if (!PInvoke.IsWindowVisible(new HWND(hwnd)))
                return false;

            var title = GetWindowTitleInternal(hwnd);
            if (string.IsNullOrEmpty(title))
                return false;

            if (excludeMinimized && PInvoke.IsIconic(new HWND(hwnd)))
                return false;

            if (excludeToolWindows && IsToolWindowInternal(hwnd))
                return false;

            var (x, y, w, h) = GetWindowRectInternal(hwnd);
            if (w <= 0 || h <= 0)
                return false;

            return true;
        }

        /// <summary>
        /// 枚举所有可见窗口
        /// </summary>
        public static List<nint> EnumerateVisibleWindows(bool excludeMinimized = true, bool excludeToolWindows = true)
        {
            var windows = new List<nint>();

            PInvoke.EnumWindows((hwnd, lParam) =>
            {
                if (IsWindowValid(hwnd.Value, excludeMinimized, excludeToolWindows))
                {
                    windows.Add(hwnd.Value);
                }
                return true;
            }, default);

            return windows;
        }

        /// <summary>
        /// 枚举指定进程的所有窗口
        /// </summary>
        public static List<nint> EnumerateWindowsByProcess(string processName, bool excludeMinimized = true)
        {
            var windows = new List<nint>();

            PInvoke.EnumWindows((hwnd, lParam) =>
            {
                if (IsWindowValid(hwnd.Value, excludeMinimized, true))
                {
                    var process = GetProcessNameFromWindow(hwnd.Value);
                    if (process.Equals(processName, StringComparison.OrdinalIgnoreCase))
                    {
                        windows.Add(hwnd.Value);
                    }
                }
                return true;
            }, default);

            return windows;
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
}
