using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Rectangle.Windows.WinUI.Views;
using System;

namespace Rectangle.Windows.WinUI.Views.Controls
{
    public sealed partial class ShortcutEditor : UserControl
    {
        // 依赖属性
        public static readonly DependencyProperty ActionProperty =
            DependencyProperty.Register(nameof(Action), typeof(string), typeof(ShortcutEditor),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty DisplayNameProperty =
            DependencyProperty.Register(nameof(DisplayName), typeof(string), typeof(ShortcutEditor),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty IconSourceProperty =
            DependencyProperty.Register(nameof(IconSource), typeof(ImageSource), typeof(ShortcutEditor),
                new PropertyMetadata(null));

        public static readonly DependencyProperty IconGlyphProperty =
            DependencyProperty.Register(nameof(IconGlyph), typeof(string), typeof(ShortcutEditor),
                new PropertyMetadata("\uE71C", OnIconGlyphChanged));

        public static readonly DependencyProperty ShortcutTextProperty =
            DependencyProperty.Register(nameof(ShortcutText), typeof(string), typeof(ShortcutEditor),
                new PropertyMetadata("记录快捷键", OnShortcutTextChanged));

        public string Action
        {
            get => (string)GetValue(ActionProperty);
            set => SetValue(ActionProperty, value);
        }

        public string DisplayName
        {
            get => (string)GetValue(DisplayNameProperty);
            set => SetValue(DisplayNameProperty, value);
        }

        public ImageSource IconSource
        {
            get => (ImageSource)GetValue(IconSourceProperty);
            set => SetValue(IconSourceProperty, value);
        }

        public string IconGlyph
        {
            get => (string)GetValue(IconGlyphProperty);
            set => SetValue(IconGlyphProperty, value);
        }

        public string ShortcutText
        {
            get => (string)GetValue(ShortcutTextProperty);
            set => SetValue(ShortcutTextProperty, value);
        }

        // 事件
        public event EventHandler<ShortcutChangedEventArgs>? ShortcutChanged;
        public event EventHandler<ShortcutClearedEventArgs>? ShortcutCleared;

        public ShortcutEditor()
        {
            this.InitializeComponent();
            UpdateVisualState();
        }

        private static void OnIconGlyphChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // 图标已更新
        }

        private static void OnShortcutTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var editor = (ShortcutEditor)d;
            editor.UpdateVisualState();
        }

        private void UpdateVisualState()
        {
            // 根据是否有快捷键更新视觉状态
            bool hasShortcut = !string.IsNullOrEmpty(ShortcutText) && ShortcutText != "记录快捷键";

            if (hasShortcut)
            {
                ShortcutButton.Foreground = (SolidColorBrush)Application.Current.Resources["TextFillColorPrimaryBrush"];
                ClearButton.Visibility = Visibility.Visible;
            }
            else
            {
                ShortcutButton.Foreground = (SolidColorBrush)Application.Current.Resources["SecondaryTextBrush"];
                ClearButton.Visibility = Visibility.Collapsed;
            }
        }

        private async void ShortcutButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ShortcutCaptureDialog();

            // 获取 XamlRoot
            if (this.XamlRoot != null)
            {
                dialog.XamlRoot = this.XamlRoot;
            }
            else if (App.MainWindow?.Content?.XamlRoot != null)
            {
                dialog.XamlRoot = App.MainWindow.Content.XamlRoot;
            }

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
            ShortcutCleared?.Invoke(this, new ShortcutClearedEventArgs
            {
                Action = Action
            });
        }
    }
}
