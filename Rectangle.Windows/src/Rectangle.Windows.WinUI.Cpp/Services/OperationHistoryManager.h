#pragma once
#include "pch.h"

namespace winrt::Rectangle::Services
{
    class OperationHistoryManager
    {
    public:
        OperationHistoryManager();
        ~OperationHistoryManager();

        void RecordOperation(Core::WindowAction action, int64_t hwnd,
            const Core::WindowRect& oldRect, const Core::WindowRect& newRect,
            const std::wstring& processName);

        bool CanUndo() const;
        bool CanRedo() const;

        void Undo();
        void Redo();

        void SetMaxHistoryCount(int32_t count);

    private:
        struct OperationRecord
        {
            Core::WindowAction Action;
            int64_t Hwnd;
            Core::WindowRect OldRect;
            Core::WindowRect NewRect;
            std::wstring ProcessName;
        };

        std::vector<OperationRecord> m_history;
        int32_t m_currentIndex{ -1 };
        int32_t m_maxHistoryCount{ 50 };
    };
}
