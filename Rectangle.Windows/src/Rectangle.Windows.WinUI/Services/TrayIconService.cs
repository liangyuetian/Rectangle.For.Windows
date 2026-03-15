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

                // 遍历 ContextFlyout 注入图标、快捷键文字、点击命令
                if (_taskbarIcon.ContextFlyout is MenuFlyout flyout)
                    DecorateItems(flyout.Items, shortcuts);

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
            foreach (var item in items)
            {
                if (item is MenuFlyoutSubItem sub)
                {
                    // 递归处理子菜单
                    DecorateItems(sub.Items, shortcuts);
                }
                else if (item is MenuFlyoutItem fi && fi.Tag is string tag)
                {
                    // 添加快捷键文本
                    var shortcutText = GetShortcutText(tag, shortcuts);
                    if (!string.IsNullOrEmpty(shortcutText))
                    {
                        fi.KeyboardAcceleratorTextOverride = shortcutText;
                        Logger.Info("TrayIconService", $"菜单项 '{fi.Text}' 设置快捷键: {shortcutText}");
                    }
                    
                    // 添加点击事件
                    fi.Click += (_, _) =>
                    {
                        if (_tagToAction.TryGetValue(tag, out var action))
                            _windowManager.Execute(action);
                    };
                }
            }
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
            0x25 => "←", 0x26 => "↑", 0x27 => "→", 0x28 => "↓",
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
            _taskbarIcon?.Dispose();
            _taskbarIcon = null;
        }
    }
}
