#include "pch.h"
#include "Core/WindowHistory.h"

namespace winrt::Rectangle::Core
{
    void WindowHistory::SaveRestoreRect(int64_t hwnd, int32_t x, int32_t y, int32_t width, int32_t height)
    {
        std::unique_lock lock(m_mutex);
        auto& data = m_windowData[hwnd];
        data.RestoreRect = WindowRect(x, y, width, height);
        data.HasRestoreRect = true;
    }

    bool WindowHistory::HasRestoreRect(int64_t hwnd) const
    {
        std::shared_lock lock(m_mutex);
        auto it = m_windowData.find(hwnd);
        return it != m_windowData.end() && it->second.HasRestoreRect;
    }

    WindowRect WindowHistory::GetRestoreRect(int64_t hwnd) const
    {
        std::shared_lock lock(m_mutex);
        auto it = m_windowData.find(hwnd);
        if (it != m_windowData.end() && it->second.HasRestoreRect)
        {
            return it->second.RestoreRect;
        }
        return WindowRect{};
    }

    void WindowHistory::ClearRestoreRect(int64_t hwnd)
    {
        std::unique_lock lock(m_mutex);
        auto it = m_windowData.find(hwnd);
        if (it != m_windowData.end())
        {
            it->second.HasRestoreRect = false;
        }
    }

    void WindowHistory::MarkAsProgramAdjusted(int64_t hwnd)
    {
        std::unique_lock lock(m_mutex);
        m_windowData[hwnd].LastProgramAdjustTime = std::chrono::steady_clock::now();
    }

    bool WindowHistory::IsWindowMovedExternally(int64_t hwnd, int32_t x, int32_t y, int32_t width, int32_t height) const
    {
        std::shared_lock lock(m_mutex);
        auto it = m_windowData.find(hwnd);
        if (it == m_windowData.end() || !it->second.HasLastAction)
        {
            return false;
        }

        const auto& last = it->second.LastAction;
        const int32_t tolerance = 2;

        bool xDiffers = std::abs(x - last.X) > tolerance;
        bool yDiffers = std::abs(y - last.Y) > tolerance;

        return xDiffers || yDiffers;
    }

    void WindowHistory::RemoveLastAction(int64_t hwnd)
    {
        std::unique_lock lock(m_mutex);
        auto it = m_windowData.find(hwnd);
        if (it != m_windowData.end())
        {
            it->second.HasLastAction = false;
        }
    }

    bool WindowHistory::TryGetLastAction(int64_t hwnd, WindowHistoryRecord& record) const
    {
        std::shared_lock lock(m_mutex);
        auto it = m_windowData.find(hwnd);
        if (it != m_windowData.end() && it->second.HasLastAction)
        {
            record = it->second.LastAction;
            return true;
        }
        return false;
    }

    void WindowHistory::RecordAction(int64_t hwnd, WindowAction action, int32_t x, int32_t y, int32_t width, int32_t height)
    {
        std::unique_lock lock(m_mutex);
        auto& data = m_windowData[hwnd];
        auto& last = data.LastAction;

        if (data.HasLastAction && last.Action == action && last.X == x && last.Y == y && last.Width == width && last.Height == height)
        {
            last.Count++;
        }
        else
        {
            last = WindowHistoryRecord{ action, x, y, width, height, 1, std::chrono::steady_clock::now() };
            data.HasLastAction = true;
        }
    }

    void WindowHistory::Cleanup()
    {
        std::unique_lock lock(m_mutex);
        auto now = std::chrono::steady_clock::now();
        const auto expiration = std::chrono::minutes(60);

        for (auto it = m_windowData.begin(); it != m_windowData.end(); )
        {
            if (it->second.HasLastAction && (now - it->second.LastAction.Time) > expiration)
            {
                it = m_windowData.erase(it);
            }
            else
            {
                ++it;
            }
        }
    }
}
