#pragma once
#include "pch.h"
#include <functional>
#include <string>

namespace winrt::Rectangle::Views
{
    struct ShortcutChangedEventArgs
    {
        std::wstring Action;
        int32_t KeyCode;
        uint32_t ModifierFlags;
    };

    struct ShortcutClearedEventArgs
    {
        std::wstring Action;
    };

    class ShortcutEditor
    {
    public:
        using ShortcutChangedHandler = std::function<void(const ShortcutChangedEventArgs&)>;
        using ShortcutClearedHandler = std::function<void(const ShortcutClearedEventArgs&)>;

        ShortcutEditor();
        ~ShortcutEditor() = default;

        void SetAction(const std::wstring& action);
        void SetDisplayName(const std::wstring& name);
        void SetIconGlyph(const std::wstring& glyph);
        void SetShortcutText(const std::wstring& text);
        void SetEnabled(bool enabled);

        std::wstring GetAction() const { return m_action; }
        std::wstring GetDisplayName() const { return m_displayName; }
        std::wstring GetIconGlyph() const { return m_iconGlyph; }
        std::wstring GetShortcutText() const { return m_shortcutText; }
        int32_t GetKeyCode() const { return m_keyCode; }
        uint32_t GetModifierFlags() const { return m_modifierFlags; }
        bool IsEnabled() const { return m_enabled; }

        void OnShortcutChanged(ShortcutChangedHandler handler);
        void OnShortcutCleared(ShortcutClearedHandler handler);

        void NotifyShortcutChanged();
        void NotifyShortcutCleared();

        void Show();
        void Hide();

    private:
        std::wstring m_action;
        std::wstring m_displayName;
        std::wstring m_iconGlyph;
        std::wstring m_shortcutText;
        int32_t m_keyCode{ 0 };
        uint32_t m_modifierFlags{ 0 };
        bool m_enabled{ true };
        bool m_isVisible{ true };

        ShortcutChangedHandler m_onShortcutChanged;
        ShortcutClearedHandler m_onShortcutCleared;
    };
}
