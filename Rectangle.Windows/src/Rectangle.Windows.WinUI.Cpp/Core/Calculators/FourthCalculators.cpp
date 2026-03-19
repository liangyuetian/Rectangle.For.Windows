#include "pch.h"
#include "FourthCalculators.h"

namespace winrt::Rectangle::Core
{
    WindowRect FirstFourthCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t width = (workArea.Width() - gap) / 4;
        return WindowRect(workArea.Left, workArea.Top + gap / 2, width, workArea.Height() - gap);
    }

    WindowRect SecondFourthCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t width = (workArea.Width() - gap) / 4;
        return WindowRect(workArea.Left + width + (gap % 4) / 3, workArea.Top + gap / 2, width, workArea.Height() - gap);
    }

    WindowRect ThirdFourthCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t width = (workArea.Width() - gap) / 4;
        return WindowRect(workArea.Left + 2 * width + (gap % 4) / 2, workArea.Top + gap / 2, width, workArea.Height() - gap);
    }

    WindowRect LastFourthCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t width = (workArea.Width() - gap) / 4;
        return WindowRect(workArea.Right() - width, workArea.Top + gap / 2, width, workArea.Height() - gap);
    }

    WindowRect FirstThreeFourthsCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t width = 3 * (workArea.Width() - gap) / 4;
        return WindowRect(workArea.Left, workArea.Top + gap / 2, width, workArea.Height() - gap);
    }

    WindowRect CenterThreeFourthsCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t width = 3 * (workArea.Width() - gap) / 4;
        return WindowRect(workArea.Left + (workArea.Width() - width) / 2, workArea.Top + gap / 2, width, workArea.Height() - gap);
    }

    WindowRect LastThreeFourthsCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t width = 3 * (workArea.Width() - gap) / 4;
        return WindowRect(workArea.Right() - width, workArea.Top + gap / 2, width, workArea.Height() - gap);
    }
}
