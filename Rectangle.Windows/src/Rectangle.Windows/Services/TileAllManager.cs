using System;
using System.Collections.Generic;
using Windows.Win32.Foundation;
using Rectangle.Windows.Core;

namespace Rectangle.Windows.Services;

/// <summary>
/// 平铺所有窗口管理器
/// 将所有可见窗口平铺排列在屏幕上
/// </summary>
public class TileAllManager
{
    private readonly Win32WindowService _win32;
    private readonly ConfigService? _configService;

    public TileAllManager(Win32WindowService win32, ConfigService? configService = null)
    {
        _win32 = win32;
        _configService = configService;
    }

    /// <summary>
    /// 平铺所有窗口
    /// </summary>
    /// <param name="workArea">工作区域</param>
    public void TileAll(WorkArea workArea)
    {
        var windows = WindowEnumerator.EnumerateVisibleWindows();
        if (windows.Count == 0) return;

        // 计算最佳网格布局
        var (cols, rows) = CalculateGridLayout(windows.Count, workArea);

        // 计算每个单元格的大小
        var cellWidth = workArea.Width / cols;
        var cellHeight = workArea.Height / rows;

        // 应用布局
        int index = 0;
        foreach (var hwnd in windows)
        {
            if (index >= cols * rows) break;

            int col = index % cols;
            int row = index / cols;

            int x = workArea.Left + col * cellWidth;
            int y = workArea.Top + row * cellHeight;

            _win32.SetWindowRect(hwnd, x, y, cellWidth, cellHeight);
            index++;
        }

        Logger.Info("TileAllManager", $"平铺 {windows.Count} 个窗口，布局: {cols}x{rows}");
    }

    /// <summary>
    /// 平铺当前应用的所有窗口
    /// </summary>
    /// <param name="workArea">工作区域</param>
    /// <param name="processName">进程名称</param>
    public void TileActiveApp(WorkArea workArea, string processName)
    {
        var windows = WindowEnumerator.EnumerateWindowsByProcess(processName);
        if (windows.Count == 0) return;

        // 计算最佳网格布局
        var (cols, rows) = CalculateGridLayout(windows.Count, workArea);

        // 计算每个单元格的大小
        var cellWidth = workArea.Width / cols;
        var cellHeight = workArea.Height / rows;

        // 应用布局
        int index = 0;
        foreach (var hwnd in windows)
        {
            if (index >= cols * rows) break;

            int col = index % cols;
            int row = index / cols;

            int x = workArea.Left + col * cellWidth;
            int y = workArea.Top + row * cellHeight;

            _win32.SetWindowRect(hwnd, x, y, cellWidth, cellHeight);
            index++;
        }

        Logger.Info("TileAllManager", $"平铺应用 {processName} 的 {windows.Count} 个窗口，布局: {cols}x{rows}");
    }

    /// <summary>
    /// 计算最佳网格布局
    /// </summary>
    private (int cols, int rows) CalculateGridLayout(int windowCount, WorkArea workArea)
    {
        if (windowCount <= 1) return (1, 1);
        if (windowCount == 2) return (2, 1);
        if (windowCount == 3) return (3, 1);
        if (windowCount == 4) return (2, 2);
        if (windowCount <= 6) return (3, 2);
        if (windowCount <= 9) return (3, 3);
        if (windowCount <= 12) return (4, 3);
        if (windowCount <= 16) return (4, 4);

        // 更多窗口时按比例计算
        var aspectRatio = (double)workArea.Width / workArea.Height;
        var total = (double)windowCount;
        var cols = (int)Math.Ceiling(Math.Sqrt(total * aspectRatio));
        var rows = (int)Math.Ceiling(total / cols);

        return (cols, rows);
    }
}