using System;
using System.Drawing;
using System.Runtime.InteropServices;
using Rectangle.Windows.WinUI.Core;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Rectangle.Windows.WinUI.Services
{
    /// <summary>
    /// 吸附检测服务 - 检测窗口拖拽到屏幕边缘
    /// </summary>
    public class SnapDetectionService : IDisposable
    {
        private readonly Win32WindowService _win32;
        private readonly WindowManager _windowManager;
        private readonly MouseHookService _mouseHook;
        private readonly ScreenDetectionService _screenService;
        private readonly ConfigService _configService;

        private bool _isDragging;
        private nint _draggedWindow;
        private Point _dragStartPoint;
        private WindowAction? _pendingSnapAction;
        private SnapPreviewWindow? _previewWindow;

        public event Action<WindowAction?>? SnapPreviewRequested;
        public event Action? SnapPreviewHidden;
        public event Action<WindowAction>? SnapTriggered;

        public SnapDetectionService(
            Win32WindowService win32,
            WindowManager windowManager,
            ConfigService configService)
        {
            _win32 = win32;
            _windowManager = windowManager;
            _configService = configService;
            _mouseHook = new MouseHookService();
            _screenService = new ScreenDetectionService(win32);

            _mouseHook.MouseDown += OnMouseDown;
            _mouseHook.MouseMove += OnMouseMove;
            _mouseHook.MouseUp += OnMouseUp;

            Logger.Info("SnapDetectionService", "吸附检测服务初始化完成");
        }

        private void OnMouseDown(object? sender, MouseHookEventArgs e)
        {
            if (e.Button != MouseButton.Left) return;

            // 获取鼠标下的窗口
            var hwnd = GetWindowUnderCursor(e.X, e.Y);
            if (hwnd == 0) return;

            // 检查是否是可移动的窗口
            if (!IsMovableWindow(hwnd)) return;

            _isDragging = true;
            _draggedWindow = hwnd;
            _dragStartPoint = new Point(e.X, e.Y);
            _pendingSnapAction = null;

            Logger.Debug("SnapDetectionService", $"开始拖拽窗口: {hwnd}");
        }

        private void OnMouseMove(object? sender, MouseHookEventArgs e)
        {
            if (!_isDragging || _draggedWindow == 0) return;

            var config = _configService.Load();
            if (!config.SnapAreas.DragToSnap) return;

            // 检测是否在屏幕边缘
            var action = DetectSnapAction(e.X, e.Y, config);

            if (action != _pendingSnapAction)
            {
                _pendingSnapAction = action;

                if (action.HasValue)
                {
                    SnapPreviewRequested?.Invoke(action);
                    ShowPreview(action.Value);
                }
                else
                {
                    SnapPreviewHidden?.Invoke();
                    HidePreview();
                }
            }
        }

        private void OnMouseUp(object? sender, MouseHookEventArgs e)
        {
            if (!_isDragging) return;

            if (_pendingSnapAction.HasValue && _draggedWindow != 0)
            {
                // 执行吸附
                ExecuteSnap(_pendingSnapAction.Value);
                SnapTriggered?.Invoke(_pendingSnapAction.Value);
            }

            _isDragging = false;
            _draggedWindow = 0;
            _pendingSnapAction = null;
            HidePreview();

            Logger.Debug("SnapDetectionService", "拖拽结束");
        }

        private void ExecuteSnap(WindowAction action)
        {
            if (_draggedWindow == 0) return;

            var config = _configService.Load();
            var workArea = _screenService.GetWorkAreaFromWindow(_draggedWindow);

            // 保存当前位置到历史
            var (x, y, w, h) = _win32.GetWindowRect(_draggedWindow);
            // TODO: 保存到 WindowHistory

            // 计算目标位置
            var calculator = new CalculatorFactory(_configService).GetCalculator(action);
            if (calculator != null)
            {
                var target = calculator.Calculate(
                    new WorkArea(workArea.X, workArea.Y, workArea.Width, workArea.Height),
                    new WindowRect(x, y, w, h),
                    action,
                    config.GapSize);

                _win32.SetWindowRect(_draggedWindow, target.X, target.Y, target.Width, target.Height);
                Logger.Info("SnapDetectionService", $"窗口吸附到: {action}");
            }
        }

        private WindowAction? DetectSnapAction(int x, int y, AppConfig config)
        {
            var workArea = _screenService.GetWorkAreaFromPoint(x, y);
            var margin = config.CornerSnapAreaSize;

            // 检测角落（优先）
            if (y < workArea.Y + margin)
            {
                if (x < workArea.X + margin) return WindowAction.TopLeft;
                if (x > workArea.X + workArea.Width - margin) return WindowAction.TopRight;
            }
            else if (y > workArea.Y + workArea.Height - margin)
            {
                if (x < workArea.X + margin) return WindowAction.BottomLeft;
                if (x > workArea.X + workArea.Width - margin) return WindowAction.BottomRight;
            }

            // 检测边缘
            if (y < workArea.Y + config.SnapEdgeMarginTop)
                return WindowAction.Maximize;

            if (x < workArea.X + config.SnapEdgeMarginLeft)
                return WindowAction.LeftHalf;

            if (x > workArea.X + workArea.Width - config.SnapEdgeMarginRight)
                return WindowAction.RightHalf;

            return null;
        }

        private unsafe nint GetWindowUnderCursor(int x, int y)
        {
            var pt = new System.Drawing.Point(x, y);
            var hwnd = PInvoke.WindowFromPoint(pt);

            if ((nint)hwnd.Value == 0) return 0;

            // 获取根窗口
            const uint GA_ROOT = 2;
            var rootHwnd = PInvoke.GetAncestor(hwnd, GET_ANCESTOR_FLAGS.GA_ROOT);

            return (nint)rootHwnd.Value != 0 ? (nint)rootHwnd.Value : (nint)hwnd.Value;
        }

        private bool IsMovableWindow(nint hwnd)
        {
            // 检查窗口是否可见
            if (!PInvoke.IsWindowVisible(new HWND(hwnd)))
                return false;

            // 检查窗口是否有标题栏（可移动）
            var style = (uint)GetWindowLong(hwnd, -16); // GWL_STYLE
            const uint WS_CAPTION = 0x00C00000;
            const uint WS_THICKFRAME = 0x00040000;

            return (style & WS_CAPTION) != 0 || (style & WS_THICKFRAME) != 0;
        }

        [DllImport("user32.dll", EntryPoint = "GetWindowLongW")]
        private static extern nint GetWindowLong32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW")]
        private static extern nint GetWindowLong64(IntPtr hWnd, int nIndex);

        private static nint GetWindowLong(nint hWnd, int nIndex)
        {
            if (IntPtr.Size == 8)
                return GetWindowLong64(hWnd, nIndex);
            else
                return GetWindowLong32(hWnd, nIndex);
        }

        private void ShowPreview(WindowAction action)
        {
            // 预览窗口逻辑
        }

        private void HidePreview()
        {
            // 隐藏预览窗口
        }

        public void Dispose()
        {
            _mouseHook?.Dispose();
            _previewWindow?.Dispose();
            Logger.Info("SnapDetectionService", "吸附检测服务已释放");
        }
    }
}
