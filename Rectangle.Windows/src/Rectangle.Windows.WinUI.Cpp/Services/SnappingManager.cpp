#include "pch.h"
#include "Services/SnappingManager.h"
#include "Services/MouseHookService.h"
#include "Services/Logger.h"
#include "Services/Win32WindowService.h"
#include "Services/ConfigService.h"
#include "Core/WindowHistory.h"

namespace winrt::Rectangle::Services
{
    SnappingManager::SnappingManager()
    {
        m_mouseHook = std::make_unique<MouseHookService>();
        Logger::Instance().Info(L"SnappingManager", L"SnappingManager constructed");
    }

    void SnappingManager::Initialize()
    {
        m_mouseHook->OnMouseDown([this](const MouseHookEventArgs& args) {
            m_dragStartX = args.X;
            m_dragStartY = args.Y;
            m_lastX = args.X;
            m_lastY = args.Y;
            m_hasMoved = false;
            StartDragDetection();
        });

        m_mouseHook->OnMouseMove([this](const MouseHookEventArgs& args) {
            ProcessMouseMove(args.X, args.Y);
        });

        m_mouseHook->OnMouseUp([this](const MouseHookEventArgs& args) {
            ProcessMouseUp(args.X, args.Y);
        });

        m_mouseHook->Start();
        Logger::Instance().Info(L"SnappingManager", L"SnappingManager initialized");
    }

    void SnappingManager::Enable()
    {
        m_isEnabled = true;
        Logger::Instance().Info(L"SnappingManager", L"Snapping enabled");
    }

    void SnappingManager::Disable()
    {
        m_isEnabled = false;
        Logger::Instance().Info(L"SnappingManager", L"Snapping disabled");
    }

    void SnappingManager::Shutdown()
    {
        m_mouseHook->Stop();
        Logger::Instance().Info(L"SnappingManager", L"SnappingManager shutdown");
    }

    void SnappingManager::SetWindowManager(void* windowManager)
    {
        m_windowManager = windowManager;
    }

    void SnappingManager::OnSnapTriggered(std::function<void(const SnapEventArgs&)> handler)
    {
        m_onSnapTriggered = handler;
    }

    void SnappingManager::OnDragStarted(std::function<void()> handler)
    {
        m_onDragStarted = handler;
    }

    void SnappingManager::OnDragEnded(std::function<void()> handler)
    {
        m_onDragEnded = handler;
    }

    void SnappingManager::StartDragDetection()
    {
        if (!m_isEnabled) return;
        m_isDragging = true;
    }

    void SnappingManager::StopDragDetection()
    {
        m_isDragging = false;
        m_hasMoved = false;
    }

    void SnappingManager::ProcessMouseMove(int32_t x, int32_t y)
    {
        if (!m_isDragging || !m_isEnabled) return;

        int32_t dx = x - m_dragStartX;
        int32_t dy = y - m_dragStartY;
        int32_t distance = static_cast<int32_t>(std::sqrt(static_cast<double>(dx * dx + dy * dy)));

        if (distance > 5 && !m_hasMoved)
        {
            m_hasMoved = true;
            if (m_onDragStarted)
            {
                m_onDragStarted();
            }
            Logger::Instance().Info(L"SnappingManager", L"Drag started");
        }

        m_lastX = x;
        m_lastY = y;
    }

    void SnappingManager::ProcessMouseUp(int32_t x, int32_t y)
    {
        if (!m_isDragging) return;

        if (m_hasMoved && m_onDragEnded)
        {
            m_onDragEnded();
            Logger::Instance().Info(L"SnappingManager", L"Drag ended");
        }

        StopDragDetection();
    }
}