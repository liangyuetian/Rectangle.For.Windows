#include "pch.h"
#include "Views/SettingsWindow.h"
#include "Services/Logger.h"
#include "Services/ConfigService.h"
#include "ViewModels/SettingsViewModel.h"
#include <winrt/Microsoft.UI.Xaml.Controls.h>
#include <winrt/Microsoft.UI.Xaml.Input.h>

using namespace winrt::Microsoft::UI::Xaml;
using namespace winrt::Microsoft::UI::Xaml::Controls;
using namespace winrt::Microsoft::UI::Xaml::Input;

namespace winrt::Rectangle::Views
{
    SettingsWindow::SettingsWindow()
    {
        Logger::Instance().Info(L"SettingsWindow", L"SettingsWindow constructed");
        InitializeComponent();
    }

    SettingsWindow::~SettingsWindow()
    {
        Logger::Instance().Info(L"SettingsWindow", L"SettingsWindow destructed");
    }

    void SettingsWindow::Initialize()
    {
        LoadSettings();
        Logger::Instance().Info(L"SettingsWindow", L"SettingsWindow initialized");
    }

    void SettingsWindow::ShowWindow()
    {
        Activate();
    }

    void SettingsWindow::CloseWindow()
    {
        Close();
    }

    void SettingsWindow::LoadSettings()
    {
        auto configService = Services::ConfigService();
        auto config = configService.Load();

        m_gapSize = config.GapSize;
        m_launchOnLogin = config.LaunchOnLogin;
    }

    void SettingsWindow::SaveSettings()
    {
        auto configService = Services::ConfigService();
        auto config = configService.Load();
        config.GapSize = m_gapSize;
        config.LaunchOnLogin = m_launchOnLogin;
        configService.Save(config);
        Logger::Instance().Info(L"SettingsWindow", L"Settings saved");
    }

    void SettingsWindow::OnGapSizeChanged(int32_t newValue)
    {
        m_gapSize = newValue;
        SaveSettings();
        Logger::Instance().Info(L"SettingsWindow", L"Gap size changed to: " + std::to_wstring(newValue));
    }

    void SettingsWindow::OnLaunchOnLoginChanged(bool enabled)
    {
        m_launchOnLogin = enabled;
        SaveSettings();
        Logger::Instance().Info(L"SettingsWindow", L"Launch on login changed to: " + std::to_wstring(enabled));
    }
}
