#include "pch.h"
#include "Services/OperationHistoryManager.h"
#include "Services/Logger.h"
#include "Services/Win32WindowService.h"

namespace winrt::Rectangle::Services
{
    OperationHistoryManager::OperationHistoryManager()
    {
        Logger::Instance().Info(L"OperationHistoryManager", L"OperationHistoryManager initialized");
    }

    OperationHistoryManager::~OperationHistoryManager() = default;

    void OperationHistoryManager::RecordOperation(Core::WindowAction action, int64_t hwnd,
        const Core::WindowRect& oldRect, const Core::WindowRect& newRect,
        const std::wstring& processName)
    {
        if (m_currentIndex < static_cast<int32_t>(m_history.size()) - 1)
        {
            m_history.resize(m_currentIndex + 1);
        }

        OperationRecord record;
        record.Action = action;
        record.Hwnd = hwnd;
        record.OldRect = oldRect;
        record.NewRect = newRect;
        record.ProcessName = processName;

        m_history.push_back(record);
        m_currentIndex++;

        if (static_cast<int32_t>(m_history.size()) > m_maxHistoryCount)
        {
            m_history.erase(m_history.begin());
            m_currentIndex--;
        }

        Logger::Instance().Debug(L"OperationHistoryManager", L"Recorded operation: " + Core::ToString(action));
    }

    bool OperationHistoryManager::CanUndo() const
    {
        return m_currentIndex >= 0;
    }

    bool OperationHistoryManager::CanRedo() const
    {
        return m_currentIndex < static_cast<int32_t>(m_history.size()) - 1;
    }

    void OperationHistoryManager::Undo()
    {
        if (!CanUndo()) return;

        auto& record = m_history[m_currentIndex];
        Win32WindowService win32;
        win32.SetWindowRect(record.Hwnd, record.OldRect.X, record.OldRect.Y, record.OldRect.Width, record.OldRect.Height);

        Logger::Instance().Info(L"OperationHistoryManager", L"Undo: " + Core::ToString(record.Action));
        m_currentIndex--;
    }

    void OperationHistoryManager::Redo()
    {
        if (!CanRedo()) return;

        m_currentIndex++;
        auto& record = m_history[m_currentIndex];
        Win32WindowService win32;
        win32.SetWindowRect(record.Hwnd, record.NewRect.X, record.NewRect.Y, record.NewRect.Width, record.NewRect.Height);

        Logger::Instance().Info(L"OperationHistoryManager", L"Redo: " + Core::ToString(record.Action));
    }

    void OperationHistoryManager::SetMaxHistoryCount(int32_t count)
    {
        m_maxHistoryCount = count;
    }
}
