#include "pch.h"
#include "Views/MainWindow.h"
#include "Views/SettingsWindow.h"
#include "Services/Logger.h"

namespace winrt::Rectangle::Views
{
    std::unique_ptr<SettingsWindow> MainWindow::s_settingsWindow = nullptr;

    void MainWindow::Initialize()
    {
        Logger::Instance().Info(L"MainWindow", L"MainWindow initialized");
    }

    void MainWindow::ShowSettings()
    {
        if (!s_settingsWindow)
        {
            s_settingsWindow = std::make_unique<SettingsWindow>();
            s_settingsWindow->Initialize();
        }
        s_settingsWindow->ShowWindow();
        Logger::Instance().Info(L"MainWindow", L"Settings window shown");
    }

    void MainWindow::CloseSettings()
    {
        if (s_settingsWindow)
        {
            s_settingsWindow->CloseWindow();
            s_settingsWindow.reset();
        }
        Logger::Instance().Info(L"MainWindow", L"Settings window closed");
    }
}