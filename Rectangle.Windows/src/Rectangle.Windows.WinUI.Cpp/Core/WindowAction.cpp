#include "pch.h"
#include "Core/WindowAction.h"

namespace winrt::Rectangle::Core
{
    std::vector<WindowActionInfo> WindowActionHelper::GetAllActions()
    {
        return {
            { WindowAction::LeftHalf, L"LeftHalf", L"左半屏", L"LeftHalf" },
            { WindowAction::RightHalf, L"RightHalf", L"右半屏", L"RightHalf" },
            { WindowAction::CenterHalf, L"CenterHalf", L"中间半屏", L"CenterHalf" },
            { WindowAction::TopHalf, L"TopHalf", L"上半屏", L"TopHalf" },
            { WindowAction::BottomHalf, L"BottomHalf", L"下半屏", L"BottomHalf" },
            { WindowAction::TopLeft, L"TopLeft", L"左上", L"TopLeft" },
            { WindowAction::TopRight, L"TopRight", L"右上", L"TopRight" },
            { WindowAction::BottomLeft, L"BottomLeft", L"左下", L"BottomLeft" },
            { WindowAction::BottomRight, L"BottomRight", L"右下", L"BottomRight" },
            { WindowAction::FirstThird, L"FirstThird", L"左首1/3", L"FirstThird" },
            { WindowAction::CenterThird, L"CenterThird", L"中间1/3", L"CenterThird" },
            { WindowAction::LastThird, L"LastThird", L"右首1/3", L"LastThird" },
            { WindowAction::FirstTwoThirds, L"FirstTwoThirds", L"左侧2/3", L"FirstTwoThirds" },
            { WindowAction::CenterTwoThirds, L"CenterTwoThirds", L"中间2/3", L"CenterTwoThirds" },
            { WindowAction::LastTwoThirds, L"LastTwoThirds", L"右侧2/3", L"LastTwoThirds" },
            { WindowAction::FirstFourth, L"FirstFourth", L"左首1/4", L"FirstFourth" },
            { WindowAction::SecondFourth, L"SecondFourth", L"左二1/4", L"SecondFourth" },
            { WindowAction::ThirdFourth, L"ThirdFourth", L"右二1/4", L"ThirdFourth" },
            { WindowAction::LastFourth, L"LastFourth", L"右首1/4", L"LastFourth" },
            { WindowAction::FirstThreeFourths, L"FirstThreeFourths", L"左侧3/4", L"FirstThreeFourths" },
            { WindowAction::CenterThreeFourths, L"CenterThreeFourths", L"中间3/4", L"CenterThreeFourths" },
            { WindowAction::LastThreeFourths, L"LastThreeFourths", L"右侧3/4", L"LastThreeFourths" },
            { WindowAction::TopLeftSixth, L"TopLeftSixth", L"左上1/6", L"TopLeftSixth" },
            { WindowAction::TopCenterSixth, L"TopCenterSixth", L"中上1/6", L"TopCenterSixth" },
            { WindowAction::TopRightSixth, L"TopRightSixth", L"右上1/6", L"TopRightSixth" },
            { WindowAction::BottomLeftSixth, L"BottomLeftSixth", L"左下1/6", L"BottomLeftSixth" },
            { WindowAction::BottomCenterSixth, L"BottomCenterSixth", L"中下1/6", L"BottomCenterSixth" },
            { WindowAction::BottomRightSixth, L"BottomRightSixth", L"右下1/6", L"BottomRightSixth" },
            { WindowAction::Maximize, L"Maximize", L"最大化", L"Maximize" },
            { WindowAction::AlmostMaximize, L"AlmostMaximize", L"接近最大化", L"AlmostMaximize" },
            { WindowAction::MaximizeHeight, L"MaximizeHeight", L"最大化高度", L"MaximizeHeight" },
            { WindowAction::Larger, L"Larger", L"放大", L"Larger" },
            { WindowAction::Smaller, L"Smaller", L"缩小", L"Smaller" },
            { WindowAction::Center, L"Center", L"居中", L"Center" },
            { WindowAction::Restore, L"Restore", L"恢复", L"Restore" },
            { WindowAction::MoveLeft, L"MoveLeft", L"左移", L"MoveLeft" },
            { WindowAction::MoveRight, L"MoveRight", L"右移", L"MoveRight" },
            { WindowAction::MoveUp, L"MoveUp", L"上移", L"MoveUp" },
            { WindowAction::MoveDown, L"MoveDown", L"下移", L"MoveDown" },
            { WindowAction::NextDisplay, L"NextDisplay", L"下一显示器", L"NextDisplay" },
            { WindowAction::PreviousDisplay, L"PreviousDisplay", L"上一显示器", L"PreviousDisplay" },
            { WindowAction::Undo, L"Undo", L"撤销", L"Undo" },
            { WindowAction::Redo, L"Redo", L"重做", L"Redo" },
        };
    }

    std::wstring WindowActionHelper::GetActionName(WindowAction action)
    {
        switch (action)
        {
        case WindowAction::LeftHalf: return L"LeftHalf";
        case WindowAction::RightHalf: return L"RightHalf";
        case WindowAction::TopHalf: return L"TopHalf";
        case WindowAction::BottomHalf: return L"BottomHalf";
        default: return L"Unknown";
        }
    }

    std::wstring WindowActionHelper::GetDisplayName(WindowAction action)
    {
        return ToString(action);
    }

    bool WindowActionHelper::IsValidAction(WindowAction action)
    {
        return action != WindowAction::None;
    }
}
