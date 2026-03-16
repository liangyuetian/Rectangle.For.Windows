using Rectangle.Windows.WinUI.Core;
using Rectangle.Windows.WinUI.Core.Calculators;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace Rectangle.Windows.WinUI.Services;

/// <summary>
/// 窗口管理器 - 处理所有窗口操作
/// </summary>
public class WindowManager
{
    private readonly Win32WindowService _win32;
    private readonly CalculatorFactory _factory;
    private readonly WindowHistory _history;
    private readonly ScreenDetectionService _screenDetection;
    private readonly WindowTypeService _windowType;
    private readonly HashSet<nint> _maximizedWindows = new();
    private LastActiveWindowService? _lastActiveService;
    private ConfigService? _configService;
    private int _gapSize = 0;

    public WindowManager(Win32WindowService win32, CalculatorFactory factory, WindowHistory history)
    {
        _win32 = win32;
        _factory = factory;
        _history = history;
        _screenDetection = new ScreenDetectionService(win32);
        _windowType = new WindowTypeService(win32);
    }

    public void SetLastActiveWindowService(LastActiveWindowService service)
    {
        _lastActiveService = service;
    }

    public void SetConfigService(ConfigService configService)
    {
        _configService = configService;
        ReloadConfig();
    }

    public void ReloadConfig()
    {
        if (_configService == null) return;
        var config = _configService.Load();
        _gapSize = config.GapSize;
    }

    private bool IsIgnoredApp(string processName)
    {
        if (_configService == null) return false;
        var config = _configService.Load();
        return config.IgnoredApps.Exists(app =>
            app.Equals(processName, StringComparison.OrdinalIgnoreCase) ||
            app.Equals(processName + ".exe", StringComparison.OrdinalIgnoreCase));
    }

    private static string GetActionDisplayName(WindowAction action)
    {
        return action switch
        {
            // 半屏
            WindowAction.LeftHalf => "左半屏",
            WindowAction.RightHalf => "右半屏",
            WindowAction.CenterHalf => "中间半屏",
            WindowAction.TopHalf => "上半屏",
            WindowAction.BottomHalf => "下半屏",
            // 四角
            WindowAction.TopLeft => "左上",
            WindowAction.TopRight => "右上",
            WindowAction.BottomLeft => "左下",
            WindowAction.BottomRight => "右下",
            // 三分之一
            WindowAction.FirstThird => "左首 1/3",
            WindowAction.CenterThird => "中间 1/3",
            WindowAction.LastThird => "右首 1/3",
            WindowAction.FirstTwoThirds => "左侧 2/3",
            WindowAction.CenterTwoThirds => "中间 2/3",
            WindowAction.LastTwoThirds => "右侧 2/3",
            // 四等分
            WindowAction.FirstFourth => "左首 1/4",
            WindowAction.SecondFourth => "左二 1/4",
            WindowAction.ThirdFourth => "右二 1/4",
            WindowAction.LastFourth => "右首 1/4",
            WindowAction.FirstThreeFourths => "左侧 3/4",
            WindowAction.CenterThreeFourths => "中间 3/4",
            WindowAction.LastThreeFourths => "右侧 3/4",
            // 六等分
            WindowAction.TopLeftSixth => "左上 1/6",
            WindowAction.TopCenterSixth => "中上 1/6",
            WindowAction.TopRightSixth => "右上 1/6",
            WindowAction.BottomLeftSixth => "左下 1/6",
            WindowAction.BottomCenterSixth => "中下 1/6",
            WindowAction.BottomRightSixth => "右下 1/6",
            // 移动
            WindowAction.MoveLeft => "向左移动",
            WindowAction.MoveRight => "向右移动",
            WindowAction.MoveUp => "向上移动",
            WindowAction.MoveDown => "向下移动",
            // 最大化与缩放
            WindowAction.Maximize => "最大化",
            WindowAction.AlmostMaximize => "接近最大化",
            WindowAction.MaximizeHeight => "最大化高度",
            WindowAction.Larger => "放大",
            WindowAction.Smaller => "缩小",
            WindowAction.Center => "居中",
            WindowAction.Restore => "恢复",
            // 显示器
            WindowAction.NextDisplay => "下一个显示器",
            WindowAction.PreviousDisplay => "上一个显示器",
            _ => action.ToString()
        };
    }

    public void Execute(WindowAction action, nint? targetHwnd = null)
    {
        if (action == WindowAction.Restore)
        {
            ExecuteRestore(targetHwnd);
            return;
        }

        if (action == WindowAction.Maximize)
        {
            ExecuteMaximizeToggle(targetHwnd);
            return;
        }

        if (action == WindowAction.NextDisplay)
        {
            ExecuteNextDisplay(targetHwnd);
            return;
        }

        if (action == WindowAction.PreviousDisplay)
        {
            ExecutePreviousDisplay(targetHwnd);
            return;
        }

        var hwnd = targetHwnd ?? GetTargetWindow();
        if (hwnd == 0)
        {
            PlayBeep();
            return;
        }

        var processName = _win32.GetProcessNameFromWindow(hwnd);

        // 检查是否在忽略列表中
        if (IsIgnoredApp(processName))
        {
            Logger.Info("WindowManager", $"{processName} 在忽略列表中，跳过操作");
            return;
        }

        // 检查窗口类型
        if (_windowType.IsModalDialog(hwnd))
        {
            Logger.Info("WindowManager", $"{processName} 是模态对话框，跳过操作");
            return;
        }

        var (x, y, w, h) = _win32.GetWindowRect(hwnd);

        // 根据配置获取目标屏幕（光标位置或窗口位置）
        var workArea = GetTargetWorkArea(hwnd);

        // 应用窗口间隙
        workArea = ApplyGap(workArea);

        var current = new WindowRect(x, y, w, h);

        // 检测窗口是否被用户手动移动
        bool windowMovedExternally = _history.IsWindowMovedExternally(hwnd, x, y, w, h);

        if (windowMovedExternally)
        {
            // 用户手动移动了窗口，清除程序操作记录
            _history.RemoveLastAction(hwnd);
            Logger.Info("WindowManager", $"检测到窗口被用户手动移动: {processName}");
        }

        // 处理重复执行模式（循环尺寸），支持多显示器轮询
        var (actualAction, targetDisplayIndex) = GetActualAction(hwnd, action, windowMovedExternally);
        if (actualAction != action || targetDisplayIndex.HasValue)
        {
            Logger.Info("WindowManager", targetDisplayIndex.HasValue
                ? $"循环尺寸(显示器{targetDisplayIndex.Value + 1}): {action} → {actualAction}"
                : $"循环尺寸: {action} → {actualAction}");
        }

        var calculator = _factory.GetCalculator(actualAction);
        if (calculator == null) return;

        // 多显示器轮询时使用指定显示器的工作区域
        if (targetDisplayIndex.HasValue)
        {
            workArea = ApplyGap(GetWorkAreaByDisplayIndex(targetDisplayIndex.Value));
        }

        // 保存或更新恢复点：
        // 1. 如果没有恢复点，保存当前位置
        // 2. 如果窗口被用户手动移动，更新恢复点
        if (!_history.HasRestoreRect(hwnd) || windowMovedExternally)
        {
            _history.SaveRestoreRect(hwnd, x, y, w, h);
            Logger.Debug("WindowManager", $"保存恢复点: ({x}, {y}, {w}, {h})");
        }

        // 标记此窗口由程序调整（用于窗口位置监听时排除记录）
        _history.MarkAsProgramAdjusted(hwnd);

        // 执行其他操作时，清除最大化状态
        _maximizedWindows.Remove(hwnd);

        var target = calculator.Calculate(workArea, current, actualAction);

        // 为相邻窗口应用间隙
        target = ApplyWindowGap(target, workArea, actualAction);

        // 对固定尺寸窗口特殊处理：只移动，不调整大小
        if (!_windowType.IsResizable(hwnd))
        {
            Logger.Info("WindowManager", $"{processName} 是固定尺寸窗口，只移动不调整大小");
            target = HandleFixedSizeWindow(current, target, workArea, actualAction);
        }

        // 应用最小窗口尺寸限制
        target = ApplyMinimumSize(target);

        // 确保窗口在屏幕工作区内
        target = ClampToWorkArea(target, workArea);

        _win32.SetWindowRect(hwnd, target.X, target.Y, target.Width, target.Height);

        // 记录程序操作信息（包括操作类型、次数、时间）
        // 注意：记录的是原始 action，而不是 actualAction，这样才能正确计数
        _history.RecordAction(hwnd, action, target.X, target.Y, target.Width, target.Height);

        // 根据配置移动光标到窗口中心
        MoveCursorIfEnabled(hwnd, action);

        Logger.Info("WindowManager", $"{GetActionDisplayName(actualAction)} 了 {processName}");
    }

    /// <summary>
    /// 根据重复执行模式获取实际应该执行的操作，以及目标显示器索引（多显示器轮询时）。
    /// </summary>
    private (WindowAction Action, int? TargetDisplayIndex) GetActualAction(nint hwnd, WindowAction requestedAction, bool windowMovedExternally)
    {
        // 如果窗口被用户手动移动，重置循环
        if (windowMovedExternally)
        {
            return (requestedAction, null);
        }

        // 获取配置的重复执行模式
        var config = _configService?.Load();
        var mode = (SubsequentExecutionMode)(config?.SubsequentExecutionMode ?? 0);

        // 如果模式是 None 或不支持循环，直接返回原操作
        if (mode != SubsequentExecutionMode.CycleSize ||
            !RepeatedExecutionsCalculator.SupportsCycle(requestedAction))
        {
            return (requestedAction, null);
        }

        // 获取最后操作信息
        if (!_history.TryGetLastAction(hwnd, out var lastAction))
        {
            return (requestedAction, null);
        }

        // 检查是否是同一个操作（只有连续按同一快捷键才触发循环）
        if (lastAction.Action != requestedAction)
        {
            return (requestedAction, null);
        }

        var workAreas = _win32.GetMonitorWorkAreas();
        var numDisplays = workAreas.Count;
        var executionCount = lastAction.Count + 1;

        // 多显示器：参考 macOS Rectangle，所有支持循环的操作都在多显示器间轮询
        // 轮询顺序：显示器1 所有位置 → 显示器2 所有位置 → ... → 显示器1
        if (numDisplays > 1 && RepeatedExecutionsCalculator.SupportsCycle(requestedAction))
        {
            var cycle = RepeatedExecutionsCalculator.GetCycleGroup(requestedAction);
            var groupLength = cycle.Length;
            var totalCycleLength = groupLength * numDisplays;
            var cycleIndex = (executionCount - 1) % totalCycleLength;
            var displayIndex = cycleIndex / groupLength;
            var positionInGroup = cycleIndex % groupLength;
            var actualAction = cycle[positionInGroup];
            return (actualAction, displayIndex);
        }

        // 单显示器：使用原有循环逻辑
        var nextAction = RepeatedExecutionsCalculator.GetNextCycleAction(requestedAction, executionCount);
        return (nextAction, null);
    }

    /// <summary>
    /// 根据显示器索引获取工作区域
    /// </summary>
    private WorkArea GetWorkAreaByDisplayIndex(int index)
    {
        var workAreas = _win32.GetMonitorWorkAreas();
        if (index < 0 || index >= workAreas.Count)
        {
            return workAreas.Count > 0 ? workAreas[0] : new WorkArea(0, 0, 1920, 1080);
        }
        return workAreas[index];
    }

    /// <summary>
    /// 根据配置移动光标
    /// </summary>
    private void MoveCursorIfEnabled(nint hwnd, WindowAction action)
    {
        if (_configService == null) return;

        var config = _configService.Load();

        // 检查是否启用了光标移动
        bool shouldMoveCursor = config.MoveCursor;

        // 对于跨显示器操作，检查 MoveCursorAcrossDisplays
        if ((action == WindowAction.NextDisplay || action == WindowAction.PreviousDisplay)
            && !config.MoveCursorAcrossDisplays)
        {
            shouldMoveCursor = false;
        }

        if (shouldMoveCursor)
        {
            MoveCursorToWindowCenter(hwnd);
            Logger.Debug("WindowManager", "光标已移动到窗口中心");
        }
    }

    /// <summary>
    /// 将光标移动到窗口中心
    /// </summary>
    private void MoveCursorToWindowCenter(nint hwnd)
    {
        var (x, y, w, h) = _win32.GetWindowRect(hwnd);
        var centerX = x + w / 2;
        var centerY = y + h / 2;
        SetCursorPos(centerX, centerY);
    }

    [DllImport("user32.dll")]
    private static extern bool SetCursorPos(int x, int y);

    /// <summary>
    /// 播放提示音
    /// </summary>
    private void PlayBeep()
    {
        try
        {
            // 使用 Windows API 播放提示音
            MessageBeep(0);
        }
        catch { }
    }

    [DllImport("user32.dll")]
    private static extern bool MessageBeep(uint uType);

    /// <summary>
    /// 处理固定尺寸窗口：只移动位置，保持原有尺寸
    /// </summary>
    private WindowRect HandleFixedSizeWindow(WindowRect current, WindowRect target, WorkArea workArea, WindowAction action)
    {
        // 根据操作类型决定如何移动固定尺寸窗口
        switch (action)
        {
            case WindowAction.LeftHalf:
            case WindowAction.FirstThird:
            case WindowAction.FirstFourth:
                // 左对齐
                return new WindowRect(workArea.Left, target.Y, current.Width, current.Height);

            case WindowAction.RightHalf:
            case WindowAction.LastThird:
            case WindowAction.LastFourth:
                // 右对齐
                return new WindowRect(workArea.Right - current.Width, target.Y, current.Width, current.Height);

            case WindowAction.Center:
            case WindowAction.CenterHalf:
            case WindowAction.CenterThird:
                // 居中
                var centerX = workArea.Left + (workArea.Width - current.Width) / 2;
                var centerY = workArea.Top + (workArea.Height - current.Height) / 2;
                return new WindowRect(centerX, centerY, current.Width, current.Height);

            case WindowAction.TopHalf:
                // 顶部对齐
                return new WindowRect(target.X, workArea.Top, current.Width, current.Height);

            case WindowAction.BottomHalf:
                // 底部对齐
                return new WindowRect(target.X, workArea.Bottom - current.Height, current.Width, current.Height);

            case WindowAction.MoveLeft:
            case WindowAction.MoveRight:
            case WindowAction.MoveUp:
            case WindowAction.MoveDown:
                // 移动操作保持原有尺寸
                return new WindowRect(target.X, target.Y, current.Width, current.Height);

            default:
                // 其他操作：居中放置
                var defaultCenterX = workArea.Left + (workArea.Width - current.Width) / 2;
                var defaultCenterY = workArea.Top + (workArea.Height - current.Height) / 2;
                return new WindowRect(defaultCenterX, defaultCenterY, current.Width, current.Height);
        }
    }

    private nint GetTargetWindow()
    {
        // 获取当前前台窗口
        var foregroundHwnd = _win32.GetForegroundWindowHandle();

        // 检查前台窗口是否在忽略列表中
        bool isForegroundIgnored = false;
        if (foregroundHwnd != 0)
        {
            var processName = _win32.GetProcessNameFromWindow(foregroundHwnd);
            if (!string.IsNullOrEmpty(processName) && IsIgnoredApp(processName))
            {
                Logger.Info("WindowManager", $"前台窗口 {processName} 在忽略列表中，使用上次活跃窗口");
                isForegroundIgnored = true;
            }
        }

        // 如果有 LastActiveWindowService，优先使用它获取目标窗口
        if (_lastActiveService != null)
        {
            var lastActiveWindow = _lastActiveService.GetTargetWindow();
            if (lastActiveWindow != 0)
            {
                return lastActiveWindow;
            }
        }
        
        // 如果没有 LastActiveWindowService 或获取失败，且前台窗口未被忽略，使用前台窗口
        if (!isForegroundIgnored)
        {
            return foregroundHwnd;
        }
        
        // 所有尝试都失败，返回 0
        return 0;
    }

    private void ExecuteMaximizeToggle(nint? targetHwnd = null)
    {
        var hwnd = targetHwnd ?? GetTargetWindow();
        if (hwnd == 0)
        {
            PlayBeep();
            return;
        }

        var processName = _win32.GetProcessNameFromWindow(hwnd);

        // 检查是否在忽略列表中
        if (IsIgnoredApp(processName))
        {
            Logger.Info("WindowManager", $"{processName} 在忽略列表中，跳过操作");
            return;
        }

        // 使用 Windows 原生最大化/恢复：按一下最大化、再按一下恢复
        // IsMaximized 检测 WS_MAXIMIZE，支持本程序或用户点击标题栏最大化的窗口
        if (_windowType.IsMaximized(hwnd))
        {
            _win32.ShowWindowRestore(hwnd);
            Logger.Info("WindowManager", $"恢复了 {processName}");
        }
        else
        {
            _win32.ShowWindowMaximize(hwnd);
            Logger.Info("WindowManager", $"最大化了 {processName}");
        }
    }

    private void ExecuteRestore(nint? targetHwnd = null)
    {
        var hwnd = targetHwnd ?? GetTargetWindow();
        if (hwnd == 0)
        {
            PlayBeep();
            return;
        }

        if (!_history.TryGetRestoreRect(hwnd, out var rect))
        {
            Logger.Warning("WindowManager", "没有可恢复的窗口位置");
            PlayBeep();
            return;
        }

        var processName = _win32.GetProcessNameFromWindow(hwnd);
        _win32.SetWindowRect(hwnd, rect.X, rect.Y, rect.W, rect.H);

        // 清除程序最后操作记录（但保留恢复点，以便再次使用）
        _history.RemoveLastAction(hwnd);

        // 清除最大化状态
        _maximizedWindows.Remove(hwnd);

        Logger.Info("WindowManager", $"恢复了 {processName} 到 ({rect.X}, {rect.Y}, {rect.W}, {rect.H})");
    }

    private void ExecuteNextDisplay(nint? targetHwnd = null)
    {
        var hwnd = targetHwnd ?? GetTargetWindow();
        if (hwnd == 0) { PlayBeep(); return; }

        var processName = _win32.GetProcessNameFromWindow(hwnd);
        var (x, y, w, h) = _win32.GetWindowRect(hwnd);
        var nextWorkArea = GetNextMonitorWorkArea(hwnd);

        if (nextWorkArea == null)
        {
            PlayBeep();
            return;
        }

        // 检测窗口是否被用户手动移动
        bool windowMovedExternally = _history.IsWindowMovedExternally(hwnd, x, y, w, h);

        if (!_history.HasRestoreRect(hwnd) || windowMovedExternally)
        {
            _history.SaveRestoreRect(hwnd, x, y, w, h);
        }

        // 标记此窗口由程序调整
        _history.MarkAsProgramAdjusted(hwnd);

        // 将窗口移动到下一个显示器居中
        var newX = nextWorkArea.Value.X + (nextWorkArea.Value.Width - w) / 2;
        var newY = nextWorkArea.Value.Y + (nextWorkArea.Value.Height - h) / 2;
        _win32.SetWindowRect(hwnd, newX, newY, w, h);

        // 记录程序操作信息
        _history.RecordAction(hwnd, WindowAction.NextDisplay, newX, newY, w, h);

        Logger.Info("WindowManager", $"将 {processName} 移动到下一个显示器");
    }

    private void ExecutePreviousDisplay(nint? targetHwnd = null)
    {
        var hwnd = targetHwnd ?? GetTargetWindow();
        if (hwnd == 0) { PlayBeep(); return; }

        var processName = _win32.GetProcessNameFromWindow(hwnd);
        var (x, y, w, h) = _win32.GetWindowRect(hwnd);
        var prevWorkArea = GetPreviousMonitorWorkArea(hwnd);

        if (prevWorkArea == null)
        {
            PlayBeep();
            return;
        }

        // 检测窗口是否被用户手动移动
        bool windowMovedExternally = _history.IsWindowMovedExternally(hwnd, x, y, w, h);

        if (!_history.HasRestoreRect(hwnd) || windowMovedExternally)
        {
            _history.SaveRestoreRect(hwnd, x, y, w, h);
        }

        // 标记此窗口由程序调整
        _history.MarkAsProgramAdjusted(hwnd);

        // 将窗口移动到上一个显示器居中
        var newX = prevWorkArea.Value.X + (prevWorkArea.Value.Width - w) / 2;
        var newY = prevWorkArea.Value.Y + (prevWorkArea.Value.Height - h) / 2;
        _win32.SetWindowRect(hwnd, newX, newY, w, h);

        // 记录程序操作信息
        _history.RecordAction(hwnd, WindowAction.PreviousDisplay, newX, newY, w, h);

        Logger.Info("WindowManager", $"将 {processName} 移动到上一个显示器");
    }

    /// <summary>
    /// 获取下一个显示器的工作区域
    /// </summary>
    private WindowRect? GetNextMonitorWorkArea(nint hwnd)
    {
        var workAreas = _screenDetection.GetAllWorkAreas();
        if (workAreas.Count <= 1) return null;

        var current = _screenDetection.GetWorkAreaFromWindow(hwnd);

        // 找到当前显示器的索引
        int currentIndex = -1;
        for (int i = 0; i < workAreas.Count; i++)
        {
            if (workAreas[i].X == current.X && workAreas[i].Y == current.Y)
            {
                currentIndex = i;
                break;
            }
        }

        // 返回下一个显示器
        int nextIndex = (currentIndex + 1) % workAreas.Count;
        return workAreas[nextIndex];
    }

    /// <summary>
    /// 获取上一个显示器的工作区域
    /// </summary>
    private WindowRect? GetPreviousMonitorWorkArea(nint hwnd)
    {
        var workAreas = _screenDetection.GetAllWorkAreas();
        if (workAreas.Count <= 1) return null;

        var current = _screenDetection.GetWorkAreaFromWindow(hwnd);

        // 找到当前显示器的索引
        int currentIndex = -1;
        for (int i = 0; i < workAreas.Count; i++)
        {
            if (workAreas[i].X == current.X && workAreas[i].Y == current.Y)
            {
                currentIndex = i;
                break;
            }
        }

        // 返回上一个显示器
        int prevIndex = currentIndex <= 0 ? workAreas.Count - 1 : currentIndex - 1;
        return workAreas[prevIndex];
    }

    private WorkArea ApplyGap(WorkArea workArea)
    {
        if (_gapSize <= 0) return workArea;
        return new WorkArea(
            workArea.Left + _gapSize,
            workArea.Top + _gapSize,
            workArea.Right - _gapSize,
            workArea.Bottom - _gapSize
        );
    }

    private WindowRect ApplyWindowGap(WindowRect target, WorkArea workArea, WindowAction action)
    {
        if (_gapSize <= 0) return target;

        var halfGap = _gapSize / 2;

        // 根据操作类型，为窗口之间添加间隙
        return action switch
        {
            // 左半屏：右边留间隙
            WindowAction.LeftHalf => new WindowRect(target.X, target.Y, target.Width - halfGap, target.Height),
            // 右半屏：左边留间隙
            WindowAction.RightHalf => new WindowRect(target.X + halfGap, target.Y, target.Width - halfGap, target.Height),
            // 上半屏：下边留间隙
            WindowAction.TopHalf => new WindowRect(target.X, target.Y, target.Width, target.Height - halfGap),
            // 下半屏：上边留间隙
            WindowAction.BottomHalf => new WindowRect(target.X, target.Y + halfGap, target.Width, target.Height - halfGap),
            // 四角：相邻边留间隙
            WindowAction.TopLeft => new WindowRect(target.X, target.Y, target.Width - halfGap, target.Height - halfGap),
            WindowAction.TopRight => new WindowRect(target.X + halfGap, target.Y, target.Width - halfGap, target.Height - halfGap),
            WindowAction.BottomLeft => new WindowRect(target.X, target.Y + halfGap, target.Width - halfGap, target.Height - halfGap),
            WindowAction.BottomRight => new WindowRect(target.X + halfGap, target.Y + halfGap, target.Width - halfGap, target.Height - halfGap),
            // 三分之一
            WindowAction.FirstThird => new WindowRect(target.X, target.Y, target.Width - halfGap, target.Height),
            WindowAction.CenterThird => new WindowRect(target.X + halfGap, target.Y, target.Width - _gapSize, target.Height),
            WindowAction.LastThird => new WindowRect(target.X + halfGap, target.Y, target.Width - halfGap, target.Height),
            // 其他操作不应用间隙
            _ => target
        };
    }

    /// <summary>
    /// 应用最小窗口尺寸限制
    /// </summary>
    private WindowRect ApplyMinimumSize(WindowRect rect)
    {
        var config = _configService?.Load();
        int minWidth = (int)(config?.MinimumWindowWidth ?? 100);
        int minHeight = (int)(config?.MinimumWindowHeight ?? 100);

        return new WindowRect(
            rect.X,
            rect.Y,
            Math.Max(rect.Width, minWidth),
            Math.Max(rect.Height, minHeight)
        );
    }

    /// <summary>
    /// 将窗口矩形限制在工作区域内
    /// </summary>
    private WindowRect ClampToWorkArea(WindowRect rect, WorkArea workArea)
    {
        int x = rect.X;
        int y = rect.Y;
        int w = rect.Width;
        int h = rect.Height;

        // 确保不超出工作区边界
        if (x < workArea.Left) x = workArea.Left;
        if (y < workArea.Top) y = workArea.Top;
        if (x + w > workArea.Right) x = workArea.Right - w;
        if (y + h > workArea.Bottom) y = workArea.Bottom - h;

        // 再次确保尺寸有效
        if (w < 10) w = 10;
        if (h < 10) h = 10;

        return new WindowRect(x, y, w, h);
    }

    /// <summary>
    /// 根据配置获取目标工作区域
    /// </summary>
    private WorkArea GetTargetWorkArea(nint hwnd)
    {
        var config = _configService?.Load();
        bool useCursorScreen = config?.UseCursorScreenDetection ?? true;

        if (useCursorScreen)
        {
            return _win32.GetWorkAreaFromCursor();
        }
        else
        {
            return _win32.GetWorkAreaFromWindow(hwnd);
        }
    }
}
