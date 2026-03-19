#pragma once
#include "pch.h"

namespace winrt::Rectangle::Services
{
    class WindowTypeService
    {
    public:
        explicit WindowTypeService(void* win32Service);
        ~WindowTypeService() = default;

        bool IsMaximized(int64_t hwnd) const;
        bool IsMinimized(int64_t hwnd) const;
        bool IsResizable(int64_t hwnd) const;
        bool IsModalDialog(int64_t hwnd) const;
        bool IsValidWindow(int64_t hwnd) const;
        bool IsVisible(int64_t hwnd) const;

    private:
        void* m_win32Service;
    };
}
