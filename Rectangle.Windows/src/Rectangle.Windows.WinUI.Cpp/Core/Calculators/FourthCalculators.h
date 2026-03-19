#pragma once
#include "pch.h"
#include "Core/IRectCalculator.h"
#include "Core/WindowRect.h"
#include "Core/Enums.h"

namespace winrt::Rectangle::Core
{
    class FirstFourthCalculator : public IRectCalculator
    {
    public:
        WindowRect Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap = 0) override;
    };

    class SecondFourthCalculator : public IRectCalculator
    {
    public:
        WindowRect Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap = 0) override;
    };

    class ThirdFourthCalculator : public IRectCalculator
    {
    public:
        WindowRect Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap = 0) override;
    };

    class LastFourthCalculator : public IRectCalculator
    {
    public:
        WindowRect Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap = 0) override;
    };

    class FirstThreeFourthsCalculator : public IRectCalculator
    {
    public:
        WindowRect Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap = 0) override;
    };

    class CenterThreeFourthsCalculator : public IRectCalculator
    {
    public:
        WindowRect Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap = 0) override;
    };

    class LastThreeFourthsCalculator : public IRectCalculator
    {
    public:
        WindowRect Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap = 0) override;
    };
}
