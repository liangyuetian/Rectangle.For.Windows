#pragma once
#include "pch.h"
#include "Core/IRectCalculator.h"
#include "Core/WindowRect.h"
#include "Core/Enums.h"

namespace winrt::Rectangle::Core
{
    class MoveLeftCalculator : public IRectCalculator
    {
    public:
        WindowRect Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap = 0) override;
    };

    class MoveRightCalculator : public IRectCalculator
    {
    public:
        WindowRect Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap = 0) override;
    };

    class MoveUpCalculator : public IRectCalculator
    {
    public:
        WindowRect Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap = 0) override;
    };

    class MoveDownCalculator : public IRectCalculator
    {
    public:
        WindowRect Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap = 0) override;
    };

    class DoubleHeightUpCalculator : public IRectCalculator
    {
    public:
        WindowRect Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap = 0) override;
    };

    class DoubleHeightDownCalculator : public IRectCalculator
    {
    public:
        WindowRect Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap = 0) override;
    };

    class DoubleWidthLeftCalculator : public IRectCalculator
    {
    public:
        WindowRect Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap = 0) override;
    };

    class DoubleWidthRightCalculator : public IRectCalculator
    {
    public:
        WindowRect Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap = 0) override;
    };

    class HalveHeightUpCalculator : public IRectCalculator
    {
    public:
        WindowRect Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap = 0) override;
    };

    class HalveHeightDownCalculator : public IRectCalculator
    {
    public:
        WindowRect Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap = 0) override;
    };

    class HalveWidthLeftCalculator : public IRectCalculator
    {
    public:
        WindowRect Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap = 0) override;
    };

    class HalveWidthRightCalculator : public IRectCalculator
    {
    public:
        WindowRect Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap = 0) override;
    };
}
