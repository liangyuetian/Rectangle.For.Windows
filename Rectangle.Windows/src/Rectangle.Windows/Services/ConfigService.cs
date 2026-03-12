using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using Rectangle.Windows.Core;

namespace Rectangle.Windows.Services;

public class ConfigService
{
    private const string ConfigFileName = "config.json";
    private readonly string _configPath;

    public ConfigService()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _configPath = Path.Combine(appData, "Rectangle", ConfigFileName);
    }

    public AppConfig Load()
    {
        if (!File.Exists(_configPath))
        {
            return new AppConfig();
        }

        try
        {
            var json = File.ReadAllText(_configPath);
            return JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
        }
        catch
        {
            return new AppConfig();
        }
    }

    public void Save(AppConfig config)
    {
        var dir = Path.GetDirectoryName(_configPath);
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }

        var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_configPath, json);
    }

    public static Dictionary<string, ShortcutConfig> GetDefaultShortcuts()
    {
        // 修饰键: Ctrl=2, Alt=1, Shift=4, Win=8
        const uint MOD_CONTROL = 0x0002;
        const uint MOD_ALT = 0x0001;
        const uint MOD_SHIFT = 0x0004;
        const uint MOD_WIN = 0x0008;
        const uint ctrlAlt = MOD_CONTROL | MOD_ALT;
        const uint ctrlAltShift = ctrlAlt | MOD_SHIFT;
        const uint ctrlAltWin = ctrlAlt | MOD_WIN;

        return new Dictionary<string, ShortcutConfig>
        {
            // 半屏
            ["LeftHalf"] = new() { KeyCode = 0x25, ModifierFlags = ctrlAlt },      // Ctrl+Alt+←
            ["RightHalf"] = new() { KeyCode = 0x27, ModifierFlags = ctrlAlt },     // Ctrl+Alt+→
            ["TopHalf"] = new() { KeyCode = 0x26, ModifierFlags = ctrlAlt },       // Ctrl+Alt+↑
            ["BottomHalf"] = new() { KeyCode = 0x28, ModifierFlags = ctrlAlt },    // Ctrl+Alt+↓
            ["CenterHalf"] = new() { KeyCode = 0, ModifierFlags = 0, Enabled = false },
            // 四角
            ["TopLeft"] = new() { KeyCode = 0x55, ModifierFlags = ctrlAlt },       // Ctrl+Alt+U
            ["TopRight"] = new() { KeyCode = 0x49, ModifierFlags = ctrlAlt },      // Ctrl+Alt+I
            ["BottomLeft"] = new() { KeyCode = 0x4A, ModifierFlags = ctrlAlt },    // Ctrl+Alt+J
            ["BottomRight"] = new() { KeyCode = 0x4B, ModifierFlags = ctrlAlt },   // Ctrl+Alt+K
            // 三分之一
            ["FirstThird"] = new() { KeyCode = 0x44, ModifierFlags = ctrlAlt },    // Ctrl+Alt+D
            ["CenterThird"] = new() { KeyCode = 0x46, ModifierFlags = ctrlAlt },   // Ctrl+Alt+F
            ["LastThird"] = new() { KeyCode = 0x47, ModifierFlags = ctrlAlt },     // Ctrl+Alt+G
            ["FirstTwoThirds"] = new() { KeyCode = 0x45, ModifierFlags = ctrlAlt },// Ctrl+Alt+E
            ["CenterTwoThirds"] = new() { KeyCode = 0x52, ModifierFlags = ctrlAlt },// Ctrl+Alt+R
            ["LastTwoThirds"] = new() { KeyCode = 0x54, ModifierFlags = ctrlAlt }, // Ctrl+Alt+T
            // 最大化与恢复
            ["Maximize"] = new() { KeyCode = 0x0D, ModifierFlags = ctrlAlt },      // Ctrl+Alt+Enter
            ["AlmostMaximize"] = new() { KeyCode = 0, ModifierFlags = 0, Enabled = false },
            ["MaximizeHeight"] = new() { KeyCode = 0x26, ModifierFlags = ctrlAltShift }, // Ctrl+Alt+Shift+↑
            ["Larger"] = new() { KeyCode = 0xBB, ModifierFlags = ctrlAlt },        // Ctrl+Alt+=
            ["Smaller"] = new() { KeyCode = 0xBD, ModifierFlags = ctrlAlt },       // Ctrl+Alt+-
            ["Center"] = new() { KeyCode = 0x43, ModifierFlags = ctrlAlt },        // Ctrl+Alt+C
            ["Restore"] = new() { KeyCode = 0x08, ModifierFlags = ctrlAlt },       // Ctrl+Alt+Backspace
            // 显示器
            ["PreviousDisplay"] = new() { KeyCode = 0x25, ModifierFlags = ctrlAltWin }, // Ctrl+Alt+Win+←
            ["NextDisplay"] = new() { KeyCode = 0x27, ModifierFlags = ctrlAltWin },    // Ctrl+Alt+Win+→
            // 四等分 (默认禁用)
            ["FirstFourth"] = new() { KeyCode = 0, ModifierFlags = 0, Enabled = false },
            ["SecondFourth"] = new() { KeyCode = 0, ModifierFlags = 0, Enabled = false },
            ["ThirdFourth"] = new() { KeyCode = 0, ModifierFlags = 0, Enabled = false },
            ["LastFourth"] = new() { KeyCode = 0, ModifierFlags = 0, Enabled = false },
            ["FirstThreeFourths"] = new() { KeyCode = 0, ModifierFlags = 0, Enabled = false },
            ["CenterThreeFourths"] = new() { KeyCode = 0, ModifierFlags = 0, Enabled = false },
            ["LastThreeFourths"] = new() { KeyCode = 0, ModifierFlags = 0, Enabled = false },
            // 六等分 (默认禁用)
            ["TopLeftSixth"] = new() { KeyCode = 0, ModifierFlags = 0, Enabled = false },
            ["TopCenterSixth"] = new() { KeyCode = 0, ModifierFlags = 0, Enabled = false },
            ["TopRightSixth"] = new() { KeyCode = 0, ModifierFlags = 0, Enabled = false },
            ["BottomLeftSixth"] = new() { KeyCode = 0, ModifierFlags = 0, Enabled = false },
            ["BottomCenterSixth"] = new() { KeyCode = 0, ModifierFlags = 0, Enabled = false },
            ["BottomRightSixth"] = new() { KeyCode = 0, ModifierFlags = 0, Enabled = false },
            // 移动到边缘 (默认禁用)
            ["MoveLeft"] = new() { KeyCode = 0, ModifierFlags = 0, Enabled = false },
            ["MoveRight"] = new() { KeyCode = 0, ModifierFlags = 0, Enabled = false },
            ["MoveUp"] = new() { KeyCode = 0, ModifierFlags = 0, Enabled = false },
            ["MoveDown"] = new() { KeyCode = 0, ModifierFlags = 0, Enabled = false },
        };
    }
}

public class AppConfig
{
    public int GapSize { get; set; } = 0;
    public bool LaunchOnLogin { get; set; } = false;
    public List<string> IgnoredApps { get; set; } = GetDefaultIgnoredApps();
    public Dictionary<string, ShortcutConfig> Shortcuts { get; set; } = new();
    public SnapAreaConfig SnapAreas { get; set; } = new();
    
    /// <summary>
    /// 重复执行模式：连续按同一快捷键时的行为
    /// </summary>
    public SubsequentExecutionMode SubsequentExecutionMode { get; set; } = SubsequentExecutionMode.CycleSize;

    private static List<string> GetDefaultIgnoredApps()
    {
        return new List<string>
        {
            "Rectangle.Windows.exe"
        };
    }
}

public class ShortcutConfig
{
    public bool Enabled { get; set; } = true;
    public int KeyCode { get; set; }
    public uint ModifierFlags { get; set; }
}

public class SnapAreaConfig
{
    public bool DragToSnap { get; set; } = true;
    public bool RestoreSizeOnSnapEnd { get; set; } = true;
    public bool HapticFeedback { get; set; } = false;
    public bool SnapAnimation { get; set; } = false;
    public Dictionary<string, string> AreaActions { get; set; } = new();
}
