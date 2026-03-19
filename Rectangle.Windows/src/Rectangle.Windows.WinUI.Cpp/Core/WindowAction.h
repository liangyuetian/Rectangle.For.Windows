#pragma once
#include "pch.h"

namespace winrt::Rectangle::Core
{
    struct WindowActionInfo
    {
        WindowAction Action;
        std::wstring Name;
        std::wstring DisplayName;
        std::wstring ShortcutTag;
    };

    class WindowActionHelper
    {
    public:
        static std::vector<WindowActionInfo> GetAllActions();
        static std::wstring GetActionName(WindowAction action);
        static std::wstring GetDisplayName(WindowAction action);
        static bool IsValidAction(WindowAction action);
    };
}
