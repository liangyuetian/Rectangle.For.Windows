using System;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Rectangle.Windows.WinUI.Services
{
    /// <summary>
    /// Todo 模式管理器
    /// </summary>
    public class TodoManager
    {
        private readonly Win32WindowService _win32;
        private readonly ConfigService _configService;
        private nint _todoWindowHwnd;
        private bool _isEnabled;

        public bool IsEnabled => _isEnabled;
        public nint TodoWindowHwnd => _todoWindowHwnd;

        public TodoManager(Win32WindowService win32, ConfigService configService)
        {
            _win32 = win32;
            _configService = configService;
        }

        /// <summary>
        /// 查找 Todo 应用窗口
        /// </summary>
        public bool FindTodoWindow()
        {
            var config = _configService.Load();
            if (!config.TodoMode || string.IsNullOrEmpty(config.TodoApplication))
            {
                _isEnabled = false;
                return false;
            }

            _todoWindowHwnd = FindWindowByProcessName(config.TodoApplication);
            _isEnabled = _todoWindowHwnd != 0;

            if (_isEnabled)
            {
                Logger.Info("TodoManager", $"找到 Todo 窗口: {_todoWindowHwnd}");
            }

            return _isEnabled;
        }

        /// <summary>
        /// 根据进程名查找窗口
        /// </summary>
        private nint FindWindowByProcessName(string processName)
        {
            nint foundHwnd = 0;

            PInvoke.EnumWindows((hwnd, lParam) =>
            {
                if (!PInvoke.IsWindowVisible(hwnd))
                    return true;

                unsafe
                {
                    uint pid;
                    PInvoke.GetWindowThreadProcessId(hwnd, &pid);

                    try
                    {
                        using var process = System.Diagnostics.Process.GetProcessById((int)pid);
                        if (process.ProcessName.Equals(processName, StringComparison.OrdinalIgnoreCase))
                        {
                            foundHwnd = hwnd.Value;
                            return false; // 找到后停止枚举
                        }
                    }
                    catch { }
                }

                return true;
            }, default);

            return foundHwnd;
        }

        /// <summary>
        /// 调整 Todo 窗口位置
        /// </summary>
        public void AdjustTodoWindow()
        {
            if (!_isEnabled || _todoWindowHwnd == 0)
                return;

            var config = _configService.Load();
            var workArea = GetTodoWorkArea(config);

            _win32.SetWindowRect(_todoWindowHwnd, workArea.X, workArea.Y, workArea.Width, workArea.Height);
            Logger.Debug("TodoManager", $"调整 Todo 窗口位置: {workArea}");
        }

        /// <summary>
        /// 获取 Todo 窗口的工作区域
        /// </summary>
        private (int X, int Y, int Width, int Height) GetTodoWorkArea(AppConfig config)
        {
            var screenService = new ScreenDetectionService(_win32);
            var workArea = screenService.GetPrimaryWorkArea();

            int width = config.TodoSidebarWidth;
            int height = workArea.Height;

            if (config.TodoSidebarSide == 0) // Left
            {
                return (workArea.X, workArea.Y, width, height);
            }
            else // Right
            {
                return (workArea.X + workArea.Width - width, workArea.Y, width, height);
            }
        }

        /// <summary>
        /// 获取剩余可用区域（排除 Todo 窗口）
        /// </summary>
        public Core.WindowRect GetRemainingWorkArea()
        {
            if (!_isEnabled)
            {
                var screenService = new ScreenDetectionService(_win32);
                return screenService.GetPrimaryWorkArea();
            }

            var config = _configService.Load();
            var workArea = GetTodoWorkArea(config);

            var screenService2 = new ScreenDetectionService(_win32);
            var fullWorkArea = screenService2.GetPrimaryWorkArea();

            if (config.TodoSidebarSide == 0) // Left
            {
                return new Core.WindowRect(
                    workArea.X + workArea.Width,
                    fullWorkArea.Y,
                    fullWorkArea.Width - workArea.Width,
                    fullWorkArea.Height);
            }
            else // Right
            {
                return new Core.WindowRect(
                    fullWorkArea.X,
                    fullWorkArea.Y,
                    fullWorkArea.Width - workArea.Width,
                    fullWorkArea.Height);
            }
        }
    }
}
