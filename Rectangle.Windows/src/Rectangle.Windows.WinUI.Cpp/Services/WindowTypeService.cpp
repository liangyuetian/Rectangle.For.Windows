#include "pch.h"
#include "Services/WindowTypeService.h"
#include <Windows.h>

namespace winrt::Rectangle::Services
{
    WindowTypeService::WindowTypeService(void* win32Service)
        : m_win32Service(win32Service)
    {
    }

    bool WindowTypeService::IsMaximized(int64_t hwnd) const
    {
        HWND hwndVal = reinterpret_cast<HWND>(hwnd);
        WINDOWPLACEMENT wp = {};
        wp.length = sizeof(WINDOWPLACEMENT);
        if (GetWindowPlacement(hwndVal, &wp))
        {
            return wp.showCmd == SW_SHOWMAXIMIZED;
        }
        return false;
    }

    bool WindowTypeService::IsMinimized(int64_t hwnd) const
    {
        return IsIconic(reinterpret_cast<HWND>(hwnd)) != FALSE;
    }

    bool WindowTypeService::IsResizable(int64_t hwnd) const
    {
        HWND hwndVal = reinterpret_cast<HWND>(hwnd);
        LONG style = GetWindowLong(hwndVal, GWL_STYLE);
        LONG exStyle = GetWindowLong(hwndVal, GWL_EXSTYLE);

        if (!(style & WS_SIZEBOX) || (exStyle & WS_EX_TOOLWINDOW))
        {
            return false;
        }

        return true;
    }

    bool WindowTypeService::IsModalDialog(int64_t hwnd) const
    {
        HWND hwndVal = reinterpret_cast<HWND>(hwnd);

        wchar_t className[256];
        GetClassName(hwndVal, className, 256);

        if (wcsstr(className, L"#32770") != nullptr)
        {
            HWND owner = GetWindow(hwndVal, GW_OWNER);
            if (owner != nullptr)
            {
                return true;
            }
        }

        return false;
    }

    bool WindowTypeService::IsValidWindow(int64_t hwnd) const
    {
        return IsWindow(reinterpret_cast<HWND>(hwnd)) != FALSE;
    }

    bool WindowTypeService::IsVisible(int64_t hwnd) const
    {
        return IsWindowVisible(reinterpret_cast<HWND>(hwnd)) != FALSE;
    }
}
