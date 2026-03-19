#include "pch.h"
#include "Services/LastActiveWindowService.h"
#include <Windows.h>

namespace winrt::Rectangle::Services
{
    LastActiveWindowService::LastActiveWindowService()
    {
        Logger::Instance().Info(L"LastActiveWindowService", L"LastActiveWindowService initialized");
    }

    LastActiveWindowService::~LastActiveWindowService() = default;

    int64_t LastActiveWindowService::GetLastActiveWindow() const
    {
        return m_lastActiveWindow.load();
    }

    void LastActiveWindowService::SetLastActiveWindow(int64_t hwnd)
    {
        m_lastActiveWindow.store(hwnd);
    }

    void LastActiveWindowService::PauseTracking()
    {
        m_trackingPaused.store(true);
    }

    void LastActiveWindowService::ResumeTracking()
    {
        m_trackingPaused.store(false);
    }

    bool LastActiveWindowService::IsTrackingPaused() const
    {
        return m_trackingPaused.load();
    }

    int64_t LastActiveWindowService::GetTargetWindow()
    {
        if (m_trackingPaused.load())
        {
            return m_lastActiveWindow.load();
        }

        int64_t hwnd = reinterpret_cast<int64_t>(GetForegroundWindow());
        if (hwnd != 0)
        {
            m_lastActiveWindow.store(hwnd);
        }

        return hwnd;
    }
}
