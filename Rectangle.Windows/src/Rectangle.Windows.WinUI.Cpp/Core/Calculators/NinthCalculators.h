#pragma once
#include "pch.h"
#include "Core/IRectCalculator.h"
#include "Core/WindowRect.h"
#include "Core/Enums.h"

namespace winrt::Rectangle::Core
{
    class TopLeftNinthCalculator : public IRectCalculator
    {
    public:
        WindowRect Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap = 0) override;
    };

    class TopCenterNinthCalculator : public IRectCalculator
    {
    public:
        WindowRect Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap = 0) override;
    };

    class TopRightNinthCalculator : public IRectCalculator
    {
    public:
        WindowRect Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap = 0) override;
    };

    class MiddleLeftNinthCalculator : public IRectCalculator
    {
    public:
        WindowRect Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap = 0) override;
    };

    class MiddleCenterNinthCalculator : public IRectCalculator
    {
    public:
        WindowRect Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap = 0) override;
    };

    class MiddleRightNinthCalculator : public IRectCalculator
    {
    public:
        WindowRect Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap = 0) override;
    };

    class BottomLeftNinthCalculator : public IRectCalculator
    {
    public:
        WindowRect Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap = 0) override;
    };

    class BottomCenterNinthCalculator : public IRectCalculator
    {
    public:
        WindowRect Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap = 0) override;
    };

    class BottomRightNinthCalculator : public IRectCalculator
    {
    public:
        WindowRect Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap = 0) override;
    };
}