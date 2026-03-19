#include "pch.h"
#include "Services/ConfigService.h"
#include "Services/Logger.h"
#include <shlobj.h>

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
        std::wifstream file(m_configPath);
        if (!file.is_open())
        {
            Logger::Instance().Info(L"ConfigService", L"配置文件不存在，使用默认配置");
            return CreateDefaultConfig();
        }

        try
        {
            std::wstringstream buffer;
            buffer << file.rdbuf();
            std::wstring json = buffer.str();

            // Simple JSON parsing - in production would use a proper JSON library
            // For now, just return default
            file.close();
            return CreateDefaultConfig();
        }
        catch (const std::exception& ex)
        {
            Logger::Instance().Error(L"ConfigService", L"读取配置文件失败: " + std::to_wstring(errno));
            return CreateDefaultConfig();
        }
    }

    void ConfigService::Save(const AppConfig& config)
    {
        std::wofstream file(m_configPath);
        if (!file.is_open())
        {
            Logger::Instance().Error(L"ConfigService", L"无法保存配置文件");
            return;
        }

        // Simple JSON-like output - in production would use proper JSON serialization
        file << L"{\n";
        file << L"  \"GapSize\": " << config.GapSize << L",\n";
        file << L"  \"IgnoredApps\": [";
        for (size_t i = 0; i < config.IgnoredApps.size(); ++i)
        {
            file << L"\"" << config.IgnoredApps[i] << L"\"";
            if (i < config.IgnoredApps.size() - 1) file << L", ";
        }
        file << L"]\n";
        file << L"}\n";
        file.close();

        Logger::Instance().Info(L"ConfigService", L"配置已保存");
        ConfigChanged(config);
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
