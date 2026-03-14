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

        _subclassProc = new SUBCLASSPROC(WindowSubclassProc);
        PInvoke.SetWindowSubclass((HWND)_hwnd, _subclassProc, 0, 0);

        RegisterDefaultHotkeys();
    }

    private void RegisterDefaultHotkeys()
    {
        var ca = HOT_KEY_MODIFIERS.MOD_CONTROL | HOT_KEY_MODIFIERS.MOD_ALT;

        // 半屏
        RegisterHotKey(ca, 0x25, WindowAction.LeftHalf);
        RegisterHotKey(ca, 0x27, WindowAction.RightHalf);
        RegisterHotKey(ca, 0x26, WindowAction.TopHalf);
        RegisterHotKey(ca, 0x28, WindowAction.BottomHalf);
        // 四角
        RegisterHotKey(ca, 0x55, WindowAction.TopLeft);
        RegisterHotKey(ca, 0x49, WindowAction.TopRight);
        RegisterHotKey(ca, 0x4A, WindowAction.BottomLeft);
        RegisterHotKey(ca, 0x4B, WindowAction.BottomRight);
        // 三分屏
        RegisterHotKey(ca, 0x44, WindowAction.FirstThird);
        RegisterHotKey(ca, 0x46, WindowAction.CenterThird);
        RegisterHotKey(ca, 0x47, WindowAction.LastThird);
        RegisterHotKey(ca, 0x45, WindowAction.FirstTwoThirds);
        RegisterHotKey(ca, 0x52, WindowAction.CenterTwoThirds);
        RegisterHotKey(ca, 0x54, WindowAction.LastTwoThirds);
        // 最大化与缩放
        RegisterHotKey(ca, 0x0D, WindowAction.Maximize);
        RegisterHotKey(ca, 0x43, WindowAction.Center);
        RegisterHotKey(ca, 0x08, WindowAction.Restore);

        Console.WriteLine("[HotkeyManager] 快捷键注册完成");
    }

    private void RegisterHotKey(HOT_KEY_MODIFIERS modifiers, uint vk, WindowAction action)
    {
        var id = _nextHotkeyId++;
        if (PInvoke.RegisterHotKey((HWND)_hwnd, id, modifiers, vk))
            _hotkeyActions[id] = action;
        else
            Console.WriteLine($"[HotkeyManager] 注册失败: {action} (vk=0x{vk:X})");
    }

    private LRESULT WindowSubclassProc(HWND hWnd, uint uMsg, WPARAM wParam, LPARAM lParam, nuint uIdSubclass, nuint dwRefData)
    {
        const uint WM_HOTKEY = 0x0312;
        if (uMsg == WM_HOTKEY)
        {
            var id = (int)wParam.Value;
            if (_hotkeyActions.TryGetValue(id, out var action))
            {
                Console.WriteLine($"[HotkeyManager] 热键触发: {action}");
                _windowManager.Execute(action);
                return new LRESULT(0);
            }
        }
        return PInvoke.DefSubclassProc(hWnd, uMsg, wParam, lParam);
    }
}
