#pragma once
#include "pch.h"
#include <memory>
#include <functional>
#include <map>
#include <vector>
#include <string>

struct ITaskbarList4;
struct IContextMenu;

namespace winrt::Rectangle::Services
{
    class TrayIconService
    {
    public:
        using ShowSettingsCallback = std::function<void()>;
        using ExitCallback = std::function<void()>;
        using MenuActionCallback = std::function<void(const std::wstring& actionTag)>;

        TrayIconService(ShowSettingsCallback showSettings, ExitCallback onExit, MenuActionCallback onMenuAction);
        ~TrayIconService();

        void Initialize();
        void Dispose();

        void ShowNotification(const std::wstring& title, const std::wstring& message);
        void UpdateTooltip(const std::wstring& tooltipText);

        static void PreloadMenuIcons();

    private:
        void CreateTrayIcon();
        void CreateContextMenu();
        void SetupMenuItemCommands();
        void DestroyTrayIcon();

        std::wstring GetShortcutText(const std::wstring& actionName);

        LRESULT TrayIconWndProc(UINT msg, WPARAM wParam, LPARAM lParam);
        static LRESULT CALLBACK TrayIconWndProcHook(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam);

        void OnTrayIconClick(int x, int y);
        void OnMenuItemClick(const std::wstring& actionTag);

        ShowSettingsCallback m_showSettingsCallback;
        ExitCallback m_onExit;
        MenuActionCallback m_onMenuAction;

        HWND m_trayIconHwnd{ nullptr };
        HICON m_iconHandle{ nullptr };
        UINT m_trayIconId{ 1 };

        bool m_isInitialized{ false };
        bool m_isMenuVisible{ false };

        HMENU m_contextMenu{ nullptr };
        std::map<UINT, std::wstring> m_menuItemIds;

        static const UINT s_taskbarCreatedMsg;

        std::wstring m_tooltipText;

        static const std::map<std::wstring, Core::WindowAction> s_tagToAction;
    };
}
