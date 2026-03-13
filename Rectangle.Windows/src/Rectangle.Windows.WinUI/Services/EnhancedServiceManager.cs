using Rectangle.Windows.WinUI.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Rectangle.Windows.WinUI.Services
{
    /// <summary>
    /// 增强服务管理器 - 整合所有新功能服务
    /// </summary>
    public class EnhancedServiceManager
    {
        private readonly WindowManager _windowManager;
        private readonly ConfigService _configService;
        private readonly Logger _logger;

        // 新服务
        public OperationHistoryManager OperationHistory { get; }
        public WindowAnimationService AnimationService { get; }
        public DpiAwarenessService DpiService { get; }
        public HotkeyConflictDetector ConflictDetector { get; }
        public EdgeIndicatorService EdgeIndicator { get; }
        public WindowStatisticsService Statistics { get; }

        private readonly Dictionary<nint, WindowRect> _windowRectsBeforeOperation = new();
        private readonly object _lock = new();

        public EnhancedServiceManager(
            WindowManager windowManager,
            ConfigService configService,
            Logger logger,
            MouseHookService mouseHook,
            ScreenDetectionService screenDetection)
        {
            _windowManager = windowManager;
            _configService = configService;
            _logger = logger;

            // 初始化新服务
            OperationHistory = new OperationHistoryManager(logger);
            AnimationService = new WindowAnimationService(logger);
            DpiService = new DpiAwarenessService(logger);
            ConflictDetector = new HotkeyConflictDetector(logger);
            Statistics = new WindowStatisticsService(logger);

            // 边缘指示器需要鼠标钩子和屏幕检测服务
            var edgeConfig = new EdgeIndicatorConfig();
            EdgeIndicator = new EdgeIndicatorService(logger, mouseHook, screenDetection, edgeConfig);

            // 订阅配置变更
            _configService.ConfigChanged += OnConfigChanged;

            // 订阅操作历史事件
            OperationHistory.Undone += OnOperationUndone;
            OperationHistory.Redone += OnOperationRedone;

            // 初始化服务
            InitializeServices();
        }

        /// <summary>
        /// 初始化所有服务
        /// </summary>
        private void InitializeServices()
        {
            var config = _configService.Load();

            // 配置动画服务
            AnimationService.UpdateConfig(new AnimationConfig
            {
                DurationMs = config.Animation.DurationMs,
                FrameRate = config.Animation.FrameRate,
                Easing = ParseEasingType(config.Animation.EasingType),
                EnableMoveAnimation = config.Animation.EnableMoveAnimation,
                EnableResizeAnimation = config.Animation.EnableResizeAnimation,
                EnableHotkeyFeedback = config.Animation.EnableHotkeyFeedback,
                HotkeyFeedbackDurationMs = config.Animation.HotkeyFeedbackDurationMs
            });

            // 配置操作历史
            OperationHistory.MaxHistoryCount = config.History.MaxHistoryCount;

            // 配置统计服务
            Statistics.IsEnabled = config.Statistics.Enabled;
            Statistics.MaxRetentionDays = config.Statistics.MaxRetentionDays;
            Statistics.MaxHeatmapPoints = config.Statistics.MaxHeatmapPoints;

            // 初始化DPI服务
            if (config.Dpi.EnablePerMonitorDpi)
            {
                _ = DpiService.GetAllDisplayDpiInfo();
            }

            // 初始化边缘指示器
            if (config.EdgeIndicator.Enabled)
            {
                EdgeIndicator.Initialize();
            }

            _logger.LogInfo("[EnhancedServiceManager] 所有增强服务已初始化");
        }

        /// <summary>
        /// 配置变更处理
        /// </summary>
        private void OnConfigChanged(object? sender, AppConfig config)
        {
            // 更新动画配置
            AnimationService.UpdateConfig(new AnimationConfig
            {
                DurationMs = config.Animation.DurationMs,
                FrameRate = config.Animation.FrameRate,
                Easing = ParseEasingType(config.Animation.EasingType),
                EnableMoveAnimation = config.Animation.EnableMoveAnimation,
                EnableResizeAnimation = config.Animation.EnableResizeAnimation
            });

            // 更新历史配置
            OperationHistory.MaxHistoryCount = config.History.MaxHistoryCount;

            // 更新统计配置
            Statistics.IsEnabled = config.Statistics.Enabled;

            // 更新边缘指示器
            if (config.EdgeIndicator.Enabled)
            {
                EdgeIndicator.Initialize();
            }
            else
            {
                EdgeIndicator.Dispose();
            }
        }

        /// <summary>
        /// 执行窗口操作（增强版，带历史记录和动画）
        /// </summary>
        public async Task ExecuteOperationAsync(
            WindowAction action,
            nint hwnd,
            bool skipAnimation = false,
            bool skipHistory = false)
        {
            var config = _configService.Load();

            // 检查是否是撤销/重做操作
            if (action == WindowAction.Undo)
            {
                PerformUndo();
                return;
            }
            if (action == WindowAction.Redo)
            {
                PerformRedo();
                return;
            }

            // 获取窗口信息
            var windowTitle = GetWindowTitle(hwnd);
            var windowClass = GetWindowClass(hwnd);
            var executableName = GetExecutableName(hwnd);

            // 获取操作前的窗口位置
            var rectBefore = GetWindowRect(hwnd);

            // 记录窗口激活
            if (config.Statistics.Enabled)
            {
                Statistics.RecordWindowActivation(hwnd, windowClass, windowTitle, executableName);
            }

            // 执行操作（带或不带动画）
            var startTime = DateTime.Now;
            bool success = false;

            try
            {
                if (!skipAnimation && config.Animation.Enabled)
                {
                    // 先执行操作获取目标位置
                    _windowManager.Execute(action);
                    var rectAfter = GetWindowRect(hwnd);

                    // 执行动画
                    await AnimationService.AnimateWindowAsync(hwnd, rectAfter);
                    success = true;
                }
                else
                {
                    _windowManager.Execute(action);
                    success = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"[EnhancedServiceManager] 执行操作失败: {ex.Message}");
            }

            // 获取操作后的窗口位置
            var rectAfterFinal = GetWindowRect(hwnd);
            var executionTime = (DateTime.Now - startTime).TotalMilliseconds;

            // 记录操作历史
            if (!skipHistory && config.History.Enabled)
            {
                OperationHistory.RecordOperation(
                    action, hwnd, windowTitle, windowClass,
                    rectBefore, rectAfterFinal);
            }

            // 记录统计
            if (config.Statistics.Enabled)
            {
                Statistics.RecordLayoutOperation(
                    action, hwnd, windowClass, windowTitle,
                    rectAfterFinal, success, executionTime);
            }

            // 显示快捷键反馈
            if (config.Animation.EnableHotkeyFeedback)
            {
                _ = AnimationService.ShowHotkeyFeedbackAsync(
                    action.ToString(), rectAfterFinal);
            }
        }

        /// <summary>
        /// 执行撤销
        /// </summary>
        private void PerformUndo()
        {
            var item = OperationHistory.Undo();
            if (item != null)
            {
                try
                {
                    // 恢复窗口到之前的位置
                    SetWindowRect(item.WindowHandle, item.RectBefore);
                    _logger.LogInfo($"[EnhancedServiceManager] 已撤销: {item.Description}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"[EnhancedServiceManager] 撤销失败: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 执行重做
        /// </summary>
        private void PerformRedo()
        {
            var item = OperationHistory.Redo();
            if (item != null)
            {
                try
                {
                    // 恢复窗口到之后的位置
                    SetWindowRect(item.WindowHandle, item.RectAfter);
                    _logger.LogInfo($"[EnhancedServiceManager] 已重做: {item.Description}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"[EnhancedServiceManager] 重做失败: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 注册快捷键时检查冲突
        /// </summary>
        public HotkeyConflict? RegisterHotkeyWithConflictCheck(
            int id,
            HOT_KEY_MODIFIERS modifiers,
            uint virtualKey,
            string actionName)
        {
            var config = _configService.Load();
            if (!config.ConflictDetection.Enabled)
            {
                return null;
            }

            // 检测冲突
            var conflict = ConflictDetector.DetectConflict(modifiers, virtualKey, actionName);
            if (conflict != null)
            {
                if (config.ConflictDetection.ShowWarnings)
                {
                    _logger.LogWarning(
                        $"[EnhancedServiceManager] 快捷键冲突: {conflict.Description}");
                }
                return conflict;
            }

            // 记录已注册快捷键
            ConflictDetector.RegisterHotkey(id, modifiers, virtualKey, actionName);
            return null;
        }

        /// <summary>
        /// 获取DPI感知后的窗口尺寸
        /// </summary>
        public WindowRect GetDpiAwareRect(nint hwnd, WindowRect logicalRect)
        {
            var config = _configService.Load();
            if (!config.Dpi.EnableDpiScaling)
            {
                return logicalRect;
            }

            var dpiInfo = DpiService.GetDpiInfoForWindow(hwnd);
            if (dpiInfo == null)
            {
                return logicalRect;
            }

            return dpiInfo.LogicalToPhysicalRect(logicalRect);
        }

        /// <summary>
        /// 生成统计报告
        /// </summary>
        public StatisticsReport GenerateStatisticsReport()
        {
            return Statistics.GenerateReport();
        }

        /// <summary>
        /// 导出统计数据
        /// </summary>
        public async Task<string> ExportStatistics(string format = "json")
        {
            return await Statistics.ExportStatisticsAsync(format);
        }

        /// <summary>
        /// 解析缓动类型
        /// </summary>
        private EasingType ParseEasingType(string easingName)
        {
            return easingName switch
            {
                "Linear" => EasingType.Linear,
                "EaseInQuad" => EasingType.EaseInQuad,
                "EaseOutQuad" => EasingType.EaseOutQuad,
                "EaseInOutQuad" => EasingType.EaseInOutQuad,
                "EaseInCubic" => EasingType.EaseInCubic,
                "EaseOutCubic" => EasingType.EaseOutCubic,
                "EaseInOutCubic" => EasingType.EaseInOutCubic,
                "EaseOutBack" => EasingType.EaseOutBack,
                "EaseOutElastic" => EasingType.EaseOutElastic,
                _ => EasingType.EaseOutCubic
            };
        }

        /// <summary>
        /// 获取窗口标题
        /// </summary>
        private string GetWindowTitle(nint hwnd)
        {
            try
            {
                var length = PInvoke.GetWindowTextLength((HWND)hwnd);
                if (length > 0)
                {
                    var buffer = new char[length + 1];
                    PInvoke.GetWindowText((HWND)hwnd, ref buffer[0], length + 1);
                    return new string(buffer).TrimEnd('\0');
                }
            }
            catch { }
            return "";
        }

        /// <summary>
        /// 获取窗口类名
        /// </summary>
        private string GetWindowClass(nint hwnd)
        {
            try
            {
                var buffer = new char[256];
                PInvoke.GetClassName((HWND)hwnd, ref buffer[0], 256);
                return new string(buffer).TrimEnd('\0');
            }
            catch { }
            return "";
        }

        /// <summary>
        /// 获取可执行文件名
        /// </summary>
        private string GetExecutableName(nint hwnd)
        {
            // 简化实现，实际需要获取进程信息
            return "";
        }

        /// <summary>
        /// 获取窗口矩形
        /// </summary>
        private WindowRect GetWindowRect(nint hwnd)
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
            catch { }
            return new WindowRect();
        }

        /// <summary>
        /// 设置窗口矩形
        /// </summary>
        private void SetWindowRect(nint hwnd, WindowRect rect)
        {
            try
            {
                PInvoke.SetWindowPos(
                    (HWND)hwnd,
                    (HWND)0,
                    rect.X, rect.Y, rect.Width, rect.Height,
                    SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE |
                    SET_WINDOW_POS_FLAGS.SWP_NOZORDER |
                    SET_WINDOW_POS_FLAGS.SWP_NOOWNERZORDER);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[EnhancedServiceManager] 设置窗口位置失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            _configService.ConfigChanged -= OnConfigChanged;
            OperationHistory.Undone -= OnOperationUndone;
            OperationHistory.Redone -= OnOperationRedone;

            EdgeIndicator?.Dispose();
            Statistics?.CleanupOldData();
        }

        /// <summary>
        /// 撤销事件处理
        /// </summary>
        private void OnOperationUndone(object? sender, OperationHistoryItem item)
        {
            // 可以在这里添加撤销的视觉反馈
        }

        /// <summary>
        /// 重做事件处理
        /// </summary>
        private void OnOperationRedone(object? sender, OperationHistoryItem item)
        {
            // 可以在这里添加重做的视觉反馈
        }
    }
}
