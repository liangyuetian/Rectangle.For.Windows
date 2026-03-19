#include "pch.h"
#include "Core/RepeatedExecutionsCalculator.h"

namespace winrt::Rectangle::Core
{
    inline bool RepeatedExecutionsCalculator::SupportsCycle(WindowAction action)
    {
        switch (action)
        {
        case WindowAction::LeftHalf:
        case WindowAction::RightHalf:
        case WindowAction::TopHalf:
        case WindowAction::BottomHalf:
        case WindowAction::TopLeft:
        case WindowAction::TopRight:
        case WindowAction::BottomLeft:
        case WindowAction::BottomRight:
        case WindowAction::FirstThird:
        case WindowAction::CenterThird:
        case WindowAction::LastThird:
        case WindowAction::FirstTwoThirds:
        case WindowAction::CenterTwoThirds:
        case WindowAction::LastTwoThirds:
        case WindowAction::FirstFourth:
        case WindowAction::SecondFourth:
        case WindowAction::ThirdFourth:
        case WindowAction::LastFourth:
        case WindowAction::FirstThreeFourths:
        case WindowAction::CenterThreeFourths:
        case WindowAction::LastThreeFourths:
        case WindowAction::TopLeftSixth:
        case WindowAction::TopCenterSixth:
        case WindowAction::TopRightSixth:
        case WindowAction::BottomLeftSixth:
        case WindowAction::BottomCenterSixth:
        case WindowAction::BottomRightSixth:
            return true;
        default:
            return false;
        }
    }

    inline std::vector<WindowAction> RepeatedExecutionsCalculator::GetCycleGroup(WindowAction action)
    {
        switch (action)
        {
        case WindowAction::LeftHalf:
        case WindowAction::RightHalf:
        case WindowAction::TopHalf:
        case WindowAction::BottomHalf:
            return { WindowAction::LeftHalf, WindowAction::RightHalf, WindowAction::TopHalf, WindowAction::BottomHalf };

        case WindowAction::TopLeft:
        case WindowAction::TopRight:
        case WindowAction::BottomRight:
        case WindowAction::BottomLeft:
            return { WindowAction::TopLeft, WindowAction::TopRight, WindowAction::BottomRight, WindowAction::BottomLeft };

        case WindowAction::FirstThird:
        case WindowAction::CenterThird:
        case WindowAction::LastThird:
            return { WindowAction::FirstThird, WindowAction::CenterThird, WindowAction::LastThird };

        case WindowAction::FirstTwoThirds:
        case WindowAction::CenterTwoThirds:
        case WindowAction::LastTwoThirds:
            return { WindowAction::FirstTwoThirds, WindowAction::CenterTwoThirds, WindowAction::LastTwoThirds };

        case WindowAction::FirstFourth:
        case WindowAction::SecondFourth:
        case WindowAction::ThirdFourth:
        case WindowAction::LastFourth:
            return { WindowAction::FirstFourth, WindowAction::SecondFourth, WindowAction::ThirdFourth, WindowAction::LastFourth };

        case WindowAction::TopLeftSixth:
        case WindowAction::TopCenterSixth:
        case WindowAction::TopRightSixth:
        case WindowAction::BottomLeftSixth:
        case WindowAction::BottomCenterSixth:
        case WindowAction::BottomRightSixth:
            return {
                WindowAction::TopLeftSixth, WindowAction::TopCenterSixth, WindowAction::TopRightSixth,
                WindowAction::BottomRightSixth, WindowAction::BottomCenterSixth, WindowAction::BottomLeftSixth
            };

        default:
            return { action };
        }
    }

    inline WindowAction RepeatedExecutionsCalculator::GetNextCycleAction(WindowAction action, int32_t executionCount)
    {
        auto group = GetCycleGroup(action);
        if (group.size() <= 1) return action;

        int32_t index = -1;
        for (int32_t i = 0; i < static_cast<int32_t>(group.size()); ++i)
        {
            if (group[i] == action)
            {
                index = i;
                break;
            }
        }

        if (index < 0) return action;
        return group[(index + 1) % group.size()];
    }

    inline WindowAction RepeatedExecutionsCalculator::GetPreviousCycleAction(WindowAction action)
    {
        auto group = GetCycleGroup(action);
        if (group.size() <= 1) return action;

        int32_t index = -1;
        for (int32_t i = 0; i < static_cast<int32_t>(group.size()); ++i)
        {
            if (group[i] == action)
            {
                index = i;
                break;
            }
        }

        if (index < 0) return action;
        return group[(index - 1 + group.size()) % group.size()];
    }
}
