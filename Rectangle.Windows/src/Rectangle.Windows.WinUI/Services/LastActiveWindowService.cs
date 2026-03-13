using System;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Accessibility;

namespace Rectangle.Windows.WinUI.Services
{
    /// <summary>
    /// 跟踪最近活跃的有效窗口句柄
    /// </summary>
    public class LastActiveWindowService : IDisposable
    {
        private nint _lastValidWindowHwnd;
        private bool _disposed;
        private readonly object _lock = new();
        private bool _isPaused = false;
        private UnhookWinEventSafeHandle? _hook;
        private WINEVENTPROC? _winEventProc;

        public LastActiveWindowService()
        {
            _lastValidWindowHwnd = 0;
            StartTracking();
        }

        public void PauseTracking()
        {
            lock (_lock)
            {
                _isPaused = true;
                Logger.Info("LastActiveWindowService", "暂停窗口跟踪");
            }
        }

        public void ResumeTracking()
        {
            lock (_lock)
            {
                _isPaused = false;
                Logger.Info("LastActiveWindowService", "恢复窗口跟踪");

                var currentForeground = PInvoke.GetForegroundWindow().Value;
                if (IsValidWindow(currentForeground) && currentForeground != _lastValidWindowHwnd)
                {
                    _lastValidWindowHwnd = currentForeground;
                    var processName = GetProcessName(currentForeground);
                    Logger.Debug("LastActiveWindowService", $"恢复跟踪后更新窗口: {_lastValidWindowHwnd} ({processName})");
                }
            }
        }

        private void StartTracking()
        {
            var current = PInvoke.GetForegroundWindow().Value;
            if (IsValidWindow(current))
            {
                _lastValidWindowHwnd = current;
                Logger.Debug("LastActiveWindowService", $"初始有效窗口: {_lastValidWindowHwnd}");
            }

            _winEventProc = OnForegroundWindowChanged;

            _hook = PInvoke.SetWinEventHook(
                0x0003, // EVENT_SYSTEM_FOREGROUND
                0x0003, // EVENT_SYSTEM_FOREGROUND
                null,
                _winEventProc,
                0,
                0,
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

        private void OnForegroundWindowChanged(HWINEVENTHOOK hWinEventHook, uint eventType, HWND hwnd, int idObject, int idChild, uint idEventThread, uint dwmsEventTime)
        {
            if (_isPaused)
            {
                Logger.Debug("LastActiveWindowService", $"跟踪已暂停，忽略窗口变化: {hwnd.Value}");
                return;
            }

            if (idObject != 0 || idChild != 0) return;

            var newHwnd = hwnd.Value;

            if (IsValidWindow(newHwnd))
            {
                lock (_lock)
                {
                    _lastValidWindowHwnd = newHwnd;
                    var processName = GetProcessName(newHwnd);
                    Logger.Debug("LastActiveWindowService", $"更新有效窗口: {_lastValidWindowHwnd} ({processName})");
                }
            }
        }

        private bool IsValidWindow(nint hwnd)
        {
            if (hwnd == 0) return false;

            try
            {
                var hWnd = new HWND(hwnd);

                if (!PInvoke.IsWindow(hWnd)) return false;
                if (!PInvoke.IsWindowVisible(hWnd)) return false;

                int titleLength = PInvoke.GetWindowTextLength(hWnd);
                var style = (uint)GetWindowLong(hWnd, -16);
                var exStyle = (uint)GetWindowLong(hWnd, -20);

                if ((exStyle & 0x00000080) != 0 && titleLength == 0) return false;

                string className = GetWindowClassName(hWnd);
                if (IsSystemWindowClass(className)) return false;

                string processName = GetProcessName(hwnd);
                if (IsSystemProcess(processName)) return false;

                if (PInvoke.GetWindowRect(hWnd, out var rect))
                {
                    int width = rect.right - rect.left;
                    int height = rect.bottom - rect.top;
                    if (width < 50 || height < 50) return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("LastActiveWindowService", $"IsValidWindow 异常: {ex.Message}");
                return false;
            }
        }

        [DllImport("user32.dll", EntryPoint = "GetWindowLongW")]
        private static extern nint GetWindowLong32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW")]
        private static extern nint GetWindowLong64(IntPtr hWnd, int nIndex);

        private static nint GetWindowLong(HWND hWnd, int nIndex)
        {
            if (IntPtr.Size == 8)
                return GetWindowLong64(hWnd.Value, nIndex);
            else
                return GetWindowLong32(hWnd.Value, nIndex);
        }

        private string GetWindowClassName(HWND hWnd)
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

            string[] systemClasses = {
                "Shell_TrayWnd",
                "Shell_SecondaryTrayWnd",
                "NotifyIconOverflowWindow",
                "ToolbarWindow32",
                "SysPager",
                "TrayNotifyWnd",
                "Button",
                "Progman",
                "WorkerW",
                "IME",
                "MSCTFIME UI",
                "Default IME",
                "tooltips_class32",
                "#32768",
                "#32769",
            };

            foreach (var sysClass in systemClasses)
            {
                if (className.Equals(sysClass, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private string GetProcessName(nint hwnd)
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

            string[] systemProcesses = {
                "ShellExperienceHost",
                "SearchApp",
                "SearchHost",
                "StartMenuExperienceHost",
                "RuntimeBroker",
                "ApplicationFrameHost",
                "SystemSettings",
                "WindowsInternal.Shell",
            };

            foreach (var sysProc in systemProcesses)
            {
                if (processName.Equals(sysProc, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        public nint GetLastValidWindow()
        {
            lock (_lock)
            {
                return _lastValidWindowHwnd;
            }
        }

        public nint GetTargetWindow()
        {
            lock (_lock)
            {
                if (_lastValidWindowHwnd != 0 && IsValidWindow(_lastValidWindowHwnd))
                {
                    var processName = GetProcessName(_lastValidWindowHwnd);
                    Logger.Debug("LastActiveWindowService", $"获取目标窗口: {_lastValidWindowHwnd} ({processName})");
                    return _lastValidWindowHwnd;
                }

                var foregroundWindow = PInvoke.GetForegroundWindow().Value;
                if (IsValidWindow(foregroundWindow))
                {
                    _lastValidWindowHwnd = foregroundWindow;
                    var processName = GetProcessName(foregroundWindow);
                    Logger.Debug("LastActiveWindowService", $"缓存窗口无效，使用前台窗口: {foregroundWindow} ({processName})");
                    return foregroundWindow;
                }

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
}
