#pragma once
#include "pch.h"
#include <atomic>
#include <mutex>

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
        void StartTracking();
        void StopTracking();
        bool IsValidWindow(int64_t hwnd) const;
        static void CALLBACK WinEventProc(HWINEVENTHOOK hWinEventHook, DWORD eventType, HWND hwnd,
            LONG idObject, LONG idChild, DWORD idEventThread, DWORD dwmsEventTime);

        std::atomic<int64_t> m_lastActiveWindow{ 0 };
        std::atomic<bool> m_trackingPaused{ false };
        HWINEVENTHOOK m_foregroundHook{ nullptr };
        mutable std::mutex m_mutex;
        static LastActiveWindowService* s_instance;
    };
}
