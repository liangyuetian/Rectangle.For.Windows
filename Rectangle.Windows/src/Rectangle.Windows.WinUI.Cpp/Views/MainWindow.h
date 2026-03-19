#pragma once
#include "pch.h"
#include "Views/SettingsWindow.g.h"

namespace winrt::Rectangle::Views
{
    class MainWindow : public winrt::Microsoft::UI::Xaml::Window
    {
    public:
        static void Initialize();

        static void ShowSettings();
        static void CloseSettings();

    private:
        MainWindow() = default;

        static std::unique_ptr<SettingsWindow> s_settingsWindow;
    };
}