#include "pch.h"
#include "SixthCalculators.h"

namespace winrt::Rectangle::Core
{
    WindowRect TopLeftSixthCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t width = (workArea.Width() - 2 * gap) / 3;
        int32_t height = (workArea.Height() - 2 * gap) / 2;
        return WindowRect(workArea.Left, workArea.Top, width, height);
    }

    WindowRect TopCenterSixthCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t width = (workArea.Width() - 2 * gap) / 3;
        int32_t height = (workArea.Height() - 2 * gap) / 2;
        return WindowRect(workArea.Left + width + gap, workArea.Top, width, height);
    }

    WindowRect TopRightSixthCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t width = (workArea.Width() - 2 * gap) / 3;
        int32_t height = (workArea.Height() - 2 * gap) / 2;
        return WindowRect(workArea.Right() - width, workArea.Top, width, height);
    }

    WindowRect BottomLeftSixthCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t width = (workArea.Width() - 2 * gap) / 3;
        int32_t height = (workArea.Height() - 2 * gap) / 2;
        return WindowRect(workArea.Left, workArea.Bottom() - height, width, height);
    }

    WindowRect BottomCenterSixthCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t width = (workArea.Width() - 2 * gap) / 3;
        int32_t height = (workArea.Height() - 2 * gap) / 2;
        return WindowRect(workArea.Left + width + gap, workArea.Bottom() - height, width, height);
    }

    WindowRect BottomRightSixthCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t width = (workArea.Width() - 2 * gap) / 3;
        int32_t height = (workArea.Height() - 2 * gap) / 2;
        return WindowRect(workArea.Right() - width, workArea.Bottom() - height, width, height);
    }
}
