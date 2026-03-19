#include "pch.h"
#include "MiscCalculators.h"
#include <algorithm>

namespace winrt::Rectangle::Core
{
    WindowRect MaximizeCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        return WindowRect(workArea.Left, workArea.Top, workArea.Width(), workArea.Height());
    }

    WindowRect AlmostMaximizeCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        float ratio = 0.9f;
        int32_t width = static_cast<int32_t>(workArea.Width() * ratio);
        int32_t height = static_cast<int32_t>(workArea.Height() * ratio);
        int32_t left = workArea.Left + (workArea.Width() - width) / 2;
        int32_t top = workArea.Top + (workArea.Height() - height) / 2;
        return WindowRect(left, top, width, height);
    }

    WindowRect MaximizeHeightCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        return WindowRect(currentWindow.X(), workArea.Top, currentWindow.Width(), workArea.Height());
    }

    WindowRect LargerCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t sizeIncrease = 30;
        int32_t newWidth = std::min(workArea.Width(), currentWindow.Width() + sizeIncrease);
        int32_t newHeight = std::min(workArea.Height(), currentWindow.Height() + sizeIncrease);
        int32_t newX = std::max(workArea.Left, currentWindow.X() - (newWidth - currentWindow.Width()) / 2);
        int32_t newY = std::max(workArea.Top, currentWindow.Y() - (newHeight - currentWindow.Height()) / 2);

        newX = std::min(newX, workArea.Right() - newWidth);
        newY = std::min(newY, workArea.Bottom() - newHeight);

        return WindowRect(newX, newY, newWidth, newHeight);
    }

    WindowRect SmallerCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t sizeDecrease = 30;
        int32_t newWidth = std::max(100, currentWindow.Width() - sizeDecrease);
        int32_t newHeight = std::max(100, currentWindow.Height() - sizeDecrease);
        int32_t newX = currentWindow.X() + (currentWindow.Width() - newWidth) / 2;
        int32_t newY = currentWindow.Y() + (currentWindow.Height() - newHeight) / 2;

        return WindowRect(newX, newY, newWidth, newHeight);
    }

    WindowRect CenterCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t newX = workArea.Left + (workArea.Width() - currentWindow.Width()) / 2;
        int32_t newY = workArea.Top + (workArea.Height() - currentWindow.Height()) / 2;
        return WindowRect(newX, newY, currentWindow.Width(), currentWindow.Height());
    }

    WindowRect RestoreCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        return currentWindow;
    }

    WindowRect LargerWidthCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t sizeIncrease = 30;
        int32_t newWidth = std::min(workArea.Width(), currentWindow.Width() + sizeIncrease);
        int32_t newX = currentWindow.X() - (newWidth - currentWindow.Width()) / 2;
        newX = std::max(workArea.Left, std::min(newX, workArea.Right() - newWidth));
        return WindowRect(newX, currentWindow.Y(), newWidth, currentWindow.Height());
    }

    WindowRect SmallerWidthCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t sizeDecrease = 30;
        int32_t newWidth = std::max(100, currentWindow.Width() - sizeDecrease);
        int32_t newX = currentWindow.X() + (currentWindow.Width() - newWidth) / 2;
        return WindowRect(newX, currentWindow.Y(), newWidth, currentWindow.Height());
    }

    WindowRect LargerHeightCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t sizeIncrease = 30;
        int32_t newHeight = std::min(workArea.Height(), currentWindow.Height() + sizeIncrease);
        int32_t newY = currentWindow.Y() - (newHeight - currentWindow.Height()) / 2;
        newY = std::max(workArea.Top, std::min(newY, workArea.Bottom() - newHeight));
        return WindowRect(currentWindow.X(), newY, currentWindow.Width(), newHeight);
    }

    WindowRect SmallerHeightCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t sizeDecrease = 30;
        int32_t newHeight = std::max(100, currentWindow.Height() - sizeDecrease);
        int32_t newY = currentWindow.Y() + (currentWindow.Height() - newHeight) / 2;
        return WindowRect(currentWindow.X(), newY, currentWindow.Width(), newHeight);
    }
}
