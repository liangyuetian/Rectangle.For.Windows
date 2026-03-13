using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Input;
using Windows.System;
using Windows.UI.Core;
using System;
using System.Collections.Generic;

namespace Rectangle.Windows.WinUI.Views.Controls
{
    public sealed partial class ShortcutCaptureDialog : ContentDialog
    {
        public KeyCombination? CapturedShortcut { get; private set; }

        private HashSet<VirtualKey> _pressedKeys = new();

        public ShortcutCaptureDialog()
        {
            this.InitializeComponent();
            this.KeyDown += ShortcutCaptureDialog_KeyDown;
        }

        private void ShortcutCaptureDialog_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            e.Handled = true;

            if (e.Key == VirtualKey.Escape)
            {
                this.Hide();
                return;
            }

            // 忽略单独的修饰键
            if (IsModifierKey(e.Key))
            {
                return;
            }

            var modifiers = GetCurrentModifiers();
            int keyCode = (int)e.Key;

            // 必须有修饰键
            if (modifiers == 0)
            {
                PreviewText.Text = "需要修饰键 (Ctrl/Alt/Shift/Win)";
                return;
            }

            var displayText = FormatShortcut(keyCode, modifiers);
            PreviewText.Text = displayText;
            PreviewText.Foreground = (SolidColorBrush)Application.Current.Resources["TextFillColorPrimaryBrush"];

            CapturedShortcut = new KeyCombination
            {
                KeyCode = keyCode,
                ModifierFlags = modifiers,
                DisplayText = displayText
            };

            // 延迟关闭以显示捕获的快捷键
            _ = DispatcherQueue.TryEnqueue(async () =>
            {
                await System.Threading.Tasks.Task.Delay(200);
                this.Hide();
            });
        }

        private bool IsModifierKey(VirtualKey key)
        {
            return key == VirtualKey.Control ||
                   key == VirtualKey.LeftControl ||
                   key == VirtualKey.RightControl ||
                   key == VirtualKey.Menu ||
                   key == VirtualKey.LeftMenu ||
                   key == VirtualKey.RightMenu ||
                   key == VirtualKey.Shift ||
                   key == VirtualKey.LeftShift ||
                   key == VirtualKey.RightShift ||
                   key == VirtualKey.LeftWindows ||
                   key == VirtualKey.RightWindows;
        }

        private uint GetCurrentModifiers()
        {
            uint modifiers = 0;

            var ctrlState = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Control);
            var altState = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Menu);
            var shiftState = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift);
            var winState = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.LeftWindows);

            if ((ctrlState & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down)
                modifiers |= 0x0002; // MOD_CONTROL
            if ((altState & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down)
                modifiers |= 0x0001; // MOD_ALT
            if ((shiftState & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down)
                modifiers |= 0x0004; // MOD_SHIFT
            if ((winState & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down)
                modifiers |= 0x0008; // MOD_WIN

            return modifiers;
        }

        private string FormatShortcut(int keyCode, uint modifiers)
        {
            var parts = new List<string>();

            if ((modifiers & 0x0002) != 0) parts.Add("Ctrl");
            if ((modifiers & 0x0001) != 0) parts.Add("Alt");
            if ((modifiers & 0x0004) != 0) parts.Add("Shift");
            if ((modifiers & 0x0008) != 0) parts.Add("Win");

            var keyName = GetKeyName((VirtualKey)keyCode);
            parts.Add(keyName);

            return string.Join("+", parts);
        }

        private string GetKeyName(VirtualKey key)
        {
            return key switch
            {
                VirtualKey.Left => "左箭头",
                VirtualKey.Right => "右箭头",
                VirtualKey.Up => "上箭头",
                VirtualKey.Down => "下箭头",
                VirtualKey.Enter => "回车",
                VirtualKey.Back => "退格",
                VirtualKey.Delete => "Del",
                VirtualKey.Space => "空格",
                VirtualKey.Tab => "Tab",
                VirtualKey.Escape => "Esc",
                VirtualKey.F1 => "F1",
                VirtualKey.F2 => "F2",
                VirtualKey.F3 => "F3",
                VirtualKey.F4 => "F4",
                VirtualKey.F5 => "F5",
                VirtualKey.F6 => "F6",
                VirtualKey.F7 => "F7",
                VirtualKey.F8 => "F8",
                VirtualKey.F9 => "F9",
                VirtualKey.F10 => "F10",
                VirtualKey.F11 => "F11",
                VirtualKey.F12 => "F12",
                _ => key.ToString()
            };
        }
    }

    public class KeyCombination
    {
        public int KeyCode { get; set; }
        public uint ModifierFlags { get; set; }
        public string DisplayText { get; set; } = string.Empty;
    }
}
