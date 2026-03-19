#pragma once
#include "pch.h"
#include <Windows.h>
#include <vector>
#include <string>

namespace winrt::Rectangle::Services
{
    class WindowEnumerator
    {
    public:
        static std::vector<int64_t> GetAllWindows();
        static bool IsAltTabWindow(int64_t hwnd);
        static std::wstring GetProcessNameFromWindow(int64_t hwnd);
    };
}
