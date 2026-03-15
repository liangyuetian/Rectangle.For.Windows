using Rectangle.Windows.WinUI.Core;

namespace Rectangle.Windows.WinUI.Services;

public class WindowManager
{
    private readonly Win32WindowService _win32;
    private readonly CalculatorFactory _factory;
    private readonly WindowHistory _history;
    private LastActiveWindowService? _lastActiveService;
    private ConfigService? _configService;

    public WindowManager(Win32WindowService win32, CalculatorFactory factory, WindowHistory history)
    {
        _win32 = win32;
        _factory = factory;
        _history = history;
    }

    public void SetLastActiveWindowService(LastActiveWindowService service)
        => _lastActiveService = service;

    public void SetConfigService(ConfigService configService)
        => _configService = configService;

    // ── 目标窗口 ──────────────────────────────────────────────────

    private nint GetTargetWindow()
    {
        if (_lastActiveService != null)
            return _lastActiveService.GetTargetWindow();
        return _win32.GetForegroundWindowHandle();
    }

    private bool IsIgnoredApp(string processName)
    {
        if (_configService == null) return false;
        var config = _configService.Load();
        return config.IgnoredApps.Exists(app =>
            app.Equals(processName, StringComparison.OrdinalIgnoreCase) ||
            app.Equals(processName + ".exe", StringComparison.OrdinalIgnoreCase));
    }

    // ── 主入口 ────────────────────────────────────────────────────

    public void Execute(WindowAction action)
    {
        switch (action)
        {
            case WindowAction.Restore:        ExecuteRestore();        return;
            case WindowAction.NextDisplay:    ExecuteNextDisplay();    return;
            case WindowAction.PreviousDisplay:ExecutePreviousDisplay();return;
        }

        var hwnd = GetTargetWindow();
        if (hwnd == 0) return;

        var processName = _win32.GetProcessNameFromWindow(hwnd);
        if (IsIgnoredApp(processName)) return;

        var (x, y, w, h) = _win32.GetWindowRect(hwnd);
        var workArea = _win32.GetWorkAreaFromWindow(hwnd);
        var current = new WindowRect(x, y, w, h);

        var calculator = _factory.GetCalculator(action);
        if (calculator == null) return;

        // 首次操作时保存恢复点（保留最初位置）
        bool movedExternally = _history.IsWindowMovedExternally(hwnd, x, y, w, h);
        if (!_history.HasRestoreRect(hwnd) || movedExternally)
            _history.SaveRestoreRect(hwnd, x, y, w, h);

        _history.MarkAsProgramAdjusted(hwnd);

        var target = calculator.Calculate(workArea, current, action);
        _win32.SetWindowRect(hwnd, target.X, target.Y, target.Width, target.Height);
        _history.RecordAction(hwnd, action, target.X, target.Y, target.Width, target.Height);

        Logger.Info("WindowManager", $"{action} → {processName}");
    }

    // ── Restore ───────────────────────────────────────────────────

    private void ExecuteRestore()
    {
        var hwnd = GetTargetWindow();
        if (hwnd == 0) return;

        if (!_history.TryGetRestoreRect(hwnd, out var rect))
        {
            Logger.Warning("WindowManager", "没有可恢复的窗口位置");
            return;
        }

        _win32.SetWindowRect(hwnd, rect.X, rect.Y, rect.W, rect.H);
        _history.RemoveLastAction(hwnd);
        // 恢复后清除恢复点，下次操作重新记录
        _history.RemoveWindow(hwnd);
        Logger.Info("WindowManager", $"恢复窗口到 ({rect.X},{rect.Y},{rect.W},{rect.H})");
    }

    // ── 显示器移动 ────────────────────────────────────────────────

    private void ExecuteNextDisplay() => MoveToAdjacentDisplay(hwnd => 1);
    private void ExecutePreviousDisplay() => MoveToAdjacentDisplay(hwnd => -1);

    private void MoveToAdjacentDisplay(Func<nint, int> directionFn)
    {
        var hwnd = GetTargetWindow();
        if (hwnd == 0) return;

        var monitors = _win32.GetMonitorWorkAreas();
        if (monitors.Count < 2) return;

        // 找到窗口中心点所在的显示器索引
        var (wx, wy, ww, wh) = _win32.GetWindowRect(hwnd);
        var cx = wx + ww / 2;
        var cy = wy + wh / 2;
        var idx = monitors.FindIndex(m => cx >= m.Left && cx < m.Right && cy >= m.Top && cy < m.Bottom);
        if (idx < 0) idx = 0;

        var dir = directionFn(hwnd);
        var target = monitors[(idx + dir + monitors.Count) % monitors.Count];

        if (!_history.HasRestoreRect(hwnd)) _history.SaveRestoreRect(hwnd, wx, wy, ww, wh);
        _history.MarkAsProgramAdjusted(hwnd);

        var newX = target.Left + (target.Width - ww) / 2;
        var newY = target.Top + (target.Height - wh) / 2;
        _win32.SetWindowRect(hwnd, newX, newY, ww, wh);
        var action = dir > 0 ? WindowAction.NextDisplay : WindowAction.PreviousDisplay;
        _history.RecordAction(hwnd, action, newX, newY, ww, wh);
        Logger.Info("WindowManager", $"{action} → 显示器 {(idx + dir + monitors.Count) % monitors.Count}");
    }
}
