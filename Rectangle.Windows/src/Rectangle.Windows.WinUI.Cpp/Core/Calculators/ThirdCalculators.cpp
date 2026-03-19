#include "pch.h"
#include "ThirdCalculators.h"

namespace winrt::Rectangle::Core
{
    WindowRect FirstThirdCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t width = (workArea.Width() - gap) / 3;
        return WindowRect(
            workArea.Left,
            workArea.Top + gap / 2,
            width,
            workArea.Height() - gap
        );
    }

    WindowRect CenterThirdCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t width = (workArea.Width() - gap) / 3;
        return WindowRect(
            workArea.Left + workArea.Width() / 3 + (gap % 3) / 2,
            workArea.Top + gap / 2,
            width,
            workArea.Height() - gap
        );
    }

    WindowRect LastThirdCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t width = (workArea.Width() - gap) / 3;
        return WindowRect(
            workArea.Left + 2 * (workArea.Width() / 3),
            workArea.Top + gap / 2,
            width,
            workArea.Height() - gap
        );
    }

    WindowRect FirstTwoThirdsCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t width = 2 * (workArea.Width() - gap) / 3;
        return WindowRect(
            workArea.Left,
            workArea.Top + gap / 2,
            width,
            workArea.Height() - gap
        );
    }

    WindowRect CenterTwoThirdsCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t width = 2 * (workArea.Width() - gap) / 3;
        return WindowRect(
            workArea.Left + (workArea.Width() - width) / 2,
            workArea.Top + gap / 2,
            width,
            workArea.Height() - gap
        );
    }

    WindowRect LastTwoThirdsCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t width = 2 * (workArea.Width() - gap) / 3;
        return WindowRect(
            workArea.Right() - width,
            workArea.Top + gap / 2,
            width,
            workArea.Height() - gap
        );
    }

    WindowRect TopVerticalThirdCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t height = (workArea.Height() - gap) / 3;
        return WindowRect(
            workArea.Left + gap / 2,
            workArea.Top,
            workArea.Width() - gap,
            height
        );
    }

    WindowRect MiddleVerticalThirdCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t height = (workArea.Height() - gap) / 3;
        return WindowRect(
            workArea.Left + gap / 2,
            workArea.Top + workArea.Height() / 3 + (gap % 3) / 2,
            workArea.Width() - gap,
            height
        );
    }

    WindowRect BottomVerticalThirdCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t height = (workArea.Height() - gap) / 3;
        return WindowRect(
            workArea.Left + gap / 2,
            workArea.Top + 2 * (workArea.Height() / 3),
            workArea.Width() - gap,
            height
        );
    }

    WindowRect TopLeftThirdCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t width = (workArea.Width() - gap * 2) / 3;
        int32_t height = (workArea.Height() - gap * 2) / 3;
        return WindowRect(
            workArea.Left + gap,
            workArea.Top + gap,
            width,
            height
        );
    }

    WindowRect TopRightThirdCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t width = (workArea.Width() - gap * 2) / 3;
        int32_t height = (workArea.Height() - gap * 2) / 3;
        return WindowRect(
            workArea.Left + width * 2 + gap,
            workArea.Top + gap,
            width,
            height
        );
    }

    WindowRect BottomLeftThirdCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t width = (workArea.Width() - gap * 2) / 3;
        int32_t height = (workArea.Height() - gap * 2) / 3;
        return WindowRect(
            workArea.Left + gap,
            workArea.Top + height * 2 + gap,
            width,
            height
        );
    }

    WindowRect BottomRightThirdCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t width = (workArea.Width() - gap * 2) / 3;
        int32_t height = (workArea.Height() - gap * 2) / 3;
        return WindowRect(
            workArea.Left + width * 2 + gap,
            workArea.Top + height * 2 + gap,
            width,
            height
        );
    }

    WindowRect TopVerticalTwoThirdsCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t height = (workArea.Height() - gap * 2) / 3 * 2 + gap;
        return WindowRect(
            workArea.Left + gap,
            workArea.Top + gap,
            workArea.Width() - gap * 2,
            height
        );
    }

    WindowRect BottomVerticalTwoThirdsCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t height = (workArea.Height() - gap * 2) / 3 * 2 + gap;
        return WindowRect(
            workArea.Left + gap,
            workArea.Top + workArea.Height() - height - gap,
            workArea.Width() - gap * 2,
            height
        );
    }
}
