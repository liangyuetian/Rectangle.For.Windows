using Rectangle.Windows.Core;
using Rectangle.Windows.Services;
using Rectangle.Windows.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
    private static LastActiveWindowService? _lastActiveWindowService;
    private static ToolStripMenuItem? _ignoreAppMenuItem;

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
            Console.WriteLine("Rectangle 已经在运行中，退出。");
            return;
        }

        Console.WriteLine("Rectangle 已启动。");

        // 创建服务
        var win32 = new Win32WindowService();
        var factory = new CalculatorFactory();
        var history = new WindowHistory();
        _windowManager = new WindowManager(win32, factory, history);
        _configService = new ConfigService();
        
        // 创建活跃窗口跟踪服务
        _lastActiveWindowService = new LastActiveWindowService();
        _windowManager.SetLastActiveWindowService(_lastActiveWindowService);
        _windowManager.SetConfigService(_configService);

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
        
        // 定时更新"忽略 [应用名]"菜单项
        var updateTimer = new System.Windows.Forms.Timer { Interval = 500 };
        updateTimer.Tick += (s, e) => UpdateIgnoreMenuItem();
        updateTimer.Start();

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

        // 运行应用
        Application.Run();
        
        // 清理
        CleanupTrayIcon();
        _snapDetectionService?.Dispose();
        _hotkeyManager?.Dispose();
        _lastActiveWindowService?.Dispose();
        _mutex?.ReleaseMutex();
        _mutex?.Dispose();
        Console.WriteLine("Rectangle 已退出。");
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
                PInvoke.DestroyIcon((Windows.Win32.UI.WindowsAndMessaging.HICON)hIcon);
                return clonedIcon;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Program] 加载图标失败: {ex.Message}");
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
            PInvoke.DestroyIcon((Windows.Win32.UI.WindowsAndMessaging.HICON)hIcon);
            return clonedIcon;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Program] 生成默认图标失败: {ex.Message}");
            return null;
        }
    }

    private static System.Drawing.Image? LoadMenuIcon(string iconName)
    {
        // 先尝试从嵌入式资源加载
        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        string resourceName = $"Rectangle.Windows.Assets.WindowPositions.{iconName}";
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream != null)
        {
            try { return System.Drawing.Image.FromStream(stream); }
            catch { }
        }
        return null;
    }

    private static System.Drawing.Image? GetMenuIcon(WindowAction action)
    {
        return Views.MenuIconGenerator.GenerateIcon(action);
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
            Console.WriteLine($"[Program] 已从忽略列表移除: {processName}");
        }
        else
        {
            config.IgnoredApps.Add(processName);
            Console.WriteLine($"[Program] 已添加到忽略列表: {processName}");
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

    private static ContextMenuStrip CreateContextMenu()
    {
        var menu = new Views.AcrylicContextMenu();
        
        // 加载配置获取快捷键
        var shortcuts = _configService?.Load()?.Shortcuts ?? new();
        var defaultShortcuts = ConfigService.GetDefaultShortcuts();
        
        // 合并默认快捷键和用户配置
        var mergedShortcuts = new Dictionary<string, ShortcutConfig>(defaultShortcuts);
        foreach (var kvp in shortcuts)
        {
            mergedShortcuts[kvp.Key] = kvp.Value;
        }

        // 忽略 [应用名] - 动态菜单项
        _ignoreAppMenuItem = new ToolStripMenuItem("忽略 [无有效窗口]") { Enabled = false };
        _ignoreAppMenuItem.Click += (s, e) =>
        {
            if (_ignoreAppMenuItem.Tag is string processName && !string.IsNullOrEmpty(processName))
            {
                ToggleIgnoreApp(processName);
            }
        };
        _ignoreAppMenuItem.ForeColor = System.Drawing.Color.White;
        menu.Items.Add(_ignoreAppMenuItem);
        menu.Items.Add(new ToolStripSeparator());

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

        // 最大化与缩放
        AddMenuItem(menu, "最大化", GetShortcutText(mergedShortcuts, "Maximize"), WindowAction.Maximize);
        AddMenuItem(menu, "接近最大化", GetShortcutText(mergedShortcuts, "AlmostMaximize"), WindowAction.AlmostMaximize);
        AddMenuItem(menu, "最大化高度", GetShortcutText(mergedShortcuts, "MaximizeHeight"), WindowAction.MaximizeHeight);
        AddMenuItem(menu, "放大", GetShortcutText(mergedShortcuts, "Larger"), WindowAction.Larger);
        AddMenuItem(menu, "缩小", GetShortcutText(mergedShortcuts, "Smaller"), WindowAction.Smaller);
        AddMenuItem(menu, "居中", GetShortcutText(mergedShortcuts, "Center"), WindowAction.Center);
        AddMenuItem(menu, "恢复", GetShortcutText(mergedShortcuts, "Restore"), WindowAction.Restore);
        menu.Items.Add(new ToolStripSeparator());

        // 显示器
        AddMenuItem(menu, "下一个显示器", GetShortcutText(mergedShortcuts, "NextDisplay"), WindowAction.NextDisplay);
        AddMenuItem(menu, "上一个显示器", GetShortcutText(mergedShortcuts, "PreviousDisplay"), WindowAction.PreviousDisplay);
        menu.Items.Add(new ToolStripSeparator());

        // 设置与退出
        var settingsItem = new ToolStripMenuItem("偏好设置...", null, (s, e) => OpenSettings());
        settingsItem.ForeColor = System.Drawing.Color.White;
        menu.Items.Add(settingsItem);
        menu.Items.Add(new ToolStripSeparator());
        
        var exitItem = new ToolStripMenuItem("退出 Rectangle", null, (s, e) => Application.Exit());
        exitItem.ForeColor = System.Drawing.Color.White;
        menu.Items.Add(exitItem);

        return menu;
    }
    
    private static void AddMenuItem(ContextMenuStrip menu, string text, string shortcut, WindowAction action)
    {
        var icon = GetMenuIcon(action);
        var item = new ToolStripMenuItem(text, icon, (s, e) => _windowManager?.Execute(action));
        if (!string.IsNullOrEmpty(shortcut))
        {
            item.ShortcutKeyDisplayString = shortcut;
        }
        item.ForeColor = System.Drawing.Color.White;
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

    private static void OnSnapPreviewRequested(WindowAction? action)
    {
        if (action.HasValue && _windowManager != null)
        {
            var hwnd = PInvoke.GetForegroundWindow();
            if (hwnd.Value != 0)
            {
                var win32 = new Win32WindowService();
                var workArea = win32.GetWorkAreaFromWindow((nint)hwnd.Value);
                var factory = new CalculatorFactory();
                var calculator = factory.GetCalculator(action.Value);
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
