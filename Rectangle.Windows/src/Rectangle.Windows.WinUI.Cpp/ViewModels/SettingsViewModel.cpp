#include "pch.h"
#include "ViewModels/SettingsViewModel.h"
#include "Services/ConfigService.h"
#include "Services/Logger.h"

namespace winrt::Rectangle::ViewModels
{
    std::wstring ShortcutItem::GetShortcutText() const
    {
        if (!IsEnabled || KeyCode <= 0) return L"";

        std::wstring result;
        if (ModifierFlags & 0x0002) result += L"Ctrl+";
        if (ModifierFlags & 0x0001) result += L"Alt+";
        if (ModifierFlags & 0x0004) result += L"Shift+";
        if (ModifierFlags & 0x0008) result += L"Win+";

        if (KeyCode >= 0x41 && KeyCode <= 0x5A)
            result += static_cast<wchar_t>(KeyCode);
        else if (KeyCode >= 0x30 && KeyCode <= 0x39)
            result += static_cast<wchar_t>(KeyCode);
        else if (KeyCode == 0x25) result += L"Left";
        else if (KeyCode == 0x26) result += L"Up";
        else if (KeyCode == 0x27) result += L"Right";
        else if (KeyCode == 0x28) result += L"Down";
        else if (KeyCode == 0x0D) result += L"Enter";
        else if (KeyCode == 0x08) result += L"Back";
        else result += L"0x" + std::to_wstring(KeyCode);

        return result;
    }

    SettingsViewModel::SettingsViewModel()
        : LaunchOnLogin(false)
        , GapSize(0)
        , HorizontalSplitRatio(50)
        , VerticalSplitRatio(50)
        , LogToFile(false)
        , LogLevel(1)
        , CurrentTheme(L"Default")
        , LanguageIndex(0)
    {
        InitializeDefaultShortcuts();
        LoadSettings();
    }

    void SettingsViewModel::InitializeDefaultShortcuts()
    {
        m_halfScreenShortcuts = {
            { L"LeftHalf", L"左半屏", L"\uE7C5" },
            { L"RightHalf", L"右半屏", L"\uE7C6" },
            { L"CenterHalf", L"中间半屏", L"\uE7C4" },
            { L"TopHalf", L"上半屏", L"\uE7C3" },
            { L"BottomHalf", L"下半屏", L"\uE7C2" },
        };

        m_cornerShortcuts = {
            { L"TopLeft", L"左上", L"\uE744" },
            { L"TopRight", L"右上", L"\uE745" },
            { L"BottomLeft", L"左下", L"\uE746" },
            { L"BottomRight", L"右下", L"\uE747" },
        };

        m_thirdShortcuts = {
            { L"FirstThird", L"左首1/3", L"\uE74C" },
            { L"CenterThird", L"中间1/3", L"\uE74D" },
            { L"LastThird", L"右首1/3", L"\uE74E" },
            { L"FirstTwoThirds", L"左侧2/3", L"\uE74C" },
            { L"CenterTwoThirds", L"中间2/3", L"\uE74D" },
            { L"LastTwoThirds", L"右侧2/3", L"\uE74E" },
        };

        m_fourthShortcuts = {
            { L"FirstFourth", L"左首1/4", L"\uE74C" },
            { L"SecondFourth", L"左中1/4", L"\uE74D" },
            { L"ThirdFourth", L"右中1/4", L"\uE74D" },
            { L"LastFourth", L"右首1/4", L"\uE74E" },
        };

        m_sixthShortcuts = {
            { L"TopLeftSixth", L"左上1/6", L"\uE744" },
            { L"TopCenterSixth", L"中上1/6", L"\uE7C4" },
            { L"TopRightSixth", L"右上1/6", L"\uE745" },
            { L"BottomLeftSixth", L"左下1/6", L"\uE746" },
            { L"BottomCenterSixth", L"中下1/6", L"\uE7C4" },
            { L"BottomRightSixth", L"右下1/6", L"\uE747" },
        };

        m_maximizeShortcuts = {
            { L"Maximize", L"最大化", L"\uE739" },
            { L"AlmostMaximize", L"接近最大化", L"\uE73A" },
            { L"MaximizeHeight", L"最大化高度", L"\uE73B" },
            { L"Center", L"居中", L"\uE74D" },
            { L"Restore", L"恢复", L"\uE72A" },
        };

        m_resizeShortcuts = {
            { L"Larger", L"放大", L"\uE71F" },
            { L"Smaller", L"缩小", L"\uE71E" },
        };

        m_moveShortcuts = {
            { L"MoveLeft", L"左移", L"\uE72B" },
            { L"MoveRight", L"右移", L"\uE72A" },
            { L"MoveUp", L"上移", L"\uE72C" },
            { L"MoveDown", L"下移", L"\uE72D" },
        };

        m_displayShortcuts = {
            { L"NextDisplay", L"下一个显示器", L"\uE7F5" },
            { L"PreviousDisplay", L"上一个显示器", L"\uE7F6" },
        };

        m_otherShortcuts = {
            { L"Undo", L"撤销", L"\uE7A7" },
            { L"Redo", L"重做", L"\uE7A6" },
        };
    }

    void SettingsViewModel::LoadSettings()
    {
        Logger::Instance().Info(L"SettingsViewModel", L"Loading settings");
    }

    void SettingsViewModel::SaveSettings()
    {
        Logger::Instance().Info(L"SettingsViewModel", L"Saving settings");
    }
}
