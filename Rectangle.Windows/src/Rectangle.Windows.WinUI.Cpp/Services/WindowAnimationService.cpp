#include "pch.h"
#include "Services/WindowAnimationService.h"
#include "Services/Logger.h"
#include <thread>
#include <cmath>
#include <Windows.h>

namespace winrt::Rectangle::Services
{
    WindowAnimationService::WindowAnimationService()
    {
        m_defaultConfig.Type = WindowAnimationType::Slide;
        m_defaultConfig.Easing = EasingType::EaseOutQuad;
        m_defaultConfig.DurationMs = 200;
        m_defaultConfig.Enable = true;
        Logger::Instance().Info(L"WindowAnimationService", L"WindowAnimationService constructed");
    }

    void WindowAnimationService::Initialize()
    {
        m_isInitialized = true;
        Logger::Instance().Info(L"WindowAnimationService", L"WindowAnimationService initialized");
    }

    void WindowAnimationService::Shutdown()
    {
        m_isInitialized = false;
        Logger::Instance().Info(L"WindowAnimationService", L"WindowAnimationService shutdown");
    }

    bool WindowAnimationService::AnimateWindow(int64_t hwnd, const WindowPosition& targetPos, WindowAnimationType type)
    {
        WindowPosition fromPos = targetPos;
        fromPos.X -= 50;
        return AnimateWindow(hwnd, fromPos, targetPos, { type, EasingType::EaseOutQuad, 200, true });
    }

    bool WindowAnimationService::AnimateWindow(int64_t hwnd, const WindowPosition& fromPos, const WindowPosition& toPos, const WindowAnimationConfig& config)
    {
        if (!m_isInitialized || !config.Enable || hwnd == 0)
        {
            return false;
        }

        return PerformAnimation(hwnd, fromPos, toPos, config);
    }

    void WindowAnimationService::SetDefaultConfig(const WindowAnimationConfig& config)
    {
        m_defaultConfig = config;
    }

    WindowAnimationConfig WindowAnimationService::GetDefaultConfig() const
    {
        return m_defaultConfig;
    }

    bool WindowAnimationService::PerformAnimation(int64_t hwnd, const WindowPosition& from, const WindowPosition& to, const WindowAnimationConfig& config)
    {
        const int32_t frameCount = static_cast<int32_t>(config.DurationMs / 16);
        double t = 0.0;
        double dt = 1.0 / frameCount;

        for (int32_t i = 0; i <= frameCount; ++i)
        {
            double easedT = Ease(config.Easing, t);

            WindowPosition currentPos;
            currentPos.X = static_cast<int32_t>(from.X + (to.X - from.X) * easedT);
            currentPos.Y = static_cast<int32_t>(from.Y + (to.Y - from.Y) * easedT);
            currentPos.Width = static_cast<int32_t>(from.Width + (to.Width - from.Width) * easedT);
            currentPos.Height = static_cast<int32_t>(from.Height + (to.Height - from.Height) * easedT);

            Logger::Instance().Debug(L"WindowAnimationService",
                L"Animating: " + std::to_wstring(currentPos.X) + L"," + std::to_wstring(currentPos.Y));

            SetWindowPos(
                reinterpret_cast<HWND>(hwnd),
                nullptr,
                currentPos.X,
                currentPos.Y,
                currentPos.Width,
                currentPos.Height,
                SWP_NOZORDER | SWP_NOACTIVATE);

            t += dt;
            std::this_thread::sleep_for(std::chrono::milliseconds(16));
        }

        return true;
    }

    double WindowAnimationService::Ease(EasingType type, double t)
    {
        switch (type)
        {
        case EasingType::Linear: return t;
        case EasingType::EaseInQuad: return EaseInQuad(t);
        case EasingType::EaseOutQuad: return EaseOutQuad(t);
        case EasingType::EaseInOutQuad: return EaseInOutQuad(t);
        case EasingType::EaseInCubic: return EaseInCubic(t);
        case EasingType::EaseOutCubic: return EaseOutCubic(t);
        case EasingType::EaseInOutCubic: return EaseInOutCubic(t);
        case EasingType::EaseOutBack: return EaseOutBack(t);
        case EasingType::EaseOutElastic: return EaseOutElastic(t);
        default: return t;
        }
    }

    double WindowAnimationService::EaseInQuad(double t) { return t * t; }
    double WindowAnimationService::EaseOutQuad(double t) { return t * (2 - t); }
    double WindowAnimationService::EaseInOutQuad(double t) { return t < 0.5 ? 2 * t * t : -1 + (4 - 2 * t) * t; }
    double WindowAnimationService::EaseInCubic(double t) { return t * t * t; }
    double WindowAnimationService::EaseOutCubic(double t) { return (--t) * t * t + 1; }
    double WindowAnimationService::EaseInOutCubic(double t) { return t < 0.5 ? 4 * t * t * t : (t - 1) * (2 * t - 2) * (2 * t - 2) + 1; }
    double WindowAnimationService::EaseOutBack(double t) { const double c1 = 1.70158; const double c3 = c1 + 1; return 1 + c3 * std::pow(t - 1, 3) + c1 * std::pow(t - 1, 2); }
    double WindowAnimationService::EaseOutElastic(double t) { const double c4 = (2 * 3.14159) / 3; return t == 0 ? 0 : t == 1 ? 1 : std::pow(2, -10 * t) * std::sin((t * 10 - 0.75) * c4) + 1; }
}
