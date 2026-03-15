using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Rectangle.Windows.WinUI.Services;

namespace Rectangle.Windows.WinUI.ViewModels
{
    public class SettingsViewModel : ObservableObject
    {
        private readonly ConfigService _configService;
        private bool _launchOnLogin;
        private int _gapSize;
        private bool _logToFile;
        private int _logLevel;
        private ElementTheme _currentTheme;

        public ObservableCollection<ShortcutItem> HalfScreenShortcuts { get; } = new();
        public ObservableCollection<ShortcutItem> CornerShortcuts { get; } = new();
        public ObservableCollection<ShortcutItem> ThirdShortcuts { get; } = new();
        public ObservableCollection<ShortcutItem> FourthShortcuts { get; } = new();
        public ObservableCollection<ShortcutItem> SixthShortcuts { get; } = new();
        public ObservableCollection<ShortcutItem> EighthShortcuts { get; } = new();
        public ObservableCollection<ShortcutItem> NinthShortcuts { get; } = new();
        public ObservableCollection<ShortcutItem> MaximizeShortcuts { get; } = new();
        public ObservableCollection<ShortcutItem> ResizeShortcuts { get; } = new();
        public ObservableCollection<ShortcutItem> MoveShortcuts { get; } = new();
        public ObservableCollection<ShortcutItem> DisplayShortcuts { get; } = new();
        public ObservableCollection<ShortcutItem> OtherShortcuts { get; } = new();

        public bool LaunchOnLogin
        {
            get => _launchOnLogin;
            set { if (SetProperty(ref _launchOnLogin, value)) SaveLaunchOnLoginSetting(); }
        }

        public int GapSize
        {
            get => _gapSize;
            set { if (SetProperty(ref _gapSize, value)) SaveGapSizeSetting(); }
        }

        public bool LogToFile
        {
            get => _logToFile;
            set { if (SetProperty(ref _logToFile, value)) SaveLogSettings(); }
        }

        public int LogLevel
        {
            get => _logLevel;
            set { if (SetProperty(ref _logLevel, value)) SaveLogSettings(); }
        }

        public ElementTheme CurrentTheme
        {
            get => _currentTheme;
            set { if (SetProperty(ref _currentTheme, value)) SaveThemeSetting(); }
        }

        public SettingsViewModel()
        {
            _configService = new ConfigService();
            InitializeDefaultShortcuts();
        }

        private void InitializeDefaultShortcuts()
        {
            HalfScreenShortcuts.Add(new ShortcutItem { Action = "LeftHalf",    DisplayName = "左半屏",   IconGlyph = "\uE7C5" });
            HalfScreenShortcuts.Add(new ShortcutItem { Action = "RightHalf",   DisplayName = "右半屏",   IconGlyph = "\uE7C6" });
            HalfScreenShortcuts.Add(new ShortcutItem { Action = "CenterHalf",  DisplayName = "中间半屏", IconGlyph = "\uE7C4" });
            HalfScreenShortcuts.Add(new ShortcutItem { Action = "TopHalf",     DisplayName = "上半屏",   IconGlyph = "\uE7C3" });
            HalfScreenShortcuts.Add(new ShortcutItem { Action = "BottomHalf",  DisplayName = "下半屏",   IconGlyph = "\uE7C2" });

            CornerShortcuts.Add(new ShortcutItem { Action = "TopLeft",     DisplayName = "左上", IconGlyph = "\uE744" });
            CornerShortcuts.Add(new ShortcutItem { Action = "TopRight",    DisplayName = "右上", IconGlyph = "\uE745" });
            CornerShortcuts.Add(new ShortcutItem { Action = "BottomLeft",  DisplayName = "左下", IconGlyph = "\uE746" });
            CornerShortcuts.Add(new ShortcutItem { Action = "BottomRight", DisplayName = "右下", IconGlyph = "\uE747" });

            ThirdShortcuts.Add(new ShortcutItem { Action = "FirstThird",       DisplayName = "左首 1/3", IconGlyph = "\uE74C" });
            ThirdShortcuts.Add(new ShortcutItem { Action = "CenterThird",      DisplayName = "中间 1/3", IconGlyph = "\uE74D" });
            ThirdShortcuts.Add(new ShortcutItem { Action = "LastThird",        DisplayName = "右首 1/3", IconGlyph = "\uE74E" });
            ThirdShortcuts.Add(new ShortcutItem { Action = "FirstTwoThirds",   DisplayName = "左侧 2/3", IconGlyph = "\uE74C" });
            ThirdShortcuts.Add(new ShortcutItem { Action = "CenterTwoThirds",  DisplayName = "中间 2/3", IconGlyph = "\uE74D" });
            ThirdShortcuts.Add(new ShortcutItem { Action = "LastTwoThirds",    DisplayName = "右侧 2/3", IconGlyph = "\uE74E" });
            ThirdShortcuts.Add(new ShortcutItem { Action = "TopVerticalThird",    DisplayName = "上部 1/3", IconGlyph = "\uE7C3" });
            ThirdShortcuts.Add(new ShortcutItem { Action = "MiddleVerticalThird", DisplayName = "垂直中间 1/3", IconGlyph = "\uE7C4" });
            ThirdShortcuts.Add(new ShortcutItem { Action = "BottomVerticalThird", DisplayName = "下部 1/3", IconGlyph = "\uE7C2" });
            ThirdShortcuts.Add(new ShortcutItem { Action = "TopVerticalTwoThirds",    DisplayName = "上部 2/3", IconGlyph = "\uE7C3" });
            ThirdShortcuts.Add(new ShortcutItem { Action = "BottomVerticalTwoThirds", DisplayName = "下部 2/3", IconGlyph = "\uE7C2" });
            ThirdShortcuts.Add(new ShortcutItem { Action = "TopLeftThird",     DisplayName = "左上 1/3", IconGlyph = "\uE744" });
            ThirdShortcuts.Add(new ShortcutItem { Action = "TopRightThird",    DisplayName = "右上 1/3", IconGlyph = "\uE745" });
            ThirdShortcuts.Add(new ShortcutItem { Action = "BottomLeftThird",  DisplayName = "左下 1/3", IconGlyph = "\uE746" });
            ThirdShortcuts.Add(new ShortcutItem { Action = "BottomRightThird", DisplayName = "右下 1/3", IconGlyph = "\uE747" });

            FourthShortcuts.Add(new ShortcutItem { Action = "FirstFourth",        DisplayName = "左首 1/4", IconGlyph = "\uE74C" });
            FourthShortcuts.Add(new ShortcutItem { Action = "SecondFourth",       DisplayName = "左中 1/4", IconGlyph = "\uE74D" });
            FourthShortcuts.Add(new ShortcutItem { Action = "ThirdFourth",        DisplayName = "右中 1/4", IconGlyph = "\uE74D" });
            FourthShortcuts.Add(new ShortcutItem { Action = "LastFourth",         DisplayName = "右首 1/4", IconGlyph = "\uE74E" });
            FourthShortcuts.Add(new ShortcutItem { Action = "FirstThreeFourths",  DisplayName = "左侧 3/4", IconGlyph = "\uE74C" });
            FourthShortcuts.Add(new ShortcutItem { Action = "CenterThreeFourths", DisplayName = "中间 3/4", IconGlyph = "\uE74D" });
            FourthShortcuts.Add(new ShortcutItem { Action = "LastThreeFourths",   DisplayName = "右侧 3/4", IconGlyph = "\uE74E" });

            SixthShortcuts.Add(new ShortcutItem { Action = "TopLeftSixth",      DisplayName = "左上 1/6", IconGlyph = "\uE744" });
            SixthShortcuts.Add(new ShortcutItem { Action = "TopCenterSixth",    DisplayName = "中上 1/6", IconGlyph = "\uE7C4" });
            SixthShortcuts.Add(new ShortcutItem { Action = "TopRightSixth",     DisplayName = "右上 1/6", IconGlyph = "\uE745" });
            SixthShortcuts.Add(new ShortcutItem { Action = "BottomLeftSixth",   DisplayName = "左下 1/6", IconGlyph = "\uE746" });
            SixthShortcuts.Add(new ShortcutItem { Action = "BottomCenterSixth", DisplayName = "中下 1/6", IconGlyph = "\uE7C4" });
            SixthShortcuts.Add(new ShortcutItem { Action = "BottomRightSixth",  DisplayName = "右下 1/6", IconGlyph = "\uE747" });

            EighthShortcuts.Add(new ShortcutItem { Action = "TopLeftEighth",     DisplayName = "左上 1/8", IconGlyph = "\uE744" });
            EighthShortcuts.Add(new ShortcutItem { Action = "TopCenterLeftEighth",  DisplayName = "中上左 1/8", IconGlyph = "\uE7C4" });
            EighthShortcuts.Add(new ShortcutItem { Action = "TopCenterRightEighth", DisplayName = "中上右 1/8", IconGlyph = "\uE7C4" });
            EighthShortcuts.Add(new ShortcutItem { Action = "TopRightEighth",    DisplayName = "右上 1/8", IconGlyph = "\uE745" });
            EighthShortcuts.Add(new ShortcutItem { Action = "BottomLeftEighth",  DisplayName = "左下 1/8", IconGlyph = "\uE746" });
            EighthShortcuts.Add(new ShortcutItem { Action = "BottomCenterLeftEighth",  DisplayName = "中下左 1/8", IconGlyph = "\uE7C4" });
            EighthShortcuts.Add(new ShortcutItem { Action = "BottomCenterRightEighth", DisplayName = "中下右 1/8", IconGlyph = "\uE7C4" });
            EighthShortcuts.Add(new ShortcutItem { Action = "BottomRightEighth", DisplayName = "右下 1/8", IconGlyph = "\uE747" });

            NinthShortcuts.Add(new ShortcutItem { Action = "TopLeftNinth",    DisplayName = "左上 1/9",  IconGlyph = "\uE744" });
            NinthShortcuts.Add(new ShortcutItem { Action = "TopCenterNinth",  DisplayName = "中上 1/9",  IconGlyph = "\uE7C4" });
            NinthShortcuts.Add(new ShortcutItem { Action = "TopRightNinth",   DisplayName = "右上 1/9",  IconGlyph = "\uE745" });
            NinthShortcuts.Add(new ShortcutItem { Action = "MiddleLeftNinth",   DisplayName = "左中 1/9",  IconGlyph = "\uE744" });
            NinthShortcuts.Add(new ShortcutItem { Action = "MiddleCenterNinth", DisplayName = "正中 1/9",  IconGlyph = "\uE7C4" });
            NinthShortcuts.Add(new ShortcutItem { Action = "MiddleRightNinth",  DisplayName = "右中 1/9",  IconGlyph = "\uE745" });
            NinthShortcuts.Add(new ShortcutItem { Action = "BottomLeftNinth",   DisplayName = "左下 1/9",  IconGlyph = "\uE746" });
            NinthShortcuts.Add(new ShortcutItem { Action = "BottomCenterNinth", DisplayName = "中下 1/9",  IconGlyph = "\uE7C4" });
            NinthShortcuts.Add(new ShortcutItem { Action = "BottomRightNinth",  DisplayName = "右下 1/9",  IconGlyph = "\uE747" });

            MaximizeShortcuts.Add(new ShortcutItem { Action = "Maximize",       DisplayName = "最大化",     IconGlyph = "\uE739" });
            MaximizeShortcuts.Add(new ShortcutItem { Action = "AlmostMaximize", DisplayName = "接近最大化", IconGlyph = "\uE73A" });
            MaximizeShortcuts.Add(new ShortcutItem { Action = "MaximizeHeight", DisplayName = "最大化高度", IconGlyph = "\uE73B" });
            MaximizeShortcuts.Add(new ShortcutItem { Action = "Center",         DisplayName = "居中",       IconGlyph = "\uE74D" });
            MaximizeShortcuts.Add(new ShortcutItem { Action = "Restore",        DisplayName = "恢复",       IconGlyph = "\uE72A" });

            ResizeShortcuts.Add(new ShortcutItem { Action = "Larger",  DisplayName = "放大", IconGlyph = "\uE71F" });
            ResizeShortcuts.Add(new ShortcutItem { Action = "Smaller", DisplayName = "缩小", IconGlyph = "\uE71E" });
            ResizeShortcuts.Add(new ShortcutItem { Action = "LargerWidth",   DisplayName = "加宽", IconGlyph = "\uE71F" });
            ResizeShortcuts.Add(new ShortcutItem { Action = "SmallerWidth",  DisplayName = "减宽", IconGlyph = "\uE71E" });
            ResizeShortcuts.Add(new ShortcutItem { Action = "LargerHeight",  DisplayName = "加高", IconGlyph = "\uE71F" });
            ResizeShortcuts.Add(new ShortcutItem { Action = "SmallerHeight", DisplayName = "减高", IconGlyph = "\uE71E" });

            MoveShortcuts.Add(new ShortcutItem { Action = "MoveLeft",  DisplayName = "左移", IconGlyph = "\uE72B" });
            MoveShortcuts.Add(new ShortcutItem { Action = "MoveRight", DisplayName = "右移", IconGlyph = "\uE72A" });
            MoveShortcuts.Add(new ShortcutItem { Action = "MoveUp",    DisplayName = "上移", IconGlyph = "\uE72C" });
            MoveShortcuts.Add(new ShortcutItem { Action = "MoveDown",  DisplayName = "下移", IconGlyph = "\uE72D" });

            DisplayShortcuts.Add(new ShortcutItem { Action = "NextDisplay",     DisplayName = "下一个显示器", IconGlyph = "\uE7F5" });
            DisplayShortcuts.Add(new ShortcutItem { Action = "PreviousDisplay", DisplayName = "上一个显示器", IconGlyph = "\uE7F6" });

            OtherShortcuts.Add(new ShortcutItem { Action = "LeftTodo",  DisplayName = "左侧 Todo", IconGlyph = "\uE8FD" });
            OtherShortcuts.Add(new ShortcutItem { Action = "RightTodo", DisplayName = "右侧 Todo", IconGlyph = "\uE8FD" });
            OtherShortcuts.Add(new ShortcutItem { Action = "Undo",      DisplayName = "撤销", IconGlyph = "\uE7A7" });
            OtherShortcuts.Add(new ShortcutItem { Action = "Redo",      DisplayName = "重做", IconGlyph = "\uE7A6" });
        }

        public async Task LoadShortcutsAsync()
        {
            await Task.Run(() =>
            {
                var config = _configService.Load();
                var defaults = ConfigService.GetDefaultShortcuts();
                var merged = new System.Collections.Generic.Dictionary<string, ShortcutConfig>(defaults);
                foreach (var kvp in config.Shortcuts) merged[kvp.Key] = kvp.Value;

                UpdateShortcutCollection(HalfScreenShortcuts, merged);
                UpdateShortcutCollection(CornerShortcuts, merged);
                UpdateShortcutCollection(ThirdShortcuts, merged);
                UpdateShortcutCollection(FourthShortcuts, merged);
                UpdateShortcutCollection(SixthShortcuts, merged);
                UpdateShortcutCollection(EighthShortcuts, merged);
                UpdateShortcutCollection(NinthShortcuts, merged);
                UpdateShortcutCollection(MaximizeShortcuts, merged);
                UpdateShortcutCollection(ResizeShortcuts, merged);
                UpdateShortcutCollection(MoveShortcuts, merged);
                UpdateShortcutCollection(DisplayShortcuts, merged);
                UpdateShortcutCollection(OtherShortcuts, merged);
            });
        }

        public async Task LoadSettingsAsync()
        {
            await Task.Run(() =>
            {
                var config = _configService.Load();
                _launchOnLogin = config.LaunchOnLogin;
                _gapSize = config.GapSize;
                _logToFile = config.LogToFile;
                _logLevel = config.LogLevel;
                OnPropertyChanged(nameof(LaunchOnLogin));
                OnPropertyChanged(nameof(GapSize));
                OnPropertyChanged(nameof(LogToFile));
                OnPropertyChanged(nameof(LogLevel));
            });
        }

        private void UpdateShortcutCollection(ObservableCollection<ShortcutItem> collection,
            System.Collections.Generic.Dictionary<string, ShortcutConfig> shortcuts)
        {
            foreach (var item in collection)
            {
                if (shortcuts.TryGetValue(item.Action, out var cfg) && cfg.Enabled && cfg.KeyCode > 0)
                    item.ShortcutText = FormatShortcut(cfg.KeyCode, cfg.ModifierFlags);
                else
                    item.ShortcutText = "记录快捷键";
            }
        }

        private string FormatShortcut(int keyCode, uint modifiers)
        {
            var parts = new System.Collections.Generic.List<string>();
            if ((modifiers & 0x0002) != 0) parts.Add("Ctrl");
            if ((modifiers & 0x0001) != 0) parts.Add("Alt");
            if ((modifiers & 0x0004) != 0) parts.Add("Shift");
            if ((modifiers & 0x0008) != 0) parts.Add("Win");

            var keyMap = new System.Collections.Generic.Dictionary<int, string>
            {
                [0x25] = "←", [0x26] = "↑", [0x27] = "→", [0x28] = "↓",
                [0x0D] = "Enter", [0x08] = "Back", [0x2E] = "Del", [0x20] = "Space"
            };
            parts.Add(keyMap.TryGetValue(keyCode, out var k) ? k : $"0x{keyCode:X}");
            return string.Join("+", parts);
        }

        public void UpdateShortcut(string action, int keyCode, uint modifierFlags)
        {
            var config = _configService.Load();
            if (!config.Shortcuts.ContainsKey(action))
                config.Shortcuts[action] = new ShortcutConfig();
            config.Shortcuts[action].KeyCode = keyCode;
            config.Shortcuts[action].ModifierFlags = modifierFlags;
            config.Shortcuts[action].Enabled = true;
            _configService.Save(config);
            UpdateShortcutInCollections(action, FormatShortcut(keyCode, modifierFlags));
        }

        public void ClearShortcut(string action)
        {
            var config = _configService.Load();
            if (config.Shortcuts.ContainsKey(action))
            {
                config.Shortcuts[action].KeyCode = 0;
                config.Shortcuts[action].ModifierFlags = 0;
                config.Shortcuts[action].Enabled = false;
                _configService.Save(config);
            }
            UpdateShortcutInCollections(action, "记录快捷键");
        }

        private void UpdateShortcutInCollections(string action, string text)
        {
            foreach (var col in new[] { HalfScreenShortcuts, CornerShortcuts, ThirdShortcuts,
                FourthShortcuts, SixthShortcuts, EighthShortcuts, NinthShortcuts,
                MaximizeShortcuts, ResizeShortcuts, MoveShortcuts, DisplayShortcuts, OtherShortcuts })
            {
                foreach (var item in col)
                    if (item.Action == action) { item.ShortcutText = text; break; }
            }
        }

        public async Task RestoreDefaultsAsync()
        {
            await Task.Run(() =>
            {
                var config = _configService.Load();
                config.Shortcuts = ConfigService.GetDefaultShortcuts();
                _configService.Save(config);
            });
            await LoadShortcutsAsync();
        }

        private void SaveLaunchOnLoginSetting()
        {
            try
            {
                var config = _configService.Load();
                config.LaunchOnLogin = _launchOnLogin;
                _configService.Save(config);
                using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Run", true);
                if (_launchOnLogin)
                    key?.SetValue("Rectangle", Environment.ProcessPath ?? "");
                else
                    key?.DeleteValue("Rectangle", false);
            }
            catch { }
        }

        private void SaveGapSizeSetting()
        {
            var config = _configService.Load();
            config.GapSize = _gapSize;
            _configService.Save(config);
        }

        private void SaveLogSettings()
        {
            var config = _configService.Load();
            config.LogToFile = _logToFile;
            config.LogLevel = _logLevel;
            _configService.Save(config);
        }

        private void SaveThemeSetting()
        {
            ThemeService.Instance.SetTheme(_currentTheme);
            ThemeService.Instance.SaveThemeToConfig();
        }
    }

    public class ShortcutItem : ObservableObject
    {
        private string _shortcutText = "记录快捷键";
        public string Action { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string IconGlyph { get; set; } = "\uE71C";
        public string ShortcutText
        {
            get => _shortcutText;
            set => SetProperty(ref _shortcutText, value);
        }
    }
}
