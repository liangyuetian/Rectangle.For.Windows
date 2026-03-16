using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Navigation;
using Rectangle.Windows.WinUI.Core;
using Rectangle.Windows.WinUI.Services;
using Rectangle.Windows.WinUI.Views;
using H.NotifyIcon;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Rectangle.Windows.WinUI
{
    public partial class App : Application
    {
        public static Window? MainWindow => _instance?._settingsWindow;
        public static WindowManager? WindowManager { get; private set; }
        public static HotkeyManager? HotkeyManager { get; private set; }
        public static ConfigService? ConfigService { get; private set; }

        private static App? _instance;
        private TrayIconService? _trayIconService;
        private LastActiveWindowService? _lastActiveService;
        private Window? _settingsWindow;
        private Window? _hiddenWindow; // 隐藏窗口，保持应用运行
        private nint _hotkeyHwnd;

        // Win32 API
        [DllImport("user32.dll")] static extern bool ShowWindow(nint hWnd, int nCmdShow);
        [DllImport("user32.dll")] static extern bool SetForegroundWindow(nint hWnd);

        // 附加到父进程控制台（用于 dotnet run 时支持 Ctrl+C）
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool AttachConsole(int dwProcessId);
        const int ATTACH_PARENT_PROCESS = -1;

        [DllImport("kernel32.dll")]
        static extern bool FreeConsole();

        public App()
        {
            Environment.SetEnvironmentVariable(
                "MICROSOFT_WINDOWSAPPRUNTIME_BASE_DIRECTORY",
                AppContext.BaseDirectory);
            _instance = this;

            // 尝试附加到控制台以支持 Ctrl+C
            TryAttachConsole();

            this.InitializeComponent();
        }

        private void TryAttachConsole()
        {
            try
            {
                // 附加到父进程的控制台（dotnet run 时）
                if (AttachConsole(ATTACH_PARENT_PROCESS))
                {
                    // 注册 Ctrl+C 处理器
                    Console.CancelKeyPress += (s, e) =>
                    {
                        e.Cancel = false;
                        Logger.Info("App", "收到 Ctrl+C，正在退出...");
                        ExitApplication();
                    };
                    Logger.Debug("App", "已附加到控制台，Ctrl+C 处理已注册");
                }
            }
            catch { /* 忽略错误 */ }
        }

        private static void ExitApplication()
        {
            try
            {
                if (_instance?._trayIconService != null)
                {
                    _instance._trayIconService.Dispose();
                }
            }
            catch { }
            Environment.Exit(0);
        }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            ConfigService = new ConfigService();
            ConfigService.ConfigChanged += (_, _) => HotkeyManager?.ReloadFromConfig();
            var configService = ConfigService;
            Logger.InitializeFromConfig(configService);
            Logger.Info("App", "应用启动");

            ThemeService.Instance.LoadThemeFromConfig();

            var win32 = new Win32WindowService();
            var factory = new CalculatorFactory(configService);
            var history = new WindowHistory();
            WindowManager = new WindowManager(win32, factory, history);
            WindowManager.SetConfigService(configService);

            // 初始化活动窗口跟踪服务
            _lastActiveService = new LastActiveWindowService();
            WindowManager.SetLastActiveWindowService(_lastActiveService);

            // 创建隐藏窗口用于接收热键消息和保持应用运行
            _hiddenWindow = new Window();
            var appWindow = _hiddenWindow.AppWindow;
            var presenter = Microsoft.UI.Windowing.OverlappedPresenter.Create();
            presenter.IsMinimizable = false;
            presenter.IsMaximizable = false;
            presenter.IsResizable = false;
            presenter.SetBorderAndTitleBar(false, false);
            appWindow.SetPresenter(presenter);
            appWindow.IsShownInSwitchers = false;
            appWindow.Show(activateWindow: false);
            appWindow.Hide();

            _hotkeyHwnd = (nint)WinRT.Interop.WindowNative.GetWindowHandle(_hiddenWindow);
            HotkeyManager = new HotkeyManager(_hotkeyHwnd, WindowManager!, configService);

            // 初始化托盘（传入 lastActiveService）
            _trayIconService = new TrayIconService(WindowManager!, ShowSettingsWindow, configService, _lastActiveService);
            _trayIconService.Initialize();

            // 输出所有操作项和快捷键到日志
            // LogAllShortcuts(configService);
        }

        private void LogAllShortcuts(ConfigService configService)
        {
            try
            {
                var config = configService.Load();
                var defaultShortcuts = ConfigService.GetDefaultShortcuts();

                // 合并默认和自定义配置
                var merged = new Dictionary<string, ShortcutConfig>(defaultShortcuts);
                foreach (var kvp in config.Shortcuts)
                {
                    merged[kvp.Key] = kvp.Value;
                }

                Logger.Info("App", "");
                Logger.Info("App", "╔══════════════════════════════════════════════════════════════════════╗");
                Logger.Info("App", "║                        快捷键配置列表                                ║");
                Logger.Info("App", "╠══════════════════════════════════════════════════════════════════════╣");
                Logger.Info("App", "║  操作名称        动作标识            快捷键              状态      ║");
                Logger.Info("App", "╠══════════════════════════════════════════════════════════════════════╣");

                foreach (var action in GetOrderedActions())
                {
                    if (merged.TryGetValue(action.Key, out var shortcut))
                    {
                        var shortcutText = FormatShortcut(shortcut);
                        var status = shortcut.Enabled && shortcut.KeyCode > 0 ? "启用" : "禁用";
                        var line = string.Format("║  {0,-8}  {1,-18}  {2,-18}  {3,-6} ║",
                            PadRightUnicode(action.Value, 8),
                            PadRightUnicode(action.Key, 18),
                            PadRightUnicode(shortcutText, 18),
                            status);
                        Logger.Info("App", line);
                    }
                    else
                    {
                        var line = string.Format("║  {0,-8}  {1,-18}  {2,-18}  {3,-6} ║",
                            PadRightUnicode(action.Value, 8),
                            PadRightUnicode(action.Key, 18),
                            "未配置",
                            "--");
                        Logger.Info("App", line);
                    }
                }
                Logger.Info("App", "╚══════════════════════════════════════════════════════════════════════╝");
                Logger.Info("App", "");
            }
            catch (Exception ex)
            {
                Logger.Error("App", $"输出快捷键配置失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 处理包含Unicode字符的字符串填充，确保对齐
        /// </summary>
        private static string PadRightUnicode(string str, int totalWidth)
        {
            if (string.IsNullOrEmpty(str)) return new string(' ', totalWidth);

            int currentWidth = 0;
            foreach (char c in str)
            {
                // 中文字符和特殊符号占2个宽度，其他占1个
                currentWidth += (c > 127) ? 2 : 1;
            }

            int padding = totalWidth - currentWidth;
            if (padding <= 0) return str;

            return str + new string(' ', padding);
        }

        private static Dictionary<string, string> GetOrderedActions()
        {
            return new Dictionary<string, string>
            {
                // 半屏
                ["LeftHalf"] = "左半屏",
                ["RightHalf"] = "右半屏",
                ["CenterHalf"] = "中间半屏",
                ["TopHalf"] = "上半屏",
                ["BottomHalf"] = "下半屏",
                // 四角
                ["TopLeft"] = "左上",
                ["TopRight"] = "右上",
                ["BottomLeft"] = "左下",
                ["BottomRight"] = "右下",
                // 三分之一
                ["FirstThird"] = "左首1/3",
                ["CenterThird"] = "中间1/3",
                ["LastThird"] = "右首1/3",
                ["FirstTwoThirds"] = "左侧2/3",
                ["CenterTwoThirds"] = "中间2/3",
                ["LastTwoThirds"] = "右侧2/3",
                // 四等分
                ["FirstFourth"] = "左1/4",
                ["SecondFourth"] = "中左1/4",
                ["ThirdFourth"] = "中右1/4",
                ["LastFourth"] = "右1/4",
                ["FirstThreeFourths"] = "左3/4",
                ["CenterThreeFourths"] = "中间3/4",
                ["LastThreeFourths"] = "右3/4",
                // 六等分
                ["TopLeftSixth"] = "左上1/6",
                ["TopCenterSixth"] = "上中1/6",
                ["TopRightSixth"] = "右上1/6",
                ["BottomLeftSixth"] = "左下1/6",
                ["BottomCenterSixth"] = "下中1/6",
                ["BottomRightSixth"] = "右下1/6",
                // 最大化与缩放
                ["Maximize"] = "最大化",
                ["AlmostMaximize"] = "接近最大化",
                ["MaximizeHeight"] = "最大化高度",
                ["Larger"] = "放大",
                ["Smaller"] = "缩小",
                ["Center"] = "居中",
                ["Restore"] = "恢复",
                // 移动
                ["MoveLeft"] = "左移",
                ["MoveRight"] = "右移",
                ["MoveUp"] = "上移",
                ["MoveDown"] = "下移",
                // 显示器
                ["NextDisplay"] = "下一显示器",
                ["PreviousDisplay"] = "上一显示器",
                // 撤销/重做
                ["Undo"] = "撤销",
                ["Redo"] = "重做",
            };
        }

        private static string FormatShortcut(ShortcutConfig cfg)
        {
            if (!cfg.Enabled || cfg.KeyCode <= 0)
                return "无";

            var parts = new List<string>();
            if ((cfg.ModifierFlags & 0x0002) != 0) parts.Add("Ctrl");
            if ((cfg.ModifierFlags & 0x0001) != 0) parts.Add("Alt");
            if ((cfg.ModifierFlags & 0x0004) != 0) parts.Add("Shift");
            if ((cfg.ModifierFlags & 0x0008) != 0) parts.Add("Win");
            parts.Add(VkToString(cfg.KeyCode));
            return string.Join("+", parts);
        }

        private static string VkToString(int vk) => vk switch
        {
            0x25 => "←",
            0x26 => "↑",
            0x27 => "→",
            0x28 => "↓",
            0x0D => "Enter",
            0x08 => "Back",
            0x2E => "Del",
            0x20 => "Space",
            0xBB => "=",
            0xBD => "-",
            0x70 => "F1",
            0x71 => "F2",
            0x72 => "F3",
            0x73 => "F4",
            0x74 => "F5",
            0x75 => "F6",
            0x76 => "F7",
            0x77 => "F8",
            0x78 => "F9",
            0x79 => "F10",
            0x7A => "F11",
            0x7B => "F12",
            >= 0x41 and <= 0x5A => ((char)vk).ToString(),
            >= 0x30 and <= 0x39 => ((char)vk).ToString(),
            _ => $"0x{vk:X}"
        };

        private void ShowSettingsWindow()
        {
            try
            {
                Logger.Info("App", "准备显示设置窗口");
                
                // 每次都创建新窗口，避免重用已关闭的窗口
                if (_settingsWindow == null)
                {
                    Logger.Info("App", "创建新的设置窗口");
                    _settingsWindow = new MainWindow();
                    
                    // 监听窗口关闭事件
                    _settingsWindow.Closed += (s, e) =>
                    {
                        Logger.Info("App", "设置窗口已关闭，清理引用");
                        _settingsWindow = null;
                    };
                }

                var hwnd = (nint)WinRT.Interop.WindowNative.GetWindowHandle(_settingsWindow);
                Logger.Info("App", $"设置窗口句柄: {hwnd}");
                
                ShowWindow(hwnd, 9 /* SW_RESTORE */);
                SetForegroundWindow(hwnd);
                _settingsWindow.Activate();
                
                Logger.Info("App", "设置窗口已显示");
            }
            catch (Exception ex)
            {
                Logger.Error("App", $"显示设置窗口失败: {ex.Message}");
                Logger.Error("App", $"堆栈跟踪: {ex.StackTrace}");
                
                // 如果出错，清理窗口引用，下次重新创建
                _settingsWindow = null;
            }
        }

        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }
    }
}
