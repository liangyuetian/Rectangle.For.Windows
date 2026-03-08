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

    private static string GetActionDisplayName(WindowAction action)
    {
        return action switch
        {
            WindowAction.LeftHalf => "左半屏",
            WindowAction.RightHalf => "右半屏",
            WindowAction.TopHalf => "上半屏",
            WindowAction.BottomHalf => "下半屏",
            WindowAction.TopLeft => "左上角",
            WindowAction.TopRight => "右上角",
            WindowAction.BottomLeft => "左下角",
            WindowAction.BottomRight => "右下角",
            WindowAction.FirstThird => "左首 1/3",
            WindowAction.CenterThird => "中间 1/3",
            WindowAction.LastThird => "右首 1/3",
            WindowAction.FirstTwoThirds => "左侧 2/3",
            WindowAction.CenterTwoThirds => "中间 2/3",
            WindowAction.LastTwoThirds => "右侧 2/3",
            WindowAction.Maximize => "最大化",
            WindowAction.Restore => "恢复",
            WindowAction.Center => "居中",
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
        var (x, y, w, h) = _win32.GetWindowRect(hwnd);
        var workArea = _win32.GetWorkAreaFromWindow(hwnd);
        var current = new WindowRect(x, y, w, h);

        var calculator = _factory.GetCalculator(action);
        if (calculator == null) return;

        // 只在第一次调整时保存原始位置，Restore 会恢复到这个位置
        _history.SaveIfNotExists(hwnd, x, y, w, h);

        // 标记此窗口由程序调整（用于窗口位置监听时排除记录）
        _history.MarkAsProgramAdjusted(hwnd);

        // 执行其他操作时，清除最大化状态
        _maximizedWindows.Remove(hwnd);

        var target = calculator.Calculate(workArea, current, action);
        _win32.SetWindowRect(hwnd, target.X, target.Y, target.Width, target.Height);

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

    private bool IsIgnoredApp(string processName)
    {
        // 从 ConfigService 获取忽略列表
        // 这里需要通过 Program.ConfigService 访问
        var config = Program.ConfigService?.Load();
        if (config == null) return false;
        
        return config.IgnoredApps.Any(a => a.Equals(processName, StringComparison.OrdinalIgnoreCase));
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

        // 如果当前窗口已最大化，则恢复
        if (_maximizedWindows.Contains(hwnd))
        {
            if (_history.TryGet(hwnd, out var rect))
            {
                _win32.SetWindowRect(hwnd, rect.X, rect.Y, rect.W, rect.H);
                _history.Remove(hwnd);
            }
            _maximizedWindows.Remove(hwnd);
            Console.WriteLine($"恢复了 {processName}");
        }
        else
        {
            // 保存当前位置
            var (x, y, w, h) = _win32.GetWindowRect(hwnd);
            _history.Save(hwnd, x, y, w, h);

            // 标记此窗口由程序调整
            _history.MarkAsProgramAdjusted(hwnd);

            // 最大化
            var workArea = _win32.GetWorkAreaFromWindow(hwnd);
            var calculator = _factory.GetCalculator(WindowAction.Maximize);
            if (calculator != null)
            {
                var target = calculator.Calculate(workArea, default, WindowAction.Maximize);
                _win32.SetWindowRect(hwnd, target.X, target.Y, target.Width, target.Height);
                _maximizedWindows.Add(hwnd);
                Console.WriteLine($"最大化了 {processName}");
            }
        }
    }

    private void ExecuteRestore(nint? targetHwnd = null)
    {
        var hwnd = targetHwnd ?? GetTargetWindow();
        if (hwnd == 0) { System.Media.SystemSounds.Beep.Play(); return; }
        if (!_history.TryGet(hwnd, out var rect))
        { System.Media.SystemSounds.Beep.Play(); return; }
        
        var processName = _win32.GetProcessNameFromWindow(hwnd);
        _win32.SetWindowRect(hwnd, rect.X, rect.Y, rect.W, rect.H);
        _history.Remove(hwnd);
        Console.WriteLine($"恢复了 {processName}");
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

        _history.SaveIfNotExists(hwnd, x, y, w, h);

        // 标记此窗口由程序调整
        _history.MarkAsProgramAdjusted(hwnd);

        // 将窗口移动到下一个显示器居中
        var newX = nextWorkArea.Value.Left + (nextWorkArea.Value.Width - w) / 2;
        var newY = nextWorkArea.Value.Top + (nextWorkArea.Value.Height - h) / 2;
        _win32.SetWindowRect(hwnd, newX, newY, w, h);
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

        _history.SaveIfNotExists(hwnd, x, y, w, h);

        // 标记此窗口由程序调整
        _history.MarkAsProgramAdjusted(hwnd);

        // 将窗口移动到上一个显示器居中
        var newX = prevWorkArea.Value.Left + (prevWorkArea.Value.Width - w) / 2;
        var newY = prevWorkArea.Value.Top + (prevWorkArea.Value.Height - h) / 2;
        _win32.SetWindowRect(hwnd, newX, newY, w, h);
        Console.WriteLine($"将 {processName} 移动到上一个显示器");
    }
}
