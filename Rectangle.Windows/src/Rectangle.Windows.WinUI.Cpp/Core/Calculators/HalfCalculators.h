#pragma once
#include "pch.h"
#include "Core/IRectCalculator.h"
#include "Core/WindowRect.h"
#include "Core/Enums.h"

namespace winrt::Rectangle::Core
{
    class LeftHalfCalculator : public IRectCalculator
    {
    public:
        WindowRect Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap = 0) override;
    };

    class RightHalfCalculator : public IRectCalculator
    {
    public:
        WindowRect Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap = 0) override;
    };

    class CenterHalfCalculator : public IRectCalculator
    {
    public:
        WindowRect Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap = 0) override;
    };

    class TopHalfCalculator : public IRectCalculator
    {
    public:
        WindowRect Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap = 0) override;
    };

    class BottomHalfCalculator : public IRectCalculator
    {
    public:
        WindowRect Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap = 0) override;
    };
}
