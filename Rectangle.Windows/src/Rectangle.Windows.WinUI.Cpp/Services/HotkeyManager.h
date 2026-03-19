#pragma once
#include "pch.h"
#include <map>
#include <functional>

struct HWND__;
using HWND = struct HWND__;

namespace winrt::Rectangle::Services
{
    class HotkeyManager
    {
    public:
        HotkeyManager(int64_t hwnd, class WindowManager* windowManager, ConfigService* configService);
        ~HotkeyManager();

        void ReloadFromConfig();
        void SetCapturingMode(bool capturing);

        struct HotkeyConflict
        {
            std::wstring Description;
            std::wstring ConflictingAction;
        };

        std::vector<HotkeyConflict> DetectAllConflicts() const;

    private:
        void LoadFromConfig();
        void RegisterHotkey(int32_t id, uint32_t modifiers, uint32_t vk, const std::wstring& actionName);
        void UnregisterAllHotkeys();
        uint32_t ToHotKeyModifiers(uint32_t flags);

        static LRESULT CALLBACK WindowSubclassProc(HWND hWnd, UINT uMsg, WPARAM wParam, LPARAM lParam,
            UINT_PTR uIdSubclass, DWORD_PTR dwRefData);

        int64_t m_hwnd{ 0 };
        WindowManager* m_windowManager{ nullptr };
        ConfigService* m_configService{ nullptr };
        std::map<int32_t, class WindowAction> m_hotkeyActions;
        int32_t m_nextHotkeyId{ 1 };
        bool m_isCapturingMode{ false };

        std::function<LRESULT(HWND, UINT, WPARAM, LPARAM)> m_subclassProc;
    };
}
