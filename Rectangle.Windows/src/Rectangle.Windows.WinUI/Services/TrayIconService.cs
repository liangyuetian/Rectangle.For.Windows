using H.NotifyIcon;
using Microsoft.UI.Xaml.Controls;
using Rectangle.Windows.WinUI.Core;
using Rectangle.Windows.WinUI.ViewModels;
using System;
using System.Collections.Generic;

namespace Rectangle.Windows.WinUI.Services
{
    /// <summary>
    /// 托盘图标服务
    /// </summary>
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

        /// <summary>
        /// 创建并显示托盘图标
        /// </summary>
        public void Initialize()
        {
            _taskbarIcon = new TaskbarIcon
            {
                ToolTipText = "Rectangle - 窗口管理工具",
                IconSource = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(
                    new Uri("ms-appx:///Assets/Square44x44Logo.scale-200.png"))
            };

            var menuFlyout = CreateMenuFlyout();
            _taskbarIcon.ContextFlyout = menuFlyout;

            // 左键和双击都打开设置
            _taskbarIcon.LeftClickCommand = new RelayCommand(_showSettingsCallback);
            _taskbarIcon.DoubleClickCommand = new RelayCommand(_showSettingsCallback);
        }

        /// <summary>
        /// 创建托盘菜单
        /// </summary>
        private MenuFlyout CreateMenuFlyout()
        {
            var menu = new MenuFlyout();

            // === 常用操作 ===
            AddMenuSection(menu, "常用", new Dictionary<string, WindowAction>
            {
                { "\uE7C5 左半屏", WindowAction.LeftHalf },
                { "\uE7C6 右半屏", WindowAction.RightHalf },
                { "\uE739 最大化", WindowAction.Maximize },
                { "\uE74D 居中", WindowAction.Center },
                { "\uE72A 恢复", WindowAction.Restore }
            });

            menu.Items.Add(new MenuFlyoutSeparator());

            // === 四角 ===
            AddMenuSection(menu, "四角", new Dictionary<string, WindowAction>
            {
                { "\uE744 左上", WindowAction.TopLeft },
                { "\uE745 右上", WindowAction.TopRight },
                { "\uE746 左下", WindowAction.BottomLeft },
                { "\uE747 右下", WindowAction.BottomRight }
            });

            menu.Items.Add(new MenuFlyoutSeparator());

            // === 三分屏 ===
            AddMenuSection(menu, "三分屏", new Dictionary<string, WindowAction>
            {
                { "\uE74C 左首 1/3", WindowAction.FirstThird },
                { "\uE74D 中间 1/3", WindowAction.CenterThird },
                { "\uE74E 右首 1/3", WindowAction.LastThird }
            });

            menu.Items.Add(new MenuFlyoutSeparator());

            // === 显示器 ===
            AddMenuSection(menu, "显示器", new Dictionary<string, WindowAction>
            {
                { "\uE7F5 下一个显示器", WindowAction.NextDisplay },
                { "\uE7F6 上一个显示器", WindowAction.PreviousDisplay }
            });

            menu.Items.Add(new MenuFlyoutSeparator());

            // === 更多操作子菜单 ===
            var moreMenu = new MenuFlyoutSubItem
            {
                Text = "更多操作",
                Icon = new FontIcon { Glyph = "\uE712" }
            };

            // 更多操作 - 半屏
            moreMenu.Items.Add(CreateMenuItem("\uE7C3 上半屏", WindowAction.TopHalf));
            moreMenu.Items.Add(CreateMenuItem("\uE7C2 下半屏", WindowAction.BottomHalf));
            moreMenu.Items.Add(CreateMenuItem("\uE7C4 中间半屏", WindowAction.CenterHalf));
            moreMenu.Items.Add(new MenuFlyoutSeparator());

            // 更多操作 - 缩放
            moreMenu.Items.Add(CreateMenuItem("\uE71F 放大", WindowAction.Larger));
            moreMenu.Items.Add(CreateMenuItem("\uE71E 缩小", WindowAction.Smaller));
            moreMenu.Items.Add(new MenuFlyoutSeparator());

            // 更多操作 - 移动
            moreMenu.Items.Add(CreateMenuItem("\uE72B 左移", WindowAction.MoveLeft));
            moreMenu.Items.Add(CreateMenuItem("\uE72A 右移", WindowAction.MoveRight));
            moreMenu.Items.Add(CreateMenuItem("\uE72C 上移", WindowAction.MoveUp));
            moreMenu.Items.Add(CreateMenuItem("\uE72D 下移", WindowAction.MoveDown));

            menu.Items.Add(moreMenu);
            menu.Items.Add(new MenuFlyoutSeparator());

            // === 设置和退出 ===
            var settingsItem = new MenuFlyoutItem
            {
                Text = "偏好设置...",
                Icon = new FontIcon { Glyph = "\uE713" }
            };
            settingsItem.Click += (s, e) => _showSettingsCallback();
            menu.Items.Add(settingsItem);

            menu.Items.Add(new MenuFlyoutSeparator());

            // 关于
            var aboutItem = new MenuFlyoutItem
            {
                Text = "关于 Rectangle",
                Icon = new FontIcon { Glyph = "\uE946" }
            };
            aboutItem.Click += (s, e) => ShowAboutDialog();
            menu.Items.Add(aboutItem);

            menu.Items.Add(new MenuFlyoutSeparator());

            // 退出
            var exitItem = new MenuFlyoutItem
            {
                Text = "退出",
                Icon = new FontIcon { Glyph = "\uE711" }
            };
            exitItem.Click += (s, e) => Exit();
            menu.Items.Add(exitItem);

            return menu;
        }

        /// <summary>
        /// 添加菜单分组
        /// </summary>
        private void AddMenuSection(MenuFlyout menu, string title, Dictionary<string, WindowAction> items)
        {
            // 标题（禁用状态）
            menu.Items.Add(new MenuFlyoutItem
            {
                Text = title,
                IsEnabled = false,
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray)
            });

            foreach (var item in items)
            {
                menu.Items.Add(CreateMenuItem(item.Key, item.Value));
            }
        }

        /// <summary>
        /// 创建单个菜单项
        /// </summary>
        private MenuFlyoutItem CreateMenuItem(string text, WindowAction action)
        {
            var item = new MenuFlyoutItem { Text = text };
            item.Click += (s, e) => _windowManager.Execute(action);
            return item;
        }

        /// <summary>
        /// 显示关于对话框
        /// </summary>
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

            // 注意：这里需要设置 XamlRoot
            if (App.MainWindow?.Content?.XamlRoot != null)
            {
                dialog.XamlRoot = App.MainWindow.Content.XamlRoot;
            }

            dialog.ShowAsync();
        }

        /// <summary>
        /// 退出应用
        /// </summary>
        private void Exit()
        {
            _taskbarIcon?.Dispose();
            Microsoft.UI.Xaml.Application.Current.Exit();
        }

        /// <summary>
        /// 显示托盘提示
        /// </summary>
        public void ShowNotification(string title, string message)
        {
            // H.NotifyIcon 支持气泡提示
            _taskbarIcon?.ShowNotification(title, message);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            _taskbarIcon?.Dispose();
            _taskbarIcon = null;
        }
    }
}
