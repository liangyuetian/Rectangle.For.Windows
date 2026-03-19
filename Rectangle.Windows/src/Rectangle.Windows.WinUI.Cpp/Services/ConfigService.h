#pragma once
#include "pch.h"
#include <map>
#include <string>
#include <functional>

namespace winrt::Rectangle::Services
{
    struct ShortcutConfig
    {
        bool Enabled{ true };
        int32_t KeyCode{ 0 };
        uint32_t ModifierFlags{ 0 };
    };

    struct AnimationConfig
    {
        bool Enabled{ true };
        int32_t DurationMs{ 200 };
        int32_t FrameRate{ 60 };
        std::wstring EasingType{ L"EaseOutCubic" };
        bool EnableMoveAnimation{ true };
        bool EnableResizeAnimation{ true };
        bool EnableHotkeyFeedback{ true };
        int32_t HotkeyFeedbackDurationMs{ 800 };
    };

    struct HistoryConfig
    {
        bool Enabled{ true };
        int32_t MaxHistoryCount{ 50 };
        bool EnableUndo{ true };
        std::wstring UndoShortcut{ L"Ctrl+Alt+Z" };
        std::wstring RedoShortcut{ L"Ctrl+Alt+Shift+Z" };
    };

    struct EdgeIndicatorSettings
    {
        bool Enabled{ false };
        int32_t IndicatorWidth{ 8 };
        std::wstring DisplayMode{ L"AutoHide" };
        int32_t AutoHideDelayMs{ 2000 };
        int32_t TriggerDistance{ 10 };
        bool ShowSnapAreas{ true };
        double SnapAreaOpacity{ 0.15 };
        std::wstring NormalColor{ L"#500078D7" };
        std::wstring HoverColor{ L"#B40096FF" };
        std::wstring ActiveColor{ L"#FF00B4FF" };
    };

    struct ConflictDetectionConfig
    {
        bool Enabled{ true };
        bool ShowWarnings{ true };
        bool AutoSuggestAlternatives{ true };
        bool CheckSystemHotkeys{ true };
        bool CheckKnownApps{ true };
    };

    struct DpiConfig
    {
        bool EnablePerMonitorDpi{ true };
        bool EnableDpiScaling{ true };
        float FallbackDpi{ 96.0f };
    };

    struct StatisticsConfig
    {
        bool Enabled{ true };
        int32_t MaxRetentionDays{ 90 };
        bool TrackWindowUsage{ true };
        bool TrackLayoutUsage{ true };
        bool GenerateHeatmap{ true };
        int32_t MaxHeatmapPoints{ 10000 };
    };

    struct SnapAreaConfig
    {
        bool DragToSnap{ true };
        bool RestoreSizeOnSnapEnd{ true };
        bool HapticFeedback{ false };
        bool SnapAnimation{ false };
        std::map<std::wstring, std::wstring> AreaActions;
    };

    struct AppConfig
    {
        int32_t GapSize{ 0 };
        int32_t HorizontalSplitRatio{ 50 };
        int32_t VerticalSplitRatio{ 50 };
        bool LaunchOnLogin{ false };
        std::vector<std::wstring> IgnoredApps;
        std::map<std::wstring, ShortcutConfig> Shortcuts;
        SnapAreaConfig SnapAreas;
        int32_t SubsequentExecutionMode{ 1 };
        float AlmostMaximizeHeight{ 0.9f };
        float AlmostMaximizeWidth{ 0.9f };
        float MinimumWindowWidth{ 0 };
        float MinimumWindowHeight{ 0 };
        float SizeOffset{ 30 };
        bool CenteredDirectionalMove{ false };
        bool ResizeOnDirectionalMove{ false };
        bool UseCursorScreenDetection{ false };
        bool MoveCursor{ false };
        bool MoveCursorAcrossDisplays{ false };
        float FootprintAlpha{ 0.3f };
        int32_t FootprintBorderWidth{ 2 };
        int32_t FootprintColor{ -16711614 };
        int32_t FootprintBorderColor{ -16711614 };
        bool FootprintFade{ true };
        int32_t FootprintAnimationDuration{ 150 };
        bool UnsnapRestore{ true };
        bool DragToSnap{ true };
        int32_t SnapEdgeMarginTop{ 5 };
        int32_t SnapEdgeMarginBottom{ 5 };
        int32_t SnapEdgeMarginLeft{ 5 };
        int32_t SnapEdgeMarginRight{ 5 };
        int32_t CornerSnapAreaSize{ 20 };
        int32_t SnapModifiers{ 0 };
        bool HapticFeedbackOnSnap{ false };
        bool TodoMode{ false };
        std::wstring TodoApplication{ L"" };
        int32_t TodoSidebarWidth{ 400 };
        int32_t TodoSidebarSide{ 1 };
        int32_t CascadeAllDeltaSize{ 30 };
        int32_t LogLevel{ 1 };
        bool LogToFile{ false };
        std::wstring LogFilePath{ L"" };
        int32_t MaxLogFileSize{ 10 };
        int32_t MaxWindowHistoryCount{ 100 };
        int32_t WindowHistoryExpirationMinutes{ 60 };
        int32_t SpecifiedWidth{ 1680 };
        int32_t SpecifiedHeight{ 1050 };
        std::wstring Theme{ L"Default" };
        std::wstring Language{ L"zh-CN" };
        bool CheckForUpdates{ true };
        AnimationConfig Animation;
        HistoryConfig History;
        EdgeIndicatorSettings EdgeIndicator;
        ConflictDetectionConfig ConflictDetection;
        DpiConfig Dpi;
        StatisticsConfig Statistics;
    };

    class ConfigService
    {
    public:
        ConfigService();
        ~ConfigService() = default;

        AppConfig Load();
        void Save(const AppConfig& config);

        static std::map<std::wstring, ShortcutConfig> GetDefaultShortcuts();

        std::function<void(const AppConfig&)> ConfigChanged;

    private:
        AppConfig CreateDefaultConfig();
        std::wstring GetConfigPath();
        void EnsureConfigDirectoryExists();

        std::wstring m_configPath;
    };
}
