using System;
using System.Collections.Generic;
using Windows.Win32.Foundation;
using Rectangle.Windows.Core;

namespace Rectangle.Windows.Services;

/// <summary>
/// 层叠所有窗口管理器
/// 将所有可见窗口层叠排列在屏幕上
/// </summary>
public class CascadeAllManager
{
    private readonly Win32WindowService _win32;
    private readonly ConfigService? _configService;

    // 默认层叠偏移量
    private const int DefaultCascadeOffset = 30;

    public CascadeAllManager(Win32WindowService win32, ConfigService? configService = null)
    {
        _win32 = win32;
        _configService = configService;
    }

    /// <summary>
    /// 层叠所有窗口
    /// </summary>
    /// <param name="workArea">工作区域</param>
    public void CascadeAll(WorkArea workArea)
    {
        var windows = WindowEnumerator.EnumerateVisibleWindows();
        if (windows.Count == 0) return;

        var offset = GetCascadeOffset();
        var (windowWidth, windowHeight) = CalculateWindowSize(workArea, windows.Count);

        // 层叠窗口
        int index = 0;
        foreach (var hwnd in windows)
        {
            int x = workArea.Left + index * offset;
            int y = workArea.Top + index * offset;

            // 确保不超出工作区边界
            if (x + windowWidth > workArea.Right)
                x = workArea.Right - windowWidth;
            if (y + windowHeight > workArea.Bottom)
                y = workArea.Bottom - windowHeight;

            _win32.SetWindowRect(hwnd, x, y, windowWidth, windowHeight);
            index++;
        }

        Logger.Info("CascadeAllManager", $"层叠 {windows.Count} 个窗口");
    }

    /// <summary>
    /// 层叠当前应用的所有窗口
    /// </summary>
    /// <param name="workArea">工作区域</param>
    /// <param name="processName">进程名称</param>
    public void CascadeActiveApp(WorkArea workArea, string processName)
    {
        var windows = WindowEnumerator.EnumerateWindowsByProcess(processName);
        if (windows.Count == 0) return;

        var offset = GetCascadeOffset();
        var (windowWidth, windowHeight) = CalculateWindowSize(workArea, windows.Count);

        // 层叠窗口
        int index = 0;
        foreach (var hwnd in windows)
        {
            int x = workArea.Left + index * offset;
            int y = workArea.Top + index * offset;

            // 确保不超出工作区边界
            if (x + windowWidth > workArea.Right)
                x = workArea.Right - windowWidth;
            if (y + windowHeight > workArea.Bottom)
                y = workArea.Bottom - windowHeight;

            _win32.SetWindowRect(hwnd, x, y, windowWidth, windowHeight);
            index++;
        }

        Logger.Info("CascadeAllManager", $"层叠应用 {processName} 的 {windows.Count} 个窗口");
    }

    /// <summary>
    /// 获取层叠偏移量
    /// </summary>
    private int GetCascadeOffset()
    {
        // 可以从配置读取
        return DefaultCascadeOffset;
    }

    /// <summary>
    /// 计算窗口大小（基于层叠窗口数量）
    /// </summary>
    private (int width, int height) CalculateWindowSize(WorkArea workArea, int windowCount)
    {
        var offset = GetCascadeOffset();
        var totalOffset = (windowCount - 1) * offset;

        var width = workArea.Width - totalOffset;
        var height = workArea.Height - totalOffset;

        // 确保最小尺寸
        width = Math.Max(width, 400);
        height = Math.Max(height, 300);

        return (width, height);
    }
}