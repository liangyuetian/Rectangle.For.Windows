#pragma once
#include "pch.h"
#include "Views/SettingsWindow.g.h"

namespace winrt::Rectangle::Views
{
    class SettingsWindow : public winrt::Microsoft::UI::Xaml::Window
    {
    public:
        SettingsWindow();
        ~SettingsWindow();

        void Initialize();
        void ShowWindow();
        void CloseWindow();

    private:
        void CreateUI();
        void LoadSettings();
        void SaveSettings();

        void OnGapSizeChanged(int32_t newValue);
        void OnLaunchOnLoginChanged(bool enabled);

        int32_t m_gapSize{ 0 };
        bool m_launchOnLogin{ false };
    };
}