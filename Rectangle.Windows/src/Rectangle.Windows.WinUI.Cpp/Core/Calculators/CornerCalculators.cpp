#include "pch.h"
#include "CornerCalculators.h"

namespace winrt::Rectangle::Core
{
    WindowRect TopLeftCornerCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t width = (workArea.Width() - gap) / 2;
        int32_t height = (workArea.Height() - gap) / 2;
        return WindowRect(
            workArea.Left,
            workArea.Top,
            width,
            height
        );
    }

    WindowRect TopRightCornerCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t width = (workArea.Width() - gap) / 2;
        int32_t height = (workArea.Height() - gap) / 2;
        return WindowRect(
            workArea.Left + workArea.Width() / 2 + (gap % 2),
            workArea.Top,
            width,
            height
        );
    }

    WindowRect BottomLeftCornerCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t width = (workArea.Width() - gap) / 2;
        int32_t height = (workArea.Height() - gap) / 2;
        return WindowRect(
            workArea.Left,
            workArea.Top + workArea.Height() / 2 + (gap % 2),
            width,
            height
        );
    }

    WindowRect BottomRightCornerCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t width = (workArea.Width() - gap) / 2;
        int32_t height = (workArea.Height() - gap) / 2;
        return WindowRect(
            workArea.Left + workArea.Width() / 2 + (gap % 2),
            workArea.Top + workArea.Height() / 2 + (gap % 2),
            width,
            height
        );
    }
}
