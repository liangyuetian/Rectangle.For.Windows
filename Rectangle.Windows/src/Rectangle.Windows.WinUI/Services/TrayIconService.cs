using H.NotifyIcon;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Rectangle.Windows.WinUI.Core;
using System;
using System.Collections.Generic;

namespace Rectangle.Windows.WinUI.Services
{
    public class TrayIconService
    {
        private TaskbarIcon? _taskbarIcon;
        private readonly WindowManager _windowManager;
        private readonly Action _showSettingsCallback;

        private static readonly Dictionary<string, WindowAction> _tagToAction = new()
        {
            ["LeftHalf"]        = WindowAction.LeftHalf,
            ["RightHalf"]       = WindowAction.RightHalf,
            ["Maximize"]        = WindowAction.Maximize,
            ["Center"]          = WindowAction.Center,
            ["Restore"]         = WindowAction.Restore,
            ["TopLeft"]         = WindowAction.TopLeft,
            ["TopRight"]        = WindowAction.TopRight,
            ["BottomLeft"]      = WindowAction.BottomLeft,
            ["BottomRight"]     = WindowAction.BottomRight,
            ["TopHalf"]         = WindowAction.TopHalf,
            ["BottomHalf"]      = WindowAction.BottomHalf,
            ["FirstThird"]      = WindowAction.FirstThird,
            ["CenterThird"]     = WindowAction.CenterThird,
            ["LastThird"]       = WindowAction.LastThird,
            ["NextDisplay"]     = WindowAction.NextDisplay,
            ["PreviousDisplay"] = WindowAction.PreviousDisplay,
        };

        public TrayIconService(WindowManager windowManager, Action showSettingsCallback)
        {
            _windowManager = windowManager;
            _showSettingsCallback = showSettingsCallback;
        }

        public void Initialize()
        {
            try
            {
                // 从 XAML Resources 取出（必须这样，不能 new TaskbarIcon()）
                _taskbarIcon = (TaskbarIcon)Application.Current.Resources["TrayIcon"];

                // 绑定命令
                var showCmd = (XamlUICommand)Application.Current.Resources["ShowSettingsCommand"];
                showCmd.ExecuteRequested += (_, _) => _showSettingsCallback();

                var exitCmd = (XamlUICommand)Application.Current.Resources["ExitCommand"];
                exitCmd.ExecuteRequested += (_, _) =>
                {
                    _taskbarIcon?.Dispose();
                    Environment.Exit(0);
                };

                // 通过 Tag 绑定菜单项
                if (_taskbarIcon.ContextFlyout is MenuFlyout flyout)
                {
                    foreach (var item in flyout.Items)
                    {
                        if (item is MenuFlyoutItem mfi &&
                            mfi.Tag is string tag &&
                            _tagToAction.TryGetValue(tag, out var action))
                        {
                            var captured = action;
                            mfi.Click += (_, _) => _windowManager.Execute(captured);
                        }
                    }
                }

                // ForceCreate 让托盘图标在无可见窗口时也能工作
                _taskbarIcon.ForceCreate(enablesEfficiencyMode: false);

                Logger.Info("TrayIconService", "托盘图标初始化成功");
            }
            catch (Exception ex)
            {
                Logger.Error("TrayIconService", $"托盘图标初始化失败: {ex}");
            }
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
