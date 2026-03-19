#pragma once
#include "pch.h"
#include <map>
#include <functional>

namespace winrt::Rectangle::Services
{
    class SnapDetectionService
    {
    public:
        SnapDetectionService(void* win32, void* windowManager, ConfigService* config);
        ~SnapDetectionService();

        void Start();
        void Stop();
        bool IsRunning() const;

    private:
        void InstallMouseHook();
        void UninstallMouseHook();
        static LRESULT CALLBACK MouseHookProc(int32_t nCode, WPARAM wParam, LPARAM lParam);

        void* m_win32{ nullptr };
        void* m_windowManager{ nullptr };
        ConfigService* m_config{ nullptr };
        HHOOK m_mouseHook{ nullptr };
        bool m_isRunning{ false };
    };
}
