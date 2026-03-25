#include "pch.h"
#include "Services/ConfigService.h"
#include "Services/Logger.h"
#include <shlobj.h>
#include <filesystem>
#include <winrt/Windows.Data.Json.h>

namespace winrt::Rectangle::Services
{
    ConfigService::ConfigService()
    {
        EnsureConfigDirectoryExists();
        m_configPath = GetConfigPath();
    }

    std::wstring ConfigService::GetConfigPath()
    {
        wchar_t* appData = nullptr;
        if (SHGetKnownFolderPath(FOLDERID_RoamingAppData, 0, nullptr, &appData) != S_OK)
        {
            return L"config.json";
        }

        std::wstring path = std::wstring(appData) + L"\\Rectangle\\config.json";
        CoTaskMemFree(appData);
        return path;
    }

    void ConfigService::EnsureConfigDirectoryExists()
    {
        wchar_t* appData = nullptr;
        if (SHGetKnownFolderPath(FOLDERID_RoamingAppData, 0, nullptr, &appData) == S_OK)
        {
            std::wstring dir = std::wstring(appData) + L"\\Rectangle";
            CreateDirectory(dir.c_str(), nullptr);
            CoTaskMemFree(appData);
        }
    }

    AppConfig ConfigService::CreateDefaultConfig()
    {
        AppConfig config;
        config.Shortcuts = GetDefaultShortcuts();
        config.IgnoredApps = { L"Rectangle.Windows.exe", L"Rectangle.Windows.WinUI.exe" };
        return config;
    }

    AppConfig ConfigService::Load()
    {
        if (!std::filesystem::exists(m_configPath))
        {
            Logger::Instance().Info(L"ConfigService", L"配置文件不存在，使用默认配置");
            return CreateDefaultConfig();
        }

        try
        {
            AppConfig config = CreateDefaultConfig();
            std::wifstream file(m_configPath);
            if (!file.is_open()) return config;
            std::wstringstream buffer;
            buffer << file.rdbuf();
            std::wstring json = buffer.str();
            file.close();
            if (json.empty()) return config;

            auto root = winrt::Windows::Data::Json::JsonObject::Parse(json);
            auto tryGetBool = [&](wchar_t const* key, bool fallback)
            {
                if (!root.HasKey(key)) return fallback;
                return root.GetNamedBoolean(key, fallback);
            };
            auto tryGetInt = [&](wchar_t const* key, int32_t fallback)
            {
                if (!root.HasKey(key)) return fallback;
                return static_cast<int32_t>(root.GetNamedNumber(key, fallback));
            };
            auto tryGetFloat = [&](wchar_t const* key, float fallback)
            {
                if (!root.HasKey(key)) return fallback;
                return static_cast<float>(root.GetNamedNumber(key, fallback));
            };
            auto tryGetString = [&](wchar_t const* key, std::wstring const& fallback)
            {
                if (!root.HasKey(key)) return fallback;
                return std::wstring(root.GetNamedString(key, fallback).c_str());
            };

            config.GapSize = tryGetInt(L"GapSize", config.GapSize);
            config.HorizontalSplitRatio = tryGetInt(L"HorizontalSplitRatio", config.HorizontalSplitRatio);
            config.VerticalSplitRatio = tryGetInt(L"VerticalSplitRatio", config.VerticalSplitRatio);
            config.LaunchOnLogin = tryGetBool(L"LaunchOnLogin", config.LaunchOnLogin);
            config.SubsequentExecutionMode = tryGetInt(L"SubsequentExecutionMode", config.SubsequentExecutionMode);
            config.UseCursorScreenDetection = tryGetBool(L"UseCursorScreenDetection", config.UseCursorScreenDetection);
            config.MoveCursor = tryGetBool(L"MoveCursor", config.MoveCursor);
            config.MoveCursorAcrossDisplays = tryGetBool(L"MoveCursorAcrossDisplays", config.MoveCursorAcrossDisplays);
            config.SnapEdgeMarginTop = tryGetInt(L"SnapEdgeMarginTop", config.SnapEdgeMarginTop);
            config.SnapEdgeMarginBottom = tryGetInt(L"SnapEdgeMarginBottom", config.SnapEdgeMarginBottom);
            config.SnapEdgeMarginLeft = tryGetInt(L"SnapEdgeMarginLeft", config.SnapEdgeMarginLeft);
            config.SnapEdgeMarginRight = tryGetInt(L"SnapEdgeMarginRight", config.SnapEdgeMarginRight);
            config.CornerSnapAreaSize = tryGetInt(L"CornerSnapAreaSize", config.CornerSnapAreaSize);
            config.SnapModifiers = tryGetInt(L"SnapModifiers", config.SnapModifiers);
            config.LogLevel = tryGetInt(L"LogLevel", config.LogLevel);
            config.LogToFile = tryGetBool(L"LogToFile", config.LogToFile);
            config.LogFilePath = tryGetString(L"LogFilePath", config.LogFilePath);
            config.MaxLogFileSize = tryGetInt(L"MaxLogFileSize", config.MaxLogFileSize);
            config.Theme = tryGetString(L"Theme", config.Theme);
            config.Language = tryGetString(L"Language", config.Language);
            config.CheckForUpdates = tryGetBool(L"CheckForUpdates", config.CheckForUpdates);
            config.MinimumWindowWidth = tryGetFloat(L"MinimumWindowWidth", config.MinimumWindowWidth);
            config.MinimumWindowHeight = tryGetFloat(L"MinimumWindowHeight", config.MinimumWindowHeight);

            if (root.HasKey(L"IgnoredApps"))
            {
                config.IgnoredApps.clear();
                auto arr = root.GetNamedArray(L"IgnoredApps");
                for (uint32_t i = 0; i < arr.Size(); i++)
                {
                    config.IgnoredApps.push_back(std::wstring(arr.GetStringAt(i).c_str()));
                }
            }

            if (root.HasKey(L"Shortcuts"))
            {
                auto shortcutsObj = root.GetNamedObject(L"Shortcuts");
                for (auto const& kv : shortcutsObj)
                {
                    auto value = kv.Value().GetObject();
                    ShortcutConfig sc{};
                    sc.Enabled = value.GetNamedBoolean(L"Enabled", true);
                    sc.KeyCode = static_cast<int32_t>(value.GetNamedNumber(L"KeyCode", 0));
                    sc.ModifierFlags = static_cast<uint32_t>(value.GetNamedNumber(L"ModifierFlags", 0));
                    config.Shortcuts[std::wstring(kv.Key().c_str())] = sc;
                }
            }

            if (root.HasKey(L"History"))
            {
                auto history = root.GetNamedObject(L"History");
                config.History.Enabled = history.GetNamedBoolean(L"Enabled", config.History.Enabled);
                config.History.MaxHistoryCount = static_cast<int32_t>(history.GetNamedNumber(L"MaxHistoryCount", config.History.MaxHistoryCount));
                config.History.EnableUndo = history.GetNamedBoolean(L"EnableUndo", config.History.EnableUndo);
            }

            return config;
        }
        catch (const std::exception& ex)
        {
            Logger::Instance().Error(L"ConfigService", L"读取配置文件失败");
            return CreateDefaultConfig();
        }
    }

    void ConfigService::Save(const AppConfig& config)
    {
        try
        {
            winrt::Windows::Data::Json::JsonObject root;
            root.Insert(L"GapSize", winrt::Windows::Data::Json::JsonValue::CreateNumberValue(config.GapSize));
            root.Insert(L"HorizontalSplitRatio", winrt::Windows::Data::Json::JsonValue::CreateNumberValue(config.HorizontalSplitRatio));
            root.Insert(L"VerticalSplitRatio", winrt::Windows::Data::Json::JsonValue::CreateNumberValue(config.VerticalSplitRatio));
            root.Insert(L"LaunchOnLogin", winrt::Windows::Data::Json::JsonValue::CreateBooleanValue(config.LaunchOnLogin));
            root.Insert(L"SubsequentExecutionMode", winrt::Windows::Data::Json::JsonValue::CreateNumberValue(config.SubsequentExecutionMode));
            root.Insert(L"UseCursorScreenDetection", winrt::Windows::Data::Json::JsonValue::CreateBooleanValue(config.UseCursorScreenDetection));
            root.Insert(L"MoveCursor", winrt::Windows::Data::Json::JsonValue::CreateBooleanValue(config.MoveCursor));
            root.Insert(L"MoveCursorAcrossDisplays", winrt::Windows::Data::Json::JsonValue::CreateBooleanValue(config.MoveCursorAcrossDisplays));
            root.Insert(L"SnapEdgeMarginTop", winrt::Windows::Data::Json::JsonValue::CreateNumberValue(config.SnapEdgeMarginTop));
            root.Insert(L"SnapEdgeMarginBottom", winrt::Windows::Data::Json::JsonValue::CreateNumberValue(config.SnapEdgeMarginBottom));
            root.Insert(L"SnapEdgeMarginLeft", winrt::Windows::Data::Json::JsonValue::CreateNumberValue(config.SnapEdgeMarginLeft));
            root.Insert(L"SnapEdgeMarginRight", winrt::Windows::Data::Json::JsonValue::CreateNumberValue(config.SnapEdgeMarginRight));
            root.Insert(L"CornerSnapAreaSize", winrt::Windows::Data::Json::JsonValue::CreateNumberValue(config.CornerSnapAreaSize));
            root.Insert(L"SnapModifiers", winrt::Windows::Data::Json::JsonValue::CreateNumberValue(config.SnapModifiers));
            root.Insert(L"LogLevel", winrt::Windows::Data::Json::JsonValue::CreateNumberValue(config.LogLevel));
            root.Insert(L"LogToFile", winrt::Windows::Data::Json::JsonValue::CreateBooleanValue(config.LogToFile));
            root.Insert(L"LogFilePath", winrt::Windows::Data::Json::JsonValue::CreateStringValue(config.LogFilePath));
            root.Insert(L"MaxLogFileSize", winrt::Windows::Data::Json::JsonValue::CreateNumberValue(config.MaxLogFileSize));
            root.Insert(L"Theme", winrt::Windows::Data::Json::JsonValue::CreateStringValue(config.Theme));
            root.Insert(L"Language", winrt::Windows::Data::Json::JsonValue::CreateStringValue(config.Language));
            root.Insert(L"CheckForUpdates", winrt::Windows::Data::Json::JsonValue::CreateBooleanValue(config.CheckForUpdates));
            root.Insert(L"MinimumWindowWidth", winrt::Windows::Data::Json::JsonValue::CreateNumberValue(config.MinimumWindowWidth));
            root.Insert(L"MinimumWindowHeight", winrt::Windows::Data::Json::JsonValue::CreateNumberValue(config.MinimumWindowHeight));

            winrt::Windows::Data::Json::JsonArray ignoredApps;
            for (auto const& app : config.IgnoredApps)
                ignoredApps.Append(winrt::Windows::Data::Json::JsonValue::CreateStringValue(app));
            root.Insert(L"IgnoredApps", ignoredApps);

            winrt::Windows::Data::Json::JsonObject shortcutsObj;
            for (auto const& [name, sc] : config.Shortcuts)
            {
                winrt::Windows::Data::Json::JsonObject item;
                item.Insert(L"Enabled", winrt::Windows::Data::Json::JsonValue::CreateBooleanValue(sc.Enabled));
                item.Insert(L"KeyCode", winrt::Windows::Data::Json::JsonValue::CreateNumberValue(sc.KeyCode));
                item.Insert(L"ModifierFlags", winrt::Windows::Data::Json::JsonValue::CreateNumberValue(sc.ModifierFlags));
                shortcutsObj.Insert(name, item);
            }
            root.Insert(L"Shortcuts", shortcutsObj);

            winrt::Windows::Data::Json::JsonObject history;
            history.Insert(L"Enabled", winrt::Windows::Data::Json::JsonValue::CreateBooleanValue(config.History.Enabled));
            history.Insert(L"MaxHistoryCount", winrt::Windows::Data::Json::JsonValue::CreateNumberValue(config.History.MaxHistoryCount));
            history.Insert(L"EnableUndo", winrt::Windows::Data::Json::JsonValue::CreateBooleanValue(config.History.EnableUndo));
            root.Insert(L"History", history);

            std::wofstream file(m_configPath, std::ios::trunc);
            if (!file.is_open())
            {
                Logger::Instance().Error(L"ConfigService", L"无法保存配置文件");
                return;
            }
            file << root.Stringify().c_str();
            file.close();
        }
        catch (...)
        {
            Logger::Instance().Error(L"ConfigService", L"保存配置异常");
            return;
        }

        Logger::Instance().Info(L"ConfigService", L"配置已保存");
        if (ConfigChanged)
        {
            ConfigChanged(config);
        }
    }

    std::wstring ConfigService::ExportToFile(const std::wstring& filePath)
    {
        auto target = filePath.empty()
            ? (std::filesystem::path(m_configPath).parent_path() / L"config.export.json").wstring()
            : filePath;
        try
        {
            std::filesystem::copy_file(m_configPath, target, std::filesystem::copy_options::overwrite_existing);
            return target;
        }
        catch (...)
        {
            return L"";
        }
    }

    bool ConfigService::ImportFromFile(const std::wstring& filePath)
    {
        auto source = filePath.empty()
            ? (std::filesystem::path(m_configPath).parent_path() / L"config.import.json").wstring()
            : filePath;
        try
        {
            if (!std::filesystem::exists(source)) return false;
            std::filesystem::copy_file(source, m_configPath, std::filesystem::copy_options::overwrite_existing);
            return true;
        }
        catch (...)
        {
            return false;
        }
    }

    std::map<std::wstring, ShortcutConfig> ConfigService::GetDefaultShortcuts()
    {
        const uint32_t MOD_CONTROL = 0x0002;
        const uint32_t MOD_ALT = 0x0001;
        const uint32_t MOD_SHIFT = 0x0004;
        const uint32_t MOD_WIN = 0x0008;
        const uint32_t ctrlAlt = MOD_CONTROL | MOD_ALT;
        const uint32_t ctrlAltShift = ctrlAlt | MOD_SHIFT;
        const uint32_t ctrlAltWin = ctrlAlt | MOD_WIN;

        return {
            { L"LeftHalf", { true, 0x25, ctrlAlt } },
            { L"RightHalf", { true, 0x27, ctrlAlt } },
            { L"TopHalf", { true, 0x26, ctrlAlt } },
            { L"BottomHalf", { true, 0x28, ctrlAlt } },
            { L"CenterHalf", { false, 0, 0 } },
            { L"TopLeft", { true, 0x55, ctrlAlt } },
            { L"TopRight", { true, 0x49, ctrlAlt } },
            { L"BottomLeft", { true, 0x4A, ctrlAlt } },
            { L"BottomRight", { true, 0x4B, ctrlAlt } },
            { L"FirstThird", { true, 0x44, ctrlAlt } },
            { L"CenterThird", { true, 0x46, ctrlAlt } },
            { L"LastThird", { true, 0x47, ctrlAlt } },
            { L"FirstTwoThirds", { true, 0x45, ctrlAlt } },
            { L"CenterTwoThirds", { true, 0x52, ctrlAlt } },
            { L"LastTwoThirds", { true, 0x54, ctrlAlt } },
            { L"Maximize", { true, 0x0D, ctrlAlt } },
            { L"AlmostMaximize", { false, 0, 0 } },
            { L"MaximizeHeight", { true, 0x26, ctrlAltShift } },
            { L"Larger", { true, 0xBB, ctrlAlt } },
            { L"Smaller", { true, 0xBD, ctrlAlt } },
            { L"Center", { true, 0x43, ctrlAlt } },
            { L"Restore", { true, 0x08, ctrlAlt } },
            { L"PreviousDisplay", { true, 0x25, ctrlAltWin } },
            { L"NextDisplay", { true, 0x27, ctrlAltWin } },
            { L"FirstFourth", { false, 0, 0 } },
            { L"SecondFourth", { false, 0, 0 } },
            { L"ThirdFourth", { false, 0, 0 } },
            { L"LastFourth", { false, 0, 0 } },
            { L"FirstThreeFourths", { false, 0, 0 } },
            { L"CenterThreeFourths", { false, 0, 0 } },
            { L"LastThreeFourths", { false, 0, 0 } },
            { L"TopLeftSixth", { false, 0, 0 } },
            { L"TopCenterSixth", { false, 0, 0 } },
            { L"TopRightSixth", { false, 0, 0 } },
            { L"BottomLeftSixth", { false, 0, 0 } },
            { L"BottomCenterSixth", { false, 0, 0 } },
            { L"BottomRightSixth", { false, 0, 0 } },
            { L"MoveLeft", { false, 0, 0 } },
            { L"MoveRight", { false, 0, 0 } },
            { L"MoveUp", { false, 0, 0 } },
            { L"MoveDown", { false, 0, 0 } },
            { L"Undo", { true, 0x5A, ctrlAlt } },
            { L"Redo", { true, 0x5A, ctrlAltShift } },
        };
    }
}
