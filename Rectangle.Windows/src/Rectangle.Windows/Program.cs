using Rectangle.Windows.Core;
using Rectangle.Windows.Services;
using Rectangle.Windows.Views;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.Win32;

namespace Rectangle.Windows;

internal static class Program
{
    private static NotifyIcon? _notifyIcon;
    private static ContextMenuStrip? _contextMenu;
    private static WindowManager? _windowManager;
    private static ConfigService? _configService;
    private static HotkeyManager? _hotkeyManager;
    private static SnapDetectionService? _snapDetectionService;
    private static SnapPreviewWindow? _snapPreviewWindow;
    private static SnappingManager? _snappingManager;
    private static LastActiveWindowService? _lastActiveWindowService;
    private static ToolStripMenuItem? _ignoreAppMenuItem;
    private static System.Windows.Forms.Timer? _updateTimer;

    public static ConfigService? ConfigService => _configService;
    public static HotkeyManager? HotkeyManager => _hotkeyManager;
    public static LastActiveWindowService? LastActiveWindowService => _lastActiveWindowService;

    private static System.Threading.Mutex? _mutex;

    // 导入 AttachConsole 用于在 WinExe 模式下输出到控制台
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool AttachConsole(int dwProcessId);
    private const int ATTACH_PARENT_PROCESS = -1;


    [STAThread]
    public static void Main()
    {
        // 附加到父进程的控制台（用于 dotnet run 时显示输出）
        AttachConsole(ATTACH_PARENT_PROCESS);

        // 注册进程退出事件，确保在进程退出时清理托盘图标
        AppDomain.CurrentDomain.ProcessExit += (s, e) => CleanupTrayIcon();

        // 注册 Ctrl+C 处理器
        Console.CancelKeyPress += (s, e) =>
        {
            e.Cancel = false; // 允许程序继续退出
            CleanupTrayIcon();
        };

        // 单实例检查
        const string mutexName = "Rectangle.Windows.SingleInstance";
        bool createdNew;
        _mutex = new System.Threading.Mutex(true, mutexName, out createdNew);
        
        if (!createdNew)
        {
            // 已有实例在运行，默默退出
            Logger.Warning("Program", "Rectangle 已经在运行中，退出。");
            return;
        }

        Logger.Info("Program", "Rectangle 已启动。");

        // 创建服务
        var win32 = new Win32WindowService();
        _configService = new ConfigService();
        var factory = new CalculatorFactory(_configService);
        var history = new WindowHistory();
        _windowManager = new WindowManager(win32, factory, history);
        
        // 创建活跃窗口跟踪服务
        _lastActiveWindowService = new LastActiveWindowService();
        _windowManager.SetLastActiveWindowService(_lastActiveWindowService);
        _windowManager.SetConfigService(_configService);

        // 创建布局管理器
        _layoutManager = new LayoutManager(_configService, win32);

        // 创建托盘菜单
        _contextMenu = CreateContextMenu();

        // 创建托盘图标（从嵌入式资源加载）
        System.Drawing.Icon? appIcon = LoadAppIcon();
        _notifyIcon = new NotifyIcon
        {
            Icon = appIcon ?? System.Drawing.SystemIcons.Application,
            ContextMenuStrip = _contextMenu,
            Text = "Rectangle",
            Visible = true
        };
        
        // 支持左键点击打开菜单
        _notifyIcon.MouseClick += NotifyIcon_MouseClick;
        
        // 菜单打开时暂停窗口跟踪，菜单关闭时恢复
        _contextMenu.Opened += (s, e) => _lastActiveWindowService?.PauseTracking();
        _contextMenu.Closed += (s, e) => _lastActiveWindowService?.ResumeTracking();
        
        // 定时更新"忽略 [应用名]"菜单项
        _updateTimer = new System.Windows.Forms.Timer { Interval = 500 };
        _updateTimer.Tick += (s, e) => UpdateIgnoreMenuItem();
        _updateTimer.Start();

        // 创建隐藏窗口来接收热键消息
        using var hiddenForm = new HiddenForm();
        hiddenForm.Show();
        _hotkeyManager = new HotkeyManager(hiddenForm.Handle, _windowManager, _configService.Load());

        // 创建拖拽吸附服务
        var win32Service = new Win32WindowService();
        _snapDetectionService = new SnapDetectionService(win32Service, _windowManager);
        _snapPreviewWindow = new SnapPreviewWindow();

        // 订阅吸附预览事件
        _snapDetectionService.SnapPreviewRequested += OnSnapPreviewRequested;
        _snapDetectionService.SnapPreviewHidden += OnSnapPreviewHidden;

        // 创建 SnappingManager（拖拽吸附管理器）
        _snappingManager = new SnappingManager(win32, _configService, history);
        _snappingManager.SetWindowManager(_windowManager);

        // 订阅 SnappingManager 事件
        _snappingManager.DragStarted += OnDragStarted;
        _snappingManager.DragEnded += OnDragEnded;
        _snappingManager.SnapTriggered += OnSnapTriggered;

        // 启用拖拽吸附
        _snappingManager.Enable();

        // 运行应用
        Application.Run();
        
        // 清理
        CleanupTrayIcon();
        _updateTimer?.Stop();
        _updateTimer?.Dispose();
        _snappingManager?.Dispose();
        _snapPreviewWindow?.Dispose();
        _snapDetectionService?.Dispose();
        _hotkeyManager?.Dispose();
        _lastActiveWindowService?.Dispose();
        _mutex?.ReleaseMutex();
        _mutex?.Dispose();
        Logger.Info("Program", "Rectangle 已退出。");
    }

    /// <summary>
    /// 清理托盘图标
    /// </summary>
    private static void CleanupTrayIcon()
    {
        if (_notifyIcon != null)
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
            _notifyIcon = null;
        }
    }

    private static System.Drawing.Icon? LoadAppIcon()
    {
        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        string resourceName = "Rectangle.Windows.Assets.AppIcon.png";
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream != null)
        {
            try
            {
                using var bmp = new System.Drawing.Bitmap(stream);
                var hIcon = bmp.GetHicon();
                var icon = System.Drawing.Icon.FromHandle(hIcon);
                var clonedIcon = (System.Drawing.Icon)icon.Clone();
                PInvoke.DestroyIcon((global::Windows.Win32.UI.WindowsAndMessaging.HICON)hIcon);
                return clonedIcon;
            }
            catch (Exception ex)
            {
                Logger.Error("Program", $"加载图标失败: {ex.Message}");
            }
        }
        
        // 如果没有嵌入式图标，生成一个简单的图标
        return GenerateDefaultIcon();
    }
    
    private static System.Drawing.Icon? GenerateDefaultIcon()
    {
        try
        {
            using var bmp = new System.Drawing.Bitmap(32, 32);
            using var g = System.Drawing.Graphics.FromImage(bmp);
            
            // 绘制一个简单的窗口图标
            g.Clear(System.Drawing.Color.FromArgb(0, 120, 212));
            using var pen = new System.Drawing.Pen(System.Drawing.Color.White, 2);
            
            // 绘制窗口边框
            g.DrawRectangle(pen, 4, 4, 23, 23);
            // 绘制分割线（表示窗口分割）
            g.DrawLine(pen, 16, 4, 16, 27);
            
            var hIcon = bmp.GetHicon();
            var icon = System.Drawing.Icon.FromHandle(hIcon);
            var clonedIcon = (System.Drawing.Icon)icon.Clone();
            PInvoke.DestroyIcon((global::Windows.Win32.UI.WindowsAndMessaging.HICON)hIcon);
            return clonedIcon;
        }
        catch (Exception ex)
        {
            Logger.Error("Program", $"生成默认图标失败: {ex.Message}");
            return null;
        }
    }

    private static void NotifyIcon_MouseClick(object? sender, MouseEventArgs e)
    {
        // 左键点击也显示菜单
        if (e.Button == MouseButtons.Left)
        {
            var method = typeof(NotifyIcon).GetMethod("ShowContextMenu", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            method?.Invoke(_notifyIcon, null);
        }
    }

    private static void UpdateIgnoreMenuItem()
    {
        if (_ignoreAppMenuItem == null || _lastActiveWindowService == null || _configService == null) return;

        var hwnd = _lastActiveWindowService.GetLastValidWindow();
        if (hwnd == 0)
        {
            _ignoreAppMenuItem.Text = "忽略 [无有效窗口]";
            _ignoreAppMenuItem.Enabled = false;
            return;
        }

        var processName = GetProcessNameFromWindow(hwnd);
        if (string.IsNullOrEmpty(processName))
        {
            _ignoreAppMenuItem.Text = "忽略 [未知应用]";
            _ignoreAppMenuItem.Enabled = false;
            return;
        }

        var config = _configService.Load();
        bool isIgnored = config.IgnoredApps.Contains(processName, StringComparer.OrdinalIgnoreCase);

        _ignoreAppMenuItem.Text = isIgnored ? $"取消忽略 {processName}" : $"忽略 {processName}";
        _ignoreAppMenuItem.Tag = processName;
        _ignoreAppMenuItem.Enabled = true;
    }

    private static void ToggleIgnoreApp(string processName)
    {
        if (_configService == null) return;

        var config = _configService.Load();
        bool isIgnored = config.IgnoredApps.Any(a => a.Equals(processName, StringComparison.OrdinalIgnoreCase));

        if (isIgnored)
        {
            config.IgnoredApps.RemoveAll(a => a.Equals(processName, StringComparison.OrdinalIgnoreCase));
            Logger.Info("Program", $"已从忽略列表移除: {processName}");
        }
        else
        {
            config.IgnoredApps.Add(processName);
            Logger.Info("Program", $"已添加到忽略列表: {processName}");
        }

        _configService.Save(config);
    }

    private static string GetProcessNameFromWindow(nint hwnd)
    {
        try
        {
            unsafe
            {
                uint processId;
                PInvoke.GetWindowThreadProcessId(new global::Windows.Win32.Foundation.HWND(hwnd), &processId);

                if (processId == 0) return string.Empty;

                using var process = System.Diagnostics.Process.GetProcessById((int)processId);
                return process.ProcessName;
            }
        }
        catch
        {
            return string.Empty;
        }
    }

    private static List<string> _recentActionTags = new();
    private static LayoutManager? _layoutManager;

    private static ContextMenuStrip CreateContextMenu()
    {
        var config = _configService?.Load();
        var shortcuts = config?.Shortcuts ?? new();
        var defaultShortcuts = ConfigService.GetDefaultShortcuts();
        var mergedShortcuts = new Dictionary<string, ShortcutConfig>(defaultShortcuts);
        foreach (var kvp in shortcuts) mergedShortcuts[kvp.Key] = kvp.Value;

        var menu = new Views.AcrylicContextMenu();

        // === 顶部动态菜单 ===
        // 搜索动作
        var searchItem = new ToolStripMenuItem("搜索动作...", null, (s, e) => ShowActionSearch())
        {
            ShortcutKeyDisplayString = ""
        };
        menu.Items.Add(searchItem);

        // 最近操作
        var recentMenu = new ToolStripMenuItem("最近操作") { Enabled = false };
        menu.Items.Add(recentMenu);
        _recentActionsSubMenu = recentMenu;

        // 收藏动作
        var favoriteMenu = new ToolStripMenuItem("收藏动作");
        PopulateFavoriteActionsMenu(favoriteMenu, mergedShortcuts);
        menu.Items.Add(favoriteMenu);

        // 布局预设
        var layoutsMenu = new ToolStripMenuItem("布局预设");
        PopulateLayoutsMenu(layoutsMenu);
        menu.Items.Add(layoutsMenu);

        // 配置菜单
        var configMenu = new ToolStripMenuItem("配置");
        configMenu.DropDownItems.Add(new ToolStripMenuItem("导出配置", null, (s, e) => ExportConfig()));
        configMenu.DropDownItems.Add(new ToolStripMenuItem("导入配置", null, (s, e) => ImportConfig()));
        menu.Items.Add(configMenu);

        menu.Items.Add(new ToolStripSeparator());

        // === 主要操作菜单 ===
        // 半屏
        AddMenuItem(menu, "左半屏", GetShortcutText(mergedShortcuts, "LeftHalf"), WindowAction.LeftHalf);
        AddMenuItem(menu, "右半屏", GetShortcutText(mergedShortcuts, "RightHalf"), WindowAction.RightHalf);
        AddMenuItem(menu, "中间半屏", GetShortcutText(mergedShortcuts, "CenterHalf"), WindowAction.CenterHalf);
        AddMenuItem(menu, "上半屏", GetShortcutText(mergedShortcuts, "TopHalf"), WindowAction.TopHalf);
        AddMenuItem(menu, "下半屏", GetShortcutText(mergedShortcuts, "BottomHalf"), WindowAction.BottomHalf);
        menu.Items.Add(new ToolStripSeparator());

        // 四角
        AddMenuItem(menu, "左上", GetShortcutText(mergedShortcuts, "TopLeft"), WindowAction.TopLeft);
        AddMenuItem(menu, "右上", GetShortcutText(mergedShortcuts, "TopRight"), WindowAction.TopRight);
        AddMenuItem(menu, "左下", GetShortcutText(mergedShortcuts, "BottomLeft"), WindowAction.BottomLeft);
        AddMenuItem(menu, "右下", GetShortcutText(mergedShortcuts, "BottomRight"), WindowAction.BottomRight);
        menu.Items.Add(new ToolStripSeparator());

        // 三分之一
        AddMenuItem(menu, "左首 1/3", GetShortcutText(mergedShortcuts, "FirstThird"), WindowAction.FirstThird);
        AddMenuItem(menu, "中间 1/3", GetShortcutText(mergedShortcuts, "CenterThird"), WindowAction.CenterThird);
        AddMenuItem(menu, "右首 1/3", GetShortcutText(mergedShortcuts, "LastThird"), WindowAction.LastThird);
        AddMenuItem(menu, "左侧 2/3", GetShortcutText(mergedShortcuts, "FirstTwoThirds"), WindowAction.FirstTwoThirds);
        AddMenuItem(menu, "中间 2/3", GetShortcutText(mergedShortcuts, "CenterTwoThirds"), WindowAction.CenterTwoThirds);
        AddMenuItem(menu, "右侧 2/3", GetShortcutText(mergedShortcuts, "LastTwoThirds"), WindowAction.LastTwoThirds);
        menu.Items.Add(new ToolStripSeparator());

        // 四等分子菜单
        var fourthsMenu = new ToolStripMenuItem("四等分");
        AddSubMenuItem(fourthsMenu, "左 1/4", GetShortcutText(mergedShortcuts, "FirstFourth"), WindowAction.FirstFourth);
        AddSubMenuItem(fourthsMenu, "中左 1/4", GetShortcutText(mergedShortcuts, "SecondFourth"), WindowAction.SecondFourth);
        AddSubMenuItem(fourthsMenu, "中右 1/4", GetShortcutText(mergedShortcuts, "ThirdFourth"), WindowAction.ThirdFourth);
        AddSubMenuItem(fourthsMenu, "右 1/4", GetShortcutText(mergedShortcuts, "LastFourth"), WindowAction.LastFourth);
        fourthsMenu.DropDownItems.Add(new ToolStripSeparator());
        AddSubMenuItem(fourthsMenu, "左 3/4", GetShortcutText(mergedShortcuts, "FirstThreeFourths"), WindowAction.FirstThreeFourths);
        AddSubMenuItem(fourthsMenu, "中间 3/4", GetShortcutText(mergedShortcuts, "CenterThreeFourths"), WindowAction.CenterThreeFourths);
        AddSubMenuItem(fourthsMenu, "右 3/4", GetShortcutText(mergedShortcuts, "LastThreeFourths"), WindowAction.LastThreeFourths);
        menu.Items.Add(fourthsMenu);

        // 六等分子菜单
        var sixthsMenu = new ToolStripMenuItem("六等分");
        AddSubMenuItem(sixthsMenu, "左上 1/6", GetShortcutText(mergedShortcuts, "TopLeftSixth"), WindowAction.TopLeftSixth);
        AddSubMenuItem(sixthsMenu, "上中 1/6", GetShortcutText(mergedShortcuts, "TopCenterSixth"), WindowAction.TopCenterSixth);
        AddSubMenuItem(sixthsMenu, "右上 1/6", GetShortcutText(mergedShortcuts, "TopRightSixth"), WindowAction.TopRightSixth);
        sixthsMenu.DropDownItems.Add(new ToolStripSeparator());
        AddSubMenuItem(sixthsMenu, "左下 1/6", GetShortcutText(mergedShortcuts, "BottomLeftSixth"), WindowAction.BottomLeftSixth);
        AddSubMenuItem(sixthsMenu, "下中 1/6", GetShortcutText(mergedShortcuts, "BottomCenterSixth"), WindowAction.BottomCenterSixth);
        AddSubMenuItem(sixthsMenu, "右下 1/6", GetShortcutText(mergedShortcuts, "BottomRightSixth"), WindowAction.BottomRightSixth);
        menu.Items.Add(sixthsMenu);

        // 垂直三等分子菜单
        var verticalThirdsMenu = new ToolStripMenuItem("垂直三等分");
        AddSubMenuItem(verticalThirdsMenu, "上部 1/3", GetShortcutText(mergedShortcuts, "TopVerticalThird"), WindowAction.TopVerticalThird);
        AddSubMenuItem(verticalThirdsMenu, "中间 1/3", GetShortcutText(mergedShortcuts, "MiddleVerticalThird"), WindowAction.MiddleVerticalThird);
        AddSubMenuItem(verticalThirdsMenu, "下部 1/3", GetShortcutText(mergedShortcuts, "BottomVerticalThird"), WindowAction.BottomVerticalThird);
        verticalThirdsMenu.DropDownItems.Add(new ToolStripSeparator());
        AddSubMenuItem(verticalThirdsMenu, "上部 2/3", GetShortcutText(mergedShortcuts, "TopVerticalTwoThirds"), WindowAction.TopVerticalTwoThirds);
        AddSubMenuItem(verticalThirdsMenu, "下部 2/3", GetShortcutText(mergedShortcuts, "BottomVerticalTwoThirds"), WindowAction.BottomVerticalTwoThirds);
        menu.Items.Add(verticalThirdsMenu);

        menu.Items.Add(new ToolStripSeparator());

        // 最大化
        AddMenuItem(menu, "最大化", GetShortcutText(mergedShortcuts, "Maximize"), WindowAction.Maximize);

        // 尺寸调整子菜单
        var resizeMenu = new ToolStripMenuItem("尺寸调整");
        AddSubMenuItem(resizeMenu, "接近最大化", GetShortcutText(mergedShortcuts, "AlmostMaximize"), WindowAction.AlmostMaximize);
        AddSubMenuItem(resizeMenu, "最大化高度", GetShortcutText(mergedShortcuts, "MaximizeHeight"), WindowAction.MaximizeHeight);
        AddSubMenuItem(resizeMenu, "放大", GetShortcutText(mergedShortcuts, "Larger"), WindowAction.Larger);
        AddSubMenuItem(resizeMenu, "缩小", GetShortcutText(mergedShortcuts, "Smaller"), WindowAction.Smaller);
        AddSubMenuItem(resizeMenu, "加宽", GetShortcutText(mergedShortcuts, "LargerWidth"), WindowAction.LargerWidth);
        AddSubMenuItem(resizeMenu, "减宽", GetShortcutText(mergedShortcuts, "SmallerWidth"), WindowAction.SmallerWidth);
        AddSubMenuItem(resizeMenu, "加高", GetShortcutText(mergedShortcuts, "LargerHeight"), WindowAction.LargerHeight);
        AddSubMenuItem(resizeMenu, "减高", GetShortcutText(mergedShortcuts, "SmallerHeight"), WindowAction.SmallerHeight);
        AddSubMenuItem(resizeMenu, "居中", GetShortcutText(mergedShortcuts, "Center"), WindowAction.Center);
        AddSubMenuItem(resizeMenu, "恢复", GetShortcutText(mergedShortcuts, "Restore"), WindowAction.Restore);
        menu.Items.Add(resizeMenu);

        // 窗口移动子菜单
        var moveMenu = new ToolStripMenuItem("窗口移动");
        AddSubMenuItem(moveMenu, "左移", GetShortcutText(mergedShortcuts, "MoveLeft"), WindowAction.MoveLeft);
        AddSubMenuItem(moveMenu, "右移", GetShortcutText(mergedShortcuts, "MoveRight"), WindowAction.MoveRight);
        AddSubMenuItem(moveMenu, "上移", GetShortcutText(mergedShortcuts, "MoveUp"), WindowAction.MoveUp);
        AddSubMenuItem(moveMenu, "下移", GetShortcutText(mergedShortcuts, "MoveDown"), WindowAction.MoveDown);
        moveMenu.DropDownItems.Add(new ToolStripSeparator());
        AddSubMenuItem(moveMenu, "下一个显示器", GetShortcutText(mergedShortcuts, "NextDisplay"), WindowAction.NextDisplay);
        AddSubMenuItem(moveMenu, "上一个显示器", GetShortcutText(mergedShortcuts, "PreviousDisplay"), WindowAction.PreviousDisplay);
        menu.Items.Add(moveMenu);

        menu.Items.Add(new ToolStripSeparator());

        // 撤销/重做
        AddMenuItem(menu, "撤销", GetShortcutText(mergedShortcuts, "Undo"), WindowAction.Undo);
        AddMenuItem(menu, "重做", GetShortcutText(mergedShortcuts, "Redo"), WindowAction.Redo);
        menu.Items.Add(new ToolStripSeparator());

        // 忽略应用（动态）
        _ignoreAppMenuItem = new ToolStripMenuItem("忽略 [无有效窗口]") { Enabled = false };
        _ignoreAppMenuItem.Click += (s, e) =>
        {
            if (_ignoreAppMenuItem.Tag is string processName && !string.IsNullOrEmpty(processName))
            {
                ToggleIgnoreApp(processName);
            }
        };
        menu.Items.Add(_ignoreAppMenuItem);
        menu.Items.Add(new ToolStripSeparator());

        // 偏好设置与退出
        menu.Items.Add(new ToolStripMenuItem("偏好设置...", null, (s, e) => OpenSettings()));
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(new ToolStripMenuItem("退出 Rectangle", null, (s, e) => Application.Exit()));

        return menu;
    }

    private static ToolStripMenuItem? _recentActionsSubMenu;

    private static void PopulateFavoriteActionsMenu(ToolStripMenuItem menu, Dictionary<string, ShortcutConfig> shortcuts)
    {
        menu.DropDownItems.Clear();
        var favorites = _configService?.Load().FavoriteTrayActions ?? new List<string>();

        if (favorites.Count == 0)
        {
            menu.DropDownItems.Add(new ToolStripMenuItem("暂无收藏（在配置中设置）") { Enabled = false });
            return;
        }

        foreach (var tag in favorites)
        {
            if (!TryParseActionTag(tag, out var action)) continue;
            AddSubMenuItem(menu, GetActionDisplayName(action), GetShortcutText(shortcuts, tag), action);
        }
    }

    private static void PopulateLayoutsMenu(ToolStripMenuItem menu)
    {
        menu.DropDownItems.Clear();
        menu.DropDownItems.Add(new ToolStripMenuItem("保存当前布局", null, async (s, e) => await SaveCurrentLayoutAsync()));
        menu.DropDownItems.Add(new ToolStripMenuItem("恢复最近布局", null, async (s, e) => await RestoreLatestLayoutAsync()));
        menu.DropDownItems.Add(new ToolStripSeparator());

        // 异步加载布局列表
        Task.Run(async () =>
        {
            try
            {
                if (_layoutManager == null) return;
                var layouts = await _layoutManager.GetLayoutsAsync();
                var recentLayouts = layouts.OrderByDescending(l => l.CreatedAt).Take(8);

                foreach (var layout in recentLayouts)
                {
                    var item = new ToolStripMenuItem($"{layout.Name} ({layout.CreatedAt:MM-dd HH:mm})", null, async (s, e) =>
                    {
                        try { await _layoutManager.RestoreLayoutAsync(layout.Id); }
                        catch (Exception ex) { Logger.Warning("Program", $"恢复布局失败: {ex.Message}"); }
                    });
                    menu.DropDownItems.Add(item);
                }
            }
            catch { }
        });
    }

    private static async Task SaveCurrentLayoutAsync()
    {
        try
        {
            if (_layoutManager == null) return;
            var name = $"布局 {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
            await _layoutManager.SaveCurrentLayoutAsync(name);
            _notifyIcon?.ShowBalloonTip(2000, "Rectangle", $"已保存布局: {name}", ToolTipIcon.Info);
        }
        catch (Exception ex)
        {
            Logger.Warning("Program", $"保存布局失败: {ex.Message}");
        }
    }

    private static async Task RestoreLatestLayoutAsync()
    {
        try
        {
            if (_layoutManager == null) return;
            var layouts = await _layoutManager.GetLayoutsAsync();
            var latest = layouts.OrderByDescending(l => l.CreatedAt).FirstOrDefault();
            if (latest == null)
            {
                _notifyIcon?.ShowBalloonTip(2000, "Rectangle", "暂无可恢复布局", ToolTipIcon.Info);
                return;
            }
            await _layoutManager.RestoreLayoutAsync(latest.Id);
            _notifyIcon?.ShowBalloonTip(2000, "Rectangle", $"已恢复布局: {latest.Name}", ToolTipIcon.Info);
        }
        catch (Exception ex)
        {
            Logger.Warning("Program", $"恢复布局失败: {ex.Message}");
        }
    }

    private static void ExportConfig()
    {
        try
        {
            var path = _configService?.ExportToFile();
            _notifyIcon?.ShowBalloonTip(2000, "Rectangle", $"配置已导出到: {Path.GetFileName(path)}", ToolTipIcon.Info);
        }
        catch (Exception ex)
        {
            Logger.Warning("Program", $"导出配置失败: {ex.Message}");
        }
    }

    private static void ImportConfig()
    {
        try
        {
            var ok = _configService?.ImportFromFile() ?? false;
            _notifyIcon?.ShowBalloonTip(2000, "Rectangle", ok ? "配置导入成功" : "未找到 config.import.json", ToolTipIcon.Info);
        }
        catch (Exception ex)
        {
            Logger.Warning("Program", $"导入配置失败: {ex.Message}");
        }
    }

    private static void ShowActionSearch()
    {
        var form = new Views.ActionSearchForm(_windowManager!, _configService!);
        form.Show();
    }

    private static bool TryParseActionTag(string tag, out WindowAction action)
    {
        return Enum.TryParse<WindowAction>(tag, out action);
    }

    private static string GetActionDisplayName(WindowAction action)
    {
        return action switch
        {
            WindowAction.LeftHalf => "左半屏",
            WindowAction.RightHalf => "右半屏",
            WindowAction.CenterHalf => "中间半屏",
            WindowAction.TopHalf => "上半屏",
            WindowAction.BottomHalf => "下半屏",
            WindowAction.TopLeft => "左上",
            WindowAction.TopRight => "右上",
            WindowAction.BottomLeft => "左下",
            WindowAction.BottomRight => "右下",
            WindowAction.FirstThird => "左首 1/3",
            WindowAction.CenterThird => "中间 1/3",
            WindowAction.LastThird => "右首 1/3",
            WindowAction.FirstTwoThirds => "左侧 2/3",
            WindowAction.CenterTwoThirds => "中间 2/3",
            WindowAction.LastTwoThirds => "右侧 2/3",
            WindowAction.Maximize => "最大化",
            WindowAction.AlmostMaximize => "接近最大化",
            WindowAction.MaximizeHeight => "最大化高度",
            WindowAction.Larger => "放大",
            WindowAction.Smaller => "缩小",
            WindowAction.Center => "居中",
            WindowAction.Restore => "恢复",
            WindowAction.NextDisplay => "下一个显示器",
            WindowAction.PreviousDisplay => "上一个显示器",
            WindowAction.MoveLeft => "左移",
            WindowAction.MoveRight => "右移",
            WindowAction.MoveUp => "上移",
            WindowAction.MoveDown => "下移",
            WindowAction.Undo => "撤销",
            WindowAction.Redo => "重做",
            _ => action.ToString()
        };
    }

    private static void AddSubMenuItem(ToolStripMenuItem parent, string text, string shortcut, WindowAction action)
    {
        var icon = MenuIconGenerator.GenerateIcon(action);
        var item = new ToolStripMenuItem(text, icon, (s, e) =>
        {
            _windowManager?.Execute(action, forceDirectAction: true);
            RecordRecentAction(action.ToString());
        });
        if (!string.IsNullOrEmpty(shortcut))
        {
            item.ShortcutKeyDisplayString = shortcut;
        }
        parent.DropDownItems.Add(item);
    }

    private static void RecordRecentAction(string tag)
    {
        _recentActionTags.RemoveAll(t => t.Equals(tag, StringComparison.OrdinalIgnoreCase));
        _recentActionTags.Insert(0, tag);
        if (_recentActionTags.Count > 32)
        {
            _recentActionTags.RemoveRange(32, _recentActionTags.Count - 32);
        }
    }
    
    private static void AddMenuItem(ContextMenuStrip menu, string text, string shortcut, WindowAction action)
    {
        var icon = MenuIconGenerator.GenerateIcon(action);
        var item = new ToolStripMenuItem(text, icon, (s, e) =>
        {
            _windowManager?.Execute(action, forceDirectAction: true);
            RecordRecentAction(action.ToString());
        });
        if (!string.IsNullOrEmpty(shortcut))
        {
            item.ShortcutKeyDisplayString = shortcut;
        }
        menu.Items.Add(item);
    }

    private static string GetShortcutText(Dictionary<string, ShortcutConfig> shortcuts, string actionName)
    {
        if (!shortcuts.TryGetValue(actionName, out var config) || !config.Enabled || config.KeyCode <= 0)
            return string.Empty;
        
        var parts = new System.Collections.Generic.List<string>();
        
        if ((config.ModifierFlags & 0x0002) != 0) parts.Add("Ctrl");
        if ((config.ModifierFlags & 0x0001) != 0) parts.Add("Alt");
        if ((config.ModifierFlags & 0x0004) != 0) parts.Add("Shift");
        if ((config.ModifierFlags & 0x0008) != 0) parts.Add("Win");
        
        parts.Add(GetKeyName(config.KeyCode));
        
        return string.Join("+", parts);
    }
    
    private static string GetKeyName(int vk)
    {
        return vk switch
        {
            // 方向键
            0x25 => "←",
            0x26 => "↑",
            0x27 => "→",
            0x28 => "↓",
            // 功能键
            0x0D => "Enter",
            0x08 => "Backspace",
            0x2E => "Delete",
            0x20 => "Space",
            0x1B => "Esc",
            0x09 => "Tab",
            // 字母键
            0x41 => "A", 0x42 => "B", 0x43 => "C", 0x44 => "D",
            0x45 => "E", 0x46 => "F", 0x47 => "G", 0x48 => "H",
            0x49 => "I", 0x4A => "J", 0x4B => "K", 0x4C => "L",
            0x4D => "M", 0x4E => "N", 0x4F => "O", 0x50 => "P",
            0x51 => "Q", 0x52 => "R", 0x53 => "S", 0x54 => "T",
            0x55 => "U", 0x56 => "V", 0x57 => "W", 0x58 => "X",
            0x59 => "Y", 0x5A => "Z",
            // 数字键
            0x30 => "0", 0x31 => "1", 0x32 => "2", 0x33 => "3",
            0x34 => "4", 0x35 => "5", 0x36 => "6", 0x37 => "7",
            0x38 => "8", 0x39 => "9",
            // 符号键
            0xBB => "=",
            0xBD => "-",
            0xBC => ",",
            0xBE => ".",
            0xBF => "/",
            0xBA => ";",
            0xDE => "'",
            0xDB => "[",
            0xDD => "]",
            0xDC => "\\",
            0xC0 => "`",
            // F 键
            0x70 => "F1", 0x71 => "F2", 0x72 => "F3", 0x73 => "F4",
            0x74 => "F5", 0x75 => "F6", 0x76 => "F7", 0x77 => "F8",
            0x78 => "F9", 0x79 => "F10", 0x7A => "F11", 0x7B => "F12",
            _ when vk > 0 => $"0x{vk:X}",
            _ => ""
        };
    }

    private static void OpenSettings()
    {
        var settingsForm = new Views.SettingsForm();
        settingsForm.ShowDialog();
    }

    private static readonly Win32WindowService _previewWin32Service = new();
    private static readonly CalculatorFactory _previewCalculatorFactory = new();

    private static void OnSnapPreviewRequested(WindowAction? action)
    {
        if (action.HasValue && _windowManager != null)
        {
            var hwnd = PInvoke.GetForegroundWindow();
            if (hwnd.Value != 0)
            {
                var workArea = _previewWin32Service.GetWorkAreaFromWindow((nint)hwnd.Value);
                var calculator = _previewCalculatorFactory.GetCalculator(action.Value);
                if (calculator != null)
                {
                    var rect = calculator.Calculate(workArea, default, action.Value);
                    _snapPreviewWindow?.ShowPreview(rect);
                }
            }
        }
        else
        {
            _snapPreviewWindow?.HidePreview();
        }
    }

    private static void OnSnapPreviewHidden()
    {
        _snapPreviewWindow?.HidePreview();
    }

    // SnappingManager 事件处理
    private static void OnDragStarted(object? sender, EventArgs e)
    {
        Logger.Info("Program", "拖拽开始");
    }

    private static void OnDragEnded(object? sender, EventArgs e)
    {
        Logger.Info("Program", "拖拽结束");
        // 隐藏预览窗口
        FootprintWindow.Instance.HideImmediate();
    }

    private static void OnSnapTriggered(object? sender, SnapEventArgs e)
    {
        Logger.Info("Program", $"吸附触发: {e.Action}");
    }

    // 隐藏窗口类，用于接收 WM_HOTKEY 消息
    private class HiddenForm : Form
    {
        public HiddenForm()
        {
            Opacity = 0;
            ShowInTaskbar = false;
            WindowState = FormWindowState.Minimized;
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_HOTKEY = 0x0312;
            if (m.Msg == WM_HOTKEY)
            {
                int id = m.WParam.ToInt32();
                _hotkeyManager?.HandleHotKey(id);
            }
            base.WndProc(ref m);
        }
    }
}
