#include "pch.h"
#include "App.h"
#include "Services/Logger.h"
#include "Services/ConfigService.h"
#include "Services/WindowManager.h"
#include "Services/HotkeyManager.h"
#include "Services/TrayIconService.h"
#include "Services/LastActiveWindowService.h"
#include "Services/SnapDetectionService.h"
#include "Services/OperationHistoryManager.h"
#include "Services/ThemeService.h"
#include "Core/CalculatorFactory.h"
#include "Core/WindowHistory.h"

#pragma comment(lib, "user32.lib")
#pragma comment(lib, "kernel32.lib")

namespace winrt::Rectangle
{
    App* App::s_instance = nullptr;

    App& App::Current()
    {
        if (!s_instance)
        {
            s_instance = new App();
        }
        return *s_instance;
    }

    void App::Initialize()
    {
        s_instance = this;

        Logger::Instance().Info(L"App", L"Application starting...");

        _putenv_s("MICROSOFT_WINDOWSAPPRUNTIME_BASE_DIRECTORY",
            std::filesystem::current_path().wstring().c_str());

        InitializeServices();

        Logger::Instance().Info(L"App", L"Application initialized successfully");
    }

    void App::InitializeServices()
    {
        m_configService = std::make_unique<Services::ConfigService>();
        m_configService->ConfigChanged = [this](const Services::AppConfig& config) {
            if (m_hotkeyManager)
            {
                m_hotkeyManager->ReloadFromConfig();
            }
        };

        Logger::Instance().SetConfigService(m_configService.get());

        auto config = m_configService->Load();
        Logger::Instance().SetLogLevel(static_cast<Services::LogLevel>(config.LogLevel));
        if (config.LogToFile && !config.LogFilePath.empty())
        {
            Logger::Instance().SetLogToFile(true, config.LogFilePath);
        }

        ThemeService::Instance().LoadThemeFromConfig();

        Services::Win32WindowService win32;
        Core::CalculatorFactory factory;
        Core::WindowHistory history;

        m_operationHistoryManager = std::make_unique<Services::OperationHistoryManager>();
        m_operationHistoryManager->SetMaxHistoryCount(config.History.MaxHistoryCount);

        m_windowManager = std::make_unique<Services::WindowManager>();
        m_windowManager->SetConfigService(m_configService.get());
        m_windowManager->SetOperationHistory(m_operationHistoryManager.get());

        m_lastActiveService = std::make_unique<Services::LastActiveWindowService>();
        m_windowManager->SetLastActiveWindowService(m_lastActiveService.get());

        CreateHiddenWindow();

        m_hotkeyManager = std::make_unique<Services::HotkeyManager>(
            m_hotkeyHwnd,
            m_windowManager.get(),
            m_configService.get()
        );

        m_trayIconService = std::make_unique<Services::TrayIconService>(
            [this]() { },
            [this]() { Exit(); },
            [this](const std::wstring& actionTag) {
                Logger::Instance().Info(L"App", L"Tray menu action: " + actionTag);
            }
        );
        m_trayIconService->Initialize();

        m_snapDetectionService = std::make_unique<Services::SnapDetectionService>(
            &win32,
            m_windowManager.get(),
            m_configService.get()
        );

        Services::TrayIconService::PreloadMenuIcons();

        Logger::Instance().Info(L"App", L"All services initialized");
    }

    void App::CreateHiddenWindow()
    {
        Logger::Instance().Debug(L"App", L"Creating hidden window for message handling");

        WNDCLASSEX wc = {};
        wc.cbSize = sizeof(WNDCLASSEX);
        wc.lpfnWndProc = DefWindowProc;
        wc.hInstance = GetModuleHandle(nullptr);
        wc.lpszClassName = L"RectangleHiddenWindow";

        if (!RegisterClassEx(&wc))
        {
            Logger::Instance().Error(L"App", L"Failed to register window class");
            return;
        }

        HWND hwnd = CreateWindowEx(
            0,
            L"RectangleHiddenWindow",
            L"Rectangle",
            WS_OVERLAPPEDWINDOW,
            0, 0, 0, 0,
            nullptr,
            nullptr,
            GetModuleHandle(nullptr),
            nullptr
        );

        if (!hwnd)
        {
            Logger::Instance().Error(L"App", L"Failed to create hidden window");
            return;
        }

        ShowWindow(hwnd, SW_HIDE);
        m_hiddenWindow = reinterpret_cast<void*>(hwnd);
        m_hotkeyHwnd = reinterpret_cast<int64_t>(hwnd);

        Logger::Instance().Debug(L"App", L"Hidden window created successfully");
    }

    void App::Run()
    {
        Logger::Instance().Info(L"App", L"Application entering main loop");

        MSG msg;
        while (GetMessage(&msg, nullptr, 0, 0))
        {
            TranslateMessage(&msg);
            DispatchMessage(&msg);
        }
    }

    void App::Exit()
    {
        Logger::Instance().Info(L"App", L"Application exiting...");

        Cleanup();

        Logger::Instance().Info(L"App", L"Application exited");
        exit(0);
    }

    void App::Cleanup()
    {
        m_snapDetectionService.reset();
        m_trayIconService.reset();
        m_hotkeyManager.reset();
        m_windowManager.reset();
        m_configService.reset();

        if (m_hiddenWindow)
        {
            DestroyWindow(reinterpret_cast<HWND>(m_hiddenWindow));
            m_hiddenWindow = nullptr;
        }
    }

    void* App::GetWindowManager()
    {
        return s_instance ? s_instance->m_windowManager.get() : nullptr;
    }

    void* App::GetHotkeyManager()
    {
        return s_instance ? s_instance->m_hotkeyManager.get() : nullptr;
    }

    void* App::GetConfigService()
    {
        return s_instance ? s_instance->m_configService.get() : nullptr;
    }
}
