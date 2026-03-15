using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Navigation;
using Rectangle.Windows.WinUI.Core;
using Rectangle.Windows.WinUI.Services;
using Rectangle.Windows.WinUI.Views;
using H.NotifyIcon;
using System;
using System.Runtime.InteropServices;

namespace Rectangle.Windows.WinUI
{
    public partial class App : Application
    {
        public static Window? MainWindow => _instance?._settingsWindow;
        public static WindowManager? WindowManager { get; private set; }
        public static HotkeyManager? HotkeyManager { get; private set; }

        private static App? _instance;
        private TrayIconService? _trayIconService;
        private LastActiveWindowService? _lastActiveService;
        private Window? _settingsWindow;
        private nint _hotkeyHwnd;

        [DllImport("user32.dll")] static extern bool ShowWindow(nint hWnd, int nCmdShow);
        [DllImport("user32.dll")] static extern bool SetForegroundWindow(nint hWnd);

        public App()
        {
            Environment.SetEnvironmentVariable(
                "MICROSOFT_WINDOWSAPPRUNTIME_BASE_DIRECTORY",
                AppContext.BaseDirectory);
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
            WindowManager.SetConfigService(configService);

            // 初始化活动窗口跟踪服务
            _lastActiveService = new LastActiveWindowService();
            WindowManager.SetLastActiveWindowService(_lastActiveService);

            // 创建隐藏窗口用于接收热键消息
            var msgWindow = new Window();
            var appWindow = msgWindow.AppWindow;
            var presenter = Microsoft.UI.Windowing.OverlappedPresenter.Create();
            presenter.IsMinimizable = false;
            presenter.IsMaximizable = false;
            presenter.IsResizable = false;
            presenter.SetBorderAndTitleBar(false, false);
            appWindow.SetPresenter(presenter);
            appWindow.IsShownInSwitchers = false;
            appWindow.Show(activateWindow: false);
            appWindow.Hide();

            _hotkeyHwnd = (nint)WinRT.Interop.WindowNative.GetWindowHandle(msgWindow);
            HotkeyManager = new HotkeyManager(_hotkeyHwnd, WindowManager!);

            // 初始化托盘（传入 lastActiveService）
            _trayIconService = new TrayIconService(WindowManager!, ShowSettingsWindow, configService, _lastActiveService);
            _trayIconService.Initialize();
        }

        private void ShowSettingsWindow()
        {
            if (_settingsWindow == null)
            {
                _settingsWindow = new MainWindow();
                _settingsWindow.Closed += (s, e) => _settingsWindow = null;
            }

            var hwnd = (nint)WinRT.Interop.WindowNative.GetWindowHandle(_settingsWindow);
            ShowWindow(hwnd, 9 /* SW_RESTORE */);
            SetForegroundWindow(hwnd);
            _settingsWindow.Activate();
        }

        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }
    }
}
