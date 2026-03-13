using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Rectangle.Windows.WinUI.Services
{
    /// <summary>
    /// 快捷键冲突信息
    /// </summary>
    public class HotkeyConflict
    {
        /// <summary>
        /// 快捷键ID
        /// </summary>
        public int HotkeyId { get; set; }

        /// <summary>
        /// 修饰键
        /// </summary>
        public HOT_KEY_MODIFIERS Modifiers { get; set; }

        /// <summary>
        /// 虚拟键码
        /// </summary>
        public uint VirtualKey { get; set; }

        /// <summary>
        /// 快捷键显示文本
        /// </summary>
        public string DisplayText { get; set; } = "";

        /// <summary>
        /// 冲突的应用程序名称
        /// </summary>
        public string? ConflictingApp { get; set; }

        /// <summary>
        /// 冲突类型
        /// </summary>
        public ConflictType Type { get; set; }

        /// <summary>
        /// 冲突描述
        /// </summary>
        public string Description { get; set; } = "";

        /// <summary>
        /// 推荐的替代快捷键
        /// </summary>
        public List<HotkeyAlternative> Alternatives { get; set; } = new();
    }

    /// <summary>
    /// 冲突类型
    /// </summary>
    public enum ConflictType
    {
        /// <summary>
        /// 与Windows系统快捷键冲突
        /// </summary>
        SystemConflict,

        /// <summary>
        /// 与其他应用程序冲突
        /// </summary>
        ApplicationConflict,

        /// <summary>
        /// 与内部快捷键冲突（重复注册）
        /// </summary>
        InternalConflict,

        /// <summary>
        /// 与系统保留快捷键冲突
        /// </summary>
        ReservedConflict,

        /// <summary>
        /// 潜在冲突（可能与其他应用冲突）
        /// </summary>
        PotentialConflict
    }

    /// <summary>
    /// 快捷键替代方案
    /// </summary>
    public class HotkeyAlternative
    {
        /// <summary>
        /// 修饰键
        /// </summary>
        public HOT_KEY_MODIFIERS Modifiers { get; set; }

        /// <summary>
        /// 虚拟键码
        /// </summary>
        public uint VirtualKey { get; set; }

        /// <summary>
        /// 显示文本
        /// </summary>
        public string DisplayText { get; set; } = "";

        /// <summary>
        /// 推荐理由
        /// </summary>
        public string Reason { get; set; } = "";

        /// <summary>
        /// 相似度评分（0-100）
        /// </summary>
        public int SimilarityScore { get; set; }
    }

    /// <summary>
    /// 已注册的快捷键信息
    /// </summary>
    public class RegisteredHotkeyInfo
    {
        public int Id { get; set; }
        public HOT_KEY_MODIFIERS Modifiers { get; set; }
        public uint VirtualKey { get; set; }
        public string? ActionName { get; set; }
        public DateTime RegisteredAt { get; set; }
    }

    /// <summary>
    /// 快捷键冲突检测服务
    /// </summary>
    public class HotkeyConflictDetector
    {
        private readonly Logger _logger;
        private readonly List<RegisteredHotkeyInfo> _registeredHotkeys = new();
        private readonly Dictionary<(HOT_KEY_MODIFIERS, uint), List<string>> _knownConflicts = new();
        private readonly object _lock = new();

        // 系统保留的快捷键
        private static readonly Dictionary<(HOT_KEY_MODIFIERS, uint), string> SystemReservedHotkeys = new()
        {
            // Windows 系统快捷键
            { (HOT_KEY_MODIFIERS.MOD_WIN, 0x44), "显示桌面 (Win+D)" },
            { (HOT_KEY_MODIFIERS.MOD_WIN, 0x45), "文件资源管理器 (Win+E)" },
            { (HOT_KEY_MODIFIERS.MOD_WIN, 0x46), "搜索 (Win+F)" },
            { (HOT_KEY_MODIFIERS.MOD_WIN, 0x49), "设置 (Win+I)" },
            { (HOT_KEY_MODIFIERS.MOD_WIN, 0x4C), "锁定 (Win+L)" },
            { (HOT_KEY_MODIFIERS.MOD_WIN, 0x52), "运行 (Win+R)" },
            { (HOT_KEY_MODIFIERS.MOD_WIN, 0x53), "搜索/截图 (Win+S)" },
            { (HOT_KEY_MODIFIERS.MOD_WIN, 0x58), "快速链接菜单 (Win+X)" },
            { (HOT_KEY_MODIFIERS.MOD_WIN, 0x1B), "关闭窗口 (Win+Esc)" },
            { (HOT_KEY_MODIFIERS.MOD_WIN, 0x20), "切换输入法 (Win+Space)" },

            // Ctrl+Alt+Del 系列（系统保留，无法注册）
            { (HOT_KEY_MODIFIERS.MOD_CONTROL | HOT_KEY_MODIFIERS.MOD_ALT, 0x2E), "安全选项 (Ctrl+Alt+Del)" },

            // 常见的第三方应用快捷键
            { (HOT_KEY_MODIFIERS.MOD_CONTROL | HOT_KEY_MODIFIERS.MOD_ALT, 0x54), "任务管理器 (Ctrl+Alt+T 可能冲突)" },
            { (HOT_KEY_MODIFIERS.MOD_CONTROL | HOT_KEY_MODIFIERS.MOD_SHIFT, 0x45), "某些编辑器的运行快捷键" },
        };

        // 已知的可能冲突应用
        private static readonly Dictionary<string, List<(HOT_KEY_MODIFIERS, uint, string)>> KnownAppHotkeys = new()
        {
            ["PowerToys"] = new()
            {
                (HOT_KEY_MODIFIERS.MOD_WIN | HOT_KEY_MODIFIERS.MOD_SHIFT, 0x60, "FancyZones 编辑器"),
                (HOT_KEY_MODIFIERS.MOD_WIN, 0x60, "快速布局切换")
            },
            ["AutoHotkey"] = new()
            {
                (HOT_KEY_MODIFIERS.MOD_CONTROL | HOT_KEY_MODIFIERS.MOD_ALT, 0, "多种用户自定义快捷键")
            },
            ["Snagit"] = new()
            {
                (HOT_KEY_MODIFIERS.MOD_CONTROL | HOT_KEY_MODIFIERS.MOD_SHIFT, 0x5A, "截图")
            },
            ["ShareX"] = new()
            {
                (HOT_KEY_MODIFIERS.MOD_CONTROL | HOT_KEY_MODIFIERS.MOD_SHIFT, 0x58, "截图")
            },
            ["Everything"] = new()
            {
                (HOT_KEY_MODIFIERS.MOD_WIN, 0x53, "搜索")
            },
            ["Microsoft Teams"] = new()
            {
                (HOT_KEY_MODIFIERS.MOD_CONTROL | HOT_KEY_MODIFIERS.MOD_SHIFT, 0x4D, "静音")
            }
        };

        public HotkeyConflictDetector(Logger logger)
        {
            _logger = logger;
            InitializeKnownConflicts();
        }

        /// <summary>
        /// 初始化已知冲突数据库
        /// </summary>
        private void InitializeKnownConflicts()
        {
            foreach (var kvp in KnownAppHotkeys)
            {
                foreach (var (modifiers, vk, _) in kvp.Value)
                {
                    var key = (modifiers, vk);
                    if (!_knownConflicts.ContainsKey(key))
                    {
                        _knownConflicts[key] = new List<string>();
                    }
                    if (!_knownConflicts[key].Contains(kvp.Key))
                    {
                        _knownConflicts[key].Add(kvp.Key);
                    }
                }
            }
        }

        /// <summary>
        /// 检测快捷键冲突
        /// </summary>
        public HotkeyConflict? DetectConflict(HOT_KEY_MODIFIERS modifiers, uint virtualKey, string? actionName = null)
        {
            lock (_lock)
            {
                var conflict = new HotkeyConflict
                {
                    Modifiers = modifiers,
                    VirtualKey = virtualKey,
                    DisplayText = GetHotkeyDisplayText(modifiers, virtualKey)
                };

                // 1. 检查系统保留快捷键
                if (SystemReservedHotkeys.TryGetValue((modifiers, virtualKey), out var systemAction))
                {
                    conflict.Type = ConflictType.SystemConflict;
                    conflict.Description = $"与 Windows 系统快捷键冲突: {systemAction}";
                    conflict.ConflictingApp = "Windows";
                    conflict.Alternatives = GenerateAlternatives(modifiers, virtualKey);
                    return conflict;
                }

                // 2. 检查内部冲突（重复注册）
                if (_registeredHotkeys.Any(h =>
                    h.Modifiers == modifiers &&
                    h.VirtualKey == virtualKey &&
                    h.ActionName != actionName))
                {
                    conflict.Type = ConflictType.InternalConflict;
                    conflict.Description = "此快捷键已在 Rectangle 中注册用于其他操作";
                    conflict.Alternatives = GenerateAlternatives(modifiers, virtualKey);
                    return conflict;
                }

                // 3. 检查已知应用冲突
                if (_knownConflicts.TryGetValue((modifiers, virtualKey), out var conflictingApps))
                {
                    conflict.Type = ConflictType.ApplicationConflict;
                    conflict.ConflictingApp = string.Join(", ", conflictingApps);
                    conflict.Description = $"可能与以下应用程序冲突: {conflict.ConflictingApp}";
                    conflict.Alternatives = GenerateAlternatives(modifiers, virtualKey);
                    return conflict;
                }

                // 4. 检查是否为高冲突风险组合
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

        /// <summary>
        /// 注册快捷键记录
        /// </summary>
        public void RegisterHotkey(int id, HOT_KEY_MODIFIERS modifiers, uint virtualKey, string actionName)
        {
            lock (_lock)
            {
                // 移除同ID的旧记录
                _registeredHotkeys.RemoveAll(h => h.Id == id);

                _registeredHotkeys.Add(new RegisteredHotkeyInfo
                {
                    Id = id,
                    Modifiers = modifiers,
                    VirtualKey = virtualKey,
                    ActionName = actionName,
                    RegisteredAt = DateTime.Now
                });

                _logger?.LogDebug($"[HotkeyConflict] 记录快捷键: {actionName} = {GetHotkeyDisplayText(modifiers, virtualKey)}");
            }
        }

        /// <summary>
        /// 注销快捷键记录
        /// </summary>
        public void UnregisterHotkey(int id)
        {
            lock (_lock)
            {
                _registeredHotkeys.RemoveAll(h => h.Id == id);
            }
        }

        /// <summary>
        /// 获取所有已注册快捷键
        /// </summary>
        public IReadOnlyList<RegisteredHotkeyInfo> GetAllRegisteredHotkeys()
        {
            lock (_lock)
            {
                return _registeredHotkeys.ToList().AsReadOnly();
            }
        }

        /// <summary>
        /// 检测所有已注册快捷键的冲突
        /// </summary>
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

        /// <summary>
        /// 生成替代快捷键建议
        /// </summary>
        private List<HotkeyAlternative> GenerateAlternatives(HOT_KEY_MODIFIERS originalModifiers, uint originalVirtualKey)
        {
            var alternatives = new List<HotkeyAlternative>();

            // 1. 尝试添加 Shift 修饰键
            alternatives.Add(new HotkeyAlternative
            {
                Modifiers = originalModifiers | HOT_KEY_MODIFIERS.MOD_SHIFT,
                VirtualKey = originalVirtualKey,
                DisplayText = GetHotkeyDisplayText(originalModifiers | HOT_KEY_MODIFIERS.MOD_SHIFT, originalVirtualKey),
                Reason = "添加 Shift 键通常可以避免冲突",
                SimilarityScore = 90
            });

            // 2. 尝试添加 Win 修饰键
            if ((originalModifiers & HOT_KEY_MODIFIERS.MOD_WIN) == 0)
            {
                alternatives.Add(new HotkeyAlternative
                {
                    Modifiers = originalModifiers | HOT_KEY_MODIFIERS.MOD_WIN,
                    VirtualKey = originalVirtualKey,
                    DisplayText = GetHotkeyDisplayText(originalModifiers | HOT_KEY_MODIFIERS.MOD_WIN, originalVirtualKey),
                    Reason = "添加 Win 键降低冲突概率",
                    SimilarityScore = 85
                });
            }

            // 3. 尝试更换为不常用的功能键
            var alternativeKeys = new[] { 0x70, 0x71, 0x72, 0x73, 0x74, 0x75 }; // F1-F6
            foreach (var key in alternativeKeys)
            {
                if (key != originalVirtualKey)
                {
                    alternatives.Add(new HotkeyAlternative
                    {
                        Modifiers = originalModifiers,
                        VirtualKey = (uint)key,
                        DisplayText = GetHotkeyDisplayText(originalModifiers, (uint)key),
                        Reason = "使用功能键降低冲突",
                        SimilarityScore = 70
                    });
                }
            }

            // 过滤掉仍然冲突的方案
            alternatives = alternatives
                .Where(a => DetectConflict(a.Modifiers, a.VirtualKey) == null)
                .OrderByDescending(a => a.SimilarityScore)
                .Take(3)
                .ToList();

            return alternatives;
        }

        /// <summary>
        /// 检查是否为高冲突风险组合
        /// </summary>
        private bool IsHighRiskCombination(HOT_KEY_MODIFIERS modifiers, uint virtualKey)
        {
            // 常见的冲突组合
            var highRiskModifiers = new[] {
                HOT_KEY_MODIFIERS.MOD_CONTROL,
                HOT_KEY_MODIFIERS.MOD_ALT,
                HOT_KEY_MODIFIERS.MOD_CONTROL | HOT_KEY_MODIFIERS.MOD_ALT
            };

            // 常见的冲突按键（字母、数字）
            bool isCommonKey = (virtualKey >= 0x41 && virtualKey <= 0x5A) || // A-Z
                              (virtualKey >= 0x30 && virtualKey <= 0x39);   // 0-9

            return highRiskModifiers.Contains(modifiers) && isCommonKey;
        }

        /// <summary>
        /// 获取快捷键显示文本
        /// </summary>
        public string GetHotkeyDisplayText(HOT_KEY_MODIFIERS modifiers, uint virtualKey)
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

            parts.Add(GetKeyName(virtualKey));

            return string.Join("+", parts);
        }

        /// <summary>
        /// 获取按键名称
        /// </summary>
        private string GetKeyName(uint virtualKey)
        {
            // 功能键
            if (virtualKey >= 0x70 && virtualKey <= 0x7B)
                return $"F{virtualKey - 0x6F}";

            // 方向键
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

        /// <summary>
        /// 解析快捷键字符串
        /// </summary>
        public (HOT_KEY_MODIFIERS modifiers, uint virtualKey)? ParseHotkey(string hotkeyText)
        {
            try
            {
                var parts = hotkeyText.Split('+', StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => p.Trim().ToLower())
                    .ToList();

                HOT_KEY_MODIFIERS modifiers = 0;
                uint? virtualKey = null;

                foreach (var part in parts)
                {
                    switch (part)
                    {
                        case "ctrl":
                        case "control":
                            modifiers |= HOT_KEY_MODIFIERS.MOD_CONTROL;
                            break;
                        case "alt":
                            modifiers |= HOT_KEY_MODIFIERS.MOD_ALT;
                            break;
                        case "shift":
                            modifiers |= HOT_KEY_MODIFIERS.MOD_SHIFT;
                            break;
                        case "win":
                        case "windows":
                        case "command":
                            modifiers |= HOT_KEY_MODIFIERS.MOD_WIN;
                            break;
                        default:
                            virtualKey = ParseKeyName(part);
                            break;
                    }
                }

                if (virtualKey.HasValue)
                {
                    return (modifiers, virtualKey.Value);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError($"[HotkeyConflict] 解析快捷键失败: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// 解析按键名称
        /// </summary>
        private uint? ParseKeyName(string name)
        {
            name = name.ToLower();

            // 功能键
            if (name.StartsWith("f") && int.TryParse(name[1..], out int fNumber))
            {
                if (fNumber >= 1 && fNumber <= 24)
                    return (uint)(0x6F + fNumber);
            }

            // 特殊键
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
                _ => name.Length == 1 ? char.ToUpper(name[0]) : null
            };
        }
    }
}
