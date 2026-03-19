#pragma once
#include "pch.h"
#include "Core/WindowRect.h"
#include "Core/Enums.h"

namespace winrt::Rectangle::Services
{
    class WindowManager
    {
    public:
        WindowManager();
        ~WindowManager();

        void SetConfigService(ConfigService* configService);
        void SetLastActiveWindowService(void* service);
        void SetOperationHistory(void* history);

        void Execute(WindowAction action, int64_t targetHwnd = 0, bool forceDirectAction = false);
        void ExecuteRestore(int64_t targetHwnd = 0);
        void ExecuteMaximizeToggle(int64_t targetHwnd = 0);
        void ExecuteNextDisplay(int64_t targetHwnd = 0);
        void ExecutePreviousDisplay(int64_t targetHwnd = 0);
        void ExecuteUndo();
        void ExecuteRedo();

        int64_t GetTargetWindow() const;

    private:
        void ReloadConfig();
        bool IsIgnoredApp(const std::wstring& processName) const;
        bool IsWindowMovable(int64_t hwnd) const;

        int64_t GetTargetWindowCore() const;
        Core::WorkArea GetTargetWorkArea(int64_t hwnd) const;
        Core::WorkArea GetWorkAreaByDisplayIndex(int32_t index) const;

        std::pair<WindowAction, std::optional<int32_t>> GetActualAction(
            int64_t hwnd, WindowAction action, bool windowMovedExternally);

        Core::WindowRect ApplyWindowGap(Core::WindowRect target, const Core::WorkArea& workArea, WindowAction action);
        Core::WindowRect ApplyMinimumSize(Core::WindowRect target);
        Core::WindowRect ClampToWorkArea(Core::WindowRect target, const Core::WorkArea& workArea);

        void MoveCursorIfEnabled(int64_t hwnd, WindowAction action);
        void MoveCursorToWindowCenter(int64_t hwnd);
        void PlayBeep();
        void RecordToOperationHistory(WindowAction action, int64_t hwnd,
            const Core::WindowRect& oldRect, const Core::WindowRect& newRect,
            const std::wstring& processName);

        ConfigService* m_configService{ nullptr };
        void* m_lastActiveService{ nullptr };
        void* m_operationHistory{ nullptr };
        void* m_win32{ nullptr };
        void* m_factory{ nullptr };
        void* m_history{ nullptr };

        int32_t m_gapSize{ 0 };
        std::set<int64_t> m_maximizedWindows;
        mutable int64_t m_cachedTargetWindow{ 0 };

        std::function<void(int64_t, WindowAction)> m_windowTypeCheck;
    };
}
