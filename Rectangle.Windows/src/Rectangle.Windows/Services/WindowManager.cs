using Rectangle.Windows.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rectangle.Windows.Services;

public class WindowManager
{
    private readonly Win32WindowService _win32;
    private readonly CalculatorFactory _factory;
    private readonly WindowHistory _history;
    private readonly HashSet<nint> _maximizedWindows = new();
    private LastActiveWindowService? _lastActiveService;
    private ConfigService? _configService;
    private int _gapSize = 0;

    public WindowManager(Win32WindowService win32, CalculatorFactory factory, WindowHistory history)
    {
        _win32 = win32;
        _factory = factory;
        _history = history;
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
            System.Media.SystemSounds.Beep.Play();
            return;
        }

        var processName = _win32.GetProcessNameFromWindow(hwnd);
        
        // 检查是否在忽略列表中
        if (IsIgnoredApp(processName))
        {
            Console.WriteLine($"[WindowManager] {processName} 在忽略列表中，跳过操作");
            return;
        }

        var (x, y, w, h) = _win32.GetWindowRect(hwnd);
        var workArea = _win32.GetWorkAreaFromWindow(hwnd);
        
        // 应用窗口间隙
        workArea = ApplyGap(workArea);
        
        var current = new WindowRect(x, y, w, h);

        var calculator = _factory.GetCalculator(action);
        if (calculator == null) return;

        // 检测窗口是否被用户手动移动
        bool windowMovedExternally = _history.IsWindowMovedExternally(hwnd, x, y, w, h);
        
        if (windowMovedExternally)
        {
            // 用户手动移动了窗口，清除程序操作记录
            _history.RemoveLastAction(hwnd);
            Console.WriteLine($"[WindowManager] 检测到窗口被用户手动移动: {processName}");
        }

        // 保存或更新恢复点：
        // 1. 如果没有恢复点，保存当前位置
        // 2. 如果窗口被用户手动移动，更新恢复点
        if (!_history.HasRestoreRect(hwnd) || windowMovedExternally)
        {
            _history.SaveRestoreRect(hwnd, x, y, w, h);
            Console.WriteLine($"[WindowManager] 保存恢复点: ({x}, {y}, {w}, {h})");
        }

        // 标记此窗口由程序调整（用于窗口位置监听时排除记录）
        _history.MarkAsProgramAdjusted(hwnd);

        // 执行其他操作时，清除最大化状态
        _maximizedWindows.Remove(hwnd);

        var target = calculator.Calculate(workArea, current, action);
        
        // 为相邻窗口应用间隙
        target = ApplyWindowGap(target, workArea, action);
        
        _win32.SetWindowRect(hwnd, target.X, target.Y, target.Width, target.Height);

        // 记录程序操作信息（包括操作类型、次数、时间）
        _history.RecordAction(hwnd, action, target.X, target.Y, target.Width, target.Height);

        Console.WriteLine($"{GetActionDisplayName(action)} 了 {processName}");
    }

    private nint GetTargetWindow()
    {
        // 获取当前前台窗口
        var foregroundHwnd = _win32.GetForegroundWindowHandle();
        
        // 检查前台窗口是否在忽略列表中
        if (foregroundHwnd != 0)
        {
            var processName = _win32.GetProcessNameFromWindow(foregroundHwnd);
            if (!string.IsNullOrEmpty(processName) && IsIgnoredApp(processName))
            {
                Console.WriteLine($"[WindowManager] 前台窗口 {processName} 在忽略列表中，不执行操作");
                return 0; // 返回 0 表示不执行操作
            }
        }
        
        // 如果有 LastActiveWindowService，使用它获取目标窗口
        if (_lastActiveService != null)
        {
            return _lastActiveService.GetTargetWindow();
        }
        return foregroundHwnd;
    }

    private void ExecuteMaximizeToggle(nint? targetHwnd = null)
    {
        var hwnd = targetHwnd ?? GetTargetWindow();
        if (hwnd == 0)
        {
            System.Media.SystemSounds.Beep.Play();
            return;
        }

        var processName = _win32.GetProcessNameFromWindow(hwnd);
        
        // 检查是否在忽略列表中
        if (IsIgnoredApp(processName))
        {
            Console.WriteLine($"[WindowManager] {processName} 在忽略列表中，跳过操作");
            return;
        }

        // 如果当前窗口已最大化，则恢复
        if (_maximizedWindows.Contains(hwnd))
        {
            if (_history.TryGetRestoreRect(hwnd, out var rect))
            {
                _win32.SetWindowRect(hwnd, rect.X, rect.Y, rect.W, rect.H);
                _history.RemoveLastAction(hwnd);
            }
            _maximizedWindows.Remove(hwnd);
            Console.WriteLine($"恢复了 {processName}");
        }
        else
        {
            // 保存当前位置到恢复点
            var (x, y, w, h) = _win32.GetWindowRect(hwnd);
            
            // 检测窗口是否被用户手动移动
            bool windowMovedExternally = _history.IsWindowMovedExternally(hwnd, x, y, w, h);
            
            if (!_history.HasRestoreRect(hwnd) || windowMovedExternally)
            {
                _history.SaveRestoreRect(hwnd, x, y, w, h);
                Console.WriteLine($"[WindowManager] 最大化前保存恢复点: ({x}, {y}, {w}, {h})");
            }

            // 标记此窗口由程序调整
            _history.MarkAsProgramAdjusted(hwnd);

            // 最大化
            var workArea = _win32.GetWorkAreaFromWindow(hwnd);
            var calculator = _factory.GetCalculator(WindowAction.Maximize);
            if (calculator != null)
            {
                var target = calculator.Calculate(workArea, default, WindowAction.Maximize);
                _win32.SetWindowRect(hwnd, target.X, target.Y, target.Width, target.Height);
                
                // 记录程序操作信息
                _history.RecordAction(hwnd, WindowAction.Maximize, target.X, target.Y, target.Width, target.Height);
                
                _maximizedWindows.Add(hwnd);
                Console.WriteLine($"最大化了 {processName}");
            }
        }
    }

    private void ExecuteRestore(nint? targetHwnd = null)
    {
        var hwnd = targetHwnd ?? GetTargetWindow();
        if (hwnd == 0) 
        { 
            System.Media.SystemSounds.Beep.Play(); 
            return; 
        }
        
        if (!_history.TryGetRestoreRect(hwnd, out var rect))
        { 
            Console.WriteLine("[WindowManager] 没有可恢复的窗口位置");
            System.Media.SystemSounds.Beep.Play(); 
            return; 
        }

        var processName = _win32.GetProcessNameFromWindow(hwnd);
        _win32.SetWindowRect(hwnd, rect.X, rect.Y, rect.W, rect.H);
        
        // 清除程序最后操作记录（但保留恢复点，以便再次使用）
        _history.RemoveLastAction(hwnd);
        
        // 清除最大化状态
        _maximizedWindows.Remove(hwnd);
        
        Console.WriteLine($"恢复了 {processName} 到 ({rect.X}, {rect.Y}, {rect.W}, {rect.H})");
    }

    private void ExecuteNextDisplay(nint? targetHwnd = null)
    {
        var hwnd = targetHwnd ?? GetTargetWindow();
        if (hwnd == 0) { System.Media.SystemSounds.Beep.Play(); return; }

        var processName = _win32.GetProcessNameFromWindow(hwnd);
        var (x, y, w, h) = _win32.GetWindowRect(hwnd);
        var nextWorkArea = _win32.GetNextMonitorWorkArea(hwnd);
        
        if (nextWorkArea == null)
        {
            System.Media.SystemSounds.Beep.Play();
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
        var newX = nextWorkArea.Value.Left + (nextWorkArea.Value.Width - w) / 2;
        var newY = nextWorkArea.Value.Top + (nextWorkArea.Value.Height - h) / 2;
        _win32.SetWindowRect(hwnd, newX, newY, w, h);
        
        // 记录程序操作信息
        _history.RecordAction(hwnd, WindowAction.NextDisplay, newX, newY, w, h);
        
        Console.WriteLine($"将 {processName} 移动到下一个显示器");
    }

    private void ExecutePreviousDisplay(nint? targetHwnd = null)
    {
        var hwnd = targetHwnd ?? GetTargetWindow();
        if (hwnd == 0) { System.Media.SystemSounds.Beep.Play(); return; }

        var processName = _win32.GetProcessNameFromWindow(hwnd);
        var (x, y, w, h) = _win32.GetWindowRect(hwnd);
        var prevWorkArea = _win32.GetPreviousMonitorWorkArea(hwnd);
        
        if (prevWorkArea == null)
        {
            System.Media.SystemSounds.Beep.Play();
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
        var newX = prevWorkArea.Value.Left + (prevWorkArea.Value.Width - w) / 2;
        var newY = prevWorkArea.Value.Top + (prevWorkArea.Value.Height - h) / 2;
        _win32.SetWindowRect(hwnd, newX, newY, w, h);
        
        // 记录程序操作信息
        _history.RecordAction(hwnd, WindowAction.PreviousDisplay, newX, newY, w, h);
        
        Console.WriteLine($"将 {processName} 移动到上一个显示器");
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
}
