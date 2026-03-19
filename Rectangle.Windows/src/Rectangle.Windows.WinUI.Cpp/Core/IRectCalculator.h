#pragma once
#include "pch.h"
#include "Core/WindowRect.h"
#include "Core/Enums.h"

namespace winrt::Rectangle::Core
{
    struct IRectCalculator
    {
        virtual ~IRectCalculator() = default;
        virtual WindowRect Calculate(const WorkArea& workArea, const WindowRect& currentWindow, WindowAction action, int32_t gap = 0) = 0;
    };
}
