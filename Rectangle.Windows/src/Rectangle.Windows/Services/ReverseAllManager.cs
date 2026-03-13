using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Win32.Foundation;
using Rectangle.Windows.Core;

namespace Rectangle.Windows.Services;

/// <summary>
/// 反转所有窗口管理器
/// 将所有窗口的位置进行镜像反转
/// </summary>
public class ReverseAllManager
{
    private readonly Win32WindowService _win32;

    public ReverseAllManager(Win32WindowService win32)
    {
        _win32 = win32;
    }

    /// <summary>
    /// 反转所有窗口位置（水平镜像）
    /// </summary>
    /// <param name="workArea">工作区域</param>
    public void ReverseAll(WorkArea workArea)
    {
        var windows = WindowEnumerator.EnumerateVisibleWindows();
        if (windows.Count == 0) return;

        // 记录所有窗口的当前位置
        var windowRects = new List<(nint hwnd, WindowRect rect)>();
        foreach (var hwnd in windows)
        {
            var (x, y, w, h) = _win32.GetWindowRect(hwnd);
            windowRects.Add((hwnd, new WindowRect(x, y, w, h)));
        }

        // 水平镜像反转每个窗口
        foreach (var (hwnd, rect) in windowRects)
        {
            // 计算镜像位置
            var newX = workArea.Right - (rect.X - workArea.Left) - rect.Width;

            _win32.SetWindowRect(hwnd, newX, rect.Y, rect.Width, rect.Height);
        }

        Logger.Info("ReverseAllManager", $"反转 {windows.Count} 个窗口位置");
    }

    /// <summary>
    /// 垂直镜像反转所有窗口位置
    /// </summary>
    /// <param name="workArea">工作区域</param>
    public void ReverseAllVertical(WorkArea workArea)
    {
        var windows = WindowEnumerator.EnumerateVisibleWindows();
        if (windows.Count == 0) return;

        // 记录所有窗口的当前位置
        var windowRects = new List<(nint hwnd, WindowRect rect)>();
        foreach (var hwnd in windows)
        {
            var (x, y, w, h) = _win32.GetWindowRect(hwnd);
            windowRects.Add((hwnd, new WindowRect(x, y, w, h)));
        }

        // 垂直镜像反转每个窗口
        foreach (var (hwnd, rect) in windowRects)
        {
            // 计算镜像位置
            var newY = workArea.Bottom - (rect.Y - workArea.Top) - rect.Height;

            _win32.SetWindowRect(hwnd, rect.X, newY, rect.Width, rect.Height);
        }

        Logger.Info("ReverseAllManager", $"垂直反转 {windows.Count} 个窗口位置");
    }
}