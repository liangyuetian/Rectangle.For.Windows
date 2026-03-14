using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Rectangle.Windows.WinUI.Services
{
    // Modifier key constants (mirrors HOT_KEY_MODIFIERS from Win32)
    internal static class HotkeyModifiers
    {
        public const uint MOD_ALT     = 0x0001;
        public const uint MOD_CONTROL = 0x0002;
        public const uint MOD_SHIFT   = 0x0004;
        public const uint MOD_WIN     = 0x0008;
    }

    /// <summary>
    /// 快捷键冲突信息
    /// </summary>
    public class HotkeyConflict
    {
        public int HotkeyId { get; set; }
        public uint Modifiers { get; set; }
        public uint VirtualKey { get; set; }
        public string DisplayText { get; set; } = "";
        public string? ConflictingApp { get; set; }
        public ConflictType Type { get; set; }
        public string Description { get; set; } = "";
        public List<HotkeyAlternative> Alternatives { get; set; } = new();
    }

    public enum ConflictType
    {
        SystemConflict,
        ApplicationConflict,
        InternalConflict,
        ReservedConflict,
        PotentialConflict
    }

    public class HotkeyAlternative
    {
        public uint Modifiers { get; set; }
        public uint VirtualKey { get; set; }
        public string DisplayText { get; set; } = "";
        public string Reason { get; set; } = "";
        public int SimilarityScore { get; set; }
    }

    public class RegisteredHotkeyInfo
    {
        public int Id { get; set; }
        public uint Modifiers { get; set; }
        public uint VirtualKey { get; set; }
        public string? ActionName { get; set; }
        public DateTime RegisteredAt { get; set; }
    }

    /// <summary>
    /// 快捷键冲突检测服务
    /// </summary>
    public class HotkeyConflictDetector
    {
        private readonly List<RegisteredHotkeyInfo> _registeredHotkeys = new();
        private readonly Dictionary<(uint, uint), List<string>> _knownConflicts = new();
        private readonly object _lock = new();

        private static readonly Dictionary<(uint, uint), string> SystemReservedHotkeys = new()
        {
            { (HotkeyModifiers.MOD_WIN, 0x44), "显示桌面 (Win+D)" },
            { (HotkeyModifiers.MOD_WIN, 0x45), "文件资源管理器 (Win+E)" },
            { (HotkeyModifiers.MOD_WIN, 0x46), "搜索 (Win+F)" },
            { (HotkeyModifiers.MOD_WIN, 0x49), "设置 (Win+I)" },
            { (HotkeyModifiers.MOD_WIN, 0x4C), "锁定 (Win+L)" },
            { (HotkeyModifiers.MOD_WIN, 0x52), "运行 (Win+R)" },
            { (HotkeyModifiers.MOD_WIN, 0x53), "搜索/截图 (Win+S)" },
            { (HotkeyModifiers.MOD_WIN, 0x58), "快速链接菜单 (Win+X)" },
            { (HotkeyModifiers.MOD_WIN, 0x1B), "关闭窗口 (Win+Esc)" },
            { (HotkeyModifiers.MOD_WIN, 0x20), "切换输入法 (Win+Space)" },
            { (HotkeyModifiers.MOD_CONTROL | HotkeyModifiers.MOD_ALT, 0x2E), "安全选项 (Ctrl+Alt+Del)" },
            { (HotkeyModifiers.MOD_CONTROL | HotkeyModifiers.MOD_ALT, 0x54), "任务管理器 (Ctrl+Alt+T 可能冲突)" },
            { (HotkeyModifiers.MOD_CONTROL | HotkeyModifiers.MOD_SHIFT, 0x45), "某些编辑器的运行快捷键" },
        };

        private static readonly Dictionary<string, List<(uint, uint, string)>> KnownAppHotkeys = new()
        {
            ["PowerToys"] = new()
            {
                (HotkeyModifiers.MOD_WIN | HotkeyModifiers.MOD_SHIFT, 0x60, "FancyZones 编辑器"),
                (HotkeyModifiers.MOD_WIN, 0x60, "快速布局切换")
            },
            ["AutoHotkey"] = new()
            {
                (HotkeyModifiers.MOD_CONTROL | HotkeyModifiers.MOD_ALT, 0, "多种用户自定义快捷键")
            },
            ["Snagit"] = new()
            {
                (HotkeyModifiers.MOD_CONTROL | HotkeyModifiers.MOD_SHIFT, 0x5A, "截图")
            },
            ["ShareX"] = new()
            {
                (HotkeyModifiers.MOD_CONTROL | HotkeyModifiers.MOD_SHIFT, 0x58, "截图")
            },
            ["Everything"] = new()
            {
                (HotkeyModifiers.MOD_WIN, 0x53, "搜索")
            },
            ["Microsoft Teams"] = new()
            {
                (HotkeyModifiers.MOD_CONTROL | HotkeyModifiers.MOD_SHIFT, 0x4D, "静音")
            }
        };

        public HotkeyConflictDetector()
        {
            InitializeKnownConflicts();
        }

        private void InitializeKnownConflicts()
        {
            foreach (var kvp in KnownAppHotkeys)
            {
                foreach (var (modifiers, vk, _) in kvp.Value)
                {
                    var key = (modifiers, vk);
                    if (!_knownConflicts.ContainsKey(key))
                        _knownConflicts[key] = new List<string>();
                    if (!_knownConflicts[key].Contains(kvp.Key))
                        _knownConflicts[key].Add(kvp.Key);
                }
            }
        }

        public HotkeyConflict? DetectConflict(uint modifiers, uint virtualKey, string? actionName = null)
        {
            lock (_lock)
            {
                var conflict = new HotkeyConflict
                {
                    Modifiers = modifiers,
                    VirtualKey = virtualKey,
                    DisplayText = GetHotkeyDisplayText(modifiers, virtualKey)
                };

                if (SystemReservedHotkeys.TryGetValue((modifiers, virtualKey), out var systemAction))
                {
                    conflict.Type = ConflictType.SystemConflict;
                    conflict.Description = $"与 Windows 系统快捷键冲突: {systemAction}";
                    conflict.ConflictingApp = "Windows";
                    conflict.Alternatives = GenerateAlternatives(modifiers, virtualKey);
                    return conflict;
                }

                if (_registeredHotkeys.Any(h => h.Modifiers == modifiers && h.VirtualKey == virtualKey && h.ActionName != actionName))
                {
                    conflict.Type = ConflictType.InternalConflict;
                    conflict.Description = "此快捷键已在 Rectangle 中注册用于其他操作";
                    conflict.Alternatives = GenerateAlternatives(modifiers, virtualKey);
                    return conflict;
                }

                if (_knownConflicts.TryGetValue((modifiers, virtualKey), out var conflictingApps))
                {
                    conflict.Type = ConflictType.ApplicationConflict;
                    conflict.ConflictingApp = string.Join(", ", conflictingApps);
                    conflict.Description = $"可能与以下应用程序冲突: {conflict.ConflictingApp}";
                    conflict.Alternatives = GenerateAlternatives(modifiers, virtualKey);
                    return conflict;
                }

                if (IsHighRiskCombination(modifiers, virtualKey))
                {
                    conflict.Type = ConflictType.PotentialConflict;
                    conflict.Description = "此快捷键组合被许多应用程序使用，可能存在冲突风险";
                    conflict.Alternatives = GenerateAlternatives(modifiers, virtualKey);
                    return conflict;
                }

                return null;
            }
        }

        public void RegisterHotkey(int id, uint modifiers, uint virtualKey, string actionName)
        {
            lock (_lock)
            {
                _registeredHotkeys.RemoveAll(h => h.Id == id);
                _registeredHotkeys.Add(new RegisteredHotkeyInfo
                {
                    Id = id,
                    Modifiers = modifiers,
                    VirtualKey = virtualKey,
                    ActionName = actionName,
                    RegisteredAt = DateTime.Now
                });
                Logger.Debug("HotkeyConflict", $"[HotkeyConflict] 记录快捷键: {actionName} = {GetHotkeyDisplayText(modifiers, virtualKey)}");
            }
        }

        public void UnregisterHotkey(int id)
        {
            lock (_lock)
            {
                _registeredHotkeys.RemoveAll(h => h.Id == id);
            }
        }

        public IReadOnlyList<RegisteredHotkeyInfo> GetAllRegisteredHotkeys()
        {
            lock (_lock)
            {
                return _registeredHotkeys.ToList().AsReadOnly();
            }
        }

        public List<HotkeyConflict> DetectAllConflicts()
        {
            var conflicts = new List<HotkeyConflict>();
            lock (_lock)
            {
                foreach (var hotkey in _registeredHotkeys)
                {
                    var conflict = DetectConflict(hotkey.Modifiers, hotkey.VirtualKey, hotkey.ActionName);
                    if (conflict != null)
                    {
                        conflict.HotkeyId = hotkey.Id;
                        conflicts.Add(conflict);
                    }
                }
            }
            return conflicts;
        }

        private List<HotkeyAlternative> GenerateAlternatives(uint originalModifiers, uint originalVirtualKey)
        {
            var alternatives = new List<HotkeyAlternative>();

            alternatives.Add(new HotkeyAlternative
            {
                Modifiers = originalModifiers | HotkeyModifiers.MOD_SHIFT,
                VirtualKey = originalVirtualKey,
                DisplayText = GetHotkeyDisplayText(originalModifiers | HotkeyModifiers.MOD_SHIFT, originalVirtualKey),
                Reason = "添加 Shift 键通常可以避免冲突",
                SimilarityScore = 90
            });

            if ((originalModifiers & HotkeyModifiers.MOD_WIN) == 0)
            {
                alternatives.Add(new HotkeyAlternative
                {
                    Modifiers = originalModifiers | HotkeyModifiers.MOD_WIN,
                    VirtualKey = originalVirtualKey,
                    DisplayText = GetHotkeyDisplayText(originalModifiers | HotkeyModifiers.MOD_WIN, originalVirtualKey),
                    Reason = "添加 Win 键降低冲突概率",
                    SimilarityScore = 85
                });
            }

            var alternativeKeys = new[] { 0x70u, 0x71u, 0x72u, 0x73u, 0x74u, 0x75u };
            foreach (var key in alternativeKeys)
            {
                if (key != originalVirtualKey)
                {
                    alternatives.Add(new HotkeyAlternative
                    {
                        Modifiers = originalModifiers,
                        VirtualKey = key,
                        DisplayText = GetHotkeyDisplayText(originalModifiers, key),
                        Reason = "使用功能键降低冲突",
                        SimilarityScore = 70
                    });
                }
            }

            return alternatives
                .Where(a => DetectConflict(a.Modifiers, a.VirtualKey) == null)
                .OrderByDescending(a => a.SimilarityScore)
                .Take(3)
                .ToList();
        }

        private bool IsHighRiskCombination(uint modifiers, uint virtualKey)
        {
            var highRiskModifiers = new[] {
                HotkeyModifiers.MOD_CONTROL,
                HotkeyModifiers.MOD_ALT,
                HotkeyModifiers.MOD_CONTROL | HotkeyModifiers.MOD_ALT
            };

            bool isCommonKey = (virtualKey >= 0x41 && virtualKey <= 0x5A) ||
                               (virtualKey >= 0x30 && virtualKey <= 0x39);

            return highRiskModifiers.Contains(modifiers) && isCommonKey;
        }

        public string GetHotkeyDisplayText(uint modifiers, uint virtualKey)
        {
            var parts = new List<string>();
            if ((modifiers & HotkeyModifiers.MOD_CONTROL) != 0) parts.Add("Ctrl");
            if ((modifiers & HotkeyModifiers.MOD_ALT) != 0) parts.Add("Alt");
            if ((modifiers & HotkeyModifiers.MOD_SHIFT) != 0) parts.Add("Shift");
            if ((modifiers & HotkeyModifiers.MOD_WIN) != 0) parts.Add("Win");
            parts.Add(GetKeyName(virtualKey));
            return string.Join("+", parts);
        }

        private string GetKeyName(uint virtualKey)
        {
            if (virtualKey >= 0x70 && virtualKey <= 0x7B)
                return $"F{virtualKey - 0x6F}";

            return virtualKey switch
            {
                0x25 => "Left",
                0x26 => "Up",
                0x27 => "Right",
                0x28 => "Down",
                0x0D => "Enter",
                0x1B => "Esc",
                0x20 => "Space",
                0x08 => "Backspace",
                0x09 => "Tab",
                0x2E => "Delete",
                0x24 => "Home",
                0x23 => "End",
                0x21 => "PageUp",
                0x22 => "PageDown",
                0x2D => "Insert",
                _ => ((char)virtualKey).ToString().ToUpper()
            };
        }

        public (uint modifiers, uint virtualKey)? ParseHotkey(string hotkeyText)
        {
            try
            {
                var parts = hotkeyText.Split('+', StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => p.Trim().ToLower())
                    .ToList();

                uint modifiers = 0;
                uint? virtualKey = null;

                foreach (var part in parts)
                {
                    switch (part)
                    {
                        case "ctrl":
                        case "control":
                            modifiers |= HotkeyModifiers.MOD_CONTROL;
                            break;
                        case "alt":
                            modifiers |= HotkeyModifiers.MOD_ALT;
                            break;
                        case "shift":
                            modifiers |= HotkeyModifiers.MOD_SHIFT;
                            break;
                        case "win":
                        case "windows":
                        case "command":
                            modifiers |= HotkeyModifiers.MOD_WIN;
                            break;
                        default:
                            virtualKey = ParseKeyName(part);
                            break;
                    }
                }

                if (virtualKey.HasValue)
                    return (modifiers, virtualKey.Value);
            }
            catch (Exception ex)
            {
                Logger.Error("HotkeyConflict", $"[HotkeyConflict] 解析快捷键失败: {ex.Message}");
            }

            return null;
        }

        private uint? ParseKeyName(string name)
        {
            name = name.ToLower();

            if (name.StartsWith("f") && int.TryParse(name[1..], out int fNumber))
            {
                if (fNumber >= 1 && fNumber <= 24)
                    return (uint)(0x6F + fNumber);
            }

            return name switch
            {
                "left" => 0x25,
                "up" => 0x26,
                "right" => 0x27,
                "down" => 0x28,
                "enter" or "return" => 0x0D,
                "esc" or "escape" => 0x1B,
                "space" => 0x20,
                "backspace" => 0x08,
                "tab" => 0x09,
                "delete" or "del" => 0x2E,
                "home" => 0x24,
                "end" => 0x23,
                "pageup" or "pgup" => 0x21,
                "pagedown" or "pgdn" => 0x22,
                "insert" or "ins" => 0x2D,
                _ => name.Length == 1 ? (uint?)char.ToUpper(name[0]) : null
            };
        }
    }
}
