using H.NotifyIcon;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Rectangle.Windows.WinUI.Core;
using Rectangle.Windows.WinUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Rectangle.Windows.WinUI.Services
{
    public class TrayIconService : IDisposable
    {
        private TaskbarIcon? _taskbarIcon;
        private readonly WindowManager _windowManager;
        private readonly Action _showSettingsCallback;
        private readonly ConfigService _configService;
        private LastActiveWindowService? _lastActiveService;

        private nint _hMenu;
        private readonly Dictionary<uint, WindowAction> _idToAction = new();
        private uint _nextId = 2000;
        private uint _idSettings;
        private uint _idExit;
        private readonly List<nint> _hBitmaps = new();

        // Win32 menu flags
        private const uint MF_STRING    = 0x00000000;
        private const uint MF_SEPARATOR = 0x00000800;
        private const uint MF_POPUP     = 0x00000010;
        private const uint MF_BYCOMMAND = 0x00000000;
        private const uint MF_BYPOSITION = 0x00000400;
        private const uint TPM_RETURNCMD   = 0x0100;
        private const uint TPM_RIGHTBUTTON = 0x0002;
        private const uint TPM_NONOTIFY    = 0x0080;

        [DllImport("user32.dll")] static extern nint CreatePopupMenu();
        [DllImport("user32.dll")] static extern bool DestroyMenu(nint hMenu);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern bool AppendMenuW(nint hMenu, uint uFlags, nint uIDNewItem, string? lpNewItem);
        [DllImport("user32.dll")]
        static extern uint TrackPopupMenu(nint hMenu, uint uFlags, int x, int y, int nReserved, nint hWnd, nint prcRect);
        [DllImport("user32.dll")]
        static extern bool SetMenuItemBitmaps(nint hMenu, uint uPosition, uint uFlags, nint hBitmapUnchecked, nint hBitmapChecked);
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetMenuItemInfo(nint hMenu, uint uItem, bool fByPosition, ref MENUITEMINFOW lpmii);
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct MENUITEMINFOW
        {
            public uint cbSize, fMask, fType, fState, wID;
            public nint hSubMenu, hbmpChecked, hbmpUnchecked, dwItemData, dwTypeData;
            public uint cch;
            public nint hbmpItem;
        }
        private const uint MIIM_BITMAP = 0x00000080;
        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(nint hWnd);
        [DllImport("user32.dll")]
        static extern bool GetCursorPos(out POINT lpPoint);
        [DllImport("gdi32.dll")]
        static extern bool DeleteObject(nint hObject);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT { public int X, Y; }

        private static readonly Dictionary<string, string> _actionIcons = new()
        {
            ["LeftHalf"]             = "leftHalfTemplate.png",
            ["RightHalf"]            = "rightHalfTemplate.png",
            ["CenterHalf"]           = "halfWidthCenterTemplate.png",
            ["TopHalf"]              = "topHalfTemplate.png",
            ["BottomHalf"]           = "bottomHalfTemplate.png",
            ["TopLeft"]              = "topLeftTemplate.png",
            ["TopRight"]             = "topRightTemplate.png",
            ["BottomLeft"]           = "bottomLeftTemplate.png",
            ["BottomRight"]          = "bottomRightTemplate.png",
            ["FirstThird"]           = "firstThirdTemplate.png",
            ["CenterThird"]          = "centerThirdTemplate.png",
            ["LastThird"]            = "lastThirdTemplate.png",
            ["FirstTwoThirds"]       = "firstTwoThirdsTemplate.png",
            ["CenterTwoThirds"]      = "centerTwoThirdsTemplate.png",
            ["LastTwoThirds"]        = "lastTwoThirdsTemplate.png",
            ["FirstFourth"]          = "leftFourthTemplate.png",
            ["SecondFourth"]         = "centerLeftFourthTemplate.png",
            ["ThirdFourth"]          = "centerRightFourthTemplate.png",
            ["LastFourth"]           = "rightFourthTemplate.png",
            ["FirstThreeFourths"]    = "firstThreeFourthsTemplate.png",
            ["CenterThreeFourths"]   = "centerThreeFourthsTemplate.png",
            ["LastThreeFourths"]     = "lastThreeFourthsTemplate.png",
            ["TopLeftSixth"]         = "topLeftSixthTemplate.png",
            ["TopCenterSixth"]       = "topCenterSixthTemplate.png",
            ["TopRightSixth"]        = "topRightSixthTemplate.png",
            ["BottomLeftSixth"]      = "bottomLeftSixthTemplate.png",
            ["BottomCenterSixth"]    = "bottomCenterSixthTemplate.png",
            ["BottomRightSixth"]     = "bottomRightSixthTemplate.png",
            ["Maximize"]             = "maximizeTemplate.png",
            ["AlmostMaximize"]       = "almostMaximizeTemplate.png",
            ["MaximizeHeight"]       = "maximizeHeightTemplate.png",
            ["Larger"]               = "makeLargerTemplate.png",
            ["Smaller"]              = "makeSmallerTemplate.png",
            ["Center"]               = "centerTemplate.png",
            ["Restore"]              = "restoreTemplate.png",
            ["MoveLeft"]             = "moveLeftTemplate.png",
            ["MoveRight"]            = "moveRightTemplate.png",
            ["MoveUp"]               = "moveUpTemplate.png",
            ["MoveDown"]             = "moveDownTemplate.png",
            ["NextDisplay"]          = "nextDisplayTemplate.png",
            ["PreviousDisplay"]      = "prevDisplayTemplate.png",
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

                BuildNativeMenu();

                _taskbarIcon.RightClickCommand = new RelayCommand(ShowNativeMenu);
                _taskbarIcon.ForceCreate(enablesEfficiencyMode: false);
                Logger.Info("TrayIconService", "托盘图标初始化成功");
            }
            catch (Exception ex)
            {
                Logger.Error("TrayIconService", $"托盘图标初始化失败: {ex}");
            }
        }

        // ── 菜单构建 ──────────────────────────────────────────────

        private void BuildNativeMenu()
        {
            var sc = LoadShortcuts();
            _hMenu = CreatePopupMenu();

            Add(_hMenu, "左半屏",   "LeftHalf",   WindowAction.LeftHalf,   sc);
            Add(_hMenu, "右半屏",   "RightHalf",  WindowAction.RightHalf,  sc);
            Add(_hMenu, "中间半屏", "CenterHalf", WindowAction.CenterHalf, sc);
            Add(_hMenu, "上半屏",   "TopHalf",    WindowAction.TopHalf,    sc);
            Add(_hMenu, "下半屏",   "BottomHalf", WindowAction.BottomHalf, sc);
            Sep(_hMenu);

            Add(_hMenu, "左上", "TopLeft",     WindowAction.TopLeft,     sc);
            Add(_hMenu, "右上", "TopRight",    WindowAction.TopRight,    sc);
            Add(_hMenu, "左下", "BottomLeft",  WindowAction.BottomLeft,  sc);
            Add(_hMenu, "右下", "BottomRight", WindowAction.BottomRight, sc);
            Sep(_hMenu);

            var thirds = CreatePopupMenu();
            Add(thirds, "左首 1/3", "FirstThird",      WindowAction.FirstThird,      sc);
            Add(thirds, "中间 1/3", "CenterThird",     WindowAction.CenterThird,     sc);
            Add(thirds, "右首 1/3", "LastThird",       WindowAction.LastThird,       sc);
            Sep(thirds);
            Add(thirds, "左侧 2/3", "FirstTwoThirds",  WindowAction.FirstTwoThirds,  sc);
            Add(thirds, "中间 2/3", "CenterTwoThirds", WindowAction.CenterTwoThirds, sc);
            Add(thirds, "右侧 2/3", "LastTwoThirds",   WindowAction.LastTwoThirds,   sc);
            AppendMenuWithIcon(_hMenu, thirds, "三等分", "FirstThird");

            var fourths = CreatePopupMenu();
            Add(fourths, "左 1/4",   "FirstFourth",        WindowAction.FirstFourth,        sc);
            Add(fourths, "中左 1/4", "SecondFourth",       WindowAction.SecondFourth,       sc);
            Add(fourths, "中右 1/4", "ThirdFourth",        WindowAction.ThirdFourth,        sc);
            Add(fourths, "右 1/4",   "LastFourth",         WindowAction.LastFourth,         sc);
            Sep(fourths);
            Add(fourths, "左 3/4",   "FirstThreeFourths",  WindowAction.FirstThreeFourths,  sc);
            Add(fourths, "中间 3/4", "CenterThreeFourths", WindowAction.CenterThreeFourths, sc);
            Add(fourths, "右 3/4",   "LastThreeFourths",   WindowAction.LastThreeFourths,   sc);
            AppendMenuWithIcon(_hMenu, fourths, "四等分", "FirstFourth");

            var sixths = CreatePopupMenu();
            Add(sixths, "左上 1/6", "TopLeftSixth",      WindowAction.TopLeftSixth,      sc);
            Add(sixths, "上中 1/6", "TopCenterSixth",    WindowAction.TopCenterSixth,    sc);
            Add(sixths, "右上 1/6", "TopRightSixth",     WindowAction.TopRightSixth,     sc);
            Sep(sixths);
            Add(sixths, "左下 1/6", "BottomLeftSixth",   WindowAction.BottomLeftSixth,   sc);
            Add(sixths, "下中 1/6", "BottomCenterSixth", WindowAction.BottomCenterSixth, sc);
            Add(sixths, "右下 1/6", "BottomRightSixth",  WindowAction.BottomRightSixth,  sc);
            AppendMenuWithIcon(_hMenu, sixths, "六等分", "TopLeftSixth");
            Sep(_hMenu);

            Add(_hMenu, "最大化",     "Maximize",       WindowAction.Maximize,       sc);
            Add(_hMenu, "接近最大化", "AlmostMaximize", WindowAction.AlmostMaximize, sc);
            Add(_hMenu, "最大化高度", "MaximizeHeight", WindowAction.MaximizeHeight, sc);
            Add(_hMenu, "放大",       "Larger",         WindowAction.Larger,         sc);
            Add(_hMenu, "缩小",       "Smaller",        WindowAction.Smaller,        sc);
            Add(_hMenu, "居中",       "Center",         WindowAction.Center,         sc);
            Add(_hMenu, "恢复",       "Restore",        WindowAction.Restore,        sc);
            Sep(_hMenu);

            Add(_hMenu, "左移", "MoveLeft",  WindowAction.MoveLeft,  sc);
            Add(_hMenu, "右移", "MoveRight", WindowAction.MoveRight, sc);
            Add(_hMenu, "上移", "MoveUp",    WindowAction.MoveUp,    sc);
            Add(_hMenu, "下移", "MoveDown",  WindowAction.MoveDown,  sc);
            Sep(_hMenu);

            Add(_hMenu, "下一个显示器", "NextDisplay",     WindowAction.NextDisplay,     sc);
            Add(_hMenu, "上一个显示器", "PreviousDisplay", WindowAction.PreviousDisplay, sc);
            Sep(_hMenu);

            _idSettings = _nextId++;
            AppendMenuW(_hMenu, MF_STRING, (nint)_idSettings, "偏好设置...");
            Sep(_hMenu);
            _idExit = _nextId++;
            AppendMenuW(_hMenu, MF_STRING, (nint)_idExit, "退出");
        }

        private void Add(nint menu, string label, string actionName, WindowAction action,
                         Dictionary<string, ShortcutConfig> shortcuts)
        {
            uint id = _nextId++;
            _idToAction[id] = action;
            var shortcut = GetShortcutText(actionName, shortcuts);
            // Win32 菜单使用 \t 分隔文本和快捷键，确保足够的空格
            var text = string.IsNullOrEmpty(shortcut) ? label : $"{label}\t\t{shortcut}";
            AppendMenuW(menu, MF_STRING, (nint)id, text);

            // 加载并设置图标
            var hBmp = LoadIconBitmap(actionName);
            if (hBmp != nint.Zero)
            {
                var mii = new MENUITEMINFOW
                {
                    cbSize = (uint)Marshal.SizeOf<MENUITEMINFOW>(),
                    fMask = MIIM_BITMAP,
                    hbmpItem = hBmp
                };
                if (!SetMenuItemInfo(menu, id, false, ref mii))
                    Logger.Warning("TrayIcon", $"设置菜单图标失败: {actionName}");
            }
        }

        private static void Sep(nint menu) => AppendMenuW(menu, MF_SEPARATOR, 0, null);

        private void AppendMenuWithIcon(nint parentMenu, nint submenu, string label, string iconActionName)
        {
            AppendMenuW(parentMenu, MF_POPUP, submenu, label);
            var hBmp = LoadIconBitmap(iconActionName);
            if (hBmp != nint.Zero)
            {
                int position = GetMenuItemCount(parentMenu) - 1;
                if (position >= 0)
                {
                    var mii = new MENUITEMINFOW
                    {
                        cbSize = (uint)Marshal.SizeOf<MENUITEMINFOW>(),
                        fMask = MIIM_BITMAP,
                        hbmpItem = hBmp
                    };
                    SetMenuItemInfo(parentMenu, (uint)position, true, ref mii);
                }
            }
        }

        [DllImport("user32.dll")]
        static extern int GetMenuItemCount(nint hMenu);

        // ── 图标 ──────────────────────────────────────────────────

        private static string GetAssetsDir()
        {
            // 优先用 exe 所在目录（发布后），其次用 BaseDirectory（调试时）
            try
            {
                var exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
                if (!string.IsNullOrEmpty(exePath))
                {
                    var dir = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(exePath)!, "Assets", "WindowPositions");
                    if (System.IO.Directory.Exists(dir)) return dir;
                }
            }
            catch { }
            return System.IO.Path.Combine(AppContext.BaseDirectory, "Assets", "WindowPositions");
        }

        private nint LoadIconBitmap(string actionName)
        {
            if (!_actionIcons.TryGetValue(actionName, out var file)) return nint.Zero;
            try
            {
                var path = System.IO.Path.Combine(GetAssetsDir(), file);
                if (!System.IO.File.Exists(path))
                {
                    Logger.Warning("TrayIcon", $"图标文件不存在: {path}");
                    return nint.Zero;
                }

                using var src = new System.Drawing.Bitmap(path);
                using var scaled = new System.Drawing.Bitmap(16, 16, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                using (var g = System.Drawing.Graphics.FromImage(scaled))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.Clear(System.Drawing.Color.Transparent);
                    g.DrawImage(src, 0, 0, 16, 16);
                }

                var hBmp = CreatePremultipliedDIB(scaled);
                if (hBmp == nint.Zero)
                    Logger.Warning("TrayIcon", $"无法创建 DIB 位图: {file}");
                else
                    _hBitmaps.Add(hBmp);
                return hBmp;
            }
            catch (Exception ex)
            {
                Logger.Error("TrayIcon", $"加载图标失败 {file}: {ex.Message}");
                return nint.Zero;
            }
        }

        private static unsafe nint CreatePremultipliedDIB(System.Drawing.Bitmap src)
        {
            int w = src.Width, h = src.Height;
            var bmi = new BITMAPV5HEADER
            {
                bV5Size        = (uint)sizeof(BITMAPV5HEADER),
                bV5Width       = w,
                bV5Height      = -h,
                bV5Planes      = 1,
                bV5BitCount    = 32,
                bV5Compression = BI_BITFIELDS,
                bV5RedMask     = 0x00FF0000,
                bV5GreenMask   = 0x0000FF00,
                bV5BlueMask    = 0x000000FF,
                bV5AlphaMask   = 0xFF000000,
                bV5Endpoints   = new byte[36],
            };
            var hdc = GetDC(nint.Zero);
            var hBmp = CreateDIBSection(hdc, ref bmi, 0, out void* bits, nint.Zero, 0);
            ReleaseDC(nint.Zero, hdc);
            if (hBmp == nint.Zero || bits == null) return nint.Zero;

            var rect = new System.Drawing.Rectangle(0, 0, w, h);
            var data = src.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly,
                                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            try
            {
                var dst = (uint*)bits;
                var srcPtr = (uint*)data.Scan0;
                for (int i = 0; i < w * h; i++)
                {
                    uint px = srcPtr[i];
                    uint a = px >> 24;
                    uint r = (px >> 16) & 0xFF;
                    uint g2 = (px >> 8) & 0xFF;
                    uint b = px & 0xFF;
                    // 预乘 alpha
                    dst[i] = (a << 24) | ((r * a / 255) << 16) | ((g2 * a / 255) << 8) | (b * a / 255);
                }
            }
            finally { src.UnlockBits(data); }
            return hBmp;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct BITMAPV5HEADER
        {
            public uint bV5Size;
            public int bV5Width, bV5Height;
            public ushort bV5Planes, bV5BitCount;
            public uint bV5Compression, bV5SizeImage;
            public int bV5XPelsPerMeter, bV5YPelsPerMeter;
            public uint bV5ClrUsed, bV5ClrImportant;
            public uint bV5RedMask, bV5GreenMask, bV5BlueMask, bV5AlphaMask;
            public uint bV5CSType;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 36)] public byte[] bV5Endpoints;
            public uint bV5GammaRed, bV5GammaGreen, bV5GammaBlue;
            public uint bV5Intent, bV5ProfileData, bV5ProfileSize, bV5Reserved;
        }
        private const uint BI_BITFIELDS = 3;

        [DllImport("gdi32.dll")]
        private static extern unsafe nint CreateDIBSection(nint hdc, ref BITMAPV5HEADER pbmi, uint usage, out void* ppvBits, nint hSection, uint offset);
        [DllImport("user32.dll")] private static extern nint GetDC(nint hWnd);
        [DllImport("user32.dll")] private static extern int ReleaseDC(nint hWnd, nint hDC);

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
            // 使用更短的格式
            var result = string.Join("+", parts);
            return result.Length > 15 ? result[..15] : result;
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

        // ── 菜单弹出 ──────────────────────────────────────────────

        private void ShowNativeMenu()
        {
            if (_hMenu == nint.Zero) return;

            // 弹出菜单前暂停跟踪，保留当前活动窗口
            _lastActiveService?.PauseTracking();

            GetCursorPos(out var pt);
            var hwnd = GetInternalHwnd();
            if (hwnd != nint.Zero) SetForegroundWindow(hwnd);

            uint cmd = TrackPopupMenu(
                _hMenu,
                TPM_RETURNCMD | TPM_RIGHTBUTTON | TPM_NONOTIFY,
                pt.X, pt.Y, 0, hwnd, nint.Zero);

            // 菜单关闭后恢复跟踪
            _lastActiveService?.ResumeTracking();

            if (cmd == 0) return;
            if (cmd == _idSettings) _showSettingsCallback();
            else if (cmd == _idExit) DoExit();
            else if (_idToAction.TryGetValue(cmd, out var action)) _windowManager.Execute(action);
        }

        private nint GetInternalHwnd()
        {
            try
            {
                var f = typeof(TaskbarIcon).GetField("_messageWindow",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var w = f?.GetValue(_taskbarIcon);
                if (w == null) return nint.Zero;
                var p = w.GetType().GetProperty("Handle",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                return p?.GetValue(w) is nint h ? h : nint.Zero;
            }
            catch { return nint.Zero; }
        }

        private void DoExit() { Dispose(); Environment.Exit(0); }

        public void ShowNotification(string title, string message) =>
            _taskbarIcon?.ShowNotification(title, message);

        public void Dispose()
        {
            if (_hMenu != nint.Zero) { DestroyMenu(_hMenu); _hMenu = nint.Zero; }
            foreach (var h in _hBitmaps)
                try { DeleteObject(h); } catch { }
            _hBitmaps.Clear();
            _taskbarIcon?.Dispose();
            _taskbarIcon = null;
        }
    }
}
