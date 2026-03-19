#pragma once
#include "pch.h"

namespace winrt::Rectangle::Services
{
    class ThemeService
    {
    public:
        static ThemeService& Instance();

        void LoadThemeFromConfig();
        void SetTheme(const std::wstring& themeName);
        std::wstring GetCurrentTheme() const;

    private:
        ThemeService() = default;
        ~ThemeService() = default;

        std::wstring m_currentTheme{ L"Default" };
    };
}
