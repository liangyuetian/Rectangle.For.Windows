#pragma once
#include "pch.h"
#include "Core/Enums.h"

namespace winrt::Rectangle::Core
{
    class RepeatedExecutionsCalculator
    {
    public:
        static bool SupportsCycle(WindowAction action);
        static std::vector<WindowAction> GetCycleGroup(WindowAction action);
        static WindowAction GetNextCycleAction(WindowAction action, int32_t executionCount);
        static WindowAction GetPreviousCycleAction(WindowAction action);
    };
}
