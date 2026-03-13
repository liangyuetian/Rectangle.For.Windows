using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Rectangle.Windows.WinUI.Core;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Rectangle.Windows.WinUI.Views
{
    /// <summary>
    /// 吸附预览窗口 - 显示窗口即将吸附的位置
    /// </summary>
    public sealed partial class SnapPreviewWindow : Window
    {
        private readonly ConfigService _configService;
        private Border? _previewBorder;

        public SnapPreviewWindow()
        {
            _configService = new ConfigService();
            InitializeWindow();
        }

        private void InitializeWindow()
        {
            // 创建预览内容
            _previewBorder = new Border
            {
                Background = new SolidColorBrush(Microsoft.UI.Colors.FromArgb(100, 0, 120, 212)),
                BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.FromArgb(200, 0, 120, 212)),
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(4)
            };

            this.Content = _previewBorder;

            // 设置窗口样式为透明、无边框、置顶
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            SetWindowStyles(hwnd);
        }

        private void SetWindowStyles(nint hwnd)
        {
            const uint WS_EX_TOOLWINDOW = 0x00000080;
            const uint WS_EX_NOACTIVATE = 0x08000000;
            const uint WS_EX_TRANSPARENT = 0x00000020;
            const uint WS_EX_LAYERED = 0x00080000;

            // 获取当前扩展样式
            var exStyle = PInvoke.GetWindowLong(new HWND(hwnd), WINDOWS_LONG_PTR_INDEX.GWL_EXSTYLE);

            // 添加需要的样式
            PInvoke.SetWindowLong(
                new HWND(hwnd),
                WINDOWS_LONG_PTR_INDEX.GWL_EXSTYLE,
                exStyle | (int)(WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE | WS_EX_TRANSPARENT | WS_EX_LAYERED));

            // 设置窗口透明
            const uint LWA_ALPHA = 0x00000002;
            PInvoke.SetLayeredWindowAttributes(new HWND(hwnd), new COLORREF(0), 180, LWA_ALPHA);

            // 置顶
            PInvoke.SetWindowPos(
                new HWND(hwnd),
                new HWND(-1), // HWND_TOPMOST
                0, 0, 0, 0,
                SET_WINDOW_POS_FLAGS.SWP_NOMOVE | SET_WINDOW_POS_FLAGS.SWP_NOSIZE | SET_WINDOW_POS_FLAGS.SWP_SHOWWINDOW);
        }

        /// <summary>
        /// 显示预览
        /// </summary>
        public void ShowPreview(WindowRect rect)
        {
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);

            // 设置窗口位置和大小
            PInvoke.SetWindowPos(
                new HWND(hwnd),
                new HWND(0),
                rect.X, rect.Y, rect.Width, rect.Height,
                SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE | SET_WINDOW_POS_FLAGS.SWP_SHOWWINDOW);

            this.Activate();
        }

        /// <summary>
        /// 隐藏预览
        /// </summary>
        public void HidePreview()
        {
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            PInvoke.ShowWindow(new HWND(hwnd), SHOW_WINDOW_CMD.SW_HIDE);
        }

        /// <summary>
        /// 更新预览位置和大小
        /// </summary>
        public void UpdatePreview(WindowAction action, WindowRect workArea)
        {
            var config = _configService.Load();
            var calculator = new CalculatorFactory().GetCalculator(action);

            if (calculator != null)
            {
                var rect = calculator.Calculate(
                    new WorkArea(workArea.X, workArea.Y, workArea.Width, workArea.Height),
                    workArea,
                    action,
                    config.GapSize);

                ShowPreview(rect);
            }
        }
    }
}
