#pragma once
#include "pch.h"
#include <functional>
#include <memory>
#include <string>
#include <vector>

namespace winrt::Rectangle::Services
{
    struct WindowPositionInfo
    {
        std::wstring ProcessName;
        std::wstring WindowTitle;
        int32_t X{ 0 };
        int32_t Y{ 0 };
        int32_t Width{ 0 };
        int32_t Height{ 0 };
    };

    struct WindowLayout
    {
        std::wstring Id;
        std::wstring Name;
        std::wstring CreatedAt;
        std::vector<WindowPositionInfo> Windows;
    };

    class LayoutManager
    {
    public:
        LayoutManager();
        ~LayoutManager() = default;

        void Initialize();
        void Shutdown();

        std::wstring SaveCurrentLayout(const std::wstring& name);
        bool RestoreLayout(const std::wstring& layoutId);
        bool DeleteLayout(const std::wstring& layoutId);
        std::vector<WindowLayout> GetAllLayouts();
        WindowLayout* GetLayout(const std::wstring& layoutId);

    private:
        std::wstring GetLayoutsFilePath();
        void SaveLayoutsToFile();
        void LoadLayoutsFromFile();

        std::vector<WindowLayout> m_layouts;
        std::wstring m_layoutsFilePath;
        bool m_isInitialized{ false };
    };
}