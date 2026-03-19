#include "pch.h"
#include "Views/SnapAreasViewModel.h"
#include "Services/Logger.h"
#include "Services/ConfigService.h"

namespace winrt::Rectangle::Views
{
    SnapAreasViewModel::SnapAreasViewModel()
    {
        Logger::Instance().Info(L"SnapAreasViewModel", L"SnapAreasViewModel constructed");
    }

    void SnapAreasViewModel::LoadSettings()
    {
        Logger::Instance().Info(L"SnapAreasViewModel", L"Loading settings");

        Services::ConfigService configService;
        auto config = configService.Load();

        m_dragToSnap = config.SnapAreas.DragToSnap;
        m_restoreSizeOnSnapEnd = config.SnapAreas.RestoreSizeOnSnapEnd;
        m_snapAnimation = config.SnapAreas.SnapAnimation;
        m_hapticFeedback = config.SnapAreas.HapticFeedback;
        m_gapSize = config.GapSize;

        if (m_onChanged)
        {
            m_onChanged();
        }
    }

    void SnapAreasViewModel::SaveSettings()
    {
        Logger::Instance().Info(L"SnapAreasViewModel", L"Saving settings");

        Services::ConfigService configService;
        auto config = configService.Load();

        config.SnapAreas.DragToSnap = m_dragToSnap;
        config.SnapAreas.RestoreSizeOnSnapEnd = m_restoreSizeOnSnapEnd;
        config.SnapAreas.SnapAnimation = m_snapAnimation;
        config.SnapAreas.HapticFeedback = m_hapticFeedback;
        config.GapSize = m_gapSize;

        configService.Save(config);
    }

    void SnapAreasViewModel::SetDragToSnap(bool value)
    {
        if (m_dragToSnap != value)
        {
            m_dragToSnap = value;
            SaveSettings();
        }
    }

    void SnapAreasViewModel::SetRestoreSizeOnSnapEnd(bool value)
    {
        if (m_restoreSizeOnSnapEnd != value)
        {
            m_restoreSizeOnSnapEnd = value;
            SaveSettings();
        }
    }

    void SnapAreasViewModel::SetSnapAnimation(bool value)
    {
        if (m_snapAnimation != value)
        {
            m_snapAnimation = value;
            SaveSettings();
        }
    }

    void SnapAreasViewModel::SetHapticFeedback(bool value)
    {
        if (m_hapticFeedback != value)
        {
            m_hapticFeedback = value;
            SaveSettings();
        }
    }

    void SnapAreasViewModel::SetGapSize(int32_t value)
    {
        if (m_gapSize != value)
        {
            m_gapSize = value;
            SaveSettings();
        }
    }
}