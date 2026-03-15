using System;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Accessibility;

namespace Rectangle.Windows.WinUI.Services;

/// <summary>
/// 跟踪最近活跃的有效窗口句柄。
/// 只有真正的应用程序窗口才会被记录，排除状态栏、任务栏、托盘菜单等系统窗口。
/// </summary>
public class LastActiveWindowService : IDisposable
{
    private nint _lastValidWindowHwnd;
    private bool _disposed;
    private readonly object _lock = new();
    private bool _isPaused = false;

    // Win32 事件钩子 - 使用 SafeHandle
    private UnhookWinEventSafeHandle? _hook;

    // 委托必须保持引用防止被 GC 回收
    private WINEVENTPROC? _winEventProc;

    public LastActiveWindowService()
    {
        _lastValidWindowHwnd = 0;
        StartTracking();
    }

    /// <summary>
    /// 暂停窗口跟踪（在显示托盘菜单时使用）
    /// </summary>
    public void PauseTracking()
    {
        lock (_lock)
        {
            _isPaused = true;
            Logger.Info("LastActiveWindowService", "暂停窗口跟踪");
        }
    }

    /// <summary>
    /// 恢复窗口跟踪
    /// </summary>
    public unsafe void ResumeTracking()
    {
        lock (_lock)
        {
            _isPaused = false;
            Logger.Info("LastActiveWindowService", "恢复窗口跟踪");

            // 恢复跟踪时，立即检查当前前台窗口
            var currentForeground = (nint)PInvoke.GetForegroundWindow().Value;
            if (IsValidWindow(currentForeground) && currentForeground != _lastValidWindowHwnd)
            {
                _lastValidWindowHwnd = currentForeground;
                var processName = GetProcessName(currentForeground);
                Logger.Debug("LastActiveWindowService", $"恢复跟踪后更新窗口: {_lastValidWindowHwnd} ({processName})");
            }
        }
    }

    private unsafe void StartTracking()
    {
        // 获取当前前台窗口
        var current = (nint)PInvoke.GetForegroundWindow().Value;
        if (IsValidWindow(current))
        {
            _lastValidWindowHwnd = current;
            Logger.Debug("LastActiveWindowService", $"初始有效窗口: {_lastValidWindowHwnd}");
        }
        else
        {
            Logger.Warning("LastActiveWindowService", $"初始前台窗口无效: {current}");
        }

        // 设置事件钩子，监听前台窗口变化
        // EVENT_SYSTEM_FOREGROUND = 0x0003, WINEVENT_OUTOFCONTEXT = 0x0000
        _winEventProc = OnForegroundWindowChanged;

        _hook = PInvoke.SetWinEventHook(
            0x0003, // EVENT_SYSTEM_FOREGROUND
            0x0003, // EVENT_SYSTEM_FOREGROUND
            null,
            _winEventProc,
            0, // 所有进程
            0, // 所有线程
            0x0000 // WINEVENT_OUTOFCONTEXT
        );

        if (_hook == null || _hook.IsInvalid)
        {
            Logger.Error("LastActiveWindowService", "无法设置窗口事件钩子");
        }
        else
        {
            Logger.Info("LastActiveWindowService", "窗口事件钩子设置成功");
        }
    }

    private unsafe void OnForegroundWindowChanged(HWINEVENTHOOK hWinEventHook, uint eventType, HWND hwnd, int idObject, int idChild, uint idEventThread, uint dwmsEventTime)
    {
        if (_isPaused)
        {
            Logger.Debug("LastActiveWindowService", $"跟踪已暂停，忽略窗口变化: {(nint)hwnd.Value}");
            return;
        }

        // 只关心窗口本身（idObject == 0, idChild == 0）
        if (idObject != 0 || idChild != 0) return;

        var newHwnd = (nint)hwnd.Value;

        // 只有有效窗口才更新记录
        if (IsValidWindow(newHwnd))
        {
            lock (_lock)
            {
                _lastValidWindowHwnd = newHwnd;
                var processName = GetProcessName(newHwnd);
                Logger.Debug("LastActiveWindowService", $"更新有效窗口: {_lastValidWindowHwnd} ({processName})");
            }
        }
        else
        {
            // 无效窗口（如托盘、任务栏），不更新记录
            Logger.Debug("LastActiveWindowService", $"忽略无效窗口: {newHwnd}");
        }
    }

    /// <summary>
    /// 判断窗口是否是有效的应用程序窗口
    /// </summary>
    private unsafe bool IsValidWindow(nint hwnd)
    {
        if (hwnd == 0) return false;

        try
        {
            var hWnd = new HWND(hwnd);

            // 检查窗口句柄是否仍然有效
            if (!PInvoke.IsWindow(hWnd))
            {
                Logger.Warning("LastActiveWindowService", $"窗口句柄无效: {hwnd}");
                return false;
            }

            // 检查窗口是否可见
            if (!PInvoke.IsWindowVisible(hWnd)) return false;

            // 获取窗口标题
            int titleLength = PInvoke.GetWindowTextLength(hWnd);

            // 获取窗口样式 - 使用 GetWindowLong
            var style = (uint)GetWindowLong(hWnd, -16); // GWL_STYLE = -16
            var exStyle = (uint)GetWindowLong(hWnd, -20); // GWL_EXSTYLE = -20

            // 检查是否是工具窗口或弹出式窗口（通常是系统窗口）
            // WS_EX_TOOLWINDOW = 0x00000080
            if ((exStyle & 0x00000080) != 0)
            {
                // 检查是否是真正的应用程序窗口
                // 如果窗口有标题且不是工具窗口，可能是有效的
                if (titleLength == 0) return false;
            }

            // 排除纯弹出窗口（没有边框的）
            // WS_POPUP = 0x80000000
            bool isPopup = (style & 0x80000000) != 0;

            // 获取窗口类名来进一步判断
            string className = GetWindowClassName(hWnd);

            // 排除已知的系统窗口类
            if (IsSystemWindowClass(className)) return false;

            // 获取进程名
            string processName = GetProcessName(hwnd);

            // 排除已知的系统进程
            if (IsSystemProcess(processName)) return false;

            // 检查窗口是否有适当的大小（不是太小）
            if (PInvoke.GetWindowRect(hWnd, out var rect))
            {
                int width = rect.right - rect.left;
                int height = rect.bottom - rect.top;
                if (width < 50 || height < 50) return false; // 太小的窗口
            }

            return true;
        }
        catch (Exception ex)
        {
            Logger.Error("LastActiveWindowService", $"IsValidWindow 异常: {ex.Message}");
            return false;
        }
    }

    // 使用 P/Invoke 直接调用 GetWindowLong，因为 CsWin32 对 32/64 位处理不同
    [DllImport("user32.dll", EntryPoint = "GetWindowLongW")]
    private static extern nint GetWindowLong32(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW")]
    private static extern nint GetWindowLong64(IntPtr hWnd, int nIndex);

    private static unsafe nint GetWindowLong(HWND hWnd, int nIndex)
    {
        if (IntPtr.Size == 8)
            return GetWindowLong64((nint)hWnd.Value, nIndex);
        else
            return GetWindowLong32((nint)hWnd.Value, nIndex);
    }

    private unsafe string GetWindowClassName(HWND hWnd)
    {
        try
        {
            unsafe
            {
                char* buffer = stackalloc char[256];
                int length = PInvoke.GetClassName(hWnd, buffer, 256);
                if (length > 0)
                {
                    return new string(buffer, 0, length);
                }
            }
        }
        catch { }
        return string.Empty;
    }

    private bool IsSystemWindowClass(string className)
    {
        if (string.IsNullOrEmpty(className)) return false;

        // 已知的系统窗口类（注意：CabinetWClass 是资源管理器文件夹窗口，不应排除）
        string[] systemClasses = {
            "Shell_TrayWnd",           // 任务栏
            "Shell_SecondaryTrayWnd",  // 第二显示器任务栏
            "NotifyIconOverflowWindow",// 托盘溢出窗口
            "ToolbarWindow32",         // 工具栏
            "SysPager",                // 分页器
            "TrayNotifyWnd",           // 托盘通知区域
            "Button",                  // 按钮（开始按钮等）
            "Progman",                 // 程序管理器（桌面）
            "WorkerW",                 // 桌面壁纸窗口
            "IME",                     // 输入法
            "MSCTFIME UI",            // 输入法 UI
            "Default IME",            // 默认输入法
            "tooltips_class32",        // 工具提示
            "#32768",                  // 菜单
            "#32769",                  // 桌面
            // "#32770" - 对话框，不排除，有些是有效的应用窗口
            // 注意：CabinetWClass 和 ExplorerWClass 是资源管理器文件夹窗口，不应排除
        };

        foreach (var sysClass in systemClasses)
        {
            if (className.Equals(sysClass, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private unsafe string GetProcessName(nint hwnd)
    {
        try
        {
            unsafe
            {
                uint processId;
                PInvoke.GetWindowThreadProcessId(new HWND(hwnd), &processId);

                if (processId == 0) return string.Empty;

                using var process = System.Diagnostics.Process.GetProcessById((int)processId);
                return process.ProcessName;
            }
        }
        catch
        {
            return string.Empty;
        }
    }

    private bool IsSystemProcess(string processName)
    {
        if (string.IsNullOrEmpty(processName)) return false;

        // 已知的系统进程（这些进程的窗口不应被记录）
        // 注意：explorer 已从排除列表移除，因为资源管理器文件夹窗口是有效的应用窗口
        // 通过 IsSystemWindowClass 排除任务栏和桌面等系统窗口
        string[] systemProcesses = {
            "ShellExperienceHost",
            "SearchApp",
            "SearchHost",
            "StartMenuExperienceHost",
            "RuntimeBroker",
            "ApplicationFrameHost",
            "SystemSettings",
            "WindowsInternal.Shell", // Shell 组件
        };

        foreach (var sysProc in systemProcesses)
        {
            if (processName.Equals(sysProc, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 获取最近活跃的有效窗口句柄
    /// </summary>
    public nint GetLastValidWindow()
    {
        lock (_lock)
        {
            return _lastValidWindowHwnd;
        }
    }

    /// <summary>
    /// 获取目标窗口（用于窗口操作）
    /// </summary>
    public unsafe nint GetTargetWindow()
    {
        lock (_lock)
        {
            // 先检查缓存的窗口是否仍然有效
            if (_lastValidWindowHwnd != 0 && IsValidWindow(_lastValidWindowHwnd))
            {
                var processName = GetProcessName(_lastValidWindowHwnd);
                Logger.Debug("LastActiveWindowService", $"获取目标窗口: {_lastValidWindowHwnd} ({processName})");
                return _lastValidWindowHwnd;
            }

            // 如果缓存的窗口无效，尝试获取当前前台窗口
            var foregroundWindow = (nint)PInvoke.GetForegroundWindow().Value;
            if (IsValidWindow(foregroundWindow))
            {
                // 更新缓存
                _lastValidWindowHwnd = foregroundWindow;
                var processName = GetProcessName(foregroundWindow);
                Logger.Debug("LastActiveWindowService", $"缓存窗口无效，使用前台窗口: {foregroundWindow} ({processName})");
                return foregroundWindow;
            }

            // 如果前台窗口也无效，返回缓存的窗口（尽管可能无效）
            Logger.Warning("LastActiveWindowService", $"无有效窗口可用，返回缓存窗口: {_lastValidWindowHwnd}");
            return _lastValidWindowHwnd;
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        _hook?.Dispose();
        _disposed = true;
    }
}
