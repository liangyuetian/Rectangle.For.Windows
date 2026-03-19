#pragma once
#include "pch.h"
#include "Views/Controls/ShortcutCaptureDialog.g.h"

namespace winrt::Rectangle::Views::Controls
{
    struct CapturedShortcut
    {
        std::wstring DisplayText;
        int32_t KeyCode;
        uint32_t ModifierFlags;
    };

    class ShortcutCaptureDialog : public winrt::Microsoft::UI::Xaml::Controls::ContentDialog
    {
    public:
        ShortcutCaptureDialog();
        ~ShortcutCaptureDialog() = default;

        CapturedShortcut* GetCapturedShortcut() { return m_capturedShortcut.get(); }
        void ClearCapturedShortcut();

        void OnKeyPressed(int32_t keyCode, uint32_t modifierFlags);

    private:
        std::wstring GetModifierString(uint32_t modifierFlags);
        std::wstring GetKeyString(int32_t keyCode);

        std::unique_ptr<CapturedShortcut> m_capturedShortcut;
    };
}