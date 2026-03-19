#pragma once
#include "pch.h"
#include <atomic>

namespace winrt::Rectangle::Services
{
    class LastActiveWindowService
    {
    public:
        LastActiveWindowService();
        ~LastActiveWindowService();

        int64_t GetLastActiveWindow() const;
        void SetLastActiveWindow(int64_t hwnd);

        void PauseTracking();
        void ResumeTracking();
        bool IsTrackingPaused() const;

        int64_t GetTargetWindow();

    private:
        std::atomic<int64_t> m_lastActiveWindow{ 0 };
        std::atomic<bool> m_trackingPaused{ false };
    };
}
