#include "pch.h"
#include "EighthCalculators.h"

namespace winrt::Rectangle::Core
{
    WindowRect TopLeftEighthCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t width = (workArea.Width() - 3 * gap) / 4;
        int32_t height = (workArea.Height() - gap) / 2;
        return WindowRect(workArea.Left, workArea.Top, width, height);
    }

    WindowRect TopCenterLeftEighthCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t width = (workArea.Width() - 3 * gap) / 4;
        int32_t height = (workArea.Height() - gap) / 2;
        return WindowRect(workArea.Left + width + gap, workArea.Top, width, height);
    }

    WindowRect TopCenterRightEighthCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t width = (workArea.Width() - 3 * gap) / 4;
        int32_t height = (workArea.Height() - gap) / 2;
        return WindowRect(workArea.Left + 2 * (width + gap), workArea.Top, width, height);
    }

    WindowRect TopRightEighthCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t width = (workArea.Width() - 3 * gap) / 4;
        int32_t height = (workArea.Height() - gap) / 2;
        return WindowRect(workArea.Right() - width, workArea.Top, width, height);
    }

    WindowRect BottomLeftEighthCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t width = (workArea.Width() - 3 * gap) / 4;
        int32_t height = (workArea.Height() - gap) / 2;
        return WindowRect(workArea.Left, workArea.Bottom() - height, width, height);
    }

    WindowRect BottomCenterLeftEighthCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t width = (workArea.Width() - 3 * gap) / 4;
        int32_t height = (workArea.Height() - gap) / 2;
        return WindowRect(workArea.Left + width + gap, workArea.Bottom() - height, width, height);
    }

    WindowRect BottomCenterRightEighthCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t width = (workArea.Width() - 3 * gap) / 4;
        int32_t height = (workArea.Height() - gap) / 2;
        return WindowRect(workArea.Left + 2 * (width + gap), workArea.Bottom() - height, width, height);
    }

    WindowRect BottomRightEighthCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t width = (workArea.Width() - 3 * gap) / 4;
        int32_t height = (workArea.Height() - gap) / 2;
        return WindowRect(workArea.Right() - width, workArea.Bottom() - height, width, height);
    }
}
