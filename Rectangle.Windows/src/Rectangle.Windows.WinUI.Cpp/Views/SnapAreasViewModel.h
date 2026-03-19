#pragma once
#include "pch.h"
#include <functional>
#include <string>

namespace winrt::Rectangle::Views
{
    class SnapAreasViewModel
    {
    public:
        SnapAreasViewModel();
        ~SnapAreasViewModel() = default;

        void LoadSettings();
        void SaveSettings();

        bool GetDragToSnap() const { return m_dragToSnap; }
        void SetDragToSnap(bool value);

        bool GetRestoreSizeOnSnapEnd() const { return m_restoreSizeOnSnapEnd; }
        void SetRestoreSizeOnSnapEnd(bool value);

        bool GetSnapAnimation() const { return m_snapAnimation; }
        void SetSnapAnimation(bool value);

        bool GetHapticFeedback() const { return m_hapticFeedback; }
        void SetHapticFeedback(bool value);

        int32_t GetGapSize() const { return m_gapSize; }
        void SetGapSize(int32_t value);

        void OnChanged(std::function<void()> handler) { m_onChanged = handler; }

    private:
        void* m_configService{ nullptr };

        bool m_dragToSnap{ true };
        bool m_restoreSizeOnSnapEnd{ true };
        bool m_snapAnimation{ false };
        bool m_hapticFeedback{ false };
        int32_t m_gapSize{ 0 };

        std::function<void()> m_onChanged;
    };
}