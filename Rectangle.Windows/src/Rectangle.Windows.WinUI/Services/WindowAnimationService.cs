using Rectangle.Windows.WinUI.Core;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Rectangle.Windows.WinUI.Services
{
    /// <summary>
    /// 窗口动画类型
    /// </summary>
    public enum WindowAnimationType
    {
        /// <summary>
        /// 无动画
        /// </summary>
        None,

        /// <summary>
        /// 淡入淡出
        /// </summary>
        Fade,

        /// <summary>
        /// 滑动
        /// </summary>
        Slide,

        /// <summary>
        /// 缩放
        /// </summary>
        Zoom,

        /// <summary>
        /// 组合动画
        /// </summary>
        Combined
    }

    /// <summary>
    /// 窗口动画配置
    /// </summary>
    public class AnimationConfig
    {
        /// <summary>
        /// 动画持续时间（毫秒）
        /// </summary>
        public int DurationMs { get; set; } = 200;

        /// <summary>
        /// 动画帧率
        /// </summary>
        public int FrameRate { get; set; } = 60;

        /// <summary>
        /// 缓动函数类型
        /// </summary>
        public EasingType Easing { get; set; } = EasingType.EaseOutCubic;

        /// <summary>
        /// 启用窗口移动动画
        /// </summary>
        public bool EnableMoveAnimation { get; set; } = true;

        /// <summary>
        /// 启用窗口缩放动画
        /// </summary>
        public bool EnableResizeAnimation { get; set; } = true;

        /// <summary>
        /// 最大动画窗口尺寸（像素）
        /// 大于此尺寸的窗口将使用简单动画
        /// </summary>
        public int MaxAnimationArea { get; set; } = 2560 * 1440; // 2K屏幕面积

        /// <summary>
        /// 启用快捷键操作反馈
        /// </summary>
        public bool EnableHotkeyFeedback { get; set; } = true;

        /// <summary>
        /// 快捷键反馈显示时长（毫秒）
        /// </summary>
        public int HotkeyFeedbackDurationMs { get; set; } = 800;
    }

    /// <summary>
    /// 缓动函数类型
    /// </summary>
    public enum EasingType
    {
        Linear,
        EaseInQuad,
        EaseOutQuad,
        EaseInOutQuad,
        EaseInCubic,
        EaseOutCubic,
        EaseInOutCubic,
        EaseOutBack,
        EaseOutElastic
    }

    /// <summary>
    /// 窗口动画服务 - 提供平滑的窗口动画效果
    /// </summary>
    public class WindowAnimationService
    {
        private readonly Logger _logger;
        private AnimationConfig _config = new();

        // 用于动画的常量
        private const uint SWP_FRAMECHANGED = 0x0020;
        private const uint SWP_NOACTIVATE = 0x0010;
        private const uint SWP_NOZORDER = 0x0004;
        private const uint SWP_SHOWWINDOW = 0x0040;

        public WindowAnimationService(Logger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 更新配置
        /// </summary>
        public void UpdateConfig(AnimationConfig config)
        {
            _config = config;
        }

        /// <summary>
        /// 获取当前配置
        /// </summary>
        public AnimationConfig GetConfig() => _config;

        /// <summary>
        /// 执行窗口位置和尺寸动画
        /// </summary>
        public async Task AnimateWindowAsync(nint hwnd, WindowRect targetRect, WindowAnimationType animationType = WindowAnimationType.Combined)
        {
            if (hwnd == 0)
            {
                return;
            }

            // 获取当前窗口位置
            var currentRect = GetWindowRect(hwnd);
            if (currentRect == null)
            {
                // 如果无法获取当前位置，直接设置为目标位置
                SetWindowPos(hwnd, targetRect);
                return;
            }

            // 如果位置相同，无需动画
            if (currentRect.Value == targetRect)
            {
                return;
            }

            // 检查窗口面积，大窗口使用简化动画
            int windowArea = currentRect.Value.Width * currentRect.Value.Height;
            if (windowArea > _config.MaxAnimationArea)
            {
                animationType = WindowAnimationType.Fade;
            }

            // 根据配置决定是否使用动画
            if (!_config.EnableMoveAnimation && !_config.EnableResizeAnimation)
            {
                SetWindowPos(hwnd, targetRect);
                return;
            }

            _logger?.LogDebug($"[Animation] 开始动画: {currentRect.Value} -> {targetRect}");

            await PerformAnimationAsync(hwnd, currentRect.Value, targetRect, animationType);
        }

        /// <summary>
        /// 执行动画
        /// </summary>
        private async Task PerformAnimationAsync(nint hwnd, WindowRect from, WindowRect to, WindowAnimationType type)
        {
            int totalFrames = (_config.DurationMs * _config.FrameRate) / 1000;
            int frameDelay = 1000 / _config.FrameRate;

            // 如果动画时间太短，直接设置
            if (totalFrames < 2)
            {
                SetWindowPos(hwnd, to);
                return;
            }

            // 启用窗口动画样式
            EnableWindowAnimation(hwnd, true);

            try
            {
                for (int frame = 0; frame <= totalFrames; frame++)
                {
                    double progress = (double)frame / totalFrames;
                    double easedProgress = ApplyEasing(progress, _config.Easing);

                    var currentRect = InterpolateRect(from, to, easedProgress);
                    SetWindowPos(hwnd, currentRect);

                    if (frame < totalFrames)
                    {
                        await Task.Delay(frameDelay);
                    }
                }
            }
            finally
            {
                // 确保最终位置正确
                SetWindowPos(hwnd, to);
                EnableWindowAnimation(hwnd, false);
            }

            _logger?.LogDebug("[Animation] 动画完成");
        }

        /// <summary>
        /// 矩形插值
        /// </summary>
        private WindowRect InterpolateRect(WindowRect from, WindowRect to, double progress)
        {
            return new WindowRect
            {
                X = (int)Math.Round(from.X + (to.X - from.X) * progress),
                Y = (int)Math.Round(from.Y + (to.Y - from.Y) * progress),
                Width = (int)Math.Round(from.Width + (to.Width - from.Width) * progress),
                Height = (int)Math.Round(from.Height + (to.Height - from.Height) * progress)
            };
        }

        /// <summary>
        /// 应用缓动函数
        /// </summary>
        private double ApplyEasing(double t, EasingType easing)
        {
            return easing switch
            {
                EasingType.Linear => t,
                EasingType.EaseInQuad => t * t,
                EasingType.EaseOutQuad => 1 - (1 - t) * (1 - t),
                EasingType.EaseInOutQuad => t < 0.5 ? 2 * t * t : 1 - Math.Pow(-2 * t + 2, 2) / 2,
                EasingType.EaseInCubic => t * t * t,
                EasingType.EaseOutCubic => 1 - Math.Pow(1 - t, 3),
                EasingType.EaseInOutCubic => t < 0.5 ? 4 * t * t * t : 1 - Math.Pow(-2 * t + 2, 3) / 2,
                EasingType.EaseOutBack => {
                    double c1 = 1.70158;
                    double c3 = c1 + 1;
                    return 1 + c3 * Math.Pow(t - 1, 3) + c1 * Math.Pow(t - 1, 2);
                },
                EasingType.EaseOutElastic => {
                    double c4 = (2 * Math.PI) / 3;
                    return t == 0 ? 0 : t == 1 ? 1 : Math.Pow(2, -10 * t) * Math.Sin((t * 10 - 0.75) * c4) + 1;
                },
                _ => t
            };
        }

        /// <summary>
        /// 启用/禁用窗口动画样式
        /// </summary>
        private void EnableWindowAnimation(nint hwnd, bool enable)
        {
            // 使用 SetWindowPos 的 SWP_FRAMECHANGED 来触发系统动画
            // 注意：这是可选的，某些窗口可能不支持
            try
            {
                var style = (WINDOW_STYLE)PInvoke.GetWindowLong((HWND)hwnd, WINDOW_LONG_PTR_INDEX.GWL_STYLE);
                if (enable)
                {
                    // 保留原有样式
                }
            }
            catch
            {
                // 忽略错误
            }
        }

        /// <summary>
        /// 获取窗口位置
        /// </summary>
        private WindowRect? GetWindowRect(nint hwnd)
        {
            try
            {
                Windows.Win32.Foundation.RECT rect;
                if (PInvoke.GetWindowRect((HWND)hwnd, out rect))
                {
                    return new WindowRect
                    {
                        X = rect.left,
                        Y = rect.top,
                        Width = rect.right - rect.left,
                        Height = rect.bottom - rect.top
                    };
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError($"[Animation] 获取窗口位置失败: {ex.Message}");
            }
            return null;
        }

        /// <summary>
        /// 设置窗口位置
        /// </summary>
        private void SetWindowPos(nint hwnd, WindowRect rect)
        {
            try
            {
                SET_WINDOW_POS_FLAGS flags = SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE |
                                              SET_WINDOW_POS_FLAGS.SWP_NOZORDER |
                                              SET_WINDOW_POS_FLAGS.SWP_NOOWNERZORDER;

                PInvoke.SetWindowPos(
                    (HWND)hwnd,
                    (HWND)0,
                    rect.X, rect.Y,
                    rect.Width, rect.Height,
                    flags);
            }
            catch (Exception ex)
            {
                _logger?.LogError($"[Animation] 设置窗口位置失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 显示快捷键操作反馈（视觉提示）
        /// </summary>
        public async Task ShowHotkeyFeedbackAsync(string actionName, WindowRect windowRect)
        {
            if (!_config.EnableHotkeyFeedback)
            {
                return;
            }

            // 创建反馈窗口
            var feedbackHwnd = CreateFeedbackWindow(actionName, windowRect);
            if (feedbackHwnd == 0)
            {
                return;
            }

            // 显示并淡出
            try
            {
                await Task.Delay(_config.HotkeyFeedbackDurationMs);
            }
            finally
            {
                DestroyFeedbackWindow(feedbackHwnd);
            }
        }

        /// <summary>
        /// 创建反馈窗口（简化实现）
        /// </summary>
        private nint CreateFeedbackWindow(string text, WindowRect rect)
        {
            // 实际实现需要创建顶层窗口显示文字
            // 这里返回0作为占位
            return 0;
        }

        /// <summary>
        /// 销毁反馈窗口
        /// </summary>
        private void DestroyFeedbackWindow(nint hwnd)
        {
            if (hwnd != 0)
            {
                PInvoke.DestroyWindow((HWND)hwnd);
            }
        }
    }
}
