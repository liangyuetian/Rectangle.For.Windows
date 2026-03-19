#pragma once
#include "pch.h"
#include "Core/IRectCalculator.h"
#include "Core/WindowRect.h"
#include "Core/Enums.h"

namespace winrt::Rectangle::Core
{
    class TopLeftSixthCalculator : public IRectCalculator
    {
    public:
        WindowRect Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap = 0) override;
    };

    class TopCenterSixthCalculator : public IRectCalculator
    {
    public:
        WindowRect Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap = 0) override;
    };

    class TopRightSixthCalculator : public IRectCalculator
    {
    public:
        WindowRect Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap = 0) override;
    };

    class BottomLeftSixthCalculator : public IRectCalculator
    {
    public:
        WindowRect Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap = 0) override;
    };

    class BottomCenterSixthCalculator : public IRectCalculator
    {
    public:
        WindowRect Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap = 0) override;
    };

    class BottomRightSixthCalculator : public IRectCalculator
    {
    public:
        WindowRect Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap = 0) override;
    };
}
