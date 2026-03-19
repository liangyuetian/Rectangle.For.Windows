#pragma once
#include "pch.h"

namespace winrt::Rectangle::Core
{
    enum class SubsequentExecutionMode
    {
        None = 0,
        CycleSize = 1,
        CyclePosition = 2,
        CycleDisplay = 3
    };

    enum class TodoSidebarSide
    {
        Left = 0,
        Right = 1
    };

    enum class WindowActionType
    {
        None,
        HalfScreen,
        Corner,
        Third,
        Fourth,
        Sixth,
        Eighth,
        Ninth,
        Maximize,
        Move,
        Resize,
        Display,
        Restore,
        Special
    };

    struct DisplayInfo
    {
        int32_t Index{ 0 };
        std::wstring Name;
        int32_t Width{ 0 };
        int32_t Height{ 0 };
        int32_t X{ 0 };
        int32_t Y{ 0 };
        bool IsPrimary{ false };
        int32_t Dpi{ 0 };
    };

    enum class WindowAction
    {
        None = 0,

        LeftHalf,
        RightHalf,
        CenterHalf,
        TopHalf,
        BottomHalf,

        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,

        FirstThird,
        CenterThird,
        LastThird,
        FirstTwoThirds,
        CenterTwoThirds,
        LastTwoThirds,

        FirstFourth,
        SecondFourth,
        ThirdFourth,
        LastFourth,
        FirstThreeFourths,
        CenterThreeFourths,
        LastThreeFourths,

        TopLeftSixth,
        TopCenterSixth,
        TopRightSixth,
        BottomLeftSixth,
        BottomCenterSixth,
        BottomRightSixth,

        TopLeftNinth,
        TopCenterNinth,
        TopRightNinth,
        MiddleLeftNinth,
        MiddleCenterNinth,
        MiddleRightNinth,
        BottomLeftNinth,
        BottomCenterNinth,
        BottomRightNinth,

        TopLeftEighth,
        TopCenterLeftEighth,
        TopCenterRightEighth,
        TopRightEighth,
        BottomLeftEighth,
        BottomCenterLeftEighth,
        BottomCenterRightEighth,
        BottomRightEighth,

        TopLeftThird,
        TopRightThird,
        BottomLeftThird,
        BottomRightThird,

        TopVerticalThird,
        MiddleVerticalThird,
        BottomVerticalThird,
        TopVerticalTwoThirds,
        BottomVerticalTwoThirds,

        CenterProminently,

        MoveLeft,
        MoveRight,
        MoveUp,
        MoveDown,

        DoubleHeightUp,
        DoubleHeightDown,
        DoubleWidthLeft,
        DoubleWidthRight,
        HalveHeightUp,
        HalveHeightDown,
        HalveWidthLeft,
        HalveWidthRight,

        LargerWidth,
        SmallerWidth,
        LargerHeight,
        SmallerHeight,

        Specified,

        Maximize,
        AlmostMaximize,
        MaximizeHeight,
        Larger,
        Smaller,
        Center,
        Restore,

        NextDisplay,
        PreviousDisplay,

        LeftTodo,
        RightTodo,

        Undo,
        Redo
    };

    inline std::wstring_view ToString(WindowAction action)
    {
        switch (action)
        {
        case WindowAction::LeftHalf: return L"左半屏";
        case WindowAction::RightHalf: return L"右半屏";
        case WindowAction::CenterHalf: return L"中间半屏";
        case WindowAction::TopHalf: return L"上半屏";
        case WindowAction::BottomHalf: return L"下半屏";
        case WindowAction::TopLeft: return L"左上";
        case WindowAction::TopRight: return L"右上";
        case WindowAction::BottomLeft: return L"左下";
        case WindowAction::BottomRight: return L"右下";
        case WindowAction::FirstThird: return L"左首1/3";
        case WindowAction::CenterThird: return L"中间1/3";
        case WindowAction::LastThird: return L"右首1/3";
        case WindowAction::FirstTwoThirds: return L"左侧2/3";
        case WindowAction::CenterTwoThirds: return L"中间2/3";
        case WindowAction::LastTwoThirds: return L"右侧2/3";
        case WindowAction::FirstFourth: return L"左首1/4";
        case WindowAction::SecondFourth: return L"左二1/4";
        case WindowAction::ThirdFourth: return L"右二1/4";
        case WindowAction::LastFourth: return L"右首1/4";
        case WindowAction::FirstThreeFourths: return L"左侧3/4";
        case WindowAction::CenterThreeFourths: return L"中间3/4";
        case WindowAction::LastThreeFourths: return L"右侧3/4";
        case WindowAction::TopLeftSixth: return L"左上1/6";
        case WindowAction::TopCenterSixth: return L"中上1/6";
        case WindowAction::TopRightSixth: return L"右上1/6";
        case WindowAction::BottomLeftSixth: return L"左下1/6";
        case WindowAction::BottomCenterSixth: return L"中下1/6";
        case WindowAction::BottomRightSixth: return L"右下1/6";
        case WindowAction::Maximize: return L"最大化";
        case WindowAction::AlmostMaximize: return L"接近最大化";
        case WindowAction::MaximizeHeight: return L"最大化高度";
        case WindowAction::Larger: return L"放大";
        case WindowAction::Smaller: return L"缩小";
        case WindowAction::Center: return L"居中";
        case WindowAction::Restore: return L"恢复";
        case WindowAction::MoveLeft: return L"左移";
        case WindowAction::MoveRight: return L"右移";
        case WindowAction::MoveUp: return L"上移";
        case WindowAction::MoveDown: return L"下移";
        case WindowAction::NextDisplay: return L"下一显示器";
        case WindowAction::PreviousDisplay: return L"上一显示器";
        case WindowAction::Undo: return L"撤销";
        case WindowAction::Redo: return L"重做";
        default: return L"未知";
        }
    }
}
