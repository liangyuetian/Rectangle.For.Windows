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

        /// <summary>
        /// 显示设置窗口
        /// </summary>
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
            // 初始化配置服务
            var configService = new ConfigService();

            // 初始化日志系统
            Logger.InitializeFromConfig(configService);
            Logger.Info("App", "应用启动");

            // 加载并应用主题
            ThemeService.Instance.LoadThemeFromConfig();

            // 创建隐藏的主窗口（用于接收热键消息）
            MainWindow = new Window();

            // 初始化服务
            var win32 = new Win32WindowService();
            var factory = new CalculatorFactory();
            var history = new WindowHistory();
            WindowManager = new WindowManager(win32, factory, history);

            // 创建 Frame 作为窗口内容
            if (MainWindow.Content is not Frame rootFrame)
            {
                rootFrame = new Frame();
                rootFrame.NavigationFailed += OnNavigationFailed;
                MainWindow.Content = rootFrame;
            }

            // 导航到默认页面（用于初始化）
            _ = rootFrame.Navigate(typeof(MainPage), e.Arguments);

            // 应用主题
            ThemeService.Instance.ApplyThemeToWindow(MainWindow);

            // 激活窗口以注册热键
            MainWindow.Activate();

            // 获取窗口句柄并初始化热键管理器
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(MainWindow);
            HotkeyManager = new HotkeyManager(hwnd, WindowManager!);

            // 隐藏主窗口，只在托盘显示
            var hwndMain = WinRT.Interop.WindowNative.GetWindowHandle(MainWindow);
            PInvoke.ShowWindow((HWND)hwndMain, SHOW_WINDOW_CMD.SW_HIDE);

            // 创建托盘图标（使用新的服务）
            _trayIconService = new TrayIconService(WindowManager!, ShowSettingsWindow);
            _trayIconService.Initialize();

            // 显示启动提示（可选）
            _trayIconService.ShowNotification("Rectangle", "Rectangle 已在后台运行");

            System.Diagnostics.Debug.WriteLine("[App] 初始化完成");
        }

        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }
    }

    /// <summary>
    /// 简单的命令实现，用于托盘图标点击
    /// </summary>
    public class RelayCommand : System.Windows.Input.ICommand
    {
        private readonly Action _execute;

        public RelayCommand(Action execute)
        {
            _execute = execute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            _execute();
        }
    }
}
