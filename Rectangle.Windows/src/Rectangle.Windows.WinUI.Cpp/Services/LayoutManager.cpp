#include "pch.h"
#include "Services/LayoutManager.h"
#include "Services/Logger.h"
#include "Services/Win32WindowService.h"
#include "Services/ConfigService.h"
#include "Services/WindowEnumerator.h"
#include "Services/ScreenDetectionService.h"
#include <shlobj.h>

namespace winrt::Rectangle::Services
{
    LayoutManager::LayoutManager()
    {
        Logger::Instance().Info(L"LayoutManager", L"LayoutManager constructed");
    }

    void LayoutManager::Initialize()
    {
        PWSTR appDataPath = nullptr;
        if (SHGetKnownFolderPath(FOLDERID_AppData, 0, nullptr, &appDataPath) == S_OK)
        {
            std::wstring path(appDataPath);
            path += L"\\Rectangle\\layouts.json";
            m_layoutsFilePath = path;
            CoTaskMemFree(appDataPath);
        }

        LoadLayoutsFromFile();
        m_isInitialized = true;
        Logger::Instance().Info(L"LayoutManager", L"LayoutManager initialized");
    }

    void LayoutManager::Shutdown()
    {
        SaveLayoutsToFile();
        m_isInitialized = false;
        Logger::Instance().Info(L"LayoutManager", L"LayoutManager shutdown");
    }

    std::wstring LayoutManager::SaveCurrentLayout(const std::wstring& name)
    {
        WindowLayout layout;
        layout.Id = L"guid_" + std::to_wstring(GetTickCount64());
        layout.Name = name;

        auto now = std::chrono::system_clock::now();
        std::time_t nowTime = std::chrono::system_clock::to_time_t(now);
        wchar_t timeBuffer[32];
        wcsftime(timeBuffer, 32, L"%Y-%m-%d %H:%M:%S", localtime(&nowTime));
        layout.CreatedAt = timeBuffer;

        Win32WindowService win32;
        ScreenDetectionService screenService(&win32);
        auto workArea = screenService.GetPrimaryWorkArea();

        auto windows = WindowEnumerator::EnumerateVisibleWindows();
        for (int64_t hwnd : windows)
        {
            int32_t x, y, w, h;
            if (win32.GetWindowRect(hwnd, x, y, w, h))
            {
                if (x >= workArea.Left && x < workArea.Right &&
                    y >= workArea.Top && y < workArea.Bottom)
                {
                    WindowPositionInfo info;
                    info.X = x;
                    info.Y = y;
                    info.Width = w;
                    info.Height = h;
                    layout.Windows.push_back(info);
                }
            }
        }

        m_layouts.push_back(layout);
        SaveLayoutsToFile();

        Logger::Instance().Info(L"LayoutManager", L"Layout saved: " + name);
        return layout.Id;
    }

    bool LayoutManager::RestoreLayout(const std::wstring& layoutId)
    {
        auto it = std::find_if(m_layouts.begin(), m_layouts.end(),
            [&layoutId](const WindowLayout& l) { return l.Id == layoutId; });

        if (it == m_layouts.end())
        {
            Logger::Instance().Error(L"LayoutManager", L"Layout not found: " + layoutId);
            return false;
        }

        Win32WindowService win32;
        for (const auto& info : it->Windows)
        {
            auto windows = WindowEnumerator::EnumerateVisibleWindows();
            for (int64_t hwnd : windows)
            {
                win32.SetWindowRect(hwnd, info.X, info.Y, info.Width, info.Height);
            }
        }

        Logger::Instance().Info(L"LayoutManager", L"Layout restored: " + it->Name);
        return true;
    }

    bool LayoutManager::DeleteLayout(const std::wstring& layoutId)
    {
        auto it = std::remove_if(m_layouts.begin(), m_layouts.end(),
            [&layoutId](const WindowLayout& l) { return l.Id == layoutId; });

        if (it == m_layouts.end())
        {
            return false;
        }

        m_layouts.erase(it, m_layouts.end());
        SaveLayoutsToFile();
        Logger::Instance().Info(L"LayoutManager", L"Layout deleted: " + layoutId);
        return true;
    }

    std::vector<WindowLayout> LayoutManager::GetAllLayouts()
    {
        return m_layouts;
    }

    WindowLayout* LayoutManager::GetLayout(const std::wstring& layoutId)
    {
        auto it = std::find_if(m_layouts.begin(), m_layouts.end(),
            [&layoutId](const WindowLayout& l) { return l.Id == layoutId; });

        if (it != m_layouts.end())
        {
            return &(*it);
        }
        return nullptr;
    }

    std::wstring LayoutManager::GetLayoutsFilePath()
    {
        return m_layoutsFilePath;
    }

    void LayoutManager::SaveLayoutsToFile()
    {
        Logger::Instance().Info(L"LayoutManager", L"Saving layouts to file");
    }

    void LayoutManager::LoadLayoutsFromFile()
    {
        Logger::Instance().Info(L"LayoutManager", L"Loading layouts from file");
    }
}