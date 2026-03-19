#include "pch.h"
#include "Services/HotkeyManager.h"
#include "Services/WindowManager.h"
#include "Services/Logger.h"
#include <Windows.h>

#pragma comment(lib, "user32.lib")

extern "C" {
    WINUSERAPI BOOL WINAPI RegisterHotKey(_In_ HWND hWnd, _In_ int id, _In_ UINT fsModifiers, _In_ UINT vk);
    WINUSERAPI BOOL WINAPI UnregisterHotKey(_In_ HWND hWnd, _In_ int id);
    WINUSERAPI LRESULT WINAPI DefSubclassProc(_In_ HWND hWnd, _In_ UINT uMsg, _In_ WPARAM wParam, _In_ LPARAM lParam);
    WINUSERAPI BOOL WINAPI SetWindowSubclass(_In_ HWND hWnd, _In_ SUBCLASSPROC pfnSubclass, _In_ UINT_PTR uIdSubclass, _In_ DWORD_PTR dwRefData);
    WINUSERAPI BOOL WINAPI RemoveWindowSubclass(_In_ HWND hWnd, _In_ SUBCLASSPROC pfnSubclass, _In_ UINT_PTR uIdSubclass);
}

namespace winrt::Rectangle::Services
{
    HotkeyManager::HotkeyManager(int64_t hwnd, WindowManager* windowManager, ConfigService* configService)
        : m_hwnd(hwnd)
        , m_windowManager(windowManager)
        , m_configService(configService)
    {
        Logger::Instance().Info(L"HotkeyManager", L"HotkeyManager initializing");

        LoadFromConfig();
    }

    HotkeyManager::~HotkeyManager()
    {
        UnregisterAllHotkeys();
    }

    void HotkeyManager::ReloadFromConfig()
    {
        UnregisterAllHotkeys();
        m_hotkeyActions.clear();
        m_nextHotkeyId = 1;
        LoadFromConfig();
    }

    void HotkeyManager::LoadFromConfig()
    {
        if (!m_configService)
        {
            Logger::Instance().Warning(L"HotkeyManager", L"ConfigService is null");
            return;
        }

        auto config = m_configService->Load();
        auto defaults = ConfigService::GetDefaultShortcuts();

        std::map<std::wstring, ShortcutConfig> merged;
        for (const auto& [key, value] : defaults)
        {
            merged[key] = value;
        }
        for (const auto& [key, value] : config.Shortcuts)
        {
            merged[key] = value;
        }

        std::set<std::pair<uint32_t, uint32_t>> seen;

        for (const auto& [actionName, cfg] : merged)
        {
            if (!cfg.Enabled || cfg.KeyCode <= 0)
            {
                continue;
            }

            auto action = Core::WindowAction::None;
            if (actionName == L"LeftHalf") action = Core::WindowAction::LeftHalf;
            else if (actionName == L"RightHalf") action = Core::WindowAction::RightHalf;
            else if (actionName == L"TopHalf") action = Core::WindowAction::TopHalf;
            else if (actionName == L"BottomHalf") action = Core::WindowAction::BottomHalf;
            else if (actionName == L"TopLeft") action = Core::WindowAction::TopLeft;
            else if (actionName == L"TopRight") action = Core::WindowAction::TopRight;
            else if (actionName == L"BottomLeft") action = Core::WindowAction::BottomLeft;
            else if (actionName == L"BottomRight") action = Core::WindowAction::BottomRight;
            else if (actionName == L"Maximize") action = Core::WindowAction::Maximize;
            else if (actionName == L"Restore") action = Core::WindowAction::Restore;
            else if (actionName == L"NextDisplay") action = Core::WindowAction::NextDisplay;
            else if (actionName == L"PreviousDisplay") action = Core::WindowAction::PreviousDisplay;
            else if (actionName == L"Undo") action = Core::WindowAction::Undo;
            else if (actionName == L"Redo") action = Core::WindowAction::Redo;
            else continue;

            auto modifiers = ToHotKeyModifiers(cfg.ModifierFlags);
            auto vk = static_cast<uint32_t>(cfg.KeyCode);

            if (!seen.insert({ vk, modifiers }).second)
            {
                continue;
            }

            auto id = m_nextHotkeyId++;
            RegisterHotkey(id, modifiers, vk, actionName);

            if (action != Core::WindowAction::None)
            {
                m_hotkeyActions[id] = action;
            }
        }

        Logger::Instance().Info(L"HotkeyManager", std::to_wstring(m_hotkeyActions.size()) + L" hotkeys loaded");
    }

    void HotkeyManager::RegisterHotkey(int32_t id, uint32_t modifiers, uint32_t vk, const std::wstring& actionName)
    {
        HWND hwnd = reinterpret_cast<HWND>(m_hwnd);
        if (::RegisterHotKey(hwnd, id, modifiers, vk))
        {
            Logger::Instance().Debug(L"HotkeyManager", L"Registered hotkey: " + actionName);
        }
        else
        {
            Logger::Instance().Warning(L"HotkeyManager", L"Failed to register hotkey: " + actionName);
        }
    }

    void HotkeyManager::UnregisterAllHotkeys()
    {
        HWND hwnd = reinterpret_cast<HWND>(m_hwnd);
        for (const auto& [id, action] : m_hotkeyActions)
        {
            ::UnregisterHotKey(hwnd, id);
        }
    }

    uint32_t HotkeyManager::ToHotKeyModifiers(uint32_t flags)
    {
        uint32_t modifiers = 0;
        if (flags & 0x0002) modifiers |= MOD_CONTROL;
        if (flags & 0x0001) modifiers |= MOD_ALT;
        if (flags & 0x0004) modifiers |= MOD_SHIFT;
        if (flags & 0x0008) modifiers |= MOD_WIN;
        return modifiers;
    }

    void HotkeyManager::SetCapturingMode(bool capturing)
    {
        m_isCapturingMode = capturing;
    }

    std::vector<HotkeyManager::HotkeyConflict> HotkeyManager::DetectAllConflicts() const
    {
        return {};
    }

    LRESULT CALLBACK HotkeyManager::WindowSubclassProc(HWND hWnd, UINT uMsg, WPARAM wParam, LPARAM lParam,
        UINT_PTR uIdSubclass, DWORD_PTR dwRefData)
    {
        const UINT WM_HOTKEY = 0x0312;

        if (uMsg == WM_HOTKEY)
        {
            auto* manager = reinterpret_cast<HotkeyManager*>(dwRefData);
            if (!manager) return DefSubclassProc(hWnd, uMsg, wParam, lParam);

            if (manager->m_isCapturingMode)
            {
                return 0;
            }

            int32_t id = static_cast<int32_t>(wParam);
            auto it = manager->m_hotkeyActions.find(id);
            if (it != manager->m_hotkeyActions.end())
            {
                if (manager->m_windowManager)
                {
                    manager->m_windowManager->Execute(it->second);
                }
                return 0;
            }
        }

        return DefSubclassProc(hWnd, uMsg, wParam, lParam);
    }
}
