using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Rectangle.Windows.WinUI.Core;
using Rectangle.Windows.WinUI.Services;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Rectangle.Windows.WinUI.Views
{
    public sealed partial class SnapPreviewWindow : Window, IDisposable
    {
        private readonly ConfigService _configService;
        private Border? _previewBorder;

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW")]
        private static extern nint GetWindowLongPtr(nint hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW")]
        private static extern nint SetWindowLongPtr(nint hWnd, int nIndex, nint dwNewLong);

        [DllImport("user32.dll")]
        private static extern bool SetLayeredWindowAttributes(nint hwnd, uint crKey, byte bAlpha, uint dwFlags);

        public SnapPreviewWindow()
        {
            _configService = new ConfigService();
            InitializeWindow();
        }

        private void InitializeWindow()
        {
            _previewBorder = new Border
            {
                Background = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(100, 0, 120, 212)),
                BorderBrush = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(200, 0, 120, 212)),
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(4)
            };
            this.Content = _previewBorder;

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            SetWindowStyles(hwnd);
        }

        private void SetWindowStyles(nint hwnd)
        {
            const int GWL_EXSTYLE = -20;
            const int WS_EX_TOOLWINDOW = 0x00000080;
            const int WS_EX_NOACTIVATE = unchecked((int)0x08000000);
            const int WS_EX_TRANSPARENT = 0x00000020;
            const int WS_EX_LAYERED = unchecked((int)0x00080000);
            const uint LWA_ALPHA = 0x00000002;

            var exStyle = GetWindowLongPtr(hwnd, GWL_EXSTYLE);
            SetWindowLongPtr(hwnd, GWL_EXSTYLE,
                exStyle | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE | WS_EX_TRANSPARENT | WS_EX_LAYERED);

            SetLayeredWindowAttributes(hwnd, 0, 180, LWA_ALPHA);
            PInvoke.SetWindowPos(new HWND(hwnd), new HWND(-1), 0, 0, 0, 0,
                SET_WINDOW_POS_FLAGS.SWP_NOMOVE | SET_WINDOW_POS_FLAGS.SWP_NOSIZE | SET_WINDOW_POS_FLAGS.SWP_SHOWWINDOW);
        }

        public void ShowPreview(WindowRect rect)
        {
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            PInvoke.SetWindowPos(new HWND(hwnd), new HWND(0),
                rect.X, rect.Y, rect.Width, rect.Height,
                SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE | SET_WINDOW_POS_FLAGS.SWP_SHOWWINDOW);
            this.Activate();
        }

        public void HidePreview()
        {
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            PInvoke.ShowWindow(new HWND(hwnd), SHOW_WINDOW_CMD.SW_HIDE);
        }

        public void UpdatePreview(WindowAction action, WindowRect workArea)
        {
            var config = _configService.Load();
            var calculator = new CalculatorFactory(_configService).GetCalculator(action);
            if (calculator != null)
            {
                var rect = calculator.Calculate(
                    new WorkArea(workArea.X, workArea.Y, workArea.Right, workArea.Bottom),
                    workArea, action, config.GapSize);
                ShowPreview(rect);
            }
        }

        public void Dispose()
        {
            this.Close();
        }
    }
}
