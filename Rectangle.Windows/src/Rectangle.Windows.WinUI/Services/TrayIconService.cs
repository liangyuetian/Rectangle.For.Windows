using H.NotifyIcon;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Rectangle.Windows.WinUI.Core;
using Windows.Foundation;
using Rectangle.Windows.WinUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rectangle.Windows.WinUI.Services
{
    public class TrayIconService : IDisposable
    {
        private TaskbarIcon? _taskbarIcon;
        private readonly WindowManager _windowManager;
        private readonly Action _showSettingsCallback;
        private readonly ConfigService _configService;
        private LastActiveWindowService? _lastActiveService;
        private bool _contextMenuPrewarmed;
        private bool _contextMenuPrewarming;

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

        private static readonly string[] _iconPaths = new[]
        {
            "ms-appx:///Assets/WindowPositions/leftHalfTemplate.png",
            "ms-appx:///Assets/WindowPositions/rightHalfTemplate.png",
            "ms-appx:///Assets/WindowPositions/halfWidthCenterTemplate.png",
            "ms-appx:///Assets/WindowPositions/topHalfTemplate.png",
            "ms-appx:///Assets/WindowPositions/bottomHalfTemplate.png",
            "ms-appx:///Assets/WindowPositions/topLeftTemplate.png",
            "ms-appx:///Assets/WindowPositions/topRightTemplate.png",
            "ms-appx:///Assets/WindowPositions/bottomLeftTemplate.png",
            "ms-appx:///Assets/WindowPositions/bottomRightTemplate.png",
            "ms-appx:///Assets/WindowPositions/firstThirdTemplate.png",
            "ms-appx:///Assets/WindowPositions/centerThirdTemplate.png",
            "ms-appx:///Assets/WindowPositions/lastThirdTemplate.png",
            "ms-appx:///Assets/WindowPositions/firstTwoThirdsTemplate.png",
            "ms-appx:///Assets/WindowPositions/centerTwoThirdsTemplate.png",
            "ms-appx:///Assets/WindowPositions/lastTwoThirdsTemplate.png",
            "ms-appx:///Assets/WindowPositions/leftFourthTemplate.png",
            "ms-appx:///Assets/WindowPositions/centerLeftFourthTemplate.png",
            "ms-appx:///Assets/WindowPositions/centerRightFourthTemplate.png",
            "ms-appx:///Assets/WindowPositions/rightFourthTemplate.png",
            "ms-appx:///Assets/WindowPositions/firstThreeFourthsTemplate.png",
            "ms-appx:///Assets/WindowPositions/centerThreeFourthsTemplate.png",
            "ms-appx:///Assets/WindowPositions/lastThreeFourthsTemplate.png",
            "ms-appx:///Assets/WindowPositions/topLeftSixthTemplate.png",
            "ms-appx:///Assets/WindowPositions/topCenterSixthTemplate.png",
            "ms-appx:///Assets/WindowPositions/topRightSixthTemplate.png",
            "ms-appx:///Assets/WindowPositions/bottomLeftSixthTemplate.png",
            "ms-appx:///Assets/WindowPositions/bottomCenterSixthTemplate.png",
            "ms-appx:///Assets/WindowPositions/bottomRightSixthTemplate.png",
            "ms-appx:///Assets/WindowPositions/maximizeTemplate.png",
            "ms-appx:///Assets/WindowPositions/almostMaximizeTemplate.png",
            "ms-appx:///Assets/WindowPositions/maximizeHeightTemplate.png",
            "ms-appx:///Assets/WindowPositions/makeLargerTemplate.png",
            "ms-appx:///Assets/WindowPositions/makeSmallerTemplate.png",
            "ms-appx:///Assets/WindowPositions/centerTemplate.png",
            "ms-appx:///Assets/WindowPositions/restoreTemplate.png",
            "ms-appx:///Assets/WindowPositions/moveLeftTemplate.png",
            "ms-appx:///Assets/WindowPositions/moveRightTemplate.png",
            "ms-appx:///Assets/WindowPositions/moveUpTemplate.png",
            "ms-appx:///Assets/WindowPositions/moveDownTemplate.png",
            "ms-appx:///Assets/WindowPositions/nextDisplayTemplate.png",
            "ms-appx:///Assets/WindowPositions/prevDisplayTemplate.png",
        };

        private static readonly List<BitmapImage> _preloadedIcons = new();

        public static void PreloadMenuIcons()
        {
            if (_preloadedIcons.Count > 0) return;

            var dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
            if (dispatcherQueue == null)
            {
                Logger.Warning("TrayIconService", "无法获取 DispatcherQueue，跳过图标预加载");
                return;
            }

            _ = dispatcherQueue.TryEnqueue(async () =>
            {
                try
                {
                    var tcs = new System.Threading.Tasks.TaskCompletionSource();
                    var loadedCount = 0;
                    var totalCount = _iconPaths.Length;

                    foreach (var path in _iconPaths)
                    {
                        var bitmap = new BitmapImage(new Uri(path))
                        {
                            CreateOptions = BitmapCreateOptions.None
                        };

                        bitmap.ImageOpened += (s, e) =>
                        {
                            loadedCount++;
                            if (loadedCount == totalCount)
                            {
                                tcs.TrySetResult();
                            }
                        };

                        bitmap.ImageFailed += (s, e) =>
                        {
                            loadedCount++;
                            Logger.Warning("TrayIconService", $"图标加载失败: {path}");
                            if (loadedCount == totalCount)
                            {
                                tcs.TrySetResult();
                            }
                        };

                        _preloadedIcons.Add(bitmap);
                    }

                    await System.Threading.Tasks.Task.WhenAny(tcs.Task, System.Threading.Tasks.Task.Delay(3000));
                    Logger.Info("TrayIconService", $"预加载 {_preloadedIcons.Count} 个菜单图标完成");
                }
                catch (Exception ex)
                {
                    Logger.Error("TrayIconService", $"预加载菜单图标失败: {ex}");
                }
            });
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
                    void FixMenuLayout(object? s, object? _) => TryFixMenuFlyoutPresenterWidth(flyout);

                    flyout.Opening += (_, _) =>
                    {
                        // 先更新忽略菜单项（在焦点变化前获取活动窗口），再暂停跟踪
                        UpdateIgnoreMenuItem();
                        _lastActiveService?.PauseTracking();
                        // 首次打开时强制布局：LayoutUpdated 即时响应 + 延迟重试兜底（H.NotifyIcon SecondWindow 布局问题）
                        flyout.LayoutUpdated += FixMenuLayout;
                        _ = EnqueueFixMenuLayoutAsync(flyout);
                    };
                    flyout.Closed += (_, _) =>
                    {
                        flyout.LayoutUpdated -= FixMenuLayout;
                        _lastActiveService?.ResumeTracking();
                    };

                    DecorateItems(flyout.Items, shortcuts);
                }

                _taskbarIcon.ForceCreate(enablesEfficiencyMode: false);

                // 预加载菜单以解决首次打开布局挤压（SecondWindow 首次测量可能尚未稳定）
                _ = PrewarmContextMenuAsync();

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
                    sub.AccessKey = string.Empty;
                    DecorateItems(sub.Items, shortcuts);
                }
                else if (item is MenuFlyoutItem fi)
                {
                    fi.AccessKey = string.Empty;

                    if (fi.Tag is string tag)
                    {
                        if (tag == "IgnoreApp")
                        {
                            _ignoreAppMenuItem = fi;
                            fi.Command = _ignoreAppCommand;
                            continue;
                        }

                        var shortcutText = GetShortcutText(tag, shortcuts);
                        if (!string.IsNullOrEmpty(shortcutText))
                            fi.KeyboardAcceleratorTextOverride = shortcutText;

                        fi.Command = _menuActionCommand;
                        fi.CommandParameter = tag;
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
                // 托盘菜单显式选择，跳过循环模式，始终执行用户点击的操作
                _windowManager.Execute(action, forceDirectAction: true);
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

            // 使用 GetTargetWindow 而非 GetLastValidWindow，以便在缓存无效时回退到当前前台窗口
            var hwnd = _lastActiveService.GetTargetWindow();
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

        /// <summary>
        /// 多次预加载菜单，触发布局计算，避免首次右键时挤在一起。
        /// SecondWindow 模式下单次预热偶发失效，因此采用分阶段重试。
        /// </summary>
        private async Task PrewarmContextMenuAsync()
        {
            if (_taskbarIcon?.ContextFlyout is not MenuFlyout flyout) return;
            if (_contextMenuPrewarmed || _contextMenuPrewarming) return;
            _contextMenuPrewarming = true;

            var dispatcher = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
            if (dispatcher == null)
            {
                _contextMenuPrewarming = false;
                return;
            }

            try
            {
                // 分阶段预热：让 SecondWindow 的 XamlRoot/Presenter 在启动后有充分时间完成创建与测量
                foreach (var delayMs in new[] { 0, 120, 280, 520 })
                {
                    if (delayMs > 0) await Task.Delay(delayMs);

                    dispatcher.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () =>
                    {
                        try
                        {
                            var target = _taskbarIcon as Microsoft.UI.Xaml.FrameworkElement;
                            if (target == null) return;
                            if (flyout.IsOpen) return;

                            // 屏幕外显示触发布局测量
                            flyout.ShowAt(target, new global::Windows.Foundation.Point(-10000, -10000));
                            TryFixMenuFlyoutPresenterWidth(flyout);
                        }
                        catch { }
                    });

                    await Task.Delay(180);
                    dispatcher.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () =>
                    {
                        try { if (flyout.IsOpen) flyout.Hide(); } catch { }
                    });
                }

                _contextMenuPrewarmed = true;
            }
            finally
            {
                _contextMenuPrewarming = false;
            }
        }

        /// <summary>
        /// 尝试查找并设置 MenuFlyoutPresenter 宽度，修复 H.NotifyIcon SecondWindow 首次打开挤压。
        /// </summary>
        private void TryFixMenuFlyoutPresenterWidth(MenuFlyout flyout)
        {
            try
            {
                var xamlRoot = flyout.XamlRoot ?? _taskbarIcon?.XamlRoot ?? App.MainWindow?.Content?.XamlRoot;
                if (xamlRoot == null) return;

                foreach (var popup in VisualTreeHelper.GetOpenPopupsForXamlRoot(xamlRoot))
                {
                    var presenter = FindChildByType<MenuFlyoutPresenter>(popup.Child);
                    if (presenter != null)
                    {
                        presenter.MinWidth = 408;
                        presenter.Width = 408;
                        presenter.UpdateLayout();
                        break;
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// 延迟重试修复布局，兜底处理 SecondWindow 创建较慢的情况。
        /// </summary>
        private async Task EnqueueFixMenuLayoutAsync(MenuFlyout flyout)
        {
            foreach (var delayMs in new[] { 50, 100, 150, 200 })
            {
                await Task.Delay(delayMs);
                var dispatcher = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
                dispatcher.TryEnqueue(() => TryFixMenuFlyoutPresenterWidth(flyout));
            }
        }

        private static T? FindChildByType<T>(DependencyObject? parent) where T : DependencyObject
        {
            if (parent == null) return null;
            if (parent is T t) return t;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                var found = FindChildByType<T>(child);
                if (found != null) return found;
            }
            return null;
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
