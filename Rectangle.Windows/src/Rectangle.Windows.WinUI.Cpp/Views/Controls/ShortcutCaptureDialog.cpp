#include "pch.h"
#include "Views/Controls/ShortcutCaptureDialog.h"
#include "Services/Logger.h"

namespace winrt::Rectangle::Views::Controls
{
    ShortcutCaptureDialog::ShortcutCaptureDialog()
    {
        Logger::Instance().Info(L"ShortcutCaptureDialog", L"ShortcutCaptureDialog constructed");
    }

    void ShortcutCaptureDialog::ClearCapturedShortcut()
    {
        m_capturedShortcut.reset();
    }

    void ShortcutCaptureDialog::OnKeyPressed(int32_t keyCode, uint32_t modifierFlags)
    {
        std::wstring modifierStr = GetModifierString(modifierFlags);
        std::wstring keyStr = GetKeyString(keyCode);

        if (!keyStr.empty())
        {
            m_capturedShortcut = std::make_unique<CapturedShortcut>();
            m_capturedShortcut->KeyCode = keyCode;
            m_capturedShortcut->ModifierFlags = modifierFlags;
            m_capturedShortcut->DisplayText = modifierStr + keyStr;

            Logger::Instance().Info(L"ShortcutCaptureDialog",
                L"Captured shortcut: " + m_capturedShortcut->DisplayText);
        }
    }

    std::wstring ShortcutCaptureDialog::GetModifierString(uint32_t modifierFlags)
    {
        std::wstring result;
        if (modifierFlags & 0x0001) result += L"Ctrl+";
        if (modifierFlags & 0x0002) result += L"Alt+";
        if (modifierFlags & 0x0004) result += L"Shift+";
        if (modifierFlags & 0x0008) result += L"Win+";
        return result;
    }

    std::wstring ShortcutCaptureDialog::GetKeyString(int32_t keyCode)
    {
        switch (keyCode)
        {
        case 0x41: return L"A";
        case 0x42: return L"B";
        case 0x43: return L"C";
        case 0x44: return L"D";
        case 0x45: return L"E";
        case 0x46: return L"F";
        case 0x47: return L"G";
        case 0x48: return L"H";
        case 0x49: return L"I";
        case 0x4A: return L"J";
        case 0x4B: return L"K";
        case 0x4C: return L"L";
        case 0x4D: return L"M";
        case 0x4E: return L"N";
        case 0x4F: return L"O";
        case 0x50: return L"P";
        case 0x51: return L"Q";
        case 0x52: return L"R";
        case 0x53: return L"S";
        case 0x54: return L"T";
        case 0x55: return L"U";
        case 0x56: return L"V";
        case 0x57: return L"W";
        case 0x58: return L"X";
        case 0x59: return L"Y";
        case 0x5A: return L"Z";
        case 0x30: return L"0";
        case 0x31: return L"1";
        case 0x32: return L"2";
        case 0x33: return L"3";
        case 0x34: return L"4";
        case 0x35: return L"5";
        case 0x36: return L"6";
        case 0x37: return L"7";
        case 0x38: return L"8";
        case 0x39: return L"9";
        case 0x70: return L"F1";
        case 0x71: return L"F2";
        case 0x72: return L"F3";
        case 0x73: return L"F4";
        case 0x74: return L"F5";
        case 0x75: return L"F6";
        case 0x76: return L"F7";
        case 0x77: return L"F8";
        case 0x78: return L"F9";
        case 0x79: return L"F10";
        case 0x7A: return L"F11";
        case 0x7B: return L"F12";
        case 0x26: return L"Up";
        case 0x28: return L"Down";
        case 0x25: return L"Left";
        case 0x27: return L"Right";
        case 0x0D: return L"Enter";
        case 0x1B: return L"Escape";
        case 0x20: return L"Space";
        case 0x09: return L"Tab";
        case 0x08: return L"Backspace";
        case 0x2D: return L"Insert";
        case 0x2E: return L"Delete";
        case 0x21: return L"PageUp";
        case 0x22: return L"PageDown";
        case 0x23: return L"End";
        case 0x24: return L"Home";
        case 0x6A: return L"*";
        case 0x6B: return L"+";
        case 0x6D: return L"-";
        case 0x6E: return L".";
        case 0x6F: return L"/";
        default: return std::to_wstring(keyCode);
        }
    }
}