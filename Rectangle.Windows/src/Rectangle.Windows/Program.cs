using Rectangle.Windows.Core;
using Rectangle.Windows.Services;
using Rectangle.Windows.Views;
using System;
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
        _lastActiveWindowService?.Dispose();
        Console.WriteLine("Rectangle 已退出。");
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
                return System.Drawing.Icon.FromHandle(hIcon);
            }
            catch { }
        }
        return null;
    }

    private static System.Drawing.Image? LoadMenuIcon(string iconName)
    {
        // 从嵌入式资源加载
        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        string resourceName = $"Rectangle.Windows.Assets.WindowPositions.{iconName}";
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream != null)
        {
            try { return System.Drawing.Image.FromStream(stream); }
            catch { return null; }
        }
        return null;
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
        var menu = new ContextMenuStrip();

        // 忽略 [应用名] - 动态菜单项
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

        // 半屏
        menu.Items.Add("左半屏 (&L)", LoadMenuIcon("leftHalfTemplate.png"), (s, e) => _windowManager?.Execute(WindowAction.LeftHalf));
        menu.Items.Add("右半屏 (&R)", LoadMenuIcon("rightHalfTemplate.png"), (s, e) => _windowManager?.Execute(WindowAction.RightHalf));
        menu.Items.Add("上半屏 (&T)", LoadMenuIcon("topHalfTemplate.png"), (s, e) => _windowManager?.Execute(WindowAction.TopHalf));
        menu.Items.Add("下半屏 (&B)", LoadMenuIcon("bottomHalfTemplate.png"), (s, e) => _windowManager?.Execute(WindowAction.BottomHalf));
        menu.Items.Add(new ToolStripSeparator());

        // 四角
        menu.Items.Add("左上角", LoadMenuIcon("topLeftTemplate.png"), (s, e) => _windowManager?.Execute(WindowAction.TopLeft));
        menu.Items.Add("右上角", LoadMenuIcon("topRightTemplate.png"), (s, e) => _windowManager?.Execute(WindowAction.TopRight));
        menu.Items.Add("左下角", LoadMenuIcon("bottomLeftTemplate.png"), (s, e) => _windowManager?.Execute(WindowAction.BottomLeft));
        menu.Items.Add("右下角", LoadMenuIcon("bottomRightTemplate.png"), (s, e) => _windowManager?.Execute(WindowAction.BottomRight));
        menu.Items.Add(new ToolStripSeparator());

        // 三分之一
        menu.Items.Add("左首 1/3", LoadMenuIcon("firstThirdTemplate.png"), (s, e) => _windowManager?.Execute(WindowAction.FirstThird));
        menu.Items.Add("中间 1/3", LoadMenuIcon("centerThirdTemplate.png"), (s, e) => _windowManager?.Execute(WindowAction.CenterThird));
        menu.Items.Add("右首 1/3", LoadMenuIcon("lastThirdTemplate.png"), (s, e) => _windowManager?.Execute(WindowAction.LastThird));
        menu.Items.Add(new ToolStripSeparator());

        // 最大化与恢复
        menu.Items.Add("最大化", LoadMenuIcon("maximizeTemplate.png"), (s, e) => _windowManager?.Execute(WindowAction.Maximize));
        menu.Items.Add("恢复", LoadMenuIcon("restoreTemplate.png"), (s, e) => _windowManager?.Execute(WindowAction.Restore));
        menu.Items.Add("居中", LoadMenuIcon("centerTemplate.png"), (s, e) => _windowManager?.Execute(WindowAction.Center));
        menu.Items.Add(new ToolStripSeparator());

        // 设置与退出
        menu.Items.Add("偏好设置...", null, (s, e) => OpenSettings());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("退出 Rectangle", null, (s, e) => {
            Application.Exit();
        });

        return menu;
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
