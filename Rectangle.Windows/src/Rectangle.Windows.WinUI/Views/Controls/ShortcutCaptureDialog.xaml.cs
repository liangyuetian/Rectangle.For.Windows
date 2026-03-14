using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Input;
using Windows.System;
using Windows.UI.Core;
using System.Collections.Generic;

namespace Rectangle.Windows.WinUI.Views.Controls
{
    public sealed partial class ShortcutCaptureDialog : ContentDialog
    {
        public KeyCombination? CapturedShortcut { get; private set; }

        public ShortcutCaptureDialog()
        {
            this.InitializeComponent();
            this.KeyDown += OnKeyDown;
        }

        private void OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            e.Handled = true;
            if (e.Key == VirtualKey.Escape) { this.Hide(); return; }
            if (IsModifierKey(e.Key)) return;

            var modifiers = GetCurrentModifiers();
            if (modifiers == 0) { PreviewText.Text = "需要修饰键 (Ctrl/Alt/Shift/Win)"; return; }

            var keyCode = (int)e.Key;
            var displayText = FormatShortcut(keyCode, modifiers);
            PreviewText.Text = displayText;

            CapturedShortcut = new KeyCombination { KeyCode = keyCode, ModifierFlags = modifiers, DisplayText = displayText };

            _ = DispatcherQueue.TryEnqueue(async () =>
            {
                await System.Threading.Tasks.Task.Delay(200);
                this.Hide();
            });
        }

        private static bool IsModifierKey(VirtualKey key) =>
            key is VirtualKey.Control or VirtualKey.LeftControl or VirtualKey.RightControl
                or VirtualKey.Menu or VirtualKey.LeftMenu or VirtualKey.RightMenu
                or VirtualKey.Shift or VirtualKey.LeftShift or VirtualKey.RightShift
                or VirtualKey.LeftWindows or VirtualKey.RightWindows;

        private static uint GetCurrentModifiers()
        {
            uint m = 0;
            if ((InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Control) & CoreVirtualKeyStates.Down) != 0) m |= 0x0002;
            if ((InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Menu) & CoreVirtualKeyStates.Down) != 0) m |= 0x0001;
            if ((InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift) & CoreVirtualKeyStates.Down) != 0) m |= 0x0004;
            if ((InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.LeftWindows) & CoreVirtualKeyStates.Down) != 0) m |= 0x0008;
            return m;
        }

        private static string FormatShortcut(int keyCode, uint modifiers)
        {
            var parts = new List<string>();
            if ((modifiers & 0x0002) != 0) parts.Add("Ctrl");
            if ((modifiers & 0x0001) != 0) parts.Add("Alt");
            if ((modifiers & 0x0004) != 0) parts.Add("Shift");
            if ((modifiers & 0x0008) != 0) parts.Add("Win");
            parts.Add(GetKeyName((VirtualKey)keyCode));
            return string.Join("+", parts);
        }

        private static string GetKeyName(VirtualKey key) => key switch
        {
            VirtualKey.Left => "←", VirtualKey.Right => "→",
            VirtualKey.Up => "↑", VirtualKey.Down => "↓",
            VirtualKey.Enter => "Enter", VirtualKey.Back => "Back",
            VirtualKey.Delete => "Del", VirtualKey.Space => "Space",
            VirtualKey.Tab => "Tab", VirtualKey.Escape => "Esc",
            _ => key.ToString()
        };
    }

    public class KeyCombination
    {
        public int KeyCode { get; set; }
        public uint ModifierFlags { get; set; }
        public string DisplayText { get; set; } = string.Empty;
    }
}
