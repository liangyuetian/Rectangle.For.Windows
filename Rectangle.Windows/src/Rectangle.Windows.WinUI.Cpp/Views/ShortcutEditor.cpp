#include "pch.h"
#include "Views/ShortcutEditor.h"
#include "Services/Logger.h"

namespace winrt::Rectangle::Views
{
    ShortcutEditor::ShortcutEditor()
    {
        m_shortcutText = L"记录快捷键";
        Logger::Instance().Info(L"ShortcutEditor", L"ShortcutEditor constructed");
    }

    void ShortcutEditor::SetAction(const std::wstring& action)
    {
        m_action = action;
    }

    void ShortcutEditor::SetDisplayName(const std::wstring& name)
    {
        m_displayName = name;
    }

    void ShortcutEditor::SetIconGlyph(const std::wstring& glyph)
    {
        m_iconGlyph = glyph;
    }

    void ShortcutEditor::SetShortcutText(const std::wstring& text)
    {
        m_shortcutText = text;
    }

    void ShortcutEditor::SetEnabled(bool enabled)
    {
        m_enabled = enabled;
    }

    void ShortcutEditor::OnShortcutChanged(ShortcutChangedHandler handler)
    {
        m_onShortcutChanged = handler;
    }

    void ShortcutEditor::OnShortcutCleared(ShortcutClearedHandler handler)
    {
        m_onShortcutCleared = handler;
    }

    void ShortcutEditor::NotifyShortcutChanged()
    {
        if (m_onShortcutChanged)
        {
            ShortcutChangedEventArgs args;
            args.Action = m_action;
            args.KeyCode = m_keyCode;
            args.ModifierFlags = m_modifierFlags;
            m_onShortcutChanged(args);
        }
    }

    void ShortcutEditor::NotifyShortcutCleared()
    {
        m_shortcutText = L"记录快捷键";
        if (m_onShortcutCleared)
        {
            ShortcutClearedEventArgs args;
            args.Action = m_action;
            m_onShortcutCleared(args);
        }
    }

    void ShortcutEditor::Show()
    {
        m_isVisible = true;
    }

    void ShortcutEditor::Hide()
    {
        m_isVisible = false;
    }
}