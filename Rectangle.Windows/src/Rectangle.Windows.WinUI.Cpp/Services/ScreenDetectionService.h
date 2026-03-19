#pragma once
#include "pch.h"

namespace winrt::Rectangle::Services
{
    class ScreenDetectionService
    {
    public:
        ScreenDetectionService();
        ~ScreenDetectionService();

        std::vector<Core::DisplayInfo> GetAllDisplays() const;
        Core::DisplayInfo GetPrimaryDisplay() const;
        Core::DisplayInfo GetDisplayFromPoint(int32_t x, int32_t y) const;
        Core::DisplayInfo GetDisplayFromWindow(int64_t hwnd) const;

    private:
        void UpdateDisplays();
        mutable std::vector<Core::DisplayInfo> m_displays;
        mutable bool m_needsUpdate{ true };
    };
}
