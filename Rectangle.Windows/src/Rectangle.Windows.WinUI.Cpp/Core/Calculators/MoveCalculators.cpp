#include "pch.h"
#include "MoveCalculators.h"
#include <algorithm>

namespace winrt::Rectangle::Core
{
    WindowRect MoveLeftCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t offset = currentWindow.Width() / 2;
        int32_t newX = std::max(workArea.Left, currentWindow.X() - offset);
        return WindowRect(newX, currentWindow.Y(), currentWindow.Width(), currentWindow.Height());
    }

    WindowRect MoveRightCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t offset = currentWindow.Width() / 2;
        int32_t newX = std::min(workArea.Right() - currentWindow.Width(), currentWindow.X() + offset);
        return WindowRect(newX, currentWindow.Y(), currentWindow.Width(), currentWindow.Height());
    }

    WindowRect MoveUpCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t offset = currentWindow.Height() / 2;
        int32_t newY = std::max(workArea.Top, currentWindow.Y() - offset);
        return WindowRect(currentWindow.X(), newY, currentWindow.Width(), currentWindow.Height());
    }

    WindowRect MoveDownCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t offset = currentWindow.Height() / 2;
        int32_t newY = std::min(workArea.Bottom() - currentWindow.Height(), currentWindow.Y() + offset);
        return WindowRect(currentWindow.X(), newY, currentWindow.Width(), currentWindow.Height());
    }

    WindowRect DoubleHeightUpCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t newHeight = std::min(workArea.Height(), currentWindow.Height() * 2);
        int32_t newY = std::max(workArea.Top, currentWindow.Y() - currentWindow.Height());
        newY = std::min(newY, workArea.Bottom() - newHeight);
        return WindowRect(currentWindow.X(), newY, currentWindow.Width(), newHeight);
    }

    WindowRect DoubleHeightDownCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t newHeight = std::min(workArea.Height(), currentWindow.Height() * 2);
        int32_t newY = currentWindow.Y();
        int32_t bottom = newY + newHeight;
        if (bottom > workArea.Bottom())
        {
            newY = workArea.Bottom() - newHeight;
        }
        return WindowRect(currentWindow.X(), newY, currentWindow.Width(), newHeight);
    }

    WindowRect DoubleWidthLeftCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t newWidth = std::min(workArea.Width(), currentWindow.Width() * 2);
        int32_t newX = std::max(workArea.Left, currentWindow.X() - currentWindow.Width());
        newX = std::min(newX, workArea.Right() - newWidth);
        return WindowRect(newX, currentWindow.Y(), newWidth, currentWindow.Height());
    }

    WindowRect DoubleWidthRightCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t newWidth = std::min(workArea.Width(), currentWindow.Width() * 2);
        int32_t newX = currentWindow.X();
        int32_t right = newX + newWidth;
        if (right > workArea.Right())
        {
            newX = workArea.Right() - newWidth;
        }
        return WindowRect(newX, currentWindow.Y(), newWidth, currentWindow.Height());
    }

    WindowRect HalveHeightUpCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t newHeight = currentWindow.Height() / 2;
        int32_t newY = currentWindow.Y() + currentWindow.Height() / 2;
        newY = std::min(newY, workArea.Bottom() - newHeight);
        return WindowRect(currentWindow.X(), newY, currentWindow.Width(), newHeight);
    }

    WindowRect HalveHeightDownCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t newHeight = currentWindow.Height() / 2;
        int32_t newY = currentWindow.Y();
        newY = std::max(workArea.Top, newY);
        return WindowRect(currentWindow.X(), newY, currentWindow.Width(), newHeight);
    }

    WindowRect HalveWidthLeftCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t newWidth = currentWindow.Width() / 2;
        int32_t newX = currentWindow.X() + currentWindow.Width() / 2;
        newX = std::min(newX, workArea.Right() - newWidth);
        return WindowRect(newX, currentWindow.Y(), newWidth, currentWindow.Height());
    }

    WindowRect HalveWidthRightCalculator::Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap)
    {
        int32_t newWidth = currentWindow.Width() / 2;
        int32_t newX = currentWindow.X();
        newX = std::max(workArea.Left, newX);
        return WindowRect(newX, currentWindow.Y(), newWidth, currentWindow.Height());
    }
}
