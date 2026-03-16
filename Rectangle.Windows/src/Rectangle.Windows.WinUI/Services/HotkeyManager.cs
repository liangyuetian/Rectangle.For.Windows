using System.Linq;
using System.Runtime.InteropServices;
using Rectangle.Windows.WinUI.Core;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using Windows.Win32.UI.Shell;

namespace Rectangle.Windows.WinUI.Services;

public unsafe class HotkeyManager
{
    private readonly nint _hwnd;
    private readonly WindowManager _windowManager;
    private readonly ConfigService _configService;
    private readonly Dictionary<int, WindowAction> _hotkeyActions = new();
    private int _nextHotkeyId = 1;
    private SUBCLASSPROC? _subclassProc;
    private bool _isCapturingMode;

    /// <summary>
    /// 设置捕获模式。在记录快捷键时启用，此时热键仅用于记录，不触发窗口操作。
    /// </summary>
    public void SetCapturingMode(bool capturing) => _isCapturingMode = capturing;

    public HotkeyManager(nint hwnd, WindowManager windowManager, ConfigService configService)
    {
        _hwnd = hwnd;
        _windowManager = windowManager;
        _configService = configService;

        _subclassProc = new SUBCLASSPROC(WindowSubclassProc);
        PInvoke.SetWindowSubclass((HWND)_hwnd, _subclassProc, 0, 0);

        LoadFromConfig();
    }

    /// <summary>
    /// 从配置重新加载快捷键。配置变更时调用。
    /// </summary>
    public void ReloadFromConfig()
    {
        foreach (var id in _hotkeyActions.Keys.ToList())
        {
            PInvoke.UnregisterHotKey((HWND)_hwnd, id);
        }
        _hotkeyActions.Clear();
        _nextHotkeyId = 1;
        LoadFromConfig();
    }

    private void LoadFromConfig()
    {
        var config = _configService.Load();
        var defaults = ConfigService.GetDefaultShortcuts();
        var merged = new Dictionary<string, ShortcutConfig>(defaults);
        foreach (var kvp in config.Shortcuts)
            merged[kvp.Key] = kvp.Value;

        var seen = new HashSet<(uint Vk, uint Modifiers)>();

        foreach (var kvp in merged)
        {
            var cfg = kvp.Value;
            if (!cfg.Enabled || cfg.KeyCode <= 0) continue;
            if (!Enum.TryParse<WindowAction>(kvp.Key, out var action)) continue;
            if (!seen.Add(((uint)cfg.KeyCode, cfg.ModifierFlags))) continue;

            var modifiers = ToHotKeyModifiers(cfg.ModifierFlags);
            var id = _nextHotkeyId++;
            if (PInvoke.RegisterHotKey((HWND)_hwnd, id, modifiers, (uint)cfg.KeyCode))
                _hotkeyActions[id] = action;
            else
                System.Diagnostics.Debug.WriteLine($"[HotkeyManager] 注册失败: {action} (vk=0x{cfg.KeyCode:X})");
        }

        System.Diagnostics.Debug.WriteLine($"[HotkeyManager] 已加载 {_hotkeyActions.Count} 个快捷键");
    }

    private static HOT_KEY_MODIFIERS ToHotKeyModifiers(uint flags)
    {
        HOT_KEY_MODIFIERS m = 0;
        if ((flags & 0x0002) != 0) m |= HOT_KEY_MODIFIERS.MOD_CONTROL;
        if ((flags & 0x0001) != 0) m |= HOT_KEY_MODIFIERS.MOD_ALT;
        if ((flags & 0x0004) != 0) m |= HOT_KEY_MODIFIERS.MOD_SHIFT;
        if ((flags & 0x0008) != 0) m |= HOT_KEY_MODIFIERS.MOD_WIN;
        return m;
    }

    private LRESULT WindowSubclassProc(HWND hWnd, uint uMsg, WPARAM wParam, LPARAM lParam, nuint uIdSubclass, nuint dwRefData)
    {
        const uint WM_HOTKEY = 0x0312;
        if (uMsg == WM_HOTKEY)
        {
            if (_isCapturingMode)
                return new LRESULT(0);

            var id = (int)wParam.Value;
            if (_hotkeyActions.TryGetValue(id, out var action))
            {
                _windowManager.Execute(action);
                return new LRESULT(0);
            }
        }
        return PInvoke.DefSubclassProc(hWnd, uMsg, wParam, lParam);
    }
}
