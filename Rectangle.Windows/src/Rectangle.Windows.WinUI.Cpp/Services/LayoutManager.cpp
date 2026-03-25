#include "pch.h"
#include "Services/LayoutManager.h"
#include "Services/Logger.h"
#include "Services/Win32WindowService.h"
#include "Services/ConfigService.h"
#include "Services/WindowEnumerator.h"
#include "Services/ScreenDetectionService.h"
#include <shlobj.h>
#include <winrt/Windows.Data.Json.h>

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

        auto windows = WindowEnumerator::GetAllWindows();
        for (int64_t hwnd : windows)
        {
            int32_t x, y, w, h;
            if (win32.GetWindowRect(hwnd, x, y, w, h))
            {
                if (x >= workArea.Left && x < workArea.Right &&
                    y >= workArea.Top && y < workArea.Bottom)
                {
                    WindowPositionInfo info;
                    info.ProcessName = WindowEnumerator::GetProcessNameFromWindow(hwnd);
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
            auto windows = WindowEnumerator::GetAllWindows();
            for (int64_t hwnd : windows)
            {
                if (!info.ProcessName.empty())
                {
                    auto process = WindowEnumerator::GetProcessNameFromWindow(hwnd);
                    if (_wcsicmp(process.c_str(), info.ProcessName.c_str()) != 0)
                    {
                        continue;
                    }
                }
                win32.SetWindowRect(hwnd, info.X, info.Y, info.Width, info.Height);
                break;
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
        try
        {
            winrt::Windows::Data::Json::JsonArray layoutsArr;
            for (auto const& layout : m_layouts)
            {
                winrt::Windows::Data::Json::JsonObject layoutObj;
                layoutObj.Insert(L"Id", winrt::Windows::Data::Json::JsonValue::CreateStringValue(layout.Id));
                layoutObj.Insert(L"Name", winrt::Windows::Data::Json::JsonValue::CreateStringValue(layout.Name));
                layoutObj.Insert(L"CreatedAt", winrt::Windows::Data::Json::JsonValue::CreateStringValue(layout.CreatedAt));

                winrt::Windows::Data::Json::JsonArray windowsArr;
                for (auto const& w : layout.Windows)
                {
                    winrt::Windows::Data::Json::JsonObject wObj;
                    wObj.Insert(L"ProcessName", winrt::Windows::Data::Json::JsonValue::CreateStringValue(w.ProcessName));
                    wObj.Insert(L"WindowTitle", winrt::Windows::Data::Json::JsonValue::CreateStringValue(w.WindowTitle));
                    wObj.Insert(L"X", winrt::Windows::Data::Json::JsonValue::CreateNumberValue(w.X));
                    wObj.Insert(L"Y", winrt::Windows::Data::Json::JsonValue::CreateNumberValue(w.Y));
                    wObj.Insert(L"Width", winrt::Windows::Data::Json::JsonValue::CreateNumberValue(w.Width));
                    wObj.Insert(L"Height", winrt::Windows::Data::Json::JsonValue::CreateNumberValue(w.Height));
                    windowsArr.Append(wObj);
                }
                layoutObj.Insert(L"Windows", windowsArr);
                layoutsArr.Append(layoutObj);
            }

            std::wofstream file(m_layoutsFilePath, std::ios::trunc);
            if (!file.is_open()) return;
            file << layoutsArr.Stringify().c_str();
            file.close();
        }
        catch (...)
        {
            Logger::Instance().Warning(L"LayoutManager", L"保存布局文件失败");
        }
    }

    void LayoutManager::LoadLayoutsFromFile()
    {
        m_layouts.clear();
        try
        {
            std::wifstream file(m_layoutsFilePath);
            if (!file.is_open()) return;
            std::wstringstream buffer;
            buffer << file.rdbuf();
            file.close();
            auto text = buffer.str();
            if (text.empty()) return;

            auto layoutsArr = winrt::Windows::Data::Json::JsonArray::Parse(text);
            for (uint32_t i = 0; i < layoutsArr.Size(); i++)
            {
                auto layoutObj = layoutsArr.GetObjectAt(i);
                WindowLayout layout;
                layout.Id = layoutObj.GetNamedString(L"Id", L"");
                layout.Name = layoutObj.GetNamedString(L"Name", L"");
                layout.CreatedAt = layoutObj.GetNamedString(L"CreatedAt", L"");

                if (layoutObj.HasKey(L"Windows"))
                {
                    auto windowsArr = layoutObj.GetNamedArray(L"Windows");
                    for (uint32_t j = 0; j < windowsArr.Size(); j++)
                    {
                        auto wObj = windowsArr.GetObjectAt(j);
                        WindowPositionInfo w;
                        w.ProcessName = wObj.GetNamedString(L"ProcessName", L"");
                        w.WindowTitle = wObj.GetNamedString(L"WindowTitle", L"");
                        w.X = static_cast<int32_t>(wObj.GetNamedNumber(L"X", 0));
                        w.Y = static_cast<int32_t>(wObj.GetNamedNumber(L"Y", 0));
                        w.Width = static_cast<int32_t>(wObj.GetNamedNumber(L"Width", 0));
                        w.Height = static_cast<int32_t>(wObj.GetNamedNumber(L"Height", 0));
                        layout.Windows.push_back(w);
                    }
                }
                m_layouts.push_back(layout);
            }
        }
        catch (...)
        {
            Logger::Instance().Warning(L"LayoutManager", L"加载布局文件失败");
        }
    }
}
