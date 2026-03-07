using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Rectangle.Windows.WinUI.Core;
using Rectangle.Windows.WinUI.Services;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using H.NotifyIcon;

namespace Rectangle.Windows.WinUI
{
    public partial class App : Application
    {
        public static Window? MainWindow { get; private set; }
        public static WindowManager? WindowManager { get; private set; }
        public static HotkeyManager? HotkeyManager { get; private set; }
        private TaskbarIcon? _taskbarIcon;

        public App()
        {
            this.InitializeComponent();
        }

        private void CreateTrayIcon()
        {
            _taskbarIcon = new TaskbarIcon
            {
                ToolTipText = "Rectangle"
            };

            var menuFlyout = new MenuFlyout();
            
            // 半屏操作
            menuFlyout.Items.Add(CreateMenuItem("左半屏", WindowAction.LeftHalf));
            menuFlyout.Items.Add(CreateMenuItem("右半屏", WindowAction.RightHalf));
            menuFlyout.Items.Add(CreateMenuItem("上半屏", WindowAction.TopHalf));
            menuFlyout.Items.Add(CreateMenuItem("下半屏", WindowAction.BottomHalf));
            menuFlyout.Items.Add(new MenuFlyoutSeparator());
            
            // 四角操作
            menuFlyout.Items.Add(CreateMenuItem("左上", WindowAction.TopLeft));
            menuFlyout.Items.Add(CreateMenuItem("右上", WindowAction.TopRight));
            menuFlyout.Items.Add(CreateMenuItem("左下", WindowAction.BottomLeft));
            menuFlyout.Items.Add(CreateMenuItem("右下", WindowAction.BottomRight));
            menuFlyout.Items.Add(new MenuFlyoutSeparator());
            
            // 其他操作
            menuFlyout.Items.Add(CreateMenuItem("最大化", WindowAction.Maximize));
            menuFlyout.Items.Add(CreateMenuItem("居中", WindowAction.Center));
            menuFlyout.Items.Add(CreateMenuItem("恢复", WindowAction.Restore));
            menuFlyout.Items.Add(new MenuFlyoutSeparator());
            
            // 退出
            var exitItem = new MenuFlyoutItem { Text = "退出" };
            exitItem.Click += (s, e) => Exit();
            menuFlyout.Items.Add(exitItem);

            _taskbarIcon.ContextFlyout = menuFlyout;
        }

        private MenuFlyoutItem CreateMenuItem(string text, WindowAction action)
        {
            var item = new MenuFlyoutItem { Text = text };
            item.Click += (s, e) => WindowManager?.Execute(action);
            return item;
        }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            MainWindow = new Window();

            // 初始化服务
            var win32 = new Win32WindowService();
            var factory = new CalculatorFactory();
            var history = new WindowHistory();
            WindowManager = new WindowManager(win32, factory, history);

            // 创建主页面
            if (MainWindow.Content is not Frame rootFrame)
            {
                rootFrame = new Frame();
                rootFrame.NavigationFailed += OnNavigationFailed;
                MainWindow.Content = rootFrame;
            }

            _ = rootFrame.Navigate(typeof(MainPage), e.Arguments);
            
            // 激活窗口以注册热键
            MainWindow.Activate();
            
            // 获取窗口句柄并初始化热键管理器
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(MainWindow);
            HotkeyManager = new HotkeyManager(hwnd, WindowManager!);
            
            // 隐藏主窗口，只在托盘显示
            var hwndMain = WinRT.Interop.WindowNative.GetWindowHandle(MainWindow);
            PInvoke.ShowWindow((HWND)hwndMain, SHOW_WINDOW_CMD.SW_HIDE);
            
            // 创建托盘图标
            CreateTrayIcon();
            
            Console.WriteLine("[App] 初始化完成");
        }

        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }
    }
}
