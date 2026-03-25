using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Rectangle.Windows.WinUI.ViewModels;
using Rectangle.Windows.WinUI;
using Rectangle.Windows.WinUI.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rectangle.Windows.WinUI.Views
{
    public sealed partial class GeneralSettingsPage : Page
    {
        public SettingsViewModel ViewModel { get; } = new SettingsViewModel();
        private bool _propertyChangedHooked;
        private bool _isInitializing;

        public GeneralSettingsPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            this.Loaded += GeneralSettingsPage_Loaded;
        }

        private async void GeneralSettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _isInitializing = true;
                await ViewModel.LoadSettingsAsync();
                HorizontalSplitSlider.Value = ViewModel.HorizontalSplitRatio;
                VerticalSplitSlider.Value = ViewModel.VerticalSplitRatio;
                HorizontalSplitValue.Text = $"{ViewModel.HorizontalSplitRatio}%";
                VerticalSplitValue.Text = $"{ViewModel.VerticalSplitRatio}%";
                LogLevelComboBox.SelectedIndex = Math.Clamp(ViewModel.LogLevel, 0, 3);
                LogLevelComboBox.IsEnabled = ViewModel.LogToFile;
                ThemeComboBox.SelectedIndex = Services.ThemeService.Instance.CurrentTheme switch
                {
                    ElementTheme.Dark => 1,
                    ElementTheme.Light => 2,
                    _ => 0
                };
                LanguageComboBox.SelectedIndex = Math.Clamp(ViewModel.LanguageIndex, 0, 1);
                LoadAdvancedConfigText();

                if (!_propertyChangedHooked)
                {
                    ViewModel.PropertyChanged += (s, e) =>
                    {
                        if (e.PropertyName == nameof(ViewModel.LogToFile))
                            LogLevelComboBox.IsEnabled = ViewModel.LogToFile;
                    };
                    _propertyChangedHooked = true;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("GeneralSettingsPage", "加载设置页失败", ex);
            }
            finally
            {
                _isInitializing = false;
            }
        }

        private void HorizontalSplitSlider_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            var v = (int)e.NewValue;
            ViewModel.HorizontalSplitRatio = v;
            HorizontalSplitValue.Text = $"{v}%";
        }

        private void VerticalSplitSlider_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            var v = (int)e.NewValue;
            ViewModel.VerticalSplitRatio = v;
            VerticalSplitValue.Text = $"{v}%";
        }

        private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing) return;
            var selectedTag = ((sender as ComboBox)?.SelectedItem as ComboBoxItem)?.Tag?.ToString();
            var themeService = Services.ThemeService.Instance;

            switch (selectedTag)
            {
                case "Dark":
                    themeService.SetTheme(ElementTheme.Dark);
                    break;
                case "Light":
                    themeService.SetTheme(ElementTheme.Light);
                    break;
                default:
                    themeService.SetTheme(ElementTheme.Default);
                    break;
            }
        }

        private void LogLevelComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing) return;
            if (sender is ComboBox cb)
                ViewModel.LogLevel = cb.SelectedIndex;
        }

        private async void ResetSettings_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "确认重置设置",
                Content = "确定要重置所有设置为默认值吗？",
                PrimaryButtonText = "重置",
                SecondaryButtonText = "取消",
                DefaultButton = ContentDialogButton.Secondary,
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                await ViewModel.ResetAllSettingsAsync();
                HorizontalSplitSlider.Value = ViewModel.HorizontalSplitRatio;
                VerticalSplitSlider.Value = ViewModel.VerticalSplitRatio;
                HorizontalSplitValue.Text = $"{ViewModel.HorizontalSplitRatio}%";
                VerticalSplitValue.Text = $"{ViewModel.VerticalSplitRatio}%";
                LogLevelComboBox.SelectedIndex = ViewModel.LogLevel;
                LogLevelComboBox.IsEnabled = ViewModel.LogToFile;
                LanguageComboBox.SelectedIndex = ViewModel.LanguageIndex;
                ThemeComboBox.SelectedIndex = Services.ThemeService.Instance.CurrentTheme switch
                {
                    ElementTheme.Dark => 1,
                    ElementTheme.Light => 2,
                    _ => 0
                };
            }
        }

        private async void CheckUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (App.ConfigService == null) return;
            var updateService = new Services.UpdateService(App.ConfigService);
            await updateService.CheckForUpdatesAsync(silent: false);
        }

        private void LoadAdvancedConfigText()
        {
            var config = App.ConfigService?.Load();
            if (config == null) return;
            FavoriteActionsBox.Text = string.Join(",", config.FavoriteTrayActions ?? []);
            VisibleActionsBox.Text = string.Join(",", config.TrayVisibleActions ?? []);
            AppRulesBox.Text = string.Join(";", (config.AppRules ?? []).Select(r => $"{r.ProcessName}={r.ActionTag}"));
            MonitorGapOverridesBox.Text = string.Join(";", (config.MonitorGapOverrides ?? new Dictionary<string, int>()).Select(kv => $"{kv.Key}={kv.Value}"));
            ActionNotificationSwitch.IsOn = config.EnableActionNotification;
        }

        private static List<string> ParseCsvTags(string? text)
            => (text ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

        private void FavoriteActionsBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var config = App.ConfigService?.Load();
            if (config == null) return;
            config.FavoriteTrayActions = ParseCsvTags(FavoriteActionsBox.Text);
            App.ConfigService?.Save(config);
        }

        private void VisibleActionsBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var config = App.ConfigService?.Load();
            if (config == null) return;
            config.TrayVisibleActions = ParseCsvTags(VisibleActionsBox.Text);
            App.ConfigService?.Save(config);
        }

        private void AppRulesBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var config = App.ConfigService?.Load();
            if (config == null) return;

            var rules = new List<AppActionRule>();
            var pairs = (AppRulesBox.Text ?? "").Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var pair in pairs)
            {
                var idx = pair.IndexOf('=');
                if (idx <= 0 || idx >= pair.Length - 1) continue;
                var process = pair[..idx].Trim();
                var actionTag = pair[(idx + 1)..].Trim();
                if (string.IsNullOrWhiteSpace(process) || string.IsNullOrWhiteSpace(actionTag)) continue;
                rules.Add(new AppActionRule { ProcessName = process, ActionTag = actionTag, Enabled = true, MatchExact = false });
            }
            config.AppRules = rules;
            App.ConfigService?.Save(config);
        }

        private void MonitorGapOverridesBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var config = App.ConfigService?.Load();
            if (config == null) return;

            var map = new Dictionary<string, int>();
            var pairs = (MonitorGapOverridesBox.Text ?? "").Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var pair in pairs)
            {
                var idx = pair.IndexOf('=');
                if (idx <= 0 || idx >= pair.Length - 1) continue;
                var key = pair[..idx].Trim();
                var valueText = pair[(idx + 1)..].Trim();
                if (int.TryParse(valueText, out var gap))
                    map[key] = Math.Max(0, gap);
            }
            config.MonitorGapOverrides = map;
            App.ConfigService?.Save(config);
        }

        private void ActionNotificationSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            var config = App.ConfigService?.Load();
            if (config == null) return;
            config.EnableActionNotification = ActionNotificationSwitch.IsOn;
            App.ConfigService?.Save(config);
        }

        private async void ExportConfig_Click(object sender, RoutedEventArgs e)
        {
            if (App.ConfigService == null) return;
            var path = await App.ConfigService.ExportToFileAsync();
            var dialog = new ContentDialog
            {
                Title = "导出成功",
                Content = $"配置已导出到：{path}",
                PrimaryButtonText = "确定",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }

        private async void ImportConfig_Click(object sender, RoutedEventArgs e)
        {
            if (App.ConfigService == null) return;
            var ok = await App.ConfigService.ImportFromFileAsync();
            var dialog = new ContentDialog
            {
                Title = ok ? "导入成功" : "导入失败",
                Content = ok ? "已从 config.import.json 导入并应用配置。" : "未找到 config.import.json（放到配置目录后重试）。",
                PrimaryButtonText = "确定",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
            if (ok) LoadAdvancedConfigText();
        }
    }
}
