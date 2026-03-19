#pragma once
#include "pch.h"
#include <functional>
#include <map>
#include <string>
#include <vector>

namespace winrt::Rectangle::ViewModels
{
    template<typename T>
    class ObservableValue
    {
    public:
        using ValueChangedHandler = std::function<void(const T& oldValue, const T& newValue)>;

        ObservableValue() = default;
        ObservableValue(const T& initialValue) : m_value(initialValue) {}

        const T& Get() const { return m_value; }
        void Set(const T& value)
        {
            if (m_value == value) return;
            T old = m_value;
            m_value = value;
            if (m_onChanged) m_onChanged(old, m_value);
        }

        void OnChanged(ValueChangedHandler handler) { m_onChanged = handler; }

        operator const T& () const { return m_value; }
        const T& operator()() const { return m_value; }

    private:
        T m_value{};
        ValueChangedHandler m_onChanged;
    };

    class ShortcutItem
    {
    public:
        std::wstring Action;
        std::wstring DisplayName;
        std::wstring IconGlyph;
        bool IsEnabled{ true };
        int32_t KeyCode{ 0 };
        uint32_t ModifierFlags{ 0 };

        std::wstring GetShortcutText() const;
    };

    class SettingsViewModel
    {
    public:
        SettingsViewModel();
        ~SettingsViewModel() = default;

        void LoadSettings();
        void SaveSettings();

        ObservableValue<bool> LaunchOnLogin;
        ObservableValue<int32_t> GapSize;
        ObservableValue<int32_t> HorizontalSplitRatio;
        ObservableValue<int32_t> VerticalSplitRatio;
        ObservableValue<bool> LogToFile;
        ObservableValue<int32_t> LogLevel;
        ObservableValue<std::wstring> CurrentTheme;
        ObservableValue<int32_t> LanguageIndex;

        std::vector<ShortcutItem>& GetHalfScreenShortcuts() { return m_halfScreenShortcuts; }
        std::vector<ShortcutItem>& GetCornerShortcuts() { return m_cornerShortcuts; }
        std::vector<ShortcutItem>& GetThirdShortcuts() { return m_thirdShortcuts; }
        std::vector<ShortcutItem>& GetFourthShortcuts() { return m_fourthShortcuts; }
        std::vector<ShortcutItem>& GetSixthShortcuts() { return m_sixthShortcuts; }
        std::vector<ShortcutItem>& GetMaximizeShortcuts() { return m_maximizeShortcuts; }
        std::vector<ShortcutItem>& GetResizeShortcuts() { return m_resizeShortcuts; }
        std::vector<ShortcutItem>& GetMoveShortcuts() { return m_moveShortcuts; }
        std::vector<ShortcutItem>& GetDisplayShortcuts() { return m_displayShortcuts; }
        std::vector<ShortcutItem>& GetOtherShortcuts() { return m_otherShortcuts; }

    private:
        void InitializeDefaultShortcuts();
        void LoadShortcutsFromConfig();

        std::vector<ShortcutItem> m_halfScreenShortcuts;
        std::vector<ShortcutItem> m_cornerShortcuts;
        std::vector<ShortcutItem> m_thirdShortcuts;
        std::vector<ShortcutItem> m_fourthShortcuts;
        std::vector<ShortcutItem> m_sixthShortcuts;
        std::vector<ShortcutItem> m_maximizeShortcuts;
        std::vector<ShortcutItem> m_resizeShortcuts;
        std::vector<ShortcutItem> m_moveShortcuts;
        std::vector<ShortcutItem> m_displayShortcuts;
        std::vector<ShortcutItem> m_otherShortcuts;

        void* m_configService{ nullptr };
    };
}
