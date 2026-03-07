using Rectangle.Windows.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Rectangle.Windows.Services;

public class HotkeyManager
{
    private readonly nint _hwnd;
    private readonly WindowManager _windowManager;
    private readonly Dictionary<int, WindowAction> _hotkeyActions = new();
    private readonly Dictionary<int, (ushort Vk, HOT_KEY_MODIFIERS Modifiers)> _hotkeyInfo = new();
    private readonly List<uint> _registeredIds = new();

    public HotkeyManager(nint hwnd, WindowManager windowManager, AppConfig config)
    {
        _hwnd = hwnd;
        _windowManager = windowManager;

        RegisterDefaultHotkeys();
        PrintUsageGuide();
    }

    private void PrintUsageGuide()
    {
        Console.WriteLine();
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("                    Rectangle 快捷键使用指南");
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine();
        Console.WriteLine("【半屏操作】");
        Console.WriteLine("  Ctrl+Alt+←        左半屏");
        Console.WriteLine("  Ctrl+Alt+→        右半屏");
        Console.WriteLine("  Ctrl+Alt+↑        上半屏");
        Console.WriteLine("  Ctrl+Alt+↓        下半屏");
        Console.WriteLine();
        Console.WriteLine("【四角操作】");
        Console.WriteLine("  Ctrl+Alt+U        左上角");
        Console.WriteLine("  Ctrl+Alt+I        右上角");
        Console.WriteLine("  Ctrl+Alt+J        左下角");
        Console.WriteLine("  Ctrl+Alt+K        右下角");
        Console.WriteLine();
        Console.WriteLine("【三分之一屏】");
        Console.WriteLine("  Ctrl+Alt+D        左首 1/3");
        Console.WriteLine("  Ctrl+Alt+F        中间 1/3");
        Console.WriteLine("  Ctrl+Alt+G        右首 1/3");
        Console.WriteLine("  Ctrl+Alt+E        左侧 2/3");
        Console.WriteLine("  Ctrl+Alt+R        中间 2/3");
        Console.WriteLine("  Ctrl+Alt+T        右侧 2/3");
        Console.WriteLine();
        Console.WriteLine("【最大化与缩放】");
        Console.WriteLine("  Ctrl+Alt+Enter        最大化");
        Console.WriteLine("  Ctrl+Alt+C            居中");
        Console.WriteLine("  Ctrl+Alt+Backspace    恢复");
        Console.WriteLine("  Ctrl+Alt+Shift+↑      最大化高度");
        Console.WriteLine("  Ctrl+Alt+=            放大");
        Console.WriteLine("  Ctrl+Alt+-            缩小");
        Console.WriteLine();
        Console.WriteLine("【显示器切换】(需要 Win 键)");
        Console.WriteLine("  Ctrl+Alt+Win+→    下一个显示器");
        Console.WriteLine("  Ctrl+Alt+Win+←    上一个显示器");
        Console.WriteLine();
        Console.WriteLine("【托盘菜单】");
        Console.WriteLine("  右键点击托盘图标可查看所有操作选项");
        Console.WriteLine();
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine();
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
            Debug.WriteLine($"[HotkeyManager] 无法注册热键 {action} (VK=0x{vk:X}, Modifiers={modifiers}), 错误码: {error}");
            // 热键可能被其他应用占用，跳过此热键继续注册其他热键
        }
    }

    public void HandleHotKey(int id)
    {
        if (_hotkeyActions.TryGetValue(id, out var action))
        {
            if (_hotkeyInfo.TryGetValue(id, out var info))
            {
                var shortcutStr = GetShortcutString(info.Vk, info.Modifiers);
                Console.WriteLine($"按下快捷键: {shortcutStr}");
            }
            _windowManager.Execute(action);
        }
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

    private static string GetKeyName(ushort vk)
    {
        return vk switch
        {
            0x25 => "←",
            0x26 => "↑",
            0x27 => "→",
            0x28 => "↓",
            0x0D => "Enter",
            0x08 => "Backspace",
            0x2E => "Delete",
            0x43 => "C",
            0x44 => "D",
            0x45 => "E",
            0x46 => "F",
            0x47 => "G",
            0x49 => "I",
            0x4A => "J",
            0x4B => "K",
            0x52 => "R",
            0x54 => "T",
            0x55 => "U",
            0xBB => "=",
            0xBD => "-",
            _ => $"0x{vk:X}"
        };
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
        
        // 重新注册默认热键
        RegisterDefaultHotkeys();
    }

    public void Dispose()
    {
        foreach (var id in _registeredIds)
        {
            PInvoke.UnregisterHotKey((HWND)_hwnd, (int)id);
        }
    }
}
