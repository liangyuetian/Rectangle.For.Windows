#include "pch.h"
#include "Services/Win32WindowService.h"
#include "Services/Logger.h"
#include <Windowsx.h>
#include <psapi.h>

#pragma comment(lib, "user32.lib")
#pragma comment(lib, "kernel32.lib")
#pragma comment(lib, "psapi.lib")

namespace winrt::Rectangle::Services
{
    Win32WindowService::Win32WindowService() = default;

    Win32WindowService::~Win32WindowService() = default;

    bool Win32WindowService::GetWindowRect(int64_t hwnd, int32_t& x, int32_t& y, int32_t& width, int32_t& height) const
    {
        RECT rect;
        if (::GetWindowRect(reinterpret_cast<HWND>(hwnd), &rect))
        {
            x = rect.left;
            y = rect.top;
            width = rect.right - rect.left;
            height = rect.bottom - rect.top;
            return true;
        }
        return false;
    }

    bool Win32WindowService::SetWindowRect(int64_t hwnd, int32_t x, int32_t y, int32_t width, int32_t height)
    {
        HWND hwndVal = reinterpret_cast<HWND>(hwnd);

        if (IsMaximized(hwnd))
        {
            return SetWindowRectFromMaximized(hwnd, x, y, width, height);
        }

        BOOL result = ::SetWindowPos(
            hwndVal,
            nullptr,
            x, y,
            width, height,
            SWP_NOZORDER | SWP_NOACTIVATE | SWP_ASYNCWINDOWPOS
        );

        if (!result)
        {
            Logger::Instance().Warning(L"Win32WindowService",
                L"SetWindowRect failed: " + std::to_wstring(GetLastError()));
        }

        return result != FALSE;
    }

    bool Win32WindowService::SetWindowRectFromMaximized(int64_t hwnd, int32_t x, int32_t y, int32_t width, int32_t height)
    {
        HWND hwndVal = reinterpret_cast<HWND>(hwnd);

        WINDOWPLACEMENTEx wp = {};
        wp.Length = sizeof(WINDOWPLACEMENTEx);
        if (!GetWindowPlacement(hwnd, &wp))
        {
            return false;
        }

        wp.flags = 0;
        wp.ShowCmd = SW_SHOWNORMAL;
        wp.NormalPosition.left = x;
        wp.NormalPosition.top = y;
        wp.NormalPosition.right = x + width;
        wp.NormalPosition.bottom = y + height;

        bool result = SetWindowPlacement(hwnd, &wp);
        if (!result)
        {
            Logger::Instance().Warning(L"Win32WindowService",
                L"SetWindowRectFromMaximized failed: " + std::to_wstring(GetLastError()));
        }

        return result;
    }

    bool Win32WindowService::GetWindowPlacement(int64_t hwnd, WINDOWPLACEMENTEx* placement) const
    {
        placement->Length = sizeof(WINDOWPLACEMENTEx);
        return ::GetWindowPlacement(reinterpret_cast<HWND>(hwnd), reinterpret_cast<WINDOWPLACEMENT*>(placement)) != FALSE;
    }

    bool Win32WindowService::SetWindowPlacement(int64_t hwnd, const WINDOWPLACEMENTEx* placement)
    {
        return ::SetWindowPlacement(reinterpret_cast<HWND>(hwnd), reinterpret_cast<const WINDOWPLACEMENT*>(placement)) != FALSE;
    }

    std::wstring Win32WindowService::GetProcessNameFromWindow(int64_t hwnd) const
    {
        DWORD processId;
        if (!GetWindowThreadProcessId(reinterpret_cast<HWND>(hwnd), &processId))
        {
            return L"";
        }

        HANDLE hProcess = OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION, FALSE, processId);
        if (!hProcess)
        {
            return L"";
        }

        wchar_t buffer[MAX_PATH];
        DWORD size = MAX_PATH;
        std::wstring result;

        if (QueryFullProcessImageName(hProcess, 0, buffer, &size))
        {
            result = buffer;
            size_t pos = result.find_last_of(L"\\/");
            if (pos != std::wstring::npos)
            {
                result = result.substr(pos + 1);
            }
        }

        CloseHandle(hProcess);
        return result;
    }

    int64_t Win32WindowService::GetForegroundWindow() const
    {
        return reinterpret_cast<int64_t>(::GetForegroundWindow());
    }

    bool Win32WindowService::SetForegroundWindow(int64_t hwnd)
    {
        HWND hwndVal = reinterpret_cast<HWND>(hwnd);

        if (IsIconic(hwndVal))
        {
            ShowWindow(hwndVal, SW_RESTORE);
        }

        return ::SetForegroundWindow(hwndVal) != FALSE;
    }

    bool Win32WindowService::ShowWindow(int64_t hwnd, int32_t cmdShow)
    {
        return ::ShowWindow(reinterpret_cast<HWND>(hwnd), cmdShow) != FALSE;
    }

    bool Win32WindowService::IsWindowVisible(int64_t hwnd) const
    {
        return ::IsWindowVisible(reinterpret_cast<HWND>(hwnd)) != FALSE;
    }

    bool Win32WindowService::IsMaximized(int64_t hwnd) const
    {
        WINDOWPLACEMENT wp = {};
        wp.length = sizeof(WINDOWPLACEMENT);
        if (::GetWindowPlacement(reinterpret_cast<HWND>(hwnd), &wp))
        {
            return wp.showCmd == SW_SHOWMAXIMIZED;
        }
        return false;
    }

    bool Win32WindowService::IsMinimized(int64_t hwnd) const
    {
        return ::IsIconic(reinterpret_cast<HWND>(hwnd)) != FALSE;
    }

    bool Win32WindowService::IsWindow(int64_t hwnd) const
    {
        return ::IsWindow(reinterpret_cast<HWND>(hwnd)) != FALSE;
    }

    bool Win32WindowService::IsResizable(int64_t hwnd) const
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

    bool Win32WindowService::IsModalDialog(int64_t hwnd) const
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

    std::vector<Core::WorkArea> Win32WindowService::GetMonitorWorkAreas() const
    {
        std::vector<Core::WorkArea> result;

        auto enumCallback = [](HMONITOR hMonitor, HDC hdcMonitor, LPRECT lprcMonitor, LPARAM dwData) -> BOOL
            {
                auto* result = reinterpret_cast<std::vector<Core::WorkArea>*>(dwData);

                MONITORINFO mi = {};
                mi.cbSize = sizeof(MONITORINFO);
                if (GetMonitorInfo(hMonitor, &mi))
                {
                    result->push_back(Core::WorkArea(
                        mi.rcWork.left,
                        mi.rcWork.top,
                        mi.rcWork.right,
                        mi.rcWork.bottom
                    ));
                }

                return TRUE;
            };

        EnumDisplayMonitors(nullptr, nullptr, enumCallback, reinterpret_cast<LPARAM>(&result));

        return result;
    }

    Core::WorkArea Win32WindowService::GetMonitorWorkAreaFromWindow(int64_t hwnd) const
    {
        HWND hwndVal = reinterpret_cast<HWND>(hwnd);
        HMONITOR hMonitor = MonitorFromWindow(hwndVal, MONITOR_DEFAULTTONEAREST);
        if (!hMonitor)
        {
            return Core::WorkArea(0, 0, 1920, 1080);
        }

        MONITORINFO mi = {};
        mi.cbSize = sizeof(MONITORINFO);
        if (GetMonitorInfo(hMonitor, &mi))
        {
            return Core::WorkArea(
                mi.rcWork.left,
                mi.rcWork.top,
                mi.rcWork.right,
                mi.rcWork.bottom
            );
        }

        return Core::WorkArea(0, 0, 1920, 1080);
    }

    int32_t Win32WindowService::GetWindowDpi(int64_t hwnd) const
    {
        return GetDpiForWindow(reinterpret_cast<HWND>(hwnd));
    }

    int64_t Win32WindowService::GetAncestorWindow(int64_t hwnd) const
    {
        return reinterpret_cast<int64_t>(GetAncestor(reinterpret_cast<HWND>(hwnd), GA_ROOT));
    }
}
