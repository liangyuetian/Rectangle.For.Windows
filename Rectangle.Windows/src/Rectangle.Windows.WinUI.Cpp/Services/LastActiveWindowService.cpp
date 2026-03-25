#include "pch.h"
#include "Services/LastActiveWindowService.h"
#include "Services/Logger.h"
#include <Windows.h>

namespace winrt::Rectangle::Services
{
    LastActiveWindowService* LastActiveWindowService::s_instance = nullptr;

    LastActiveWindowService::LastActiveWindowService()
    {
        s_instance = this;
        StartTracking();
        Logger::Instance().Info(L"LastActiveWindowService", L"LastActiveWindowService initialized");
    }

    LastActiveWindowService::~LastActiveWindowService()
    {
        StopTracking();
        s_instance = nullptr;
    }

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
        if (IsValidWindow(hwnd))
        {
            m_lastActiveWindow.store(hwnd);
            return hwnd;
        }

        return m_lastActiveWindow.load();
    }

    void LastActiveWindowService::StartTracking()
    {
        std::lock_guard<std::mutex> lock(m_mutex);
        if (m_foregroundHook)
        {
            return;
        }

        m_foregroundHook = SetWinEventHook(
            EVENT_SYSTEM_FOREGROUND,
            EVENT_SYSTEM_FOREGROUND,
            nullptr,
            &LastActiveWindowService::WinEventProc,
            0,
            0,
            WINEVENT_OUTOFCONTEXT | WINEVENT_SKIPOWNPROCESS);
    }

    void LastActiveWindowService::StopTracking()
    {
        std::lock_guard<std::mutex> lock(m_mutex);
        if (m_foregroundHook)
        {
            UnhookWinEvent(m_foregroundHook);
            m_foregroundHook = nullptr;
        }
    }

    bool LastActiveWindowService::IsValidWindow(int64_t hwnd) const
    {
        HWND h = reinterpret_cast<HWND>(hwnd);
        if (!h || !IsWindow(h) || !IsWindowVisible(h) || IsIconic(h))
        {
            return false;
        }

        if (GetWindow(h, GW_OWNER) != nullptr)
        {
            return false;
        }

        wchar_t className[256]{};
        GetClassName(h, className, 256);
        static const wchar_t* ignoredClasses[] = {
            L"Progman", L"WorkerW", L"Shell_TrayWnd", L"DV2ControlHost", L"Windows.UI.Core.CoreWindow"
        };
        for (auto cls : ignoredClasses)
        {
            if (_wcsicmp(className, cls) == 0)
            {
                return false;
            }
        }

        return true;
    }

    void CALLBACK LastActiveWindowService::WinEventProc(
        HWINEVENTHOOK,
        DWORD eventType,
        HWND hwnd,
        LONG idObject,
        LONG idChild,
        DWORD,
        DWORD)
    {
        if (eventType != EVENT_SYSTEM_FOREGROUND || idObject != OBJID_WINDOW || idChild != CHILDID_SELF)
        {
            return;
        }
        if (!s_instance || s_instance->m_trackingPaused.load())
        {
            return;
        }

        auto value = reinterpret_cast<int64_t>(hwnd);
        if (s_instance->IsValidWindow(value))
        {
            s_instance->m_lastActiveWindow.store(value);
        }
    }
}
