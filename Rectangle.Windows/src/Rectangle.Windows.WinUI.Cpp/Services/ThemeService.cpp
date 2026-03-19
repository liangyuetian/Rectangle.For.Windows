#include "pch.h"
#include "Services/ThemeService.h"
#include "Services/Logger.h"

namespace winrt::Rectangle::Services
{
    ThemeService& ThemeService::Instance()
    {
        static ThemeService instance;
        return instance;
    }

    void ThemeService::LoadThemeFromConfig()
    {
        Logger::Instance().Info(L"ThemeService", L"Loading theme from config");
    }

    void ThemeService::SetTheme(const std::wstring& themeName)
    {
        m_currentTheme = themeName;
        Logger::Instance().Info(L"ThemeService", L"Theme set to: " + themeName);
    }

    std::wstring ThemeService::GetCurrentTheme() const
    {
        return m_currentTheme;
    }
}
