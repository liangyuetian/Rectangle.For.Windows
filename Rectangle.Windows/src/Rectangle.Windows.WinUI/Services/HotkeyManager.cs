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
    private readonly Dictionary<int, WindowAction> _hotkeyActions = new();
    private int _nextHotkeyId = 1;
    private SUBCLASSPROC? _subclassProc;

    public HotkeyManager(nint hwnd, WindowManager windowManager)
    {
        _hwnd = hwnd;
        _windowManager = windowManager;
        
        // 注册子类化以接收 WM_HOTKEY
        _subclassProc = new SUBCLASSPROC(WindowSubclassProc);
        PInvoke.SetWindowSubclass((HWND)_hwnd, _subclassProc, 0, 0);
        
        // 注册默认快捷键
        RegisterDefaultHotkeys();
    }

    private void RegisterDefaultHotkeys()
    {
        // 半屏
        RegisterHotKey(HOT_KEY_MODIFIERS.MOD_CONTROL | HOT_KEY_MODIFIERS.MOD_ALT, 0x25, WindowAction.LeftHalf);     // Ctrl+Alt+Left
        RegisterHotKey(HOT_KEY_MODIFIERS.MOD_CONTROL | HOT_KEY_MODIFIERS.MOD_ALT, 0x27, WindowAction.RightHalf);    // Ctrl+Alt+Right
        RegisterHotKey(HOT_KEY_MODIFIERS.MOD_CONTROL | HOT_KEY_MODIFIERS.MOD_ALT, 0x26, WindowAction.TopHalf);      // Ctrl+Alt+Up
        RegisterHotKey(HOT_KEY_MODIFIERS.MOD_CONTROL | HOT_KEY_MODIFIERS.MOD_ALT, 0x28, WindowAction.BottomHalf);   // Ctrl+Alt+Down

        // 四角
        RegisterHotKey(HOT_KEY_MODIFIERS.MOD_CONTROL | HOT_KEY_MODIFIERS.MOD_ALT, 0x55, WindowAction.TopLeft);      // Ctrl+Alt+U
        RegisterHotKey(HOT_KEY_MODIFIERS.MOD_CONTROL | HOT_KEY_MODIFIERS.MOD_ALT, 0x49, WindowAction.TopRight);     // Ctrl+Alt+I
        RegisterHotKey(HOT_KEY_MODIFIERS.MOD_CONTROL | HOT_KEY_MODIFIERS.MOD_ALT, 0x4A, WindowAction.BottomLeft);   // Ctrl+Alt+J
        RegisterHotKey(HOT_KEY_MODIFIERS.MOD_CONTROL | HOT_KEY_MODIFIERS.MOD_ALT, 0x4B, WindowAction.BottomRight);  // Ctrl+Alt+K

        // 三分之一
        RegisterHotKey(HOT_KEY_MODIFIERS.MOD_CONTROL | HOT_KEY_MODIFIERS.MOD_ALT, 0x44, WindowAction.FirstThird);       // Ctrl+Alt+D
        RegisterHotKey(HOT_KEY_MODIFIERS.MOD_CONTROL | HOT_KEY_MODIFIERS.MOD_ALT, 0x46, WindowAction.CenterThird);      // Ctrl+Alt+F
        RegisterHotKey(HOT_KEY_MODIFIERS.MOD_CONTROL | HOT_KEY_MODIFIERS.MOD_ALT, 0x47, WindowAction.LastThird);        // Ctrl+Alt+G
        RegisterHotKey(HOT_KEY_MODIFIERS.MOD_CONTROL | HOT_KEY_MODIFIERS.MOD_ALT, 0x45, WindowAction.FirstTwoThirds);   // Ctrl+Alt+E
        RegisterHotKey(HOT_KEY_MODIFIERS.MOD_CONTROL | HOT_KEY_MODIFIERS.MOD_ALT, 0x52, WindowAction.CenterTwoThirds);  // Ctrl+Alt+R
        RegisterHotKey(HOT_KEY_MODIFIERS.MOD_CONTROL | HOT_KEY_MODIFIERS.MOD_ALT, 0x54, WindowAction.LastTwoThirds);    // Ctrl+Alt+T

        // 最大化与缩放
        RegisterHotKey(HOT_KEY_MODIFIERS.MOD_CONTROL | HOT_KEY_MODIFIERS.MOD_ALT, 0x0D, WindowAction.Maximize);        // Ctrl+Alt+Enter
        RegisterHotKey(HOT_KEY_MODIFIERS.MOD_CONTROL | HOT_KEY_MODIFIERS.MOD_ALT, 0x43, WindowAction.Center);          // Ctrl+Alt+C
        RegisterHotKey(HOT_KEY_MODIFIERS.MOD_CONTROL | HOT_KEY_MODIFIERS.MOD_ALT, 0x08, WindowAction.Restore);         // Ctrl+Alt+Backspace

        Console.WriteLine("[HotkeyManager] 快捷键注册完成");
    }

    private void RegisterHotKey(HOT_KEY_MODIFIERS modifiers, uint vk, WindowAction action)
    {
        var id = _nextHotkeyId++;
        if (PInvoke.RegisterHotKey((HWND)_hwnd, id, modifiers, vk))
        {
            _hotkeyActions[id] = action;
            Console.WriteLine($"[HotkeyManager] 注册快捷键成功: {action} (id={id}, vk=0x{vk:X})");
        }
        else
        {
            Console.WriteLine($"[HotkeyManager] 注册快捷键失败: {action} (vk=0x{vk:X})");
        }
    }

    private LRESULT WindowSubclassProc(HWND hWnd, uint uMsg, WPARAM wParam, LPARAM lParam, nuint uIdSubclass, nuint dwRefData)
    {
        const uint WM_HOTKEY = 0x0312;
        
        if (uMsg == WM_HOTKEY)
        {
            var id = (int)wParam.Value;
            if (_hotkeyActions.TryGetValue(id, out var action))
            {
                Console.WriteLine($"[HotkeyManager] 收到热键: {action}");
                _windowManager.Execute(action);
                return new LRESULT(0);
            }
        }
        
        return PInvoke.DefSubclassProc(hWnd, uMsg, wParam, lParam);
    }
}
