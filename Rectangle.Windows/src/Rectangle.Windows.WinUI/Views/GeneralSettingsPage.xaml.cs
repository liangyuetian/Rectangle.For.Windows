using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Rectangle.Windows.WinUI.ViewModels;
using Rectangle.Windows.WinUI;
using System;

namespace Rectangle.Windows.WinUI.Views
{
    public sealed partial class GeneralSettingsPage : Page
    {
        public SettingsViewModel ViewModel { get; } = new SettingsViewModel();

        public GeneralSettingsPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            this.Loaded += GeneralSettingsPage_Loaded;
        }

        private async void GeneralSettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.LoadSettingsAsync();
            HorizontalSplitSlider.Value = ViewModel.HorizontalSplitRatio;
            VerticalSplitSlider.Value = ViewModel.VerticalSplitRatio;
            HorizontalSplitValue.Text = $"{ViewModel.HorizontalSplitRatio}%";
            VerticalSplitValue.Text = $"{ViewModel.VerticalSplitRatio}%";
            LogLevelComboBox.SelectedIndex = ViewModel.LogLevel;
            LogLevelComboBox.IsEnabled = ViewModel.LogToFile;
            ThemeComboBox.SelectedIndex = Services.ThemeService.Instance.CurrentTheme switch
            {
                ElementTheme.Dark => 1,
                ElementTheme.Light => 2,
                _ => 0
            };
            ViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ViewModel.LogToFile))
                    LogLevelComboBox.IsEnabled = ViewModel.LogToFile;
            };
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
    }
}
