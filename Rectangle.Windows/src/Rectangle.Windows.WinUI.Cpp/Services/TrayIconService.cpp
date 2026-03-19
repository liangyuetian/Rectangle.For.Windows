#include "pch.h"
#include "Services/TrayIconService.h"
#include "Services/Logger.h"
#include "Core/Enums.h"
#include <shellapi.h>
#include <windowsx.h>

#pragma comment(lib, "shell32.lib")
#pragma comment(lib, "user32.lib")
#pragma comment(lib, "ole32.lib")
#pragma comment(lib, "comctl32.lib")

namespace winrt::Rectangle::Services
{
    const UINT TrayIconService::s_taskbarCreatedMsg = RegisterWindowMessage(L"TaskbarCreated");

    const std::map<std::wstring, Core::WindowAction> TrayIconService::s_tagToAction = {
        { L"LeftHalf", Core::WindowAction::LeftHalf },
        { L"RightHalf", Core::WindowAction::RightHalf },
        { L"CenterHalf", Core::WindowAction::CenterHalf },
        { L"TopHalf", Core::WindowAction::TopHalf },
        { L"BottomHalf", Core::WindowAction::BottomHalf },
        { L"TopLeft", Core::WindowAction::TopLeft },
        { L"TopRight", Core::WindowAction::TopRight },
        { L"BottomLeft", Core::WindowAction::BottomLeft },
        { L"BottomRight", Core::WindowAction::BottomRight },
        { L"FirstThird", Core::WindowAction::FirstThird },
        { L"CenterThird", Core::WindowAction::CenterThird },
        { L"LastThird", Core::WindowAction::LastThird },
        { L"FirstTwoThirds", Core::WindowAction::FirstTwoThirds },
        { L"CenterTwoThirds", Core::WindowAction::CenterTwoThirds },
        { L"LastTwoThirds", Core::WindowAction::LastTwoThirds },
        { L"FirstFourth", Core::WindowAction::FirstFourth },
        { L"SecondFourth", Core::WindowAction::SecondFourth },
        { L"ThirdFourth", Core::WindowAction::ThirdFourth },
        { L"LastFourth", Core::WindowAction::LastFourth },
        { L"FirstThreeFourths", Core::WindowAction::FirstThreeFourths },
        { L"CenterThreeFourths", Core::WindowAction::CenterThreeFourths },
        { L"LastThreeFourths", Core::WindowAction::LastThreeFourths },
        { L"TopLeftSixth", Core::WindowAction::TopLeftSixth },
        { L"TopCenterSixth", Core::WindowAction::TopCenterSixth },
        { L"TopRightSixth", Core::WindowAction::TopRightSixth },
        { L"BottomLeftSixth", Core::WindowAction::BottomLeftSixth },
        { L"BottomCenterSixth", Core::WindowAction::BottomCenterSixth },
        { L"BottomRightSixth", Core::WindowAction::BottomRightSixth },
        { L"TopLeftNinth", Core::WindowAction::TopLeftNinth },
        { L"TopCenterNinth", Core::WindowAction::TopCenterNinth },
        { L"TopRightNinth", Core::WindowAction::TopRightNinth },
        { L"MiddleLeftNinth", Core::WindowAction::MiddleLeftNinth },
        { L"MiddleCenterNinth", Core::WindowAction::MiddleCenterNinth },
        { L"MiddleRightNinth", Core::WindowAction::MiddleRightNinth },
        { L"BottomLeftNinth", Core::WindowAction::BottomLeftNinth },
        { L"BottomCenterNinth", Core::WindowAction::BottomCenterNinth },
        { L"BottomRightNinth", Core::WindowAction::BottomRightNinth },
        { L"TopLeftEighth", Core::WindowAction::TopLeftEighth },
        { L"TopCenterLeftEighth", Core::WindowAction::TopCenterLeftEighth },
        { L"TopCenterRightEighth", Core::WindowAction::TopCenterRightEighth },
        { L"TopRightEighth", Core::WindowAction::TopRightEighth },
        { L"BottomLeftEighth", Core::WindowAction::BottomLeftEighth },
        { L"BottomCenterLeftEighth", Core::WindowAction::BottomCenterLeftEighth },
        { L"BottomCenterRightEighth", Core::WindowAction::BottomCenterRightEighth },
        { L"BottomRightEighth", Core::WindowAction::BottomRightEighth },
        { L"Maximize", Core::WindowAction::Maximize },
        { L"AlmostMaximize", Core::WindowAction::AlmostMaximize },
        { L"MaximizeHeight", Core::WindowAction::MaximizeHeight },
        { L"Larger", Core::WindowAction::Larger },
        { L"Smaller", Core::WindowAction::Smaller },
        { L"LargerWidth", Core::WindowAction::LargerWidth },
        { L"SmallerWidth", Core::WindowAction::SmallerWidth },
        { L"LargerHeight", Core::WindowAction::LargerHeight },
        { L"SmallerHeight", Core::WindowAction::SmallerHeight },
        { L"Center", Core::WindowAction::Center },
        { L"Restore", Core::WindowAction::Restore },
        { L"MoveLeft", Core::WindowAction::MoveLeft },
        { L"MoveRight", Core::WindowAction::MoveRight },
        { L"MoveUp", Core::WindowAction::MoveUp },
        { L"MoveDown", Core::WindowAction::MoveDown },
        { L"NextDisplay", Core::WindowAction::NextDisplay },
        { L"PreviousDisplay", Core::WindowAction::PreviousDisplay },
        { L"Undo", Core::WindowAction::Undo },
        { L"Redo", Core::WindowAction::Redo },
    };

    namespace
    {
        const wchar_t s_windowClassName[] = L"RectangleTrayIconWindow";
        UINT s_nextMenuId = 1000;

        std::vector<std::pair<std::wstring, std::wstring>> GetMenuItems()
        {
            return {
                { L"LeftHalf", L"左半屏" },
                { L"RightHalf", L"右半屏" },
                { L"CenterHalf", L"中间半屏" },
                { L"TopHalf", L"上半屏" },
                { L"BottomHalf", L"下半屏" },
                { L"---", L"" },
                { L"TopLeft", L"左上角" },
                { L"TopRight", L"右上角" },
                { L"BottomLeft", L"左下角" },
                { L"BottomRight", L"右下角" },
                { L"---", L"" },
                { L"FirstThird", L"左侧 1/3" },
                { L"CenterThird", L"中间 1/3" },
                { L"LastThird", L"右侧 1/3" },
                { L"---", L"" },
                { L"Maximize", L"最大化" },
                { L"AlmostMaximize", L"接近最大化" },
                { L"MaximizeHeight", L"最大化高度" },
                { L"Center", L"居中" },
                { L"Restore", L"恢复" },
                { L"---", L"" },
                { L"Larger", L"放大" },
                { L"Smaller", L"缩小" },
                { L"---", L"" },
                { L"MoveLeft", L"左移" },
                { L"MoveRight", L"右移" },
                { L"MoveUp", L"上移" },
                { L"MoveDown", L"下移" },
                { L"---", L"" },
                { L"NextDisplay", L"下一个显示器" },
                { L"PreviousDisplay", L"上一个显示器" },
                { L"---", L"" },
                { L"Undo", L"撤销" },
                { L"Redo", L"重做" },
            };
        }
    }

    TrayIconService::TrayIconService(ShowSettingsCallback showSettings, ExitCallback onExit, MenuActionCallback onMenuAction)
        : m_showSettingsCallback(std::move(showSettings))
        , m_onExit(std::move(onExit))
        , m_onMenuAction(std::move(onMenuAction))
    {
        Logger::Instance().Info(L"TrayIconService", L"TrayIconService created");
    }

    TrayIconService::~TrayIconService()
    {
        Dispose();
    }

    void TrayIconService::Initialize()
    {
        if (m_isInitialized)
        {
            Logger::Instance().Warning(L"TrayIconService", L"TrayIconService already initialized");
            return;
        }

        try
        {
            Logger::Instance().Info(L"TrayIconService", L"Initializing tray icon");

            WNDCLASSEX wc = {};
            wc.cbSize = sizeof(WNDCLASSEX);
            wc.lpfnWndProc = DefWindowProc;
            wc.hInstance = GetModuleHandle(nullptr);
            wc.lpszClassName = s_windowClassName;
            RegisterClassEx(&wc);

            m_trayIconHwnd = CreateWindowEx(
                0,
                s_windowClassName,
                L"Rectangle Tray Icon",
                WS_POPUP,
                0, 0, 0, 0,
                nullptr,
                nullptr,
                GetModuleHandle(nullptr),
                this
            );

            if (!m_trayIconHwnd)
            {
                Logger::Instance().Error(L"TrayIconService", L"Failed to create tray icon window");
                return;
            }

            SetWindowLongPtr(m_trayIconHwnd, GWLP_WNDPROC, reinterpret_cast<LONG_PTR>(&TrayIconService::TrayIconWndProcHook));

            CreateTrayIcon();
            CreateContextMenu();

            m_isInitialized = true;
            Logger::Instance().Info(L"TrayIconService", L"Tray icon initialized successfully");
        }
        catch (const std::exception& ex)
        {
            Logger::Instance().Error(L"TrayIconService", L"Tray icon initialization failed");
        }
    }

    void TrayIconService::Dispose()
    {
        if (!m_isInitialized)
        {
            return;
        }

        DestroyTrayIcon();

        if (m_contextMenu)
        {
            DestroyMenu(m_contextMenu);
            m_contextMenu = nullptr;
        }

        if (m_trayIconHwnd)
        {
            DestroyWindow(m_trayIconHwnd);
            m_trayIconHwnd = nullptr;
        }

        UnregisterClass(s_windowClassName, GetModuleHandle(nullptr));

        m_isInitialized = false;
        Logger::Instance().Info(L"TrayIconService", L"Tray icon disposed");
    }

    void TrayIconService::CreateTrayIcon()
    {
        m_iconHandle = LoadIcon(nullptr, IDI_APPLICATION);
        if (!m_iconHandle)
        {
            Logger::Instance().Error(L"TrayIconService", L"Failed to load icon");
            return;
        }

        NOTIFYICONDATA nid = {};
        nid.cbSize = sizeof(NOTIFYICONDATA);
        nid.hWnd = m_trayIconHwnd;
        nid.uID = m_trayIconId;
        nid.uFlags = NIF_MESSAGE | NIF_ICON | NIF_TIP;
        nid.uCallbackMessage = WM_USER + 1;
        nid.hIcon = m_iconHandle;
        wcscpy_s(nid.szTip, L"Rectangle");

        if (!Shell_NotifyIcon(NIM_ADD, &nid))
        {
            Logger::Instance().Error(L"TrayIconService", L"Failed to add tray icon");
            return;
        }

        Logger::Instance().Debug(L"TrayIconService", L"Tray icon created");
    }

    void TrayIconService::DestroyTrayIcon()
    {
        if (m_trayIconHwnd && m_iconHandle)
        {
            NOTIFYICONDATA nid = {};
            nid.cbSize = sizeof(NOTIFYICONDATA);
            nid.hWnd = m_trayIconHwnd;
            nid.uID = m_trayIconId;
            Shell_NotifyIcon(NIM_DELETE, &nid);
        }

        if (m_iconHandle)
        {
            DestroyIcon(m_iconHandle);
            m_iconHandle = nullptr;
        }
    }

    void TrayIconService::CreateContextMenu()
    {
        m_contextMenu = CreatePopupMenu();
        if (!m_contextMenu)
        {
            Logger::Instance().Error(L"TrayIconService", L"Failed to create context menu");
            return;
        }

        m_menuItemIds.clear();
        UINT menuId = s_nextMenuId;

        auto items = GetMenuItems();
        for (const auto& item : items)
        {
            if (item.first == L"---")
            {
                InsertMenu(m_contextMenu, menuId, MF_SEPARATOR, 0, nullptr);
            }
            else
            {
                std::wstring displayText = item.second;
                std::wstring shortcutText = GetShortcutText(item.first);
                if (!shortcutText.empty())
                {
                    displayText += L"\t" + shortcutText;
                }

                InsertMenu(m_contextMenu, menuId, MF_STRING, menuId, displayText.c_str());
                m_menuItemIds[menuId] = item.first;
                menuId++;
            }
        }

        InsertMenu(m_contextMenu, menuId, MF_SEPARATOR, 0, nullptr);
        menuId++;

        InsertMenu(m_contextMenu, menuId, MF_STRING, menuId, L"设置...");
        m_menuItemIds[menuId] = L"_settings_";
        menuId++;

        InsertMenu(m_contextMenu, menuId, MF_STRING, menuId, L"退出");
        m_menuItemIds[menuId] = L"_exit_";

        Logger::Instance().Debug(L"TrayIconService", L"Context menu created with items");
    }

    void TrayIconService::OnTrayIconClick(int x, int y)
    {
        if (!m_contextMenu)
        {
            return;
        }

        m_isMenuVisible = true;

        SetForegroundWindow(m_trayIconHwnd);

        TrackPopupMenu(
            m_contextMenu,
            TPM_LEFTALIGN | TPM_LEFTBUTTON | TPM_RIGHTBUTTON,
            x, y,
            0,
            m_trayIconHwnd,
            nullptr
        );

        PostMessage(m_trayIconHwnd, WM_NULL, 0, 0);
        m_isMenuVisible = false;
    }

    void TrayIconService::OnMenuItemClick(const std::wstring& actionTag)
    {
        Logger::Instance().Info(L"TrayIconService", L"Menu item clicked: " + actionTag);

        if (actionTag == L"_settings_")
        {
            if (m_showSettingsCallback)
            {
                m_showSettingsCallback();
            }
        }
        else if (actionTag == L"_exit_")
        {
            if (m_onExit)
            {
                m_onExit();
            }
        }
        else
        {
            if (m_onMenuAction)
            {
                m_onMenuAction(actionTag);
            }
        }
    }

    LRESULT CALLBACK TrayIconService::TrayIconWndProcHook(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam)
    {
        TrayIconService* pThis = nullptr;

        if (msg == WM_CREATE)
        {
            auto cs = reinterpret_cast<CREATESTRUCT*>(lParam);
            pThis = reinterpret_cast<TrayIconService*>(cs->lpCreateParams);
            SetWindowLongPtr(hwnd, GWLP_USERDATA, reinterpret_cast<LONG_PTR>(pThis));
        }
        else
        {
            pThis = reinterpret_cast<TrayIconService*>(GetWindowLongPtr(hwnd, GWLP_USERDATA));
        }

        if (pThis)
        {
            return pThis->TrayIconWndProc(msg, wParam, lParam);
        }

        return DefWindowProc(hwnd, msg, wParam, lParam);
    }

    LRESULT TrayIconService::TrayIconWndProc(UINT msg, WPARAM wParam, LPARAM lParam)
    {
        switch (msg)
        {
        case WM_USER + 1:
            if (LOWORD(lParam) == WM_LBUTTONDOWN || LOWORD(lParam) == WM_RBUTTONDOWN)
            {
                POINT pt;
                GetCursorPos(&pt);
                OnTrayIconClick(pt.x, pt.y);
            }
            break;

        case WM_USER + 2:
            if (m_contextMenu)
            {
                UINT menuId = static_cast<UINT>(wParam);
                auto it = m_menuItemIds.find(menuId);
                if (it != m_menuItemIds.end())
                {
                    OnMenuItemClick(it->second);
                }
            }
            break;

        default:
            if (msg == s_taskbarCreatedMsg)
            {
                NOTIFYICONDATA nid = {};
                nid.cbSize = sizeof(NOTIFYICONDATA);
                nid.hWnd = m_trayIconHwnd;
                nid.uID = m_trayIconId;
                nid.uFlags = NIF_MESSAGE | NIF_ICON | NIF_TIP;
                nid.uCallbackMessage = WM_USER + 1;
                nid.hIcon = m_iconHandle;
                wcscpy_s(nid.szTip, m_tooltipText.c_str());
                Shell_NotifyIcon(NIM_ADD, &nid);
            }
            break;
        }

        return DefWindowProc(m_trayIconHwnd, msg, wParam, lParam);
    }

    void TrayIconService::ShowNotification(const std::wstring& title, const std::wstring& message)
    {
        if (!m_trayIconHwnd)
        {
            return;
        }

        NOTIFYICONDATA nid = {};
        nid.cbSize = sizeof(NOTIFYICONDATA);
        nid.hWnd = m_trayIconHwnd;
        nid.uID = m_trayIconId;
        nid.uFlags = NIF_INFO;
        wcscpy_s(nid.szInfoTitle, title.c_str());
        wcscpy_s(nid.szInfo, message.c_str());
        nid.uTimeout = 3000;

        Shell_NotifyIcon(NIM_MODIFY, &nid);
    }

    void TrayIconService::UpdateTooltip(const std::wstring& tooltipText)
    {
        m_tooltipText = tooltipText;

        if (!m_trayIconHwnd)
        {
            return;
        }

        NOTIFYICONDATA nid = {};
        nid.cbSize = sizeof(NOTIFYICONDATA);
        nid.hWnd = m_trayIconHwnd;
        nid.uID = m_trayIconId;
        nid.uFlags = NIF_TIP;
        wcscpy_s(nid.szTip, tooltipText.c_str());

        Shell_NotifyIcon(NIM_MODIFY, &nid);
    }

    void TrayIconService::PreloadMenuIcons()
    {
        Logger::Instance().Info(L"TrayIconService", L"Preloading menu icons");

        std::vector<HICON> loadedIcons;
        for (int i = 0; i < 16; ++i)
        {
            HICON icon = LoadIcon(nullptr, MAKEINTRESOURCE(IDI_APPLICATION));
            if (icon)
            {
                loadedIcons.push_back(icon);
            }
        }

        for (HICON icon : loadedIcons)
        {
            DestroyIcon(icon);
        }
    }

    std::wstring TrayIconService::GetShortcutText(const std::wstring& actionName)
    {
        static const std::map<std::wstring, std::wstring> shortcuts = {
            { L"LeftHalf", L"Ctrl+Alt+Left" },
            { L"RightHalf", L"Ctrl+Alt+Right" },
            { L"CenterHalf", L"Ctrl+Alt+Down" },
            { L"TopHalf", L"Ctrl+Alt+Up" },
            { L"BottomHalf", L"" },
            { L"TopLeft", L"Ctrl+Alt+U" },
            { L"TopRight", L"Ctrl+Alt+I" },
            { L"BottomLeft", L"Ctrl+Alt+J" },
            { L"BottomRight", L"Ctrl+Alt+K" },
            { L"Maximize", L"Ctrl+Alt+Enter" },
            { L"AlmostMaximize", L"Ctrl+Alt+Space" },
            { L"Center", L"Ctrl+Alt+C" },
            { L"Restore", L"Ctrl+Alt+M" },
            { L"Larger", L"Ctrl+Alt+L" },
            { L"Smaller", L"Ctrl+Alt+S" },
            { L"MoveLeft", L"Ctrl+Alt+Shift+Left" },
            { L"MoveRight", L"Ctrl+Alt+Shift+Right" },
            { L"MoveUp", L"Ctrl+Alt+Shift+Up" },
            { L"MoveDown", L"Ctrl+Alt+Shift+Down" },
            { L"NextDisplay", L"Ctrl+Alt+Tab" },
            { L"PreviousDisplay", L"Ctrl+Alt+Shift+Tab" },
            { L"Undo", L"Ctrl+Alt+Z" },
            { L"Redo", L"Ctrl+Alt+Shift+Z" },
        };

        auto it = shortcuts.find(actionName);
        if (it != shortcuts.end())
        {
            return it->second;
        }

        return L"";
    }
}
