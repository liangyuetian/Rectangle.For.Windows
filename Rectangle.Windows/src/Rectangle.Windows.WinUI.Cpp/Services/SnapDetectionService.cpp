#include "pch.h"
#include "Services/SnapDetectionService.h"
#include "Services/Logger.h"
#include "Services/Win32WindowService.h"
#include "Services/WindowManager.h"
#include "Services/ConfigService.h"
#include <Windows.h>

#pragma comment(lib, "user32.lib")

namespace winrt::Rectangle::Services
{
    SnapDetectionService::SnapDetectionService(void* win32, void* windowManager, ConfigService* config)
        : m_win32(win32)
        , m_windowManager(windowManager)
        , m_config(config)
    {
    }

    SnapDetectionService::~SnapDetectionService()
    {
        Stop();
    }

    void SnapDetectionService::Start()
    {
        if (m_isRunning) return;

        InstallMouseHook();
        m_isRunning = true;
        Logger::Instance().Info(L"SnapDetectionService", L"Snap detection started");
    }

    void SnapDetectionService::Stop()
    {
        if (!m_isRunning) return;

        UninstallMouseHook();
        m_isRunning = false;
        Logger::Instance().Info(L"SnapDetectionService", L"Snap detection stopped");
    }

    bool SnapDetectionService::IsRunning() const
    {
        return m_isRunning;
    }

    void SnapDetectionService::InstallMouseHook()
    {
        m_mouseHook = SetWindowsHookEx(WH_MOUSE_LL, MouseHookProc, GetModuleHandle(nullptr), 0);
        if (!m_mouseHook)
        {
            Logger::Instance().Error(L"SnapDetectionService", L"Failed to install mouse hook");
        }
    }

    void SnapDetectionService::UninstallMouseHook()
    {
        if (m_mouseHook)
        {
            UnhookWindowsHookEx(m_mouseHook);
            m_mouseHook = nullptr;
        }
    }

    LRESULT CALLBACK SnapDetectionService::MouseHookProc(int32_t nCode, WPARAM wParam, LPARAM lParam)
    {
        if (nCode < 0)
        {
            return CallNextHookEx(nullptr, nCode, wParam, lParam);
        }

        return CallNextHookEx(nullptr, nCode, wParam, lParam);
    }
}
