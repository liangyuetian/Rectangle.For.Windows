using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Rectangle.Windows.WinUI.ViewModels;
using System;

namespace Rectangle.Windows.WinUI.Views
{
    public sealed partial class GeneralSettingsPage : Page
    {
        public SettingsViewModel ViewModel { get; } = new SettingsViewModel();

        public GeneralSettingsPage()
        {
            this.InitializeComponent();
            this.Loaded += GeneralSettingsPage_Loaded;
        }

        private async void GeneralSettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.LoadSettingsAsync();
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
                // TODO: Implement reset settings
            }
        }

        private async void CheckUpdate_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "检查更新",
                Content = "正在检查更新...",
                CloseButtonText = "关闭",
                XamlRoot = this.XamlRoot
            };

            await dialog.ShowAsync();
        }
        private void LaunchOnLoginToggle_Toggled(object sender, RoutedEventArgs e)
        {
            // 这里写你的逻辑，例如：
            var toggle = sender as ToggleSwitch;
            if (toggle != null)
            {
                ViewModel.LaunchOnLogin = toggle.IsOn;  // 示例：同步到 ViewModel
                // 或调用服务设置开机自启
            }
        }
    }
}
