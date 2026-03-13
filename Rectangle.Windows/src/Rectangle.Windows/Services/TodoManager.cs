using System;
using System.Collections.Generic;
using System.Linq;
using Rectangle.Windows.Core;

namespace Rectangle.Windows.Services;

/// <summary>
/// Todo 模式管理器
/// 管理 Todo 应用窗口和其他窗口的协调布局
/// </summary>
public class TodoManager
{
    private readonly Win32WindowService _win32;
    private readonly ConfigService _configService;
    private readonly WindowTypeService _windowTypeService;

    public TodoManager(Win32WindowService win32, ConfigService configService, WindowTypeService windowTypeService)
    {
        _win32 = win32;
        _configService = configService;
        _windowTypeService = windowTypeService;
    }

    /// <summary>
    /// 检查 Todo 模式是否启用
    /// </summary>
    public bool IsTodoModeEnabled()
    {
        var config = _configService.Load();
        return config.TodoMode && !string.IsNullOrEmpty(config.TodoApplication);
    }

    /// <summary>
    /// 获取 Todo 应用窗口句柄
    /// </summary>
    public nint GetTodoWindow()
    {
        var config = _configService.Load();
        if (string.IsNullOrEmpty(config.TodoApplication))
            return 0;

        var windows = WindowEnumerator.EnumerateWindowsByProcess(config.TodoApplication);
        return windows.FirstOrDefault();
    }

    /// <summary>
    /// 调整 Todo 窗口到侧边栏位置
    /// </summary>
    public void AdjustTodoWindow(WorkArea workArea)
    {
        var config = _configService.Load();
        if (!config.TodoMode) return;

        var todoHwnd = GetTodoWindow();
        if (todoHwnd == 0) return;

        var sidebarWidth = config.TodoSidebarWidth;
        var isLeftSide = config.TodoSidebarSide == TodoSidebarSide.Left;

        int x = isLeftSide ? workArea.Left : workArea.Right - sidebarWidth;
        int y = workArea.Top;
        int width = sidebarWidth;
        int height = workArea.Height;

        _win32.SetWindowRect(todoHwnd, x, y, width, height);

        Logger.Info("TodoManager", $"调整 Todo 窗口到 {(isLeftSide ? "左侧" : "右侧")}: ({x}, {y}, {width}, {height})");
    }

    /// <summary>
    /// 获取排除 Todo 区域后的可用工作区
    /// </summary>
    public WorkArea GetAvailableWorkArea(WorkArea fullWorkArea)
    {
        var config = _configService.Load();
        if (!config.TodoMode || string.IsNullOrEmpty(config.TodoApplication))
            return fullWorkArea;

        var todoHwnd = GetTodoWindow();
        if (todoHwnd == 0) return fullWorkArea;

        var sidebarWidth = config.TodoSidebarWidth;
        var isLeftSide = config.TodoSidebarSide == TodoSidebarSide.Left;

        if (isLeftSide)
        {
            return new WorkArea(
                fullWorkArea.Left + sidebarWidth,
                fullWorkArea.Top,
                fullWorkArea.Right,
                fullWorkArea.Bottom
            );
        }
        else
        {
            return new WorkArea(
                fullWorkArea.Left,
                fullWorkArea.Top,
                fullWorkArea.Right - sidebarWidth,
                fullWorkArea.Bottom
            );
        }
    }

    /// <summary>
    /// 检查窗口是否是 Todo 应用窗口
    /// </summary>
    public bool IsTodoWindow(nint hwnd)
    {
        var config = _configService.Load();
        if (string.IsNullOrEmpty(config.TodoApplication))
            return false;

        var processName = WindowEnumerator.GetProcessNameFromWindow(hwnd);
        return processName.Equals(config.TodoApplication, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 获取所有非 Todo 窗口
    /// </summary>
    public List<nint> GetNonTodoWindows()
    {
        var allWindows = WindowEnumerator.EnumerateVisibleWindows();
        return allWindows.Where(hwnd => !IsTodoWindow(hwnd)).ToList();
    }

    /// <summary>
    /// 重新布局所有非 Todo 窗口（在可用区域内）
    /// </summary>
    public void RelayoutNonTodoWindows(WorkArea workArea)
    {
        var availableWorkArea = GetAvailableWorkArea(workArea);
        var windows = GetNonTodoWindows();

        if (windows.Count == 0) return;

        // 简单的平铺布局
        var tileManager = new TileAllManager(_win32, _configService);
        tileManager.TileAll(availableWorkArea);

        Logger.Info("TodoManager", $"重新布局 {windows.Count} 个非 Todo 窗口");
    }
}
