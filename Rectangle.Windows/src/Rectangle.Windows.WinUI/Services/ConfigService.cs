using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Rectangle.Windows.WinUI.Services
{
    /// <summary>
    /// 配置服务 - WinUI 3 版本
    /// </summary>
    public class ConfigService
    {
        private const string ConfigFileName = "config.json";
        private readonly string _configPath;

        public event EventHandler<AppConfig>? ConfigChanged;

        public ConfigService()
        {
            // 使用应用程序数据目录
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appFolder = Path.Combine(appData, "Rectangle");

            if (!Directory.Exists(appFolder))
            {
                Directory.CreateDirectory(appFolder);
            }

            _configPath = Path.Combine(appFolder, ConfigFileName);
        }

        public AppConfig Load()
        {
            if (!File.Exists(_configPath))
            {
                return CreateDefaultConfig();
            }

            try
            {
                var json = File.ReadAllText(_configPath);
                var config = JsonSerializer.Deserialize<AppConfig>(json);
                return config ?? CreateDefaultConfig();
            }
            catch
            {
                return CreateDefaultConfig();
            }
        }

        public async Task<AppConfig> LoadAsync()
        {
            if (!File.Exists(_configPath))
            {
                return CreateDefaultConfig();
            }

            try
            {
                using var stream = File.OpenRead(_configPath);
                var config = await JsonSerializer.DeserializeAsync<AppConfig>(stream);
                return config ?? CreateDefaultConfig();
            }
            catch
            {
                return CreateDefaultConfig();
            }
        }

        public void Save(AppConfig config)
        {
            try
            {
                var dir = Path.GetDirectoryName(_configPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                var json = JsonSerializer.Serialize(config, options);
                File.WriteAllText(_configPath, json);

                ConfigChanged?.Invoke(this, config);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ConfigService] 保存配置失败: {ex.Message}");
            }
        }

        public async Task SaveAsync(AppConfig config)
        {
            try
            {
                var dir = Path.GetDirectoryName(_configPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                using var stream = File.Create(_configPath);
                await JsonSerializer.SerializeAsync(stream, config, options);

                ConfigChanged?.Invoke(this, config);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ConfigService] 保存配置失败: {ex.Message}");
            }
        }

        private AppConfig CreateDefaultConfig()
        {
            return new AppConfig
            {
                Shortcuts = GetDefaultShortcuts()
            };
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
                ["LeftHalf"] = new() { KeyCode = 0x25, ModifierFlags = ctrlAlt },      // Ctrl+Alt+&#x2190;
                ["RightHalf"] = new() { KeyCode = 0x27, ModifierFlags = ctrlAlt },     // Ctrl+Alt+&#x2192;
                ["TopHalf"] = new() { KeyCode = 0x26, ModifierFlags = ctrlAlt },       // Ctrl+Alt+&#x2191;
                ["BottomHalf"] = new() { KeyCode = 0x28, ModifierFlags = ctrlAlt },    // Ctrl+Alt+&#x2193;
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
                ["MaximizeHeight"] = new() { KeyCode = 0x26, ModifierFlags = ctrlAltShift }, // Ctrl+Alt+Shift+&#x2191;
                ["Larger"] = new() { KeyCode = 0xBB, ModifierFlags = ctrlAlt },        // Ctrl+Alt+=
                ["Smaller"] = new() { KeyCode = 0xBD, ModifierFlags = ctrlAlt },       // Ctrl+Alt+-
                ["Center"] = new() { KeyCode = 0x43, ModifierFlags = ctrlAlt },        // Ctrl+Alt+C
                ["Restore"] = new() { KeyCode = 0x08, ModifierFlags = ctrlAlt },       // Ctrl+Alt+Backspace
                // 显示器
                ["PreviousDisplay"] = new() { KeyCode = 0x25, ModifierFlags = ctrlAltWin }, // Ctrl+Alt+Win+&#x2190;
                ["NextDisplay"] = new() { KeyCode = 0x27, ModifierFlags = ctrlAltWin },    // Ctrl+Alt+Win+&#x2192;
                // 四等分 (默认禁用)
                ["FirstFourth"] = new() { KeyCode = 0, ModifierFlags = 0, Enabled = false },
                ["SecondFourth"] = new() { KeyCode = 0, ModifierFlags = 0, Enabled = false },
                ["ThirdFourth"] = new() { KeyCode = 0, ModifierFlags = 0, Enabled = false },
                ["LastFourth"] = new() { KeyCode = 0, ModifierFlags = 0, Enabled = false },
                // 六等分 (默认禁用)
                ["TopLeftSixth"] = new() { KeyCode = 0, ModifierFlags = 0, Enabled = false },
                ["TopCenterSixth"] = new() { KeyCode = 0, ModifierFlags = 0, Enabled = false },
                ["TopRightSixth"] = new() { KeyCode = 0, ModifierFlags = 0, Enabled = false },
                ["BottomLeftSixth"] = new() { KeyCode = 0, ModifierFlags = 0, Enabled = false },
                ["BottomCenterSixth"] = new() { KeyCode = 0, ModifierFlags = 0, Enabled = false },
                ["BottomRightSixth"] = new() { KeyCode = 0, ModifierFlags = 0, Enabled = false },
            };
        }
    }

    /// <summary>
    /// 应用程序配置
    /// </summary>
    public class AppConfig
    {
        public int GapSize { get; set; } = 0;
        public bool LaunchOnLogin { get; set; } = false;
        public List<string> IgnoredApps { get; set; } = new() { "Rectangle.Windows.exe" };
        public Dictionary<string, ShortcutConfig> Shortcuts { get; set; } = new();
        public SnapAreaConfig SnapAreas { get; set; } = new();
        public int SubsequentExecutionMode { get; set; } = 0;
        public float AlmostMaximizeHeight { get; set; } = 0.9f;
        public float AlmostMaximizeWidth { get; set; } = 0.9f;
        public float MinimumWindowWidth { get; set; } = 0;
        public float MinimumWindowHeight { get; set; } = 0;
        public float SizeOffset { get; set; } = 30;
        public bool CenteredDirectionalMove { get; set; } = false;
        public bool ResizeOnDirectionalMove { get; set; } = false;
        public bool UseCursorScreenDetection { get; set; } = false;
        public bool MoveCursor { get; set; } = false;
        public bool MoveCursorAcrossDisplays { get; set; } = false;
        public float FootprintAlpha { get; set; } = 0.3f;
        public int FootprintBorderWidth { get; set; } = 2;
        public int FootprintColor { get; set; } = -16711614;
        public int FootprintBorderColor { get; set; } = -16711614;
        public bool FootprintFade { get; set; } = true;
        public int FootprintAnimationDuration { get; set; } = 150;
        public bool UnsnapRestore { get; set; } = true;
        public bool DragToSnap { get; set; } = true;
        public int SnapEdgeMarginTop { get; set; } = 5;
        public int SnapEdgeMarginBottom { get; set; } = 5;
        public int SnapEdgeMarginLeft { get; set; } = 5;
        public int SnapEdgeMarginRight { get; set; } = 5;
        public int CornerSnapAreaSize { get; set; } = 20;
        public int SnapModifiers { get; set; } = 0;
        public bool HapticFeedbackOnSnap { get; set; } = false;
        public bool TodoMode { get; set; } = false;
        public string TodoApplication { get; set; } = "";
        public int TodoSidebarWidth { get; set; } = 400;
        public int TodoSidebarSide { get; set; } = 1;
        public int CascadeAllDeltaSize { get; set; } = 30;
        public int LogLevel { get; set; } = 1;
        public bool LogToFile { get; set; } = false;
        public string LogFilePath { get; set; } = "";
        public int MaxLogFileSize { get; set; } = 10;
        public int MaxWindowHistoryCount { get; set; } = 100;
        public int WindowHistoryExpirationMinutes { get; set; } = 60;
        public int SpecifiedWidth { get; set; } = 1680;
        public int SpecifiedHeight { get; set; } = 1050;

        // === 主题设置 ===
        public string Theme { get; set; } = "Default"; // "Default", "Dark", "Light"

        // === 动画设置 ===
        public AnimationConfig Animation { get; set; } = new();

        // === 操作历史设置 ===
        public HistoryConfig History { get; set; } = new();

        // === 边缘指示器设置 ===
        public EdgeIndicatorSettings EdgeIndicator { get; set; } = new();

        // === 快捷键冲突检测设置 ===
        public ConflictDetectionConfig ConflictDetection { get; set; } = new();

        // === DPI设置 ===
        public DpiConfig Dpi { get; set; } = new();

        // === 统计设置 ===
        public StatisticsConfig Statistics { get; set; } = new();
    }

    /// <summary>
    /// 动画配置
    /// </summary>
    public class AnimationConfig
    {
        public bool Enabled { get; set; } = true;
        public int DurationMs { get; set; } = 200;
        public int FrameRate { get; set; } = 60;
        public string EasingType { get; set; } = "EaseOutCubic";
        public bool EnableMoveAnimation { get; set; } = true;
        public bool EnableResizeAnimation { get; set; } = true;
        public bool EnableHotkeyFeedback { get; set; } = true;
        public int HotkeyFeedbackDurationMs { get; set; } = 800;
    }

    /// <summary>
    /// 历史配置
    /// </summary>
    public class HistoryConfig
    {
        public bool Enabled { get; set; } = true;
        public int MaxHistoryCount { get; set; } = 50;
        public bool EnableUndo { get; set; } = true;
        public string UndoShortcut { get; set; } = "Ctrl+Alt+Z";
        public string RedoShortcut { get; set; } = "Ctrl+Alt+Shift+Z";
    }

    /// <summary>
    /// 边缘指示器设置
    /// </summary>
    public class EdgeIndicatorSettings
    {
        public bool Enabled { get; set; } = false;
        public int IndicatorWidth { get; set; } = 8;
        public string DisplayMode { get; set; } = "AutoHide"; // AlwaysVisible, AutoHide, DragOnly, Onboarding
        public int AutoHideDelayMs { get; set; } = 2000;
        public int TriggerDistance { get; set; } = 10;
        public bool ShowSnapAreas { get; set; } = true;
        public double SnapAreaOpacity { get; set; } = 0.15;
        public string NormalColor { get; set; } = "#500078D7";
        public string HoverColor { get; set; } = "#B40096FF";
        public string ActiveColor { get; set; } = "#FF00B4FF";
    }

    /// <summary>
    /// 快捷键冲突检测配置
    /// </summary>
    public class ConflictDetectionConfig
    {
        public bool Enabled { get; set; } = true;
        public bool ShowWarnings { get; set; } = true;
        public bool AutoSuggestAlternatives { get; set; } = true;
        public bool CheckSystemHotkeys { get; set; } = true;
        public bool CheckKnownApps { get; set; } = true;
    }

    /// <summary>
    /// DPI配置
    /// </summary>
    public class DpiConfig
    {
        public bool EnablePerMonitorDpi { get; set; } = true;
        public bool EnableDpiScaling { get; set; } = true;
        public float FallbackDpi { get; set; } = 96f;
    }

    /// <summary>
    /// 统计配置
    /// </summary>
    public class StatisticsConfig
    {
        public bool Enabled { get; set; } = true;
        public int MaxRetentionDays { get; set; } = 90;
        public bool TrackWindowUsage { get; set; } = true;
        public bool TrackLayoutUsage { get; set; } = true;
        public bool GenerateHeatmap { get; set; } = true;
        public int MaxHeatmapPoints { get; set; } = 10000;
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
}
