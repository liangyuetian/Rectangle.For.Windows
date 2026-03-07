using Rectangle.Windows.WinUI.Core;

namespace Rectangle.Windows.WinUI.Services;

public class WindowManager
{
    private readonly Win32WindowService _win32;
    private readonly CalculatorFactory _factory;
    private readonly WindowHistory _history;

    public WindowManager(Win32WindowService win32, CalculatorFactory factory, WindowHistory history)
    {
        _win32 = win32;
        _factory = factory;
        _history = history;
    }

    public void Execute(WindowAction action)
    {
        if (action == WindowAction.Restore)
        {
            ExecuteRestore();
            return;
        }

        var hwnd = _win32.GetForegroundWindowHandle();
        if (hwnd == 0)
        {
            Console.WriteLine("[WindowManager] 没有前台窗口");
            return;
        }

        var (x, y, w, h) = _win32.GetWindowRect(hwnd);
        var workArea = _win32.GetWorkAreaFromWindow(hwnd);
        var current = new WindowRect(x, y, w, h);

        var calculator = _factory.GetCalculator(action);
        if (calculator == null) return;

        _history.Save(hwnd, x, y, w, h);

        var target = calculator.Calculate(workArea, current, action);
        _win32.SetWindowRect(hwnd, target.X, target.Y, target.Width, target.Height);
    }

    private void ExecuteRestore()
    {
        var hwnd = _win32.GetForegroundWindowHandle();
        if (hwnd == 0) { Console.WriteLine("[WindowManager] 没有前台窗口"); return; }
        if (!_history.TryGet(hwnd, out var rect))
        { Console.WriteLine("[WindowManager] 没有历史记录"); return; }
        _win32.SetWindowRect(hwnd, rect.X, rect.Y, rect.W, rect.H);
        _history.Remove(hwnd);
    }
}
