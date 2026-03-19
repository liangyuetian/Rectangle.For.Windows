#pragma once
#include "pch.h"
#include <functional>
#include <Windows.h>
#include <winuser.h>

namespace winrt::Rectangle::Services
{
    struct MouseHookEventArgs
    {
        int32_t X{ 0 };
        int32_t Y{ 0 };
        int32_t MouseData{ 0 };
        uint32_t Flags{ 0 };
        uint32_t Time{ 0 };
    };

    class MouseHookService
    {
    public:
        using MouseEventHandler = std::function<void(const MouseHookEventArgs&)>;

        MouseHookService();
        ~MouseHookService();

        void Start();
        void Stop();

        void OnMouseDown(MouseEventHandler handler);
        void OnMouseUp(MouseEventHandler handler);
        void OnMouseMove(MouseEventHandler handler);

    private:
        static LRESULT CALLBACK MouseHookCallback(int nCode, WPARAM wParam, LPARAM lParam);
        void ProcessMouseEvent(int nCode, WPARAM wParam, LPARAM lParam);

        HHOOK m_mouseHook{ nullptr };
        bool m_isRunning{ false };

        MouseEventHandler m_onMouseDown;
        MouseEventHandler m_onMouseUp;
        MouseEventHandler m_onMouseMove;

        static MouseHookService* s_instance;
    };
}
