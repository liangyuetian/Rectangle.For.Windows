using H.NotifyIcon;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Rectangle.Windows.WinUI.Core;
using Rectangle.Windows.WinUI.ViewModels;
using System;
using System.Collections.Generic;

namespace Rectangle.Windows.WinUI.Services
{
    public class TrayIconService
    {
        private TaskbarIcon? _taskbarIcon;
        private readonly WindowManager _windowManager;
        private readonly Action _showSettingsCallback;

        public TrayIconService(WindowManager windowManager, Action showSettingsCallback)
        {
            _windowManager = windowManager;
            _showSettingsCallback = showSettingsCallback;
        }

        public void Initialize()
        {
            _taskbarIcon = new TaskbarIcon
            {
                ToolTipText = "Rectangle - 窗口管理工具",
                // 使用 .ico 文件（托盘图标必须是 .ico）
                IconSource = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(
                    new Uri("ms-appx:///Assets/AppIcon.ico"))
            };

            // ForceCreate 让 H.NotifyIcon 在没有 XAML 树的情况下也能工作
            // 并创建一个内部透明窗口来承载 XamlRoot
            _taskbarIcon.ForceCreate(enablesEfficiencyMode: false);

            _taskbarIcon.ContextFlyout = CreateMenuFlyout();
            _taskbarIcon.LeftClickCommand = new RelayCommand(_showSettingsCallback);
            _taskbarIcon.DoubleClickCommand = new RelayCommand(_showSettingsCallback);
        }

        private MenuFlyout CreateMenuFlyout()
        {
            var menu = new MenuFlyout();

            AddMenuSection(menu, "常用", new Dictionary<string, WindowAction>
            {
                { "左半屏", WindowAction.LeftHalf },
                { "右半屏", WindowAction.RightHalf },
                { "最大化", WindowAction.Maximize },
                { "居中", WindowAction.Center },
                { "恢复", WindowAction.Restore }
            });

            menu.Items.Add(new MenuFlyoutSeparator());

            AddMenuSection(menu, "四角", new Dictionary<string, WindowAction>
            {
                { "左上", WindowAction.TopLeft },
                { "右上", WindowAction.TopRight },
                { "左下", WindowAction.BottomLeft },
                { "右下", WindowAction.BottomRight }
            });

            menu.Items.Add(new MenuFlyoutSeparator());

            AddMenuSection(menu, "三分屏", new Dictionary<string, WindowAction>
            {
                { "左首 1/3", WindowAction.FirstThird },
                { "中间 1/3", WindowAction.CenterThird },
                { "右首 1/3", WindowAction.LastThird }
            });

            menu.Items.Add(new MenuFlyoutSeparator());

            AddMenuSection(menu, "显示器", new Dictionary<string, WindowAction>
            {
                { "下一个显示器", WindowAction.NextDisplay },
                { "上一个显示器", WindowAction.PreviousDisplay }
            });

            menu.Items.Add(new MenuFlyoutSeparator());

            var moreMenu = new MenuFlyoutSubItem { Text = "更多操作" };
            moreMenu.Items.Add(CreateMenuItem("上半屏", WindowAction.TopHalf));
            moreMenu.Items.Add(CreateMenuItem("下半屏", WindowAction.BottomHalf));
            moreMenu.Items.Add(new MenuFlyoutSeparator());
            moreMenu.Items.Add(CreateMenuItem("放大", WindowAction.Larger));
            moreMenu.Items.Add(CreateMenuItem("缩小", WindowAction.Smaller));
            moreMenu.Items.Add(new MenuFlyoutSeparator());
            moreMenu.Items.Add(CreateMenuItem("左移", WindowAction.MoveLeft));
            moreMenu.Items.Add(CreateMenuItem("右移", WindowAction.MoveRight));
            moreMenu.Items.Add(CreateMenuItem("上移", WindowAction.MoveUp));
            moreMenu.Items.Add(CreateMenuItem("下移", WindowAction.MoveDown));
            menu.Items.Add(moreMenu);

            menu.Items.Add(new MenuFlyoutSeparator());

            var settingsItem = new MenuFlyoutItem { Text = "偏好设置..." };
            settingsItem.Click += (s, e) => _showSettingsCallback();
            menu.Items.Add(settingsItem);

            menu.Items.Add(new MenuFlyoutSeparator());

            var aboutItem = new MenuFlyoutItem { Text = "关于 Rectangle" };
            aboutItem.Click += (s, e) => ShowAboutDialog();
            menu.Items.Add(aboutItem);

            menu.Items.Add(new MenuFlyoutSeparator());

            var exitItem = new MenuFlyoutItem { Text = "退出" };
            exitItem.Click += (s, e) => Exit();
            menu.Items.Add(exitItem);

            return menu;
        }

        private void AddMenuSection(MenuFlyout menu, string title, Dictionary<string, WindowAction> items)
        {
            menu.Items.Add(new MenuFlyoutItem
            {
                Text = title,
                IsEnabled = false,
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray)
            });
            foreach (var item in items)
                menu.Items.Add(CreateMenuItem(item.Key, item.Value));
        }

        private MenuFlyoutItem CreateMenuItem(string text, WindowAction action)
        {
            var item = new MenuFlyoutItem { Text = text };
            item.Click += (s, e) => _windowManager.Execute(action);
            return item;
        }

        private void ShowAboutDialog()
        {
            var dialog = new ContentDialog
            {
                Title = "关于 Rectangle",
                Content = "Rectangle for Windows v1.0.0\n\n基于 macOS Rectangle 移植\n开源窗口管理工具",
                PrimaryButtonText = "确定",
                SecondaryButtonText = "GitHub",
                DefaultButton = ContentDialogButton.Primary
            };

            var root = App.MainWindow?.Content?.XamlRoot;
            if (root != null)
                dialog.XamlRoot = root;

            _ = dialog.ShowAsync();
        }

        private void Exit()
        {
            _taskbarIcon?.Dispose();
            Application.Current.Exit();
        }

        public void ShowNotification(string title, string message)
        {
            _taskbarIcon?.ShowNotification(title, message);
        }

        public void Dispose()
        {
            _taskbarIcon?.Dispose();
            _taskbarIcon = null;
        }
    }
}
