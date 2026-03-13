using System;
using System.Collections.Generic;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;

namespace Rectangle.Windows.WinUI.Services
{
    /// <summary>
    /// 屏幕检测服务
    /// </summary>
    public class ScreenDetectionService
    {
        private readonly Win32WindowService _win32;

        public ScreenDetectionService(Win32WindowService win32)
        {
            _win32 = win32;
        }

        /// <summary>
        /// 获取所有显示器的工作区域
        /// </summary>
        public List<Core.WindowRect> GetAllWorkAreas()
        {
            var areas = new List<Core.WindowRect>();

            unsafe
            {
                PInvoke.EnumDisplayMonitors(null, null, (hMonitor, hdcMonitor, lprcMonitor, dwData) =>
                {
                    var info = new MONITORINFO { cbSize = (uint)sizeof(MONITORINFO) };
                    if (PInvoke.GetMonitorInfo(hMonitor, &info))
                    {
                        areas.Add(new Core.WindowRect(
                            info.rcWork.left,
                            info.rcWork.top,
                            info.rcWork.right - info.rcWork.left,
                            info.rcWork.bottom - info.rcWork.top));
                    }
                    return true;
                }, 0);
            }

            return areas;
        }

        /// <summary>
        /// 获取鼠标所在的显示器工作区域
        /// </summary>
        public Core.WindowRect GetWorkAreaFromCursor()
        {
            PInvoke.GetCursorPos(out var pt);
            return GetWorkAreaFromPoint(pt.X, pt.Y);
        }

        /// <summary>
        /// 获取指定点所在的显示器工作区域
        /// </summary>
        public Core.WindowRect GetWorkAreaFromPoint(int x, int y)
        {
            unsafe
            {
                var pt = new System.Drawing.Point(x, y);
                var hMonitor = PInvoke.MonitorFromPoint(pt, MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST);

                var info = new MONITORINFO { cbSize = (uint)sizeof(MONITORINFO) };
                if (PInvoke.GetMonitorInfo(hMonitor, &info))
                {
                    return new Core.WindowRect(
                        info.rcWork.left,
                        info.rcWork.top,
                        info.rcWork.right - info.rcWork.left,
                        info.rcWork.bottom - info.rcWork.top);
                }
            }

            // 默认返回主屏幕
            return GetPrimaryWorkArea();
        }

        /// <summary>
        /// 获取窗口所在的显示器工作区域
        /// </summary>
        public Core.WindowRect GetWorkAreaFromWindow(nint hwnd)
        {
            unsafe
            {
                var hMonitor = PInvoke.MonitorFromWindow(new HWND(hwnd), MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST);

                var info = new MONITORINFO { cbSize = (uint)sizeof(MONITORINFO) };
                if (PInvoke.GetMonitorInfo(hMonitor, &info))
                {
                    return new Core.WindowRect(
                        info.rcWork.left,
                        info.rcWork.top,
                        info.rcWork.right - info.rcWork.left,
                        info.rcWork.bottom - info.rcWork.top);
                }
            }

            return GetPrimaryWorkArea();
        }

        /// <summary>
        /// 获取主显示器工作区域
        /// </summary>
        public Core.WindowRect GetPrimaryWorkArea()
        {
            unsafe
            {
                var info = new MONITORINFO { cbSize = (uint)sizeof(MONITORINFO) };
                var hMonitor = PInvoke.MonitorFromWindow(new HWND(0), MONITOR_FROM_FLAGS.MONITOR_DEFAULTTOPRIMARY);

                if (PInvoke.GetMonitorInfo(hMonitor, &info))
                {
                    return new Core.WindowRect(
                        info.rcWork.left,
                        info.rcWork.top,
                        info.rcWork.right - info.rcWork.left,
                        info.rcWork.bottom - info.rcWork.top);
                }
            }

            // 如果都失败了，返回一个默认区域
            return new Core.WindowRect(0, 0, 1920, 1080);
        }

        /// <summary>
        /// 获取显示器数量
        /// </summary>
        public int GetDisplayCount()
        {
            int count = 0;
            unsafe
            {
                PInvoke.EnumDisplayMonitors(null, null, (hMonitor, hdcMonitor, lprcMonitor, dwData) =>
                {
                    count++;
                    return true;
                }, 0);
            }
            return count;
        }
    }
}
