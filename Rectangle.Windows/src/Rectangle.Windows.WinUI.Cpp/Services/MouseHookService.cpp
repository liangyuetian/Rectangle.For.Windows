#include "pch.h"
#include "Services/MouseHookService.h"
#include "Services/Logger.h"
#include <iostream>

namespace winrt::Rectangle::Services
{
    MouseHookService* MouseHookService::s_instance = nullptr;

    MouseHookService::MouseHookService()
    {
        s_instance = this;
        Logger::Instance().Info(L"MouseHookService", L"MouseHookService constructed");
    }

    MouseHookService::~MouseHookService()
    {
        Stop();
        s_instance = nullptr;
    }

    void MouseHookService::Start()
    {
        if (m_isRunning) return;

        m_mouseHook = SetWindowsHookEx(WH_MOUSE_LL, MouseHookCallback, NULL, 0);
        if (m_mouseHook == nullptr)
        {
            Logger::Instance().Error(L"MouseHookService", L"Failed to install mouse hook");
        }
        else
        {
            m_isRunning = true;
            Logger::Instance().Info(L"MouseHookService", L"Mouse hook installed successfully");
        }
    }

    void MouseHookService::Stop()
    {
        if (!m_isRunning) return;

        if (m_mouseHook != nullptr)
        {
            UnhookWindowsHookEx(m_mouseHook);
            m_mouseHook = nullptr;
        }
        m_isRunning = false;
        Logger::Instance().Info(L"MouseHookService", L"Mouse hook uninstalled");
    }

    void MouseHookService::OnMouseDown(MouseEventHandler handler)
    {
        m_onMouseDown = handler;
    }

    void MouseHookService::OnMouseUp(MouseEventHandler handler)
    {
        m_onMouseUp = handler;
    }

    void MouseHookService::OnMouseMove(MouseEventHandler handler)
    {
        m_onMouseMove = handler;
    }

    LRESULT CALLBACK MouseHookService::MouseHookCallback(int nCode, WPARAM wParam, LPARAM lParam)
    {
        if (s_instance)
        {
            s_instance->ProcessMouseEvent(nCode, wParam, lParam);
        }
        return CallNextHookEx(nullptr, nCode, wParam, lParam);
    }

    void MouseHookService::ProcessMouseEvent(int nCode, WPARAM wParam, LPARAM lParam)
    {
        if (nCode != HC_ACTION) return;

        MSLLHOOKSTRUCT* pMouseStruct = reinterpret_cast<MSLLHOOKSTRUCT*>(lParam);
        if (pMouseStruct == nullptr) return;

        MouseHookEventArgs args;
        args.X = pMouseStruct->pt.x;
        args.Y = pMouseStruct->pt.y;
        args.MouseData = pMouseStruct->mouseData;
        args.Flags = pMouseStruct->flags;
        args.Time = pMouseStruct->time;

        switch (wParam)
        {
        case WM_LBUTTONDOWN:
            if (m_onMouseDown) m_onMouseDown(args);
            break;
        case WM_RBUTTONDOWN:
            if (m_onMouseDown) m_onMouseDown(args);
            break;
        case WM_LBUTTONUP:
            if (m_onMouseUp) m_onMouseUp(args);
            break;
        case WM_RBUTTONUP:
            if (m_onMouseUp) m_onMouseUp(args);
            break;
        case WM_MOUSEMOVE:
            if (m_onMouseMove) m_onMouseMove(args);
            break;
        }
    }
}