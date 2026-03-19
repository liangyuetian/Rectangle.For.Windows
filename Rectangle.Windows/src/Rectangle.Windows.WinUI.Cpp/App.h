#pragma once
#include "pch.h"
#include <memory>

namespace winrt::Rectangle
{
    class App
    {
    public:
        static App& Current();

        void Initialize();
        void Run();
        void Exit();

        static void* GetWindowManager();
        static void* GetHotkeyManager();
        static void* GetConfigService();

    private:
        App() = default;
        ~App() = default;
        App(const App&) = delete;
        App& operator=(const App&) = delete;

        void CreateHiddenWindow();
        void InitializeServices();
        void Cleanup();

        static App* s_instance;

        void* m_mainWindow{ nullptr };
        void* m_hiddenWindow{ nullptr };
        int64_t m_hotkeyHwnd{ 0 };

        std::unique_ptr<Services::ConfigService> m_configService;
        std::unique_ptr<Services::WindowManager> m_windowManager;
        std::unique_ptr<Services::HotkeyManager> m_hotkeyManager;
        std::unique_ptr<Services::TrayIconService> m_trayIconService;
        std::unique_ptr<Services::LastActiveWindowService> m_lastActiveService;
        std::unique_ptr<Services::SnapDetectionService> m_snapDetectionService;
        std::unique_ptr<Services::OperationHistoryManager> m_operationHistoryManager;
    };
}
