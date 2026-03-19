#include "pch.h"
#include "Services/ScreenDetectionService.h"
#include <Windows.h>
#include <vector>

#pragma comment(lib, "user32.lib")

namespace winrt::Rectangle::Services
{
    ScreenDetectionService::ScreenDetectionService() = default;

    ScreenDetectionService::~ScreenDetectionService() = default;

    std::vector<Core::DisplayInfo> ScreenDetectionService::GetAllDisplays() const
    {
        std::vector<Core::DisplayInfo> displays;

        auto enumCallback = [](HMONITOR hMonitor, HDC hdcMonitor, LPRECT lprcMonitor, LPARAM dwData) -> BOOL
            {
                auto* displays = reinterpret_cast<std::vector<Core::DisplayInfo>*>(dwData);

                MONITORINFO mi = {};
                mi.cbSize = sizeof(MONITORINFO);
                if (GetMonitorInfo(hMonitor, &mi))
                {
                    Core::DisplayInfo info;
                    info.Index = static_cast<int32_t>(displays->size());
                    info.IsPrimary = (mi.dwFlags & MONITORINFOF_PRIMARY) != 0;
                    info.Left = mi.rcMonitor.left;
                    info.Top = mi.rcMonitor.top;
                    info.Right = mi.rcMonitor.right;
                    info.Bottom = mi.rcMonitor.bottom;
                    info.Width = mi.rcMonitor.right - mi.rcMonitor.left;
                    info.Height = mi.rcMonitor.bottom - mi.rcMonitor.top;

                    DISPLAY_DEVICE dd = {};
                    dd.cb = sizeof(DISPLAY_DEVICE);
                    if (EnumDisplayDevices(nullptr, info.Index, &dd, 0))
                    {
                        info.Name = dd.DeviceName;
                    }

                    displays->push_back(info);
                }

                return TRUE;
            };

        EnumDisplayMonitors(nullptr, nullptr, enumCallback, reinterpret_cast<LPARAM>(&displays));

        return displays;
    }

    Core::DisplayInfo ScreenDetectionService::GetPrimaryDisplay() const
    {
        auto displays = GetAllDisplays();
        for (const auto& display : displays)
        {
            if (display.IsPrimary)
            {
                return display;
            }
        }
        return Core::DisplayInfo();
    }

    Core::DisplayInfo ScreenDetectionService::GetDisplayFromPoint(int32_t x, int32_t y) const
    {
        POINT pt = { x, y };
        HMONITOR hMonitor = MonitorFromPoint(pt, MONITOR_DEFAULTTONEAREST);
        if (!hMonitor) return Core::DisplayInfo();

        MONITORINFO mi = {};
        mi.cbSize = sizeof(MONITORINFO);
        if (!GetMonitorInfo(hMonitor, &mi)) return Core::DisplayInfo();

        auto displays = GetAllDisplays();
        for (const auto& display : displays)
        {
            if (display.Left == mi.rcMonitor.left && display.Top == mi.rcMonitor.top)
            {
                return display;
            }
        }

        return Core::DisplayInfo();
    }

    Core::DisplayInfo ScreenDetectionService::GetDisplayFromWindow(int64_t hwnd) const
    {
        HMONITOR hMonitor = MonitorFromWindow(reinterpret_cast<HWND>(hwnd), MONITOR_DEFAULTTONEAREST);
        if (!hMonitor) return Core::DisplayInfo();

        MONITORINFO mi = {};
        mi.cbSize = sizeof(MONITORINFO);
        if (!GetMonitorInfo(hMonitor, &mi)) return Core::DisplayInfo();

        auto displays = GetAllDisplays();
        for (const auto& display : displays)
        {
            if (display.Left == mi.rcMonitor.left && display.Top == mi.rcMonitor.top)
            {
                return display;
            }
        }

        return Core::DisplayInfo();
    }
}
