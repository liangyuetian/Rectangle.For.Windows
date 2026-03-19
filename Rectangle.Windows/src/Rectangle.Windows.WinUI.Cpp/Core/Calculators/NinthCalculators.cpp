#include "pch.h"
#include "NinthCalculators.h"

namespace winrt::Rectangle::Core
{
    WindowRect TopLeftNinthCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t width = (workArea.Width() - 2 * gap) / 3;
        int32_t height = (workArea.Height() - 2 * gap) / 3;
        return WindowRect(workArea.Left + gap, workArea.Top + gap, width, height);
    }

    WindowRect TopCenterNinthCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t width = (workArea.Width() - 2 * gap) / 3;
        int32_t height = (workArea.Height() - 2 * gap) / 3;
        int32_t x = workArea.Left + width + gap;
        return WindowRect(x, workArea.Top + gap, width, height);
    }

    WindowRect TopRightNinthCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t width = (workArea.Width() - 2 * gap) / 3;
        int32_t height = (workArea.Height() - 2 * gap) / 3;
        int32_t x = workArea.Left + width * 2 + gap;
        return WindowRect(x, workArea.Top + gap, width, height);
    }

    WindowRect MiddleLeftNinthCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t width = (workArea.Width() - 2 * gap) / 3;
        int32_t height = (workArea.Height() - 2 * gap) / 3;
        int32_t y = workArea.Top + height + gap;
        return WindowRect(workArea.Left + gap, y, width, height);
    }

    WindowRect MiddleCenterNinthCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t width = (workArea.Width() - 2 * gap) / 3;
        int32_t height = (workArea.Height() - 2 * gap) / 3;
        int32_t x = workArea.Left + width + gap;
        int32_t y = workArea.Top + height + gap;
        return WindowRect(x, y, width, height);
    }

    WindowRect MiddleRightNinthCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t width = (workArea.Width() - 2 * gap) / 3;
        int32_t height = (workArea.Height() - 2 * gap) / 3;
        int32_t x = workArea.Left + width * 2 + gap;
        int32_t y = workArea.Top + height + gap;
        return WindowRect(x, y, width, height);
    }

    WindowRect BottomLeftNinthCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t width = (workArea.Width() - 2 * gap) / 3;
        int32_t height = (workArea.Height() - 2 * gap) / 3;
        int32_t y = workArea.Top + height * 2 + gap;
        return WindowRect(workArea.Left + gap, y, width, height);
    }

    WindowRect BottomCenterNinthCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t width = (workArea.Width() - 2 * gap) / 3;
        int32_t height = (workArea.Height() - 2 * gap) / 3;
        int32_t x = workArea.Left + width + gap;
        int32_t y = workArea.Top + height * 2 + gap;
        return WindowRect(x, y, width, height);
    }

    WindowRect BottomRightNinthCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t width = (workArea.Width() - 2 * gap) / 3;
        int32_t height = (workArea.Height() - 2 * gap) / 3;
        int32_t x = workArea.Left + width * 2 + gap;
        int32_t y = workArea.Top + height * 2 + gap;
        return WindowRect(x, y, width, height);
    }
}