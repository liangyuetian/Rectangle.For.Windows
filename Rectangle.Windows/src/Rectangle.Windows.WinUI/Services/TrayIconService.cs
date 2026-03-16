using H.NotifyIcon;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Rectangle.Windows.WinUI.Core;
using Rectangle.Windows.WinUI.ViewModels;
using System;
using System.Collections.Generic;

namespace Rectangle.Windows.WinUI.Services
{
    public class TrayIconService : IDisposable
    {
        private TaskbarIcon? _taskbarIcon;
        private readonly WindowManager _windowManager;
        private readonly Action _showSettingsCallback;
        private readonly ConfigService _configService;
        private LastActiveWindowService? _lastActiveService;

        /// <summary>
        /// 统一的菜单项命令，通过 CommandParameter 传递 action tag。使用 Command 而非 Click，因 H.NotifyIcon 托盘菜单下 Click 可能不触发。
        /// </summary>
        private XamlUICommand? _menuActionCommand;

        /// <summary>
        /// 忽略/取消忽略应用的命令
        /// </summary>
        private XamlUICommand? _ignoreAppCommand;

        /// <summary>
        /// 忽略应用菜单项引用，用于动态更新文本
        /// </summary>
        private MenuFlyoutItem? _ignoreAppMenuItem;

        // action tag → WindowAction
        private static readonly Dictionary<string, WindowAction> _tagToAction = new()
        {
            ["LeftHalf"]           = WindowAction.LeftHalf,
            ["RightHalf"]          = WindowAction.RightHalf,
            ["CenterHalf"]         = WindowAction.CenterHalf,
            ["TopHalf"]            = WindowAction.TopHalf,
            ["BottomHalf"]         = WindowAction.BottomHalf,
            ["TopLeft"]            = WindowAction.TopLeft,
            ["TopRight"]           = WindowAction.TopRight,
            ["BottomLeft"]         = WindowAction.BottomLeft,
            ["BottomRight"]        = WindowAction.BottomRight,
            ["FirstThird"]         = WindowAction.FirstThird,
            ["CenterThird"]        = WindowAction.CenterThird,
            ["LastThird"]          = WindowAction.LastThird,
            ["FirstTwoThirds"]     = WindowAction.FirstTwoThirds,
            ["CenterTwoThirds"]    = WindowAction.CenterTwoThirds,
            ["LastTwoThirds"]      = WindowAction.LastTwoThirds,
            ["FirstFourth"]        = WindowAction.FirstFourth,
            ["SecondFourth"]       = WindowAction.SecondFourth,
            ["ThirdFourth"]        = WindowAction.ThirdFourth,
            ["LastFourth"]         = WindowAction.LastFourth,
            ["FirstThreeFourths"]  = WindowAction.FirstThreeFourths,
            ["CenterThreeFourths"] = WindowAction.CenterThreeFourths,
            ["LastThreeFourths"]   = WindowAction.LastThreeFourths,
            ["TopLeftSixth"]       = WindowAction.TopLeftSixth,
            ["TopCenterSixth"]     = WindowAction.TopCenterSixth,
            ["TopRightSixth"]      = WindowAction.TopRightSixth,
            ["BottomLeftSixth"]    = WindowAction.BottomLeftSixth,
            ["BottomCenterSixth"]  = WindowAction.BottomCenterSixth,
            ["BottomRightSixth"]   = WindowAction.BottomRightSixth,
            ["Maximize"]           = WindowAction.Maximize,
            ["AlmostMaximize"]     = WindowAction.AlmostMaximize,
            ["MaximizeHeight"]     = WindowAction.MaximizeHeight,
            ["Larger"]             = WindowAction.Larger,
            ["Smaller"]            = WindowAction.Smaller,
            ["Center"]             = WindowAction.Center,
            ["Restore"]            = WindowAction.Restore,
            ["MoveLeft"]           = WindowAction.MoveLeft,
            ["MoveRight"]          = WindowAction.MoveRight,
            ["MoveUp"]             = WindowAction.MoveUp,
            ["MoveDown"]           = WindowAction.MoveDown,
            ["NextDisplay"]        = WindowAction.NextDisplay,
            ["PreviousDisplay"]    = WindowAction.PreviousDisplay,
        };

        public TrayIconService(WindowManager windowManager, Action showSettingsCallback,
                               ConfigService configService, LastActiveWindowService? lastActiveService = null)
        {
            _windowManager = windowManager;
            _showSettingsCallback = showSettingsCallback;
            _configService = configService;
            _lastActiveService = lastActiveService;
        }

        public void Initialize()
        {
            try
            {
                _taskbarIcon = (TaskbarIcon)Application.Current.Resources["TrayIcon"];

                var showCmd = (XamlUICommand)Application.Current.Resources["ShowSettingsCommand"];
                showCmd.ExecuteRequested += (_, _) => _showSettingsCallback();

                var exitCmd = (XamlUICommand)Application.Current.Resources["ExitCommand"];
                exitCmd.ExecuteRequested += (_, _) => DoExit();

                var shortcuts = LoadShortcuts();

                // 创建统一的菜单项命令（Command 在托盘菜单中比 Click 更可靠）
                _menuActionCommand = new XamlUICommand();
                _menuActionCommand.ExecuteRequested += OnMenuActionExecuteRequested;

                // 创建忽略应用命令
                _ignoreAppCommand = new XamlUICommand();
                _ignoreAppCommand.ExecuteRequested += OnIgnoreAppExecuteRequested;

                // 遍历 ContextFlyout 注入图标、快捷键文字、点击命令
                if (_taskbarIcon.ContextFlyout is MenuFlyout flyout)
                {
                    // 在菜单即将打开时暂停（Opening 比 Opened 更早，避免右键时焦点变化覆盖活动窗口）
                    flyout.Opening += (_, _) =>
                    {
                        _lastActiveService?.PauseTracking();
                        UpdateIgnoreMenuItem();
                    };
                    flyout.Closed += (_, _) => _lastActiveService?.ResumeTracking();

                    DecorateItems(flyout.Items, shortcuts);
                }

                _taskbarIcon.ForceCreate(enablesEfficiencyMode: false);
                Logger.Info("TrayIconService", "托盘图标初始化成功");
            }
            catch (Exception ex)
            {
                Logger.Error("TrayIconService", $"托盘图标初始化失败: {ex}");
            }
        }

        private void DecorateItems(IList<MenuFlyoutItemBase> items, Dictionary<string, ShortcutConfig> shortcuts)
        {
            Logger.Info("TrayIconService", $"DecorateItems 开始处理，菜单项数量: {items.Count}");
            
            foreach (var item in items)
            {
                if (item is MenuFlyoutSubItem sub)
                {
                    // 禁用访问键
                    sub.AccessKey = string.Empty;
                    // 递归处理子菜单
                    DecorateItems(sub.Items, shortcuts);
                }
                else if (item is MenuFlyoutItem fi)
                {
                    // 禁用访问键
                    fi.AccessKey = string.Empty;
                    
                    Logger.Info("TrayIconService", $"处理菜单项: {fi.Text}, Tag: {fi.Tag}");
                    
                    if (fi.Tag is string tag)
                    {
                        if (tag == "IgnoreApp")
                        {
                            // 忽略应用菜单项：使用独立命令，文本在 Opening 时动态更新
                            _ignoreAppMenuItem = fi;
                            fi.Command = _ignoreAppCommand;
                            Logger.Info("TrayIconService", "已绑定忽略应用菜单项");
                            continue;
                        }

                        // 添加快捷键文本到右侧
                        var shortcutText = GetShortcutText(tag, shortcuts);
                        if (!string.IsNullOrEmpty(shortcutText))
                        {
                            fi.KeyboardAcceleratorTextOverride = shortcutText;
                            Logger.Info("TrayIconService", $"菜单项 '{fi.Text}' 设置快捷键: {shortcutText}");
                        }

                        // 使用 Command 而非 Click（托盘菜单下 Click 可能不触发）
                        fi.Command = _menuActionCommand;
                        fi.CommandParameter = tag;
                        Logger.Info("TrayIconService", $"已为菜单项 '{fi.Text}' 绑定 Command: {tag}");
                    }
                }
            }
        }

        private void OnMenuActionExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {
            var tag = args.Parameter as string;
            if (string.IsNullOrEmpty(tag))
            {
                Logger.Warning("TrayIconService", "菜单项 Command 执行时 Parameter 为空");
                return;
            }
            Logger.Info("TrayIconService", $"菜单项 Command 执行: {tag}");
            if (_tagToAction.TryGetValue(tag, out var action))
            {
                _windowManager.Execute(action);
            }
            else
            {
                Logger.Warning("TrayIconService", $"未找到对应的动作: {tag}");
            }
        }

        private void OnIgnoreAppExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {
            var processName = args.Parameter as string;
            if (string.IsNullOrEmpty(processName))
            {
                Logger.Warning("TrayIconService", "忽略应用 Command 执行时 Parameter 为空");
                return;
            }
            ToggleIgnoreApp(processName);
        }

        private void UpdateIgnoreMenuItem()
        {
            if (_ignoreAppMenuItem == null || _lastActiveService == null) return;

            var hwnd = _lastActiveService.GetLastValidWindow();
            if (hwnd == 0)
            {
                _ignoreAppMenuItem.Text = "忽略 [无有效窗口]";
                _ignoreAppMenuItem.IsEnabled = false;
                _ignoreAppMenuItem.CommandParameter = null;
                return;
            }

            var processName = WindowEnumerator.GetProcessNameFromWindow(hwnd);
            if (string.IsNullOrEmpty(processName))
            {
                _ignoreAppMenuItem.Text = "忽略 [未知应用]";
                _ignoreAppMenuItem.IsEnabled = false;
                _ignoreAppMenuItem.CommandParameter = null;
                return;
            }

            var config = _configService.Load();
            var isIgnored = config.IgnoredApps.Exists(a =>
                a.Equals(processName, StringComparison.OrdinalIgnoreCase) ||
                a.Equals(processName + ".exe", StringComparison.OrdinalIgnoreCase));

            _ignoreAppMenuItem.Text = isIgnored ? $"取消忽略 {processName}" : $"忽略 {processName}";
            _ignoreAppMenuItem.IsEnabled = true;
            _ignoreAppMenuItem.CommandParameter = processName;
        }

        private void ToggleIgnoreApp(string processName)
        {
            var config = _configService.Load();
            var isIgnored = config.IgnoredApps.Exists(a =>
                a.Equals(processName, StringComparison.OrdinalIgnoreCase) ||
                a.Equals(processName + ".exe", StringComparison.OrdinalIgnoreCase));

            if (isIgnored)
            {
                config.IgnoredApps.RemoveAll(a =>
                    a.Equals(processName, StringComparison.OrdinalIgnoreCase) ||
                    a.Equals(processName + ".exe", StringComparison.OrdinalIgnoreCase));
                Logger.Info("TrayIconService", $"已从忽略列表移除: {processName}");
            }
            else
            {
                config.IgnoredApps.Add(processName);
                Logger.Info("TrayIconService", $"已添加到忽略列表: {processName}");
            }

            _configService.Save(config);
        }

        // ── 快捷键 ────────────────────────────────────────────────

        private Dictionary<string, ShortcutConfig> LoadShortcuts()
        {
            var config = _configService.Load();
            var merged = new Dictionary<string, ShortcutConfig>(ConfigService.GetDefaultShortcuts());
            foreach (var kvp in config.Shortcuts) merged[kvp.Key] = kvp.Value;
            return merged;
        }

        private static string GetShortcutText(string name, Dictionary<string, ShortcutConfig> shortcuts)
        {
            if (!shortcuts.TryGetValue(name, out var cfg) || !cfg.Enabled || cfg.KeyCode <= 0)
                return string.Empty;
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
            0x25 => "Left", 0x26 => "Up", 0x27 => "Right", 0x28 => "Down",
            0x0D => "Enter", 0x08 => "Back", 0x2E => "Del", 0x20 => "Space",
            0xBB => "=", 0xBD => "-",
            0x70 => "F1",  0x71 => "F2",  0x72 => "F3",  0x73 => "F4",
            0x74 => "F5",  0x75 => "F6",  0x76 => "F7",  0x77 => "F8",
            0x78 => "F9",  0x79 => "F10", 0x7A => "F11", 0x7B => "F12",
            >= 0x41 and <= 0x5A => ((char)vk).ToString(),
            >= 0x30 and <= 0x39 => ((char)vk).ToString(),
            _ => $"0x{vk:X}"
        };

        // ── 其他 ──────────────────────────────────────────────────

        private void DoExit() { Dispose(); Environment.Exit(0); }

        public void ShowNotification(string title, string message) =>
            _taskbarIcon?.ShowNotification(title, message);

        public void Dispose()
        {
            if (_menuActionCommand != null)
            {
                _menuActionCommand.ExecuteRequested -= OnMenuActionExecuteRequested;
                _menuActionCommand = null;
            }
            if (_ignoreAppCommand != null)
            {
                _ignoreAppCommand.ExecuteRequested -= OnIgnoreAppExecuteRequested;
                _ignoreAppCommand = null;
            }
            _ignoreAppMenuItem = null;
            _taskbarIcon?.Dispose();
            _taskbarIcon = null;
        }
    }
}
