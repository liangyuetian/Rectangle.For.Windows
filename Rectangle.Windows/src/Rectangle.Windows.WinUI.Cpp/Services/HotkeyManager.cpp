#include "pch.h"
#include "Services/HotkeyManager.h"
#include "Services/WindowManager.h"
#include "Services/Logger.h"
#include <Windows.h>

#pragma comment(lib, "user32.lib")
#pragma comment(lib, "comctl32.lib")

extern "C" {
    WINUSERAPI BOOL WINAPI RegisterHotKey(_In_ HWND hWnd, _In_ int id, _In_ UINT fsModifiers, _In_ UINT vk);
    WINUSERAPI BOOL WINAPI UnregisterHotKey(_In_ HWND hWnd, _In_ int id);
    WINUSERAPI LRESULT WINAPI DefSubclassProc(_In_ HWND hWnd, _In_ UINT uMsg, _In_ WPARAM wParam, _In_ LPARAM lParam);
    WINUSERAPI BOOL WINAPI SetWindowSubclass(_In_ HWND hWnd, _In_ SUBCLASSPROC pfnSubclass, _In_ UINT_PTR uIdSubclass, _In_ DWORD_PTR dwRefData);
    WINUSERAPI BOOL WINAPI RemoveWindowSubclass(_In_ HWND hWnd, _In_ SUBCLASSPROC pfnSubclass, _In_ UINT_PTR uIdSubclass);
}

namespace winrt::Rectangle::Services
{
    static Core::WindowAction TryParseAction(const std::wstring& actionName)
    {
        static const std::map<std::wstring, Core::WindowAction> actionMap = {
            { L"LeftHalf", Core::WindowAction::LeftHalf }, { L"RightHalf", Core::WindowAction::RightHalf },
            { L"CenterHalf", Core::WindowAction::CenterHalf }, { L"TopHalf", Core::WindowAction::TopHalf },
            { L"BottomHalf", Core::WindowAction::BottomHalf }, { L"TopLeft", Core::WindowAction::TopLeft },
            { L"TopRight", Core::WindowAction::TopRight }, { L"BottomLeft", Core::WindowAction::BottomLeft },
            { L"BottomRight", Core::WindowAction::BottomRight }, { L"FirstThird", Core::WindowAction::FirstThird },
            { L"CenterThird", Core::WindowAction::CenterThird }, { L"LastThird", Core::WindowAction::LastThird },
            { L"FirstTwoThirds", Core::WindowAction::FirstTwoThirds }, { L"CenterTwoThirds", Core::WindowAction::CenterTwoThirds },
            { L"LastTwoThirds", Core::WindowAction::LastTwoThirds }, { L"FirstFourth", Core::WindowAction::FirstFourth },
            { L"SecondFourth", Core::WindowAction::SecondFourth }, { L"ThirdFourth", Core::WindowAction::ThirdFourth },
            { L"LastFourth", Core::WindowAction::LastFourth }, { L"FirstThreeFourths", Core::WindowAction::FirstThreeFourths },
            { L"CenterThreeFourths", Core::WindowAction::CenterThreeFourths }, { L"LastThreeFourths", Core::WindowAction::LastThreeFourths },
            { L"TopLeftSixth", Core::WindowAction::TopLeftSixth }, { L"TopCenterSixth", Core::WindowAction::TopCenterSixth },
            { L"TopRightSixth", Core::WindowAction::TopRightSixth }, { L"BottomLeftSixth", Core::WindowAction::BottomLeftSixth },
            { L"BottomCenterSixth", Core::WindowAction::BottomCenterSixth }, { L"BottomRightSixth", Core::WindowAction::BottomRightSixth },
            { L"TopVerticalThird", Core::WindowAction::TopVerticalThird }, { L"MiddleVerticalThird", Core::WindowAction::MiddleVerticalThird },
            { L"BottomVerticalThird", Core::WindowAction::BottomVerticalThird }, { L"TopVerticalTwoThirds", Core::WindowAction::TopVerticalTwoThirds },
            { L"BottomVerticalTwoThirds", Core::WindowAction::BottomVerticalTwoThirds }, { L"Maximize", Core::WindowAction::Maximize },
            { L"AlmostMaximize", Core::WindowAction::AlmostMaximize }, { L"MaximizeHeight", Core::WindowAction::MaximizeHeight },
            { L"Larger", Core::WindowAction::Larger }, { L"Smaller", Core::WindowAction::Smaller },
            { L"LargerWidth", Core::WindowAction::LargerWidth }, { L"SmallerWidth", Core::WindowAction::SmallerWidth },
            { L"LargerHeight", Core::WindowAction::LargerHeight }, { L"SmallerHeight", Core::WindowAction::SmallerHeight },
            { L"Center", Core::WindowAction::Center }, { L"Restore", Core::WindowAction::Restore },
            { L"MoveLeft", Core::WindowAction::MoveLeft }, { L"MoveRight", Core::WindowAction::MoveRight },
            { L"MoveUp", Core::WindowAction::MoveUp }, { L"MoveDown", Core::WindowAction::MoveDown },
            { L"NextDisplay", Core::WindowAction::NextDisplay }, { L"PreviousDisplay", Core::WindowAction::PreviousDisplay },
            { L"Undo", Core::WindowAction::Undo }, { L"Redo", Core::WindowAction::Redo }
        };

        auto it = actionMap.find(actionName);
        return it != actionMap.end() ? it->second : Core::WindowAction::None;
    }

    HotkeyManager::HotkeyManager(int64_t hwnd, WindowManager* windowManager, ConfigService* configService)
        : m_hwnd(hwnd)
        , m_windowManager(windowManager)
        , m_configService(configService)
    {
        Logger::Instance().Info(L"HotkeyManager", L"HotkeyManager initializing");

        auto hWnd = reinterpret_cast<HWND>(m_hwnd);
        if (hWnd && SetWindowSubclass(hWnd, &HotkeyManager::WindowSubclassProc, 1, reinterpret_cast<DWORD_PTR>(this)))
        {
            m_subclassInstalled = true;
        }

        LoadFromConfig();
    }

    HotkeyManager::~HotkeyManager()
    {
        UnregisterAllHotkeys();
        if (m_subclassInstalled)
        {
            auto hWnd = reinterpret_cast<HWND>(m_hwnd);
            if (hWnd) RemoveWindowSubclass(hWnd, &HotkeyManager::WindowSubclassProc, 1);
            m_subclassInstalled = false;
        }
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

            auto action = TryParseAction(actionName);
            if (action == Core::WindowAction::None) continue;

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
