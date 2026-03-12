using System;
using Rectangle.Windows.Core;

namespace Rectangle.Windows.Services;

/// <summary>
/// Todo 模式管理器
/// 管理 Todo 应用窗口的位置，为其他窗口预留侧边栏空间
/// </summary>
public class TodoManager
{
    private readonly Win32WindowService _win32;
    private readonly ConfigService _configService;

    public TodoManager(Win32WindowService win32, ConfigService configService)
    {
        _win32 = win32;
        _configService = configService;
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
    /// 获取 Todo 侧边栏宽度
    /// </summary>
    public int GetTodoSidebarWidth()
    {
        var config = _configService.Load();
        return config.TodoSidebarWidth;
    }

    /// <summary>
    /// 获取 Todo 侧边栏位置
    /// </summary>
    public string GetTodoSidebarSide()
    {
        var config = _configService.Load();
        return config.TodoSidebarSide;
    }

    /// <summary>
    /// 定位 Todo 应用窗口到侧边栏
    /// </summary>
    /// <param name="workArea">工作区域</param>
    public void PositionTodoWindow(WorkArea workArea)
    {
        var config = _configService.Load();
        if (!config.TodoMode || string.IsNullOrEmpty(config.TodoApplication))
            return;

        // 查找 Todo 应用窗口
        var todoWindows = WindowEnumerator.EnumerateWindowsByProcess(config.TodoApplication);
        if (todoWindows.Count == 0)
        {
            Console.WriteLine($"[TodoManager] 未找到 Todo 应用窗口: {config.TodoApplication}");
            return;
        }

        var gap = config.GapSize;
        var todoWidth = config.TodoSidebarWidth;
        var isLeft = config.TodoSidebarSide.Equals("Left", StringComparison.OrdinalIgnoreCase);

        foreach (var hwnd in todoWindows)
        {
            int x, y, width, height;

            if (isLeft)
            {
                x = workArea.Left + gap;
                width = todoWidth - gap * 2;
            }
            else
            {
                x = workArea.Right - todoWidth + gap;
                width = todoWidth - gap * 2;
            }

            y = workArea.Top + gap;
            height = workArea.Height - gap * 2;

            _win32.SetWindowRect(hwnd, x, y, width, height);
            Console.WriteLine($"[TodoManager] 已定位 Todo 窗口: {hwnd} -> ({x}, {y}, {width}, {height})");
        }
    }

    /// <summary>
    /// 计算排除 Todo 侧边栏后的可用工作区域
    /// </summary>
    /// <param name="workArea">原始工作区域</param>
    /// <returns>调整后的工作区域</returns>
    public WorkArea GetAdjustedWorkArea(WorkArea workArea)
    {
        var config = _configService.Load();
        if (!config.TodoMode || string.IsNullOrEmpty(config.TodoApplication))
            return workArea;

        var todoWidth = config.TodoSidebarWidth;
        var isLeft = config.TodoSidebarSide.Equals("Left", StringComparison.OrdinalIgnoreCase);

        if (isLeft)
        {
            return new WorkArea(
                workArea.Left + todoWidth,
                workArea.Top,
                workArea.Right,
                workArea.Bottom
            );
        }
        else
        {
            return new WorkArea(
                workArea.Left,
                workArea.Top,
                workArea.Right - todoWidth,
                workArea.Bottom
            );
        }
    }

    /// <summary>
    /// 检查窗口是否是 Todo 应用窗口
    /// </summary>
    /// <param name="hwnd">窗口句柄</param>
    /// <returns>是否是 Todo 应用窗口</returns>
    public bool IsTodoWindow(nint hwnd)
    {
        var config = _configService.Load();
        if (!config.TodoMode || string.IsNullOrEmpty(config.TodoApplication))
            return false;

        var processName = WindowEnumerator.GetProcessNameFromWindow(hwnd);
        return processName.Equals(config.TodoApplication, StringComparison.OrdinalIgnoreCase);
    }
}