using System;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Rectangle.Windows.WinUI.Services
{
    /// <summary>
    /// 全局鼠标钩子服务
    /// </summary>
    public class MouseHookService : IDisposable
    {
        private nint _mouseHook;
        private bool _disposed;
        private readonly HookProc _hookCallback;

        public event EventHandler<MouseHookEventArgs>? MouseDown;
        public event EventHandler<MouseHookEventArgs>? MouseMove;
        public event EventHandler<MouseHookEventArgs>? MouseUp;

        private delegate nint HookProc(int nCode, nint wParam, nint lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern nint SetWindowsHookEx(int idHook, HookProc lpfn, nint hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(nint hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern nint CallNextHookEx(nint hhk, int nCode, nint wParam, nint lParam);

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public System.Drawing.Point pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public nint dwExtraInfo;
        }

        public MouseHookService()
        {
            _hookCallback = MouseHookCallback;
            InstallHook();
        }

        private void InstallHook()
        {
            const int WH_MOUSE_LL = 14;
            _mouseHook = SetWindowsHookEx(WH_MOUSE_LL, _hookCallback, nint.Zero, 0);

            if (_mouseHook == 0)
            {
                Logger.Error("MouseHookService", "无法安装鼠标钩子");
            }
            else
            {
                Logger.Info("MouseHookService", "鼠标钩子安装成功");
            }
        }

        private nint MouseHookCallback(int nCode, nint wParam, nint lParam)
        {
            if (nCode < 0)
            {
                return CallNextHookEx(_mouseHook, nCode, wParam, lParam);
            }

            var msg = (uint)wParam;
            var mouseStruct = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
            var args = new MouseHookEventArgs
            {
                X = mouseStruct.pt.X,
                Y = mouseStruct.pt.Y,
                Button = GetMouseButton(msg)
            };

            switch (msg)
            {
                case 0x0201: // WM_LBUTTONDOWN
                    MouseDown?.Invoke(this, args);
                    break;
                case 0x0200: // WM_MOUSEMOVE
                    MouseMove?.Invoke(this, args);
                    break;
                case 0x0202: // WM_LBUTTONUP
                    MouseUp?.Invoke(this, args);
                    break;
            }

            return CallNextHookEx(_mouseHook, nCode, wParam, lParam);
        }

        private static MouseButton GetMouseButton(uint msg)
        {
            return msg switch
            {
                0x0201 or 0x0202 => MouseButton.Left,
                0x0204 or 0x0205 => MouseButton.Right,
                0x0207 or 0x0208 => MouseButton.Middle,
                _ => MouseButton.None
            };
        }

        public void Dispose()
        {
            if (_disposed) return;

            if (_mouseHook != 0)
            {
                UnhookWindowsHookEx(_mouseHook);
                _mouseHook = 0;
            }

            _disposed = true;
            Logger.Info("MouseHookService", "鼠标钩子已卸载");
        }
    }

    public class MouseHookEventArgs : EventArgs
    {
        public int X { get; set; }
        public int Y { get; set; }
        public MouseButton Button { get; set; }
    }

    public enum MouseButton
    {
        None,
        Left,
        Right,
        Middle
    }
}
