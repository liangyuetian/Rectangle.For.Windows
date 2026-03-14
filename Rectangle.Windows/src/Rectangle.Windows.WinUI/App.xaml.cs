using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
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
        public static Window? MainWindow { get; private set; }
        public static WindowManager? WindowManager { get; private set; }
        public static HotkeyManager? HotkeyManager { get; private set; }
        private TrayIconService? _trayIconService;
        private Window? _settingsWindow;

        public App()
        {
            this.InitializeComponent();
        }

        private void ShowSettingsWindow()
        {
            if (_settingsWindow == null)
            {
                _settingsWindow = new MainWindow();
                _settingsWindow.Closed += (s, e) =>
                {
                    _settingsWindow = null;
                };
            }

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(_settingsWindow);
            PInvoke.ShowWindow((HWND)hwnd, SHOW_WINDOW_CMD.SW_SHOW);
            PInvoke.SetForegroundWindow((HWND)hwnd);
            _settingsWindow.Activate();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            var configService = new ConfigService();

            Logger.InitializeFromConfig(configService);
            Logger.Info("App", "应用启动");

            ThemeService.Instance.LoadThemeFromConfig();

            MainWindow = new Window();

            var win32 = new Win32WindowService();
            var factory = new CalculatorFactory();
            var history = new WindowHistory();
            WindowManager = new WindowManager(win32, factory, history);

            if (MainWindow.Content is not Frame rootFrame)
            {
                rootFrame = new Frame();
                rootFrame.NavigationFailed += OnNavigationFailed;
                MainWindow.Content = rootFrame;
            }

            _ = rootFrame.Navigate(typeof(MainPage), e.Arguments);

            ThemeService.Instance.ApplyThemeToWindow(MainWindow);

            MainWindow.Activate();

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(MainWindow);
            HotkeyManager = new HotkeyManager(hwnd, WindowManager!);

            var hwndMain = WinRT.Interop.WindowNative.GetWindowHandle(MainWindow);
            PInvoke.ShowWindow((HWND)hwndMain, SHOW_WINDOW_CMD.SW_HIDE);

            _trayIconService = new TrayIconService(WindowManager!, ShowSettingsWindow);
            _trayIconService.Initialize();

            _trayIconService.ShowNotification("Rectangle", "Rectangle 已在后台运行");

            System.Diagnostics.Debug.WriteLine("[App] 初始化完成");
        }

        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }
    }
}
