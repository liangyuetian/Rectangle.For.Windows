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
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Rectangle.Windows.WinUI.Services
{
    public class TrayIconService : IDisposable
    {
        private TaskbarIcon? _taskbarIcon;
        private readonly WindowManager _windowManager;
        private readonly Action _showSettingsCallback;
        private readonly Action _showActionSearchCallback;
        private readonly ConfigService _configService;
        private readonly LayoutManager _layoutManager;
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
        private MenuFlyoutSubItem? _recentActionsSubMenu;
        private MenuFlyoutSubItem? _favoriteActionsSubMenu;
        private MenuFlyoutSubItem? _layoutsSubMenu;
        private MenuFlyoutSubItem? _configSubMenu;
        private static readonly List<string> _recentActionTags = new();

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
            ["TopVerticalThird"]   = WindowAction.TopVerticalThird,
            ["MiddleVerticalThird"]= WindowAction.MiddleVerticalThird,
            ["BottomVerticalThird"]= WindowAction.BottomVerticalThird,
            ["TopVerticalTwoThirds"] = WindowAction.TopVerticalTwoThirds,
            ["BottomVerticalTwoThirds"] = WindowAction.BottomVerticalTwoThirds,
            ["Maximize"]           = WindowAction.Maximize,
            ["AlmostMaximize"]     = WindowAction.AlmostMaximize,
            ["MaximizeHeight"]     = WindowAction.MaximizeHeight,
            ["Larger"]             = WindowAction.Larger,
            ["Smaller"]            = WindowAction.Smaller,
            ["LargerWidth"]        = WindowAction.LargerWidth,
            ["SmallerWidth"]       = WindowAction.SmallerWidth,
            ["LargerHeight"]       = WindowAction.LargerHeight,
            ["SmallerHeight"]      = WindowAction.SmallerHeight,
            ["Center"]             = WindowAction.Center,
            ["Restore"]            = WindowAction.Restore,
            ["MoveLeft"]           = WindowAction.MoveLeft,
            ["MoveRight"]          = WindowAction.MoveRight,
            ["MoveUp"]             = WindowAction.MoveUp,
            ["MoveDown"]           = WindowAction.MoveDown,
            ["NextDisplay"]        = WindowAction.NextDisplay,
            ["PreviousDisplay"]    = WindowAction.PreviousDisplay,
            ["Undo"]               = WindowAction.Undo,
            ["Redo"]               = WindowAction.Redo,
        };

        public TrayIconService(WindowManager windowManager, Action showSettingsCallback, Action showActionSearchCallback,
                               ConfigService configService, LastActiveWindowService? lastActiveService = null)
        {
            _windowManager = windowManager;
            _showSettingsCallback = showSettingsCallback;
            _showActionSearchCallback = showActionSearchCallback;
            _configService = configService;
            _layoutManager = new LayoutManager(configService, new Win32WindowService());
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
            "ms-appx:///Assets/WindowPositions/topThirdTemplate.png",
            "ms-appx:///Assets/WindowPositions/centerThirdHorizontalTemplate.png",
            "ms-appx:///Assets/WindowPositions/bottomThirdTemplate.png",
            "ms-appx:///Assets/WindowPositions/topTwoThirdsTemplate.png",
            "ms-appx:///Assets/WindowPositions/bottomTwoThirdsTemplate.png",
            "ms-appx:///Assets/WindowPositions/maximizeTemplate.png",
            "ms-appx:///Assets/WindowPositions/almostMaximizeTemplate.png",
            "ms-appx:///Assets/WindowPositions/maximizeHeightTemplate.png",
            "ms-appx:///Assets/WindowPositions/makeLargerTemplate.png",
            "ms-appx:///Assets/WindowPositions/makeSmallerTemplate.png",
            "ms-appx:///Assets/WindowPositions/largerWidthTemplate.png",
            "ms-appx:///Assets/WindowPositions/smallerWidthTemplate.png",
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
                        PopulateDynamicSections(flyout);
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
            var config = _configService.Load();
            var visible = new HashSet<string>(config.TrayVisibleActions ?? [], StringComparer.OrdinalIgnoreCase);
            var useFilter = visible.Count > 0;

            foreach (var item in items)
            {
                if (item is MenuFlyoutSubItem sub)
                {
                    sub.AccessKey = string.Empty;
                    if (sub.Tag is string subTag)
                    {
                        if (subTag == "RecentActions") _recentActionsSubMenu = sub;
                        else if (subTag == "FavoriteActions") _favoriteActionsSubMenu = sub;
                        else if (subTag == "LayoutsMenu") _layoutsSubMenu = sub;
                        else if (subTag == "ConfigMenu") _configSubMenu = sub;
                    }
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

                        if (useFilter && _tagToAction.ContainsKey(tag) && !visible.Contains(tag))
                        {
                            fi.Visibility = Visibility.Collapsed;
                        }
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

            if (HandleSpecialMenuAction(tag))
                return;

            Logger.Info("TrayIconService", $"菜单项 Command 执行: {tag}");
            if (_tagToAction.TryGetValue(tag, out var action))
            {
                // 托盘菜单显式选择，跳过循环模式，始终执行用户点击的操作
                _windowManager.Execute(action, forceDirectAction: true);
                RecordRecentAction(tag);
                TryShowActionNotification(tag);
            }
            else
            {
                Logger.Warning("TrayIconService", $"未找到对应的动作: {tag}");
            }
        }

        private bool HandleSpecialMenuAction(string tag)
        {
            switch (tag)
            {
                case "ActionSearch":
                    _showActionSearchCallback();
                    return true;
                case "SaveCurrentLayout":
                    _ = SaveCurrentLayoutAsync();
                    return true;
                case "RestoreLatestLayout":
                    _ = RestoreLatestLayoutAsync();
                    return true;
                case "ExportConfig":
                    _ = ExportConfigAsync();
                    return true;
                case "ImportConfig":
                    _ = ImportConfigAsync();
                    return true;
            }

            if (tag.StartsWith("RestoreLayout:", StringComparison.Ordinal))
            {
                var id = tag["RestoreLayout:".Length..];
                if (!string.IsNullOrWhiteSpace(id))
                    _ = RestoreLayoutByIdAsync(id);
                return true;
            }

            return false;
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

        private void PopulateDynamicSections(MenuFlyout flyout)
        {
            try
            {
                PopulateRecentActionsMenu();
                PopulateFavoriteActionsMenu();
                _ = PopulateLayoutsMenuAsync();
                PopulateConfigMenu();
            }
            catch (Exception ex)
            {
                Logger.Warning("TrayIconService", $"动态菜单构建失败: {ex.Message}");
            }
        }

        private void PopulateRecentActionsMenu()
        {
            if (_recentActionsSubMenu == null) return;
            _recentActionsSubMenu.Items.Clear();

            var limit = Math.Max(1, _configService.Load().RecentActionLimit);
            var tags = _recentActionTags.Take(limit).ToList();
            if (tags.Count == 0)
            {
                _recentActionsSubMenu.Items.Add(new MenuFlyoutItem { Text = "暂无记录", IsEnabled = false });
                return;
            }

            var shortcuts = LoadShortcuts();
            foreach (var tag in tags)
            {
                var item = new MenuFlyoutItem { Text = tag, Command = _menuActionCommand, CommandParameter = tag };
                var shortcutText = GetShortcutText(tag, shortcuts);
                if (!string.IsNullOrWhiteSpace(shortcutText)) item.KeyboardAcceleratorTextOverride = shortcutText;
                _recentActionsSubMenu.Items.Add(item);
            }
        }

        private void PopulateFavoriteActionsMenu()
        {
            if (_favoriteActionsSubMenu == null) return;
            _favoriteActionsSubMenu.Items.Clear();

            var favorites = _configService.Load().FavoriteTrayActions ?? [];
            if (favorites.Count == 0)
            {
                _favoriteActionsSubMenu.Items.Add(new MenuFlyoutItem { Text = "暂无收藏（在配置中设置）", IsEnabled = false });
                return;
            }

            var shortcuts = LoadShortcuts();
            foreach (var tag in favorites)
            {
                if (!_tagToAction.ContainsKey(tag)) continue;
                var item = new MenuFlyoutItem { Text = tag, Command = _menuActionCommand, CommandParameter = tag };
                var shortcutText = GetShortcutText(tag, shortcuts);
                if (!string.IsNullOrWhiteSpace(shortcutText)) item.KeyboardAcceleratorTextOverride = shortcutText;
                _favoriteActionsSubMenu.Items.Add(item);
            }
        }

        private async Task PopulateLayoutsMenuAsync()
        {
            if (_layoutsSubMenu == null) return;
            _layoutsSubMenu.Items.Clear();
            _layoutsSubMenu.Items.Add(new MenuFlyoutItem { Text = "保存当前布局", Command = _menuActionCommand, CommandParameter = "SaveCurrentLayout" });
            _layoutsSubMenu.Items.Add(new MenuFlyoutItem { Text = "恢复最近布局", Command = _menuActionCommand, CommandParameter = "RestoreLatestLayout" });
            _layoutsSubMenu.Items.Add(new MenuFlyoutSeparator());

            var layouts = await _layoutManager.GetLayoutsAsync();
            foreach (var layout in layouts.OrderByDescending(l => l.CreatedAt).Take(8))
            {
                _layoutsSubMenu.Items.Add(new MenuFlyoutItem
                {
                    Text = $"{layout.Name} ({layout.CreatedAt:MM-dd HH:mm})",
                    Command = _menuActionCommand,
                    CommandParameter = $"RestoreLayout:{layout.Id}"
                });
            }
        }

        private void PopulateConfigMenu()
        {
            if (_configSubMenu == null) return;
            _configSubMenu.Items.Clear();
            _configSubMenu.Items.Add(new MenuFlyoutItem { Text = "导出配置", Command = _menuActionCommand, CommandParameter = "ExportConfig" });
            _configSubMenu.Items.Add(new MenuFlyoutItem { Text = "导入配置", Command = _menuActionCommand, CommandParameter = "ImportConfig" });
        }

        private void RecordRecentAction(string tag)
        {
            _recentActionTags.RemoveAll(t => t.Equals(tag, StringComparison.OrdinalIgnoreCase));
            _recentActionTags.Insert(0, tag);
            if (_recentActionTags.Count > 32)
            {
                _recentActionTags.RemoveRange(32, _recentActionTags.Count - 32);
            }
        }

        private void TryShowActionNotification(string tag)
        {
            var config = _configService.Load();
            if (!config.EnableActionNotification) return;
            _taskbarIcon?.ShowNotification("Rectangle", $"已执行动作: {tag}");
        }

        private async Task SaveCurrentLayoutAsync()
        {
            try
            {
                var name = $"布局 {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
                await _layoutManager.SaveCurrentLayoutAsync(name);
                _taskbarIcon?.ShowNotification("Rectangle", $"已保存布局: {name}");
            }
            catch (Exception ex)
            {
                Logger.Warning("TrayIconService", $"保存布局失败: {ex.Message}");
            }
        }

        private async Task RestoreLatestLayoutAsync()
        {
            try
            {
                var latest = (await _layoutManager.GetLayoutsAsync()).OrderByDescending(l => l.CreatedAt).FirstOrDefault();
                if (latest == null)
                {
                    _taskbarIcon?.ShowNotification("Rectangle", "暂无可恢复布局");
                    return;
                }
                await _layoutManager.RestoreLayoutAsync(latest.Id);
                _taskbarIcon?.ShowNotification("Rectangle", $"已恢复布局: {latest.Name}");
            }
            catch (Exception ex)
            {
                Logger.Warning("TrayIconService", $"恢复布局失败: {ex.Message}");
            }
        }

        private async Task RestoreLayoutByIdAsync(string id)
        {
            try { await _layoutManager.RestoreLayoutAsync(id); }
            catch (Exception ex) { Logger.Warning("TrayIconService", $"恢复布局失败: {ex.Message}"); }
        }

        private async Task ExportConfigAsync()
        {
            try
            {
                var path = await _configService.ExportToFileAsync();
                _taskbarIcon?.ShowNotification("Rectangle", $"配置已导出到: {Path.GetFileName(path)}");
            }
            catch (Exception ex)
            {
                Logger.Warning("TrayIconService", $"导出配置失败: {ex.Message}");
            }
        }

        private async Task ImportConfigAsync()
        {
            try
            {
                var ok = await _configService.ImportFromFileAsync();
                _taskbarIcon?.ShowNotification("Rectangle", ok ? "配置导入成功" : "未找到 config.import.json");
            }
            catch (Exception ex)
            {
                Logger.Warning("TrayIconService", $"导入配置失败: {ex.Message}");
            }
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
