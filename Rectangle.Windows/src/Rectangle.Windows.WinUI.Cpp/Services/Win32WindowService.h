#pragma once
#include "pch.h"
#include "Core/WindowRect.h"
#include "Core/Enums.h"

namespace winrt::Rectangle::Services
{
    class Win32WindowService
    {
    public:
        Win32WindowService();
        ~Win32WindowService();

        bool GetWindowRect(int64_t hwnd, int32_t& x, int32_t& y, int32_t& width, int32_t& height) const;
        bool SetWindowRect(int64_t hwnd, int32_t x, int32_t y, int32_t width, int32_t height);
        bool SetWindowRectFromMaximized(int64_t hwnd, int32_t x, int32_t y, int32_t width, int32_t height);

        std::wstring GetProcessNameFromWindow(int64_t hwnd) const;
        int64_t GetForegroundWindow() const;
        bool SetForegroundWindow(int64_t hwnd);
        bool ShowWindow(int64_t hwnd, int32_t cmdShow);
        bool IsWindowVisible(int64_t hwnd) const;
        bool IsMaximized(int64_t hwnd) const;
        bool IsMinimized(int64_t hwnd) const;
        bool IsWindow(int64_t hwnd) const;
        bool IsResizable(int64_t hwnd) const;
        bool IsModalDialog(int64_t hwnd) const;

        std::vector<Core::WorkArea> GetMonitorWorkAreas() const;
        Core::WorkArea GetMonitorWorkAreaFromWindow(int64_t hwnd) const;
        Core::WorkArea GetMonitorWorkAreaFromCursor() const;
        int32_t GetWindowDpi(int64_t hwnd) const;

        int64_t GetAncestorWindow(int64_t hwnd) const;

    private:
        struct WINDOWPLACEMENTEx
        {
            uint32_t Length;
            uint32_t Flags;
            uint32_t ShowCmd;
            POINT MinPosition;
            POINT MaxPosition;
            RECT NormalPosition;
        };

        bool GetWindowPlacement(int64_t hwnd, WINDOWPLACEMENTEx* placement) const;
        bool SetWindowPlacement(int64_t hwnd, const WINDOWPLACEMENTEx* placement);
    };
}
