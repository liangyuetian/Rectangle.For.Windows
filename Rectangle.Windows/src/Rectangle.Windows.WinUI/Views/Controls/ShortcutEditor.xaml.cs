using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;

namespace Rectangle.Windows.WinUI.Views.Controls
{
    public class ShortcutChangedEventArgs : EventArgs
    {
        public string Action { get; set; } = string.Empty;
        public int KeyCode { get; set; }
        public uint ModifierFlags { get; set; }
    }

    public class ShortcutClearedEventArgs : EventArgs
    {
        public string Action { get; set; } = string.Empty;
    }

    public sealed partial class ShortcutEditor : UserControl
    {
        public static readonly DependencyProperty ActionProperty =
            DependencyProperty.Register(nameof(Action), typeof(string), typeof(ShortcutEditor), new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty DisplayNameProperty =
            DependencyProperty.Register(nameof(DisplayName), typeof(string), typeof(ShortcutEditor), new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty IconGlyphProperty =
            DependencyProperty.Register(nameof(IconGlyph), typeof(string), typeof(ShortcutEditor), new PropertyMetadata("\uE71C"));
        public static readonly DependencyProperty ShortcutTextProperty =
            DependencyProperty.Register(nameof(ShortcutText), typeof(string), typeof(ShortcutEditor),
                new PropertyMetadata("记录快捷键", (d, e) => ((ShortcutEditor)d).UpdateVisualState()));

        public string Action { get => (string)GetValue(ActionProperty); set => SetValue(ActionProperty, value); }
        public string DisplayName { get => (string)GetValue(DisplayNameProperty); set => SetValue(DisplayNameProperty, value); }
        public string IconGlyph { get => (string)GetValue(IconGlyphProperty); set => SetValue(IconGlyphProperty, value); }
        public string ShortcutText { get => (string)GetValue(ShortcutTextProperty); set => SetValue(ShortcutTextProperty, value); }

        public event EventHandler<ShortcutChangedEventArgs>? ShortcutChanged;
        public event EventHandler<ShortcutClearedEventArgs>? ShortcutCleared;

        public ShortcutEditor()
        {
            this.InitializeComponent();
            UpdateVisualState();
        }

        private void UpdateVisualState()
        {
            bool hasShortcut = !string.IsNullOrEmpty(ShortcutText) && ShortcutText != "记录快捷键";
            if (ClearButton == null) return;
            ClearButton.Visibility = hasShortcut ? Visibility.Visible : Visibility.Collapsed;
        }

        private async void ShortcutButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ShortcutCaptureDialog();
            dialog.XamlRoot = this.XamlRoot ?? App.MainWindow?.Content?.XamlRoot;
            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary && dialog.CapturedShortcut != null)
            {
                ShortcutText = dialog.CapturedShortcut.DisplayText;
                ShortcutChanged?.Invoke(this, new ShortcutChangedEventArgs
                {
                    Action = Action,
                    KeyCode = dialog.CapturedShortcut.KeyCode,
                    ModifierFlags = dialog.CapturedShortcut.ModifierFlags
                });
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ShortcutText = "记录快捷键";
            ShortcutCleared?.Invoke(this, new ShortcutClearedEventArgs { Action = Action });
        }
    }
}
