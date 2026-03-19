#pragma once
#include "pch.h"
#include <functional>
#include <memory>

namespace winrt::Rectangle::Services
{
    struct SnapEventArgs
    {
        int32_t X{ 0 };
        int32_t Y{ 0 };
        int32_t Width{ 0 };
        int32_t Height{ 0 };
        std::wstring ActionName;
    };

    class SnappingManager
    {
    public:
        SnappingManager();
        ~SnappingManager() = default;

        void Initialize();
        void Enable();
        void Disable();
        void Shutdown();

        void SetWindowManager(void* windowManager);

        void OnSnapTriggered(std::function<void(const SnapEventArgs&)> handler);
        void OnDragStarted(std::function<void()> handler);
        void OnDragEnded(std::function<void()> handler);

    private:
        void StartDragDetection();
        void StopDragDetection();
        void ProcessMouseMove(int32_t x, int32_t y);
        void ProcessMouseUp(int32_t x, int32_t y);

        void* m_windowManager{ nullptr };
        std::unique_ptr<MouseHookService> m_mouseHook;

        bool m_isEnabled{ true };
        bool m_isDragging{ false };
        bool m_hasMoved{ false };

        int32_t m_dragStartX{ 0 };
        int32_t m_dragStartY{ 0 };
        int32_t m_lastX{ 0 };
        int32_t m_lastY{ 0 };

        int32_t m_edgeMarginTop{ 5 };
        int32_t m_edgeMarginBottom{ 5 };
        int32_t m_edgeMarginLeft{ 5 };
        int32_t m_edgeMarginRight{ 5 };
        int32_t m_cornerSize{ 20 };

        std::function<void(const SnapEventArgs&)> m_onSnapTriggered;
        std::function<void()> m_onDragStarted;
        std::function<void()> m_onDragEnded;
    };
}