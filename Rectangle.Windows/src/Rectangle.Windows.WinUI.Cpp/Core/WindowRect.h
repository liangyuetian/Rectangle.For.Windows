#pragma once
#include "pch.h"

namespace winrt::Rectangle::Core
{
    struct WorkArea
    {
        int32_t Left{ 0 };
        int32_t Top{ 0 };
        int32_t Right{ 0 };
        int32_t Bottom{ 0 };

        constexpr WorkArea() noexcept = default;
        constexpr WorkArea(int32_t left, int32_t top, int32_t right, int32_t bottom) noexcept
            : Left(left), Top(top), Right(right), Bottom(bottom) {}

        constexpr int32_t Width() const noexcept { return Right - Left; }
        constexpr int32_t Height() const noexcept { return Bottom - Top; }
    };

    struct WindowRect
    {
        int32_t X{ 0 };
        int32_t Y{ 0 };
        int32_t Width{ 0 };
        int32_t Height{ 0 };

        constexpr WindowRect() noexcept = default;
        constexpr WindowRect(int32_t x, int32_t y, int32_t width, int32_t height) noexcept
            : X(x), Y(y), Width(width), Height(height) {}

        constexpr int32_t Left() const noexcept { return X; }
        constexpr int32_t Top() const noexcept { return Y; }
        constexpr int32_t Right() const noexcept { return X + Width; }
        constexpr int32_t Bottom() const noexcept { return Y + Height; }

        constexpr bool operator==(const WindowRect&) const noexcept = default;
        constexpr bool operator!=(const WindowRect&) const noexcept = default;
    };
}
