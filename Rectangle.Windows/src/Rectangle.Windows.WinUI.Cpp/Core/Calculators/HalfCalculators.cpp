#include "pch.h"
#include "HalfCalculators.h"

namespace winrt::Rectangle::Core
{
    WindowRect LeftHalfCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t width = (workArea.Width() - gap) / 2;
        return WindowRect(
            workArea.Left,
            workArea.Top + gap / 2,
            width,
            workArea.Height() - gap
        );
    }

    WindowRect RightHalfCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t width = (workArea.Width() - gap) / 2;
        return WindowRect(
            workArea.Left + workArea.Width() / 2 + (gap % 2),
            workArea.Top + gap / 2,
            width,
            workArea.Height() - gap
        );
    }

    WindowRect CenterHalfCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t width = (workArea.Width() - gap) / 2;
        int32_t left = workArea.Left + (workArea.Width() - width) / 2;
        return WindowRect(
            left,
            workArea.Top + gap / 2,
            width,
            workArea.Height() - gap
        );
    }

    WindowRect TopHalfCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t height = (workArea.Height() - gap) / 2;
        return WindowRect(
            workArea.Left + gap / 2,
            workArea.Top,
            workArea.Width() - gap,
            height
        );
    }

    WindowRect BottomHalfCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t height = (workArea.Height() - gap) / 2;
        return WindowRect(
            workArea.Left + gap / 2,
            workArea.Top + workArea.Height() / 2 + (gap % 2),
            workArea.Width() - gap,
            height
        );
    }
}
