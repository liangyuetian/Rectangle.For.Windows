using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;

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
        // 修饰键: Ctrl=2, Alt=1, Ctrl+Alt=3, Ctrl+Alt+Win=11
        const uint MOD_CONTROL = 0x0002;
        const uint MOD_ALT = 0x0001;
        const uint MOD_WIN = 0x0008;
        const uint ctrlAlt = MOD_CONTROL | MOD_ALT;
        const uint ctrlAltWin = ctrlAlt | MOD_WIN;

        return new Dictionary<string, ShortcutConfig>
        {
            // 半屏
            ["LeftHalf"] = new() { KeyCode = 0x25, ModifierFlags = ctrlAlt },
            ["RightHalf"] = new() { KeyCode = 0x27, ModifierFlags = ctrlAlt },
            ["TopHalf"] = new() { KeyCode = 0x26, ModifierFlags = ctrlAlt },
            ["BottomHalf"] = new() { KeyCode = 0x28, ModifierFlags = ctrlAlt },
            // 四角
            ["TopLeft"] = new() { KeyCode = 0x55, ModifierFlags = ctrlAlt },
            ["TopRight"] = new() { KeyCode = 0x49, ModifierFlags = ctrlAlt },
            ["BottomLeft"] = new() { KeyCode = 0x4A, ModifierFlags = ctrlAlt },
            ["BottomRight"] = new() { KeyCode = 0x4B, ModifierFlags = ctrlAlt },
            // 三分之一
            ["FirstThird"] = new() { KeyCode = 0x44, ModifierFlags = ctrlAlt },
            ["CenterThird"] = new() { KeyCode = 0x46, ModifierFlags = ctrlAlt },
            ["LastThird"] = new() { KeyCode = 0x47, ModifierFlags = ctrlAlt },
            ["FirstTwoThirds"] = new() { KeyCode = 0x45, ModifierFlags = ctrlAlt },
            ["CenterTwoThirds"] = new() { KeyCode = 0x52, ModifierFlags = ctrlAlt },
            ["LastTwoThirds"] = new() { KeyCode = 0x54, ModifierFlags = ctrlAlt },
            // 最大化与恢复
            ["Maximize"] = new() { KeyCode = 0x0D, ModifierFlags = ctrlAlt },
            ["Restore"] = new() { KeyCode = 0x08, ModifierFlags = ctrlAlt },
            ["Center"] = new() { KeyCode = 0x43, ModifierFlags = ctrlAlt },
            // 显示器
            ["PreviousDisplay"] = new() { KeyCode = 0x25, ModifierFlags = ctrlAltWin },
            ["NextDisplay"] = new() { KeyCode = 0x27, ModifierFlags = ctrlAltWin },
        };
    }
}

public class AppConfig
{
    public int GapSize { get; set; } = 0;
    public bool LaunchOnLogin { get; set; } = false;
    public List<string> IgnoredApps { get; set; } = new();
    public Dictionary<string, ShortcutConfig> Shortcuts { get; set; } = new();
    public SnapAreaConfig SnapAreas { get; set; } = new();
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
