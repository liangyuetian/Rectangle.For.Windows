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
    SnapDetectionService* SnapDetectionService::s_instance = nullptr;

    SnapDetectionService::SnapDetectionService(void* win32, void* windowManager, ConfigService* config)
        : m_win32(win32)
        , m_windowManager(windowManager)
        , m_config(config)
    {
        s_instance = this;
    }

    SnapDetectionService::~SnapDetectionService()
    {
        Stop();
        s_instance = nullptr;
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

        if (s_instance && lParam)
        {
            auto* info = reinterpret_cast<MSLLHOOKSTRUCT*>(lParam);
            if (wParam == WM_LBUTTONDOWN)
            {
                s_instance->OnMouseDown(*info);
            }
            else if (wParam == WM_LBUTTONUP)
            {
                s_instance->OnMouseUp(*info);
            }
        }

        return CallNextHookEx(nullptr, nCode, wParam, lParam);
    }

    void SnapDetectionService::OnMouseDown(const MSLLHOOKSTRUCT& info)
    {
        m_dragging = true;
        m_startX = info.pt.x;
        m_startY = info.pt.y;
    }

    void SnapDetectionService::OnMouseUp(const MSLLHOOKSTRUCT& info)
    {
        if (!m_dragging)
        {
            return;
        }
        m_dragging = false;

        const int dx = info.pt.x - m_startX;
        const int dy = info.pt.y - m_startY;
        if ((dx * dx + dy * dy) < 100)
        {
            return;
        }

        auto action = DetectSnapAction(info.pt.x, info.pt.y);
        if (!action.has_value() || !m_windowManager)
        {
            return;
        }

        auto* wm = static_cast<WindowManager*>(m_windowManager);
        wm->Execute(action.value(), 0, true);
    }

    std::optional<Core::WindowAction> SnapDetectionService::DetectSnapAction(int32_t x, int32_t y) const
    {
        int32_t marginTop = 5;
        int32_t marginBottom = 5;
        int32_t marginLeft = 5;
        int32_t marginRight = 5;
        int32_t cornerSize = 20;
        if (m_config)
        {
            auto cfg = m_config->Load();
            marginTop = cfg.SnapEdgeMarginTop;
            marginBottom = cfg.SnapEdgeMarginBottom;
            marginLeft = cfg.SnapEdgeMarginLeft;
            marginRight = cfg.SnapEdgeMarginRight;
            cornerSize = cfg.CornerSnapAreaSize;
        }

        POINT pt{ x, y };
        HMONITOR monitor = MonitorFromPoint(pt, MONITOR_DEFAULTTONEAREST);
        MONITORINFO mi{};
        mi.cbSize = sizeof(mi);
        if (!monitor || !GetMonitorInfo(monitor, &mi))
        {
            return std::nullopt;
        }

        const int left = mi.rcWork.left;
        const int top = mi.rcWork.top;
        const int right = mi.rcWork.right;
        const int bottom = mi.rcWork.bottom;

        const bool nearLeft = x <= left + marginLeft;
        const bool nearRight = x >= right - marginRight;
        const bool nearTop = y <= top + marginTop;
        const bool nearBottom = y >= bottom - marginBottom;

        if (nearTop && nearLeft && x <= left + cornerSize && y <= top + cornerSize) return Core::WindowAction::TopLeft;
        if (nearTop && nearRight && x >= right - cornerSize && y <= top + cornerSize) return Core::WindowAction::TopRight;
        if (nearBottom && nearLeft && x <= left + cornerSize && y >= bottom - cornerSize) return Core::WindowAction::BottomLeft;
        if (nearBottom && nearRight && x >= right - cornerSize && y >= bottom - cornerSize) return Core::WindowAction::BottomRight;
        if (nearLeft) return Core::WindowAction::LeftHalf;
        if (nearRight) return Core::WindowAction::RightHalf;
        if (nearTop) return Core::WindowAction::TopHalf;
        if (nearBottom) return Core::WindowAction::BottomHalf;
        return std::nullopt;
    }
}
