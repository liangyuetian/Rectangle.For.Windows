using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Rectangle.Windows.WinUI.ViewModels;
using System;

namespace Rectangle.Windows.WinUI.Views
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsViewModel ViewModel { get; } = new SettingsViewModel();

        public SettingsPage()
        {
            this.Loaded += SettingsPage_Loaded;
        }

        private async void SettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.LoadShortcutsAsync();
        }

        private void ShortcutEditor_ShortcutChanged(object sender, Controls.ShortcutChangedEventArgs e)
        {
            ViewModel.UpdateShortcut(e.Action, e.KeyCode, e.ModifierFlags);
        }

        private void ShortcutEditor_ShortcutCleared(object sender, Controls.ShortcutClearedEventArgs e)
        {
            ViewModel.ClearShortcut(e.Action);
        }

        private async void RestoreDefaults_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "确认恢复默认设置",
                Content = "确定要恢复所有快捷键为默认设置吗？这将覆盖您当前的自定义配置。",
                PrimaryButtonText = "恢复",
                SecondaryButtonText = "取消",
                DefaultButton = ContentDialogButton.Secondary,
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                await ViewModel.RestoreDefaultsAsync();
            }
        }
    }
}
