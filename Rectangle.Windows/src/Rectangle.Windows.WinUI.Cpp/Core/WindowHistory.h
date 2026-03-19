#pragma once
#include "pch.h"
#include "Core/WindowRect.h"
#include "Core/Enums.h"

namespace winrt::Rectangle::Core
{
    struct WindowHistoryRecord
    {
        WindowAction Action{ WindowAction::None };
        int32_t X{ 0 };
        int32_t Y{ 0 };
        int32_t Width{ 0 };
        int32_t Height{ 0 };
        int32_t Count{ 0 };
        std::chrono::steady_clock::time_point Time;
    };

    class WindowHistory
    {
    public:
        void SaveRestoreRect(int64_t hwnd, int32_t x, int32_t y, int32_t width, int32_t height);
        bool HasRestoreRect(int64_t hwnd) const;
        WindowRect GetRestoreRect(int64_t hwnd) const;
        void ClearRestoreRect(int64_t hwnd);

        void MarkAsProgramAdjusted(int64_t hwnd);
        bool IsWindowMovedExternally(int64_t hwnd, int32_t x, int32_t y, int32_t width, int32_t height) const;
        void RemoveLastAction(int64_t hwnd);

        bool TryGetLastAction(int64_t hwnd, WindowHistoryRecord& record) const;
        void RecordAction(int64_t hwnd, WindowAction action, int32_t x, int32_t y, int32_t width, int32_t height);

        void Cleanup();

    private:
        struct WindowData
        {
            WindowRect RestoreRect;
            bool HasRestoreRect{ false };
            WindowHistoryRecord LastAction;
            bool HasLastAction{ false };
            std::chrono::steady_clock::time_point LastProgramAdjustTime;
        };

        std::map<int64_t, WindowData> m_windowData;
        mutable std::shared_mutex m_mutex;
    };
}
