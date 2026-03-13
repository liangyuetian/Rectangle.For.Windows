using Rectangle.Windows.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Rectangle.Windows.Services;

public class HotkeyManager : IDisposable
{
    private readonly nint _hwnd;
    private readonly WindowManager _windowManager;
    private readonly Dictionary<int, WindowAction> _hotkeyActions = new();
    private readonly Dictionary<int, (ushort Vk, HOT_KEY_MODIFIERS Modifiers)> _hotkeyInfo = new();
    private readonly List<uint> _registeredIds = new();
    private bool _isCapturingMode = false;

    public HotkeyManager(nint hwnd, WindowManager windowManager, AppConfig config)
    {
        _hwnd = hwnd;
        _windowManager = windowManager;

        RegisterDefaultHotkeys();
        PrintUsageGuide();
    }

    private void PrintUsageGuide()
    {
        Logger.Info("HotkeyManager", "快捷键使用指南：");
        Logger.Info("HotkeyManager", "  【半屏操作】 Ctrl+Alt+←/→/↑/↓");
        Logger.Info("HotkeyManager", "  【四角操作】 Ctrl+Alt+U/I/J/K");
        Logger.Info("HotkeyManager", "  【三分之一屏】 Ctrl+Alt+D/F/G");
        Logger.Info("HotkeyManager", "  【三分之二屏】 Ctrl+Alt+E/R/T");
        Logger.Info("HotkeyManager", "  【最大化与缩放】 Ctrl+Alt+Enter/Shift+↑/=/C/Backspace");
        Logger.Info("HotkeyManager", "  【显示器切换】 Ctrl+Alt+Win+←/→");
    }

    private void RegisterDefaultHotkeys()
    {
        var id = 1;
        var modifiers = HOT_KEY_MODIFIERS.MOD_CONTROL | HOT_KEY_MODIFIERS.MOD_ALT;

        // 半屏
        RegisterHotkey(ref id, (ushort)0x25, modifiers, WindowAction.LeftHalf);
        RegisterHotkey(ref id, (ushort)0x27, modifiers, WindowAction.RightHalf);
        RegisterHotkey(ref id, (ushort)0x26, modifiers, WindowAction.TopHalf);
        RegisterHotkey(ref id, (ushort)0x28, modifiers, WindowAction.BottomHalf);

        // 四角
        RegisterHotkey(ref id, (ushort)0x55, modifiers, WindowAction.TopLeft);
        RegisterHotkey(ref id, (ushort)0x49, modifiers, WindowAction.TopRight);
        RegisterHotkey(ref id, (ushort)0x4A, modifiers, WindowAction.BottomLeft);
        RegisterHotkey(ref id, (ushort)0x4B, modifiers, WindowAction.BottomRight);

        // 三分之一
        RegisterHotkey(ref id, (ushort)0x44, modifiers, WindowAction.FirstThird);
        RegisterHotkey(ref id, (ushort)0x46, modifiers, WindowAction.CenterThird);
        RegisterHotkey(ref id, (ushort)0x47, modifiers, WindowAction.LastThird);
        RegisterHotkey(ref id, (ushort)0x45, modifiers, WindowAction.FirstTwoThirds);
        RegisterHotkey(ref id, (ushort)0x52, modifiers, WindowAction.CenterTwoThirds);
        RegisterHotkey(ref id, (ushort)0x54, modifiers, WindowAction.LastTwoThirds);

        // 最大化与恢复
        RegisterHotkey(ref id, (ushort)0x0D, modifiers, WindowAction.Maximize);
        RegisterHotkey(ref id, (ushort)0x08, modifiers, WindowAction.Restore);
        RegisterHotkey(ref id, (ushort)0x43, modifiers, WindowAction.Center);

        // 放大与缩小
        RegisterHotkey(ref id, (ushort)0xBB, modifiers, WindowAction.Larger);   // Ctrl+Alt+=
        RegisterHotkey(ref id, (ushort)0xBD, modifiers, WindowAction.Smaller);  // Ctrl+Alt+-

        // 最大化高度 (Ctrl+Alt+Shift+↑)
        var shiftModifiers = modifiers | HOT_KEY_MODIFIERS.MOD_SHIFT;
        RegisterHotkey(ref id, (ushort)0x26, shiftModifiers, WindowAction.MaximizeHeight);

        // 显示器（加 MOD_WIN）
        var winModifiers = modifiers | HOT_KEY_MODIFIERS.MOD_WIN;
        RegisterHotkey(ref id, (ushort)0x25, winModifiers, WindowAction.PreviousDisplay);
        RegisterHotkey(ref id, (ushort)0x27, winModifiers, WindowAction.NextDisplay);
    }

    private void RegisterHotkey(ref int id, ushort vk, HOT_KEY_MODIFIERS modifiers, WindowAction action)
    {
        var result = PInvoke.RegisterHotKey(
            (HWND)_hwnd,
            id,
            modifiers,
            vk);
        
        if (result)
        {
            _hotkeyActions[id] = action;
            _hotkeyInfo[id] = (vk, modifiers);
            _registeredIds.Add((uint)id);
            id++;
        }
        else
        {
            // TASK-053: 热键冲突处理 - 记录日志但不崩溃
            var error = System.Runtime.InteropServices.Marshal.GetLastWin32Error();
            Logger.Warning("HotkeyManager", $"无法注册热键 {action} (VK=0x{vk:X}, Modifiers={modifiers}), 错误码: {error}");
            // 热键可能被其他应用占用，跳过此热键继续注册其他热键
        }
    }

    public void HandleHotKey(int id)
    {
        // 如果正在设置快捷键，忽略热键响应
        if (_isCapturingMode)
        {
            return;
        }
        
        if (_hotkeyActions.TryGetValue(id, out var action))
        {
            if (_hotkeyInfo.TryGetValue(id, out var info))
            {
                var shortcutStr = GetShortcutString(info.Vk, info.Modifiers);
                Logger.Info("HotkeyManager", $"按下快捷键: {shortcutStr}");
            }
            _windowManager.Execute(action);
        }
    }
    
    /// <summary>
    /// 设置捕获模式。在修改快捷键时启用，此时热键不响应功能
    /// </summary>
    public void SetCapturingMode(bool capturing)
    {
        _isCapturingMode = capturing;
    }

    private static string GetShortcutString(ushort vk, HOT_KEY_MODIFIERS modifiers)
    {
        var parts = new List<string>();
        
        if ((modifiers & HOT_KEY_MODIFIERS.MOD_CONTROL) != 0)
            parts.Add("Ctrl");
        if ((modifiers & HOT_KEY_MODIFIERS.MOD_ALT) != 0)
            parts.Add("Alt");
        if ((modifiers & HOT_KEY_MODIFIERS.MOD_SHIFT) != 0)
            parts.Add("Shift");
        if ((modifiers & HOT_KEY_MODIFIERS.MOD_WIN) != 0)
            parts.Add("Win");
        
        parts.Add(GetKeyName(vk));
        
        return string.Join("+", parts);
    }

    private static unsafe string GetKeyName(ushort vk)
    {
        uint scanCode = PInvoke.MapVirtualKey(vk, MAP_VIRTUAL_KEY_TYPE.MAPVK_VK_TO_VSC);
        int lParam = (int)(scanCode << 16);
        
        switch (vk)
        {
            case 0x25: case 0x26: case 0x27: case 0x28: case 0x2E:
                lParam |= (1 << 24); // extended key flag
                break;
        }

        char* buf = stackalloc char[128];
        int result = PInvoke.GetKeyNameText(lParam, buf, 128);
        if (result > 0)
        {
            return new string(buf, 0, result);
        }
        
        return $"0x{vk:X}";
    }

    public void ReloadFromConfig(Dictionary<string, ShortcutConfig> shortcuts)
    {
        // 先取消注册所有热键
        foreach (var id in _registeredIds)
        {
            PInvoke.UnregisterHotKey((HWND)_hwnd, (int)id);
        }
        _registeredIds.Clear();
        _hotkeyActions.Clear();
        _hotkeyInfo.Clear();
        
        // 获取默认快捷键
        var defaultShortcuts = ConfigService.GetDefaultShortcuts();
        
        // 合并配置：先用默认值，再用用户配置覆盖
        var mergedShortcuts = new Dictionary<string, ShortcutConfig>(defaultShortcuts);
        if (shortcuts != null)
        {
            foreach (var kvp in shortcuts)
            {
                mergedShortcuts[kvp.Key] = kvp.Value;
            }
        }
        
        // 根据合并后的配置注册热键
        var newId = 1;
        foreach (var kvp in mergedShortcuts)
        {
            var actionName = kvp.Key;
            var config = kvp.Value;
            
            // 跳过禁用的快捷键
            if (!config.Enabled || config.KeyCode <= 0)
                continue;
            
            // 解析 WindowAction
            if (!Enum.TryParse<WindowAction>(actionName, out var action))
                continue;
            
            // 转换修饰键
            HOT_KEY_MODIFIERS newModifiers = 0;
            if ((config.ModifierFlags & 0x0002) != 0) newModifiers |= HOT_KEY_MODIFIERS.MOD_CONTROL;
            if ((config.ModifierFlags & 0x0001) != 0) newModifiers |= HOT_KEY_MODIFIERS.MOD_ALT;
            if ((config.ModifierFlags & 0x0004) != 0) newModifiers |= HOT_KEY_MODIFIERS.MOD_SHIFT;
            if ((config.ModifierFlags & 0x0008) != 0) newModifiers |= HOT_KEY_MODIFIERS.MOD_WIN;
            
            RegisterHotkey(ref newId, (ushort)config.KeyCode, newModifiers, action);
        }
        
        Logger.Info("HotkeyManager", $"已重新注册 {newId - 1} 个快捷键");
    }

    private bool _disposed;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        foreach (var id in _registeredIds)
        {
            PInvoke.UnregisterHotKey((HWND)_hwnd, (int)id);
        }
        _registeredIds.Clear();
        _hotkeyActions.Clear();
        _hotkeyInfo.Clear();

        _disposed = true;
    }

    ~HotkeyManager()
    {
        Dispose(false);
    }
}
