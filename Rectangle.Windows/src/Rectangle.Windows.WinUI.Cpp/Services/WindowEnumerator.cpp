#include "pch.h"
#include "Services/WindowEnumerator.h"
#include <Psapi.h>
#include <algorithm>

#pragma comment(lib, "user32.lib")
#pragma comment(lib, "psapi.lib")

namespace winrt::Rectangle::Services
{
    std::vector<int64_t> WindowEnumerator::GetAllWindows()
    {
        std::vector<int64_t> result;

        auto enumProc = [](HWND hwnd, LPARAM lParam) -> BOOL
            {
                if (!IsWindow(hwnd) || !IsWindowVisible(hwnd))
                {
                    return TRUE;
                }

                if (!IsAltTabWindow(reinterpret_cast<int64_t>(hwnd)))
                {
                    return TRUE;
                }

                auto* windows = reinterpret_cast<std::vector<int64_t>*>(lParam);
                windows->push_back(reinterpret_cast<int64_t>(hwnd));
                return TRUE;
            };

        EnumWindows(enumProc, reinterpret_cast<LPARAM>(&result));

        return result;
    }

    bool WindowEnumerator::IsAltTabWindow(int64_t hwnd)
    {
        HWND hwndVal = reinterpret_cast<HWND>(hwnd);

        if (!IsWindowVisible(hwndVal))
        {
            return false;
        }

        if (GetWindow(hwndVal, GW_OWNER) != nullptr)
        {
            return false;
        }

        wchar_t className[256];
        GetClassName(hwndVal, className, 256);

        if (wcsstr(className, L"Shell_TrayWnd") != nullptr ||
            wcsstr(className, L"Shell_SecondaryTrayWnd") != nullptr ||
            wcsstr(className, L"Progman") != nullptr ||
            wcsstr(className, L"WorkerW") != nullptr ||
            wcsstr(className, L"Windows.UI.Core") != nullptr)
        {
            return false;
        }

        LONG exStyle = GetWindowLong(hwndVal, GWL_EXSTYLE);
        if (exStyle & WS_EX_TOOLWINDOW)
        {
            return false;
        }

        return true;
    }

    std::wstring WindowEnumerator::GetProcessNameFromWindow(int64_t hwnd)
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
}
