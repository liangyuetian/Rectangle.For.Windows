#pragma once
#include "pch.h"
#include <functional>
#include <chrono>

namespace winrt::Rectangle::Services
{
    enum class WindowAnimationType
    {
        None,
        Fade,
        Slide,
        Zoom,
        Combined
    };

    enum class EasingType
    {
        Linear,
        EaseInQuad,
        EaseOutQuad,
        EaseInOutQuad,
        EaseInCubic,
        EaseOutCubic,
        EaseInOutCubic,
        EaseOutBack,
        EaseOutElastic
    };

    struct WindowAnimationConfig
    {
        WindowAnimationType Type{ WindowAnimationType::None };
        EasingType Easing{ EasingType::EaseOutQuad };
        int32_t DurationMs{ 200 };
        bool Enable{ true };
    };

    struct WindowPosition
    {
        int32_t X{ 0 };
        int32_t Y{ 0 };
        int32_t Width{ 0 };
        int32_t Height{ 0 };
    };

    class WindowAnimationService
    {
    public:
        WindowAnimationService();
        ~WindowAnimationService() = default;

        void Initialize();
        void Shutdown();

        bool AnimateWindow(int64_t hwnd, const WindowPosition& targetPos, WindowAnimationType type = WindowAnimationType::Slide);
        bool AnimateWindow(int64_t hwnd, const WindowPosition& fromPos, const WindowPosition& toPos, const WindowAnimationConfig& config);

        void SetDefaultConfig(const WindowAnimationConfig& config);
        WindowAnimationConfig GetDefaultConfig() const;

        static double Ease(EasingType type, double t);
        static double EaseInQuad(double t);
        static double EaseOutQuad(double t);
        static double EaseInOutQuad(double t);
        static double EaseInCubic(double t);
        static double EaseOutCubic(double t);
        static double EaseInOutCubic(double t);
        static double EaseOutBack(double t);
        static double EaseOutElastic(double t);

    private:
        bool PerformAnimation(int64_t hwnd, const WindowPosition& from, const WindowPosition& to, const WindowAnimationConfig& config);

        WindowAnimationConfig m_defaultConfig;
        bool m_isInitialized{ false };
    };
}