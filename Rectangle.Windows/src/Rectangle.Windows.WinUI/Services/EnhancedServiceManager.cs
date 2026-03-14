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

        public OperationHistoryManager OperationHistory { get; }
        public WindowAnimationService AnimationService { get; }
        public DpiAwarenessService DpiService { get; }
        public HotkeyConflictDetector ConflictDetector { get; }
        public EdgeIndicatorService EdgeIndicator { get; }
        public WindowStatisticsService Statistics { get; }

        private readonly object _lock = new();

        public EnhancedServiceManager(
            WindowManager windowManager,
            ConfigService configService,
            MouseHookService mouseHook,
            ScreenDetectionService screenDetection)
        {
            _windowManager = windowManager;
            _configService = configService;

            OperationHistory = new OperationHistoryManager();
            AnimationService = new WindowAnimationService();
            DpiService = new DpiAwarenessService();
            ConflictDetector = new HotkeyConflictDetector();
            Statistics = new WindowStatisticsService();

            var edgeConfig = new EdgeIndicatorConfig();
            EdgeIndicator = new EdgeIndicatorService(mouseHook, screenDetection, edgeConfig);

            _configService.ConfigChanged += OnConfigChanged;
            OperationHistory.Undone += OnOperationUndone;
            OperationHistory.Redone += OnOperationRedone;

            InitializeServices();
        }

        private void InitializeServices()
        {
            var config = _configService.Load();

            AnimationService.UpdateConfig(new WindowAnimationConfig
            {
                DurationMs = config.Animation.DurationMs,
                FrameRate = config.Animation.FrameRate,
                Easing = ParseEasingType(config.Animation.EasingType),
                EnableMoveAnimation = config.Animation.EnableMoveAnimation,
                EnableResizeAnimation = config.Animation.EnableResizeAnimation,
                EnableHotkeyFeedback = config.Animation.EnableHotkeyFeedback,
                HotkeyFeedbackDurationMs = config.Animation.HotkeyFeedbackDurationMs
            });

            OperationHistory.MaxHistoryCount = config.History.MaxHistoryCount;

            Statistics.IsEnabled = config.Statistics.Enabled;
            Statistics.MaxRetentionDays = config.Statistics.MaxRetentionDays;
            Statistics.MaxHeatmapPoints = config.Statistics.MaxHeatmapPoints;

            if (config.Dpi.EnablePerMonitorDpi)
                _ = DpiService.GetAllDisplayDpiInfo();

            if (config.EdgeIndicator.Enabled)
                EdgeIndicator.Initialize();

            Logger.Info("EnhancedServiceManager", "所有增强服务已初始化");
        }

        private void OnConfigChanged(object? sender, AppConfig config)
        {
            AnimationService.UpdateConfig(new WindowAnimationConfig
            {
                DurationMs = config.Animation.DurationMs,
                FrameRate = config.Animation.FrameRate,
                Easing = ParseEasingType(config.Animation.EasingType),
                EnableMoveAnimation = config.Animation.EnableMoveAnimation,
                EnableResizeAnimation = config.Animation.EnableResizeAnimation
            });

            OperationHistory.MaxHistoryCount = config.History.MaxHistoryCount;
            Statistics.IsEnabled = config.Statistics.Enabled;

            if (config.EdgeIndicator.Enabled)
                EdgeIndicator.Initialize();
            else
                EdgeIndicator.Dispose();
        }

        public async Task ExecuteOperationAsync(
            WindowAction action,
            nint hwnd,
            bool skipAnimation = false,
            bool skipHistory = false)
        {
            var config = _configService.Load();

            if (action == WindowAction.Undo) { PerformUndo(); return; }
            if (action == WindowAction.Redo) { PerformRedo(); return; }

            var windowTitle = GetWindowTitle(hwnd);
            var windowClass = GetWindowClass(hwnd);
            var executableName = GetExecutableName(hwnd);
            var rectBefore = GetWindowRect(hwnd);

            if (config.Statistics.Enabled)
                Statistics.RecordWindowActivation(hwnd, windowClass, windowTitle, executableName);

            var startTime = DateTime.Now;
            bool success = false;

            try
            {
                if (!skipAnimation && config.Animation.Enabled)
                {
                    _windowManager.Execute(action);
                    var rectAfter = GetWindowRect(hwnd);
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
                Logger.Error("EnhancedServiceManager", $"执行操作失败: {ex.Message}");
            }

            var rectAfterFinal = GetWindowRect(hwnd);
            var executionTime = (DateTime.Now - startTime).TotalMilliseconds;

            if (!skipHistory && config.History.Enabled)
                OperationHistory.RecordOperation(action, hwnd, windowTitle, windowClass, rectBefore, rectAfterFinal);

            if (config.Statistics.Enabled)
                Statistics.RecordLayoutOperation(action, hwnd, windowClass, windowTitle, rectAfterFinal, success, executionTime);

            if (config.Animation.EnableHotkeyFeedback)
                _ = AnimationService.ShowHotkeyFeedbackAsync(action.ToString(), rectAfterFinal);
        }

        private void PerformUndo()
        {
            var item = OperationHistory.Undo();
            if (item != null)
            {
                try { SetWindowRect(item.WindowHandle, item.RectBefore); Logger.Info("EnhancedServiceManager", $"已撤销: {item.Description}"); }
                catch (Exception ex) { Logger.Error("EnhancedServiceManager", $"撤销失败: {ex.Message}"); }
            }
        }

        private void PerformRedo()
        {
            var item = OperationHistory.Redo();
            if (item != null)
            {
                try { SetWindowRect(item.WindowHandle, item.RectAfter); Logger.Info("EnhancedServiceManager", $"已重做: {item.Description}"); }
                catch (Exception ex) { Logger.Error("EnhancedServiceManager", $"重做失败: {ex.Message}"); }
            }
        }

        public HotkeyConflict? RegisterHotkeyWithConflictCheck(int id, uint modifiers, uint virtualKey, string actionName)
        {
            var config = _configService.Load();
            if (!config.ConflictDetection.Enabled) return null;

            var conflict = ConflictDetector.DetectConflict(modifiers, virtualKey, actionName);
            if (conflict != null)
            {
                if (config.ConflictDetection.ShowWarnings)
                    Logger.Warning("EnhancedServiceManager", $"快捷键冲突: {conflict.Description}");
                return conflict;
            }

            ConflictDetector.RegisterHotkey(id, modifiers, virtualKey, actionName);
            return null;
        }

        public WindowRect GetDpiAwareRect(nint hwnd, WindowRect logicalRect)
        {
            var config = _configService.Load();
            if (!config.Dpi.EnableDpiScaling) return logicalRect;

            var dpiInfo = DpiService.GetDpiInfoForWindow(hwnd);
            return dpiInfo?.LogicalToPhysicalRect(logicalRect) ?? logicalRect;
        }

        public StatisticsReport GenerateStatisticsReport() => Statistics.GenerateReport();

        public async Task<string> ExportStatistics(string format = "json") => await Statistics.ExportStatisticsAsync(format);

        private EasingType ParseEasingType(string easingName) => easingName switch
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

        private string GetWindowTitle(nint hwnd)
        {
            try
            {
                var length = PInvoke.GetWindowTextLength((HWND)hwnd);
                if (length > 0)
                {
                    unsafe
                    {
                        char* buffer = stackalloc char[length + 1];
                        PInvoke.GetWindowText((HWND)hwnd, buffer, length + 1);
                        return new string(buffer).TrimEnd('\0');
                    }
                }
            }
            catch { }
            return "";
        }

        private string GetWindowClass(nint hwnd)
        {
            try
            {
                unsafe
                {
                    char* buffer = stackalloc char[256];
                    PInvoke.GetClassName((HWND)hwnd, buffer, 256);
                    return new string(buffer).TrimEnd('\0');
                }
            }
            catch { }
            return "";
        }

        private string GetExecutableName(nint hwnd) => "";

        private WindowRect GetWindowRect(nint hwnd)
        {
            try
            {
                if (PInvoke.GetWindowRect((HWND)hwnd, out var rect))
                    return new WindowRect { X = rect.left, Y = rect.top, Width = rect.right - rect.left, Height = rect.bottom - rect.top };
            }
            catch { }
            return new WindowRect();
        }

        private void SetWindowRect(nint hwnd, WindowRect rect)
        {
            try
            {
                PInvoke.SetWindowPos((HWND)hwnd, new HWND(0), rect.X, rect.Y, rect.Width, rect.Height,
                    SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE | SET_WINDOW_POS_FLAGS.SWP_NOZORDER | SET_WINDOW_POS_FLAGS.SWP_NOOWNERZORDER);
            }
            catch (Exception ex)
            {
                Logger.Error("EnhancedServiceManager", $"设置窗口位置失败: {ex.Message}");
            }
        }

        public void Dispose()
        {
            _configService.ConfigChanged -= OnConfigChanged;
            OperationHistory.Undone -= OnOperationUndone;
            OperationHistory.Redone -= OnOperationRedone;
            EdgeIndicator?.Dispose();
            Statistics?.CleanupOldData();
        }

        private void OnOperationUndone(object? sender, OperationHistoryItem item) { }
        private void OnOperationRedone(object? sender, OperationHistoryItem item) { }
    }
}
