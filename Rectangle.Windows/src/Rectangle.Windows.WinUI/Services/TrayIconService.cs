using H.NotifyIcon;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Rectangle.Windows.WinUI.Core;
using System;

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
            // 必须从 XAML Resources 取出，不能 new TaskbarIcon()
            _taskbarIcon = (TaskbarIcon)Application.Current.Resources["TrayIcon"];

            // 绑定命令
            var showCmd = (XamlUICommand)Application.Current.Resources["ShowSettingsCommand"];
            showCmd.ExecuteRequested += (_, _) => _showSettingsCallback();

            var exitCmd = (XamlUICommand)Application.Current.Resources["ExitCommand"];
            exitCmd.ExecuteRequested += (_, _) => Exit();

            // 绑定菜单项点击事件
            BindMenuItems();

            // ForceCreate 让托盘图标在无窗口时也能工作
            _taskbarIcon.ForceCreate(enablesEfficiencyMode: false);
        }

        private void BindMenuItems()
        {
            if (_taskbarIcon?.ContextFlyout is not MenuFlyout flyout) return;

            BindItem(flyout, "LeftHalf",       WindowAction.LeftHalf);
            BindItem(flyout, "RightHalf",      WindowAction.RightHalf);
            BindItem(flyout, "Maximize",       WindowAction.Maximize);
            BindItem(flyout, "Center",         WindowAction.Center);
            BindItem(flyout, "Restore",        WindowAction.Restore);
            BindItem(flyout, "TopLeft",        WindowAction.TopLeft);
            BindItem(flyout, "TopRight",       WindowAction.TopRight);
            BindItem(flyout, "BottomLeft",     WindowAction.BottomLeft);
            BindItem(flyout, "BottomRight",    WindowAction.BottomRight);
            BindItem(flyout, "TopHalf",        WindowAction.TopHalf);
            BindItem(flyout, "BottomHalf",     WindowAction.BottomHalf);
            BindItem(flyout, "FirstThird",     WindowAction.FirstThird);
            BindItem(flyout, "CenterThird",    WindowAction.CenterThird);
            BindItem(flyout, "LastThird",      WindowAction.LastThird);
            BindItem(flyout, "NextDisplay",    WindowAction.NextDisplay);
            BindItem(flyout, "PreviousDisplay",WindowAction.PreviousDisplay);
        }

        private void BindItem(MenuFlyout flyout, string name, WindowAction action)
        {
            var item = FindItem(flyout, name);
            if (item != null)
                item.Click += (_, _) => _windowManager.Execute(action);
        }

        private static MenuFlyoutItem? FindItem(MenuFlyout flyout, string name)
        {
            foreach (var item in flyout.Items)
                if (item is MenuFlyoutItem mfi && mfi.Name == name)
                    return mfi;
            return null;
        }

        public void ShowNotification(string title, string message)
        {
            _taskbarIcon?.ShowNotification(title, message);
        }

        private void Exit()
        {
            _taskbarIcon?.Dispose();
            Application.Current.Exit();
        }

        public void Dispose()
        {
            _taskbarIcon?.Dispose();
            _taskbarIcon = null;
        }
    }
}
