using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Navigation;
using Rectangle.Windows.WinUI.Core;
using Rectangle.Windows.WinUI.Services;
using Rectangle.Windows.WinUI.Views;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using System;

namespace Rectangle.Windows.WinUI
{
    public partial class App : Application
    {
        public static Window? MainWindow => _instance?._settingsWindow;
        public static WindowManager? WindowManager { get; private set; }
        public static HotkeyManager? HotkeyManager { get; private set; }

        private static App? _instance;
        private TrayIconService? _trayIconService;
        private Window? _settingsWindow;
        private nint _hotkeyHwnd;

        public App()
        {
            _instance = this;
            this.InitializeComponent();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            var configService = new ConfigService();
            Logger.InitializeFromConfig(configService);
            Logger.Info("App", "应用启动");

            ThemeService.Instance.LoadThemeFromConfig();

            var win32 = new Win32WindowService();
            var factory = new CalculatorFactory();
            var history = new WindowHistory();
            WindowManager = new WindowManager(win32, factory, history);

            // 创建一个最小化隐藏窗口，仅用于接收热键消息
            var msgWindow = new Window();
            msgWindow.Activate();
            _hotkeyHwnd = (nint)WinRT.Interop.WindowNative.GetWindowHandle(msgWindow);
            PInvoke.ShowWindow((HWND)_hotkeyHwnd, SHOW_WINDOW_CMD.SW_HIDE);
            HotkeyManager = new HotkeyManager(_hotkeyHwnd, WindowManager!);

            // 初始化托盘
            _trayIconService = new TrayIconService(WindowManager!, ShowSettingsWindow);
            _trayIconService.Initialize();
        }

        private void ShowSettingsWindow()
        {
            if (_settingsWindow == null)
            {
                _settingsWindow = new MainWindow();
                _settingsWindow.Closed += (s, e) => _settingsWindow = null;
            }

            var hwnd = (HWND)WinRT.Interop.WindowNative.GetWindowHandle(_settingsWindow);
            PInvoke.ShowWindow(hwnd, SHOW_WINDOW_CMD.SW_SHOW);
            PInvoke.SetForegroundWindow(hwnd);
            _settingsWindow.Activate();
        }

        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }
    }
}
