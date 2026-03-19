#include "pch.h"
#include "Core/CalculatorFactory.h"
#include "Core/Calculators/HalfCalculators.h"
#include "Core/Calculators/CornerCalculators.h"
#include "Core/Calculators/ThirdCalculators.h"
#include "Core/Calculators/FourthCalculators.h"
#include "Core/Calculators/SixthCalculators.h"
#include "Core/Calculators/EighthCalculators.h"
#include "Core/Calculators/NinthCalculators.h"
#include "Core/Calculators/MoveCalculators.h"
#include "Core/Calculators/MiscCalculators.h"

namespace winrt::Rectangle::Core
{
    CalculatorFactory::CalculatorFactory()
    {
        RegisterCalculators();
    }

    void CalculatorFactory::RegisterCalculators()
    {
        m_calculators[WindowAction::LeftHalf] = std::make_shared<LeftHalfCalculator>();
        m_calculators[WindowAction::RightHalf] = std::make_shared<RightHalfCalculator>();
        m_calculators[WindowAction::CenterHalf] = std::make_shared<CenterHalfCalculator>();
        m_calculators[WindowAction::TopHalf] = std::make_shared<TopHalfCalculator>();
        m_calculators[WindowAction::BottomHalf] = std::make_shared<BottomHalfCalculator>();

        m_calculators[WindowAction::TopLeft] = std::make_shared<TopLeftCornerCalculator>();
        m_calculators[WindowAction::TopRight] = std::make_shared<TopRightCornerCalculator>();
        m_calculators[WindowAction::BottomLeft] = std::make_shared<BottomLeftCornerCalculator>();
        m_calculators[WindowAction::BottomRight] = std::make_shared<BottomRightCornerCalculator>();

        m_calculators[WindowAction::FirstThird] = std::make_shared<FirstThirdCalculator>();
        m_calculators[WindowAction::CenterThird] = std::make_shared<CenterThirdCalculator>();
        m_calculators[WindowAction::LastThird] = std::make_shared<LastThirdCalculator>();
        m_calculators[WindowAction::FirstTwoThirds] = std::make_shared<FirstTwoThirdsCalculator>();
        m_calculators[WindowAction::CenterTwoThirds] = std::make_shared<CenterTwoThirdsCalculator>();
        m_calculators[WindowAction::LastTwoThirds] = std::make_shared<LastTwoThirdsCalculator>();

        m_calculators[WindowAction::FirstFourth] = std::make_shared<FirstFourthCalculator>();
        m_calculators[WindowAction::SecondFourth] = std::make_shared<SecondFourthCalculator>();
        m_calculators[WindowAction::ThirdFourth] = std::make_shared<ThirdFourthCalculator>();
        m_calculators[WindowAction::LastFourth] = std::make_shared<LastFourthCalculator>();
        m_calculators[WindowAction::FirstThreeFourths] = std::make_shared<FirstThreeFourthsCalculator>();
        m_calculators[WindowAction::CenterThreeFourths] = std::make_shared<CenterThreeFourthsCalculator>();
        m_calculators[WindowAction::LastThreeFourths] = std::make_shared<LastThreeFourthsCalculator>();

        m_calculators[WindowAction::TopLeftSixth] = std::make_shared<TopLeftSixthCalculator>();
        m_calculators[WindowAction::TopCenterSixth] = std::make_shared<TopCenterSixthCalculator>();
        m_calculators[WindowAction::TopRightSixth] = std::make_shared<TopRightSixthCalculator>();
        m_calculators[WindowAction::BottomLeftSixth] = std::make_shared<BottomLeftSixthCalculator>();
        m_calculators[WindowAction::BottomCenterSixth] = std::make_shared<BottomCenterSixthCalculator>();
        m_calculators[WindowAction::BottomRightSixth] = std::make_shared<BottomRightSixthCalculator>();

        m_calculators[WindowAction::TopLeftNinth] = std::make_shared<TopLeftNinthCalculator>();
        m_calculators[WindowAction::TopCenterNinth] = std::make_shared<TopCenterNinthCalculator>();
        m_calculators[WindowAction::TopRightNinth] = std::make_shared<TopRightNinthCalculator>();
        m_calculators[WindowAction::MiddleLeftNinth] = std::make_shared<MiddleLeftNinthCalculator>();
        m_calculators[WindowAction::MiddleCenterNinth] = std::make_shared<MiddleCenterNinthCalculator>();
        m_calculators[WindowAction::MiddleRightNinth] = std::make_shared<MiddleRightNinthCalculator>();
        m_calculators[WindowAction::BottomLeftNinth] = std::make_shared<BottomLeftNinthCalculator>();
        m_calculators[WindowAction::BottomCenterNinth] = std::make_shared<BottomCenterNinthCalculator>();
        m_calculators[WindowAction::BottomRightNinth] = std::make_shared<BottomRightNinthCalculator>();

        m_calculators[WindowAction::MoveLeft] = std::make_shared<MoveLeftCalculator>();
        m_calculators[WindowAction::MoveRight] = std::make_shared<MoveRightCalculator>();
        m_calculators[WindowAction::MoveUp] = std::make_shared<MoveUpCalculator>();
        m_calculators[WindowAction::MoveDown] = std::make_shared<MoveDownCalculator>();

        m_calculators[WindowAction::DoubleHeightUp] = std::make_shared<DoubleHeightUpCalculator>();
        m_calculators[WindowAction::DoubleHeightDown] = std::make_shared<DoubleHeightDownCalculator>();
        m_calculators[WindowAction::DoubleWidthLeft] = std::make_shared<DoubleWidthLeftCalculator>();
        m_calculators[WindowAction::DoubleWidthRight] = std::make_shared<DoubleWidthRightCalculator>();
        m_calculators[WindowAction::HalveHeightUp] = std::make_shared<HalveHeightUpCalculator>();
        m_calculators[WindowAction::HalveHeightDown] = std::make_shared<HalveHeightDownCalculator>();
        m_calculators[WindowAction::HalveWidthLeft] = std::make_shared<HalveWidthLeftCalculator>();
        m_calculators[WindowAction::HalveWidthRight] = std::make_shared<HalveWidthRightCalculator>();

        m_calculators[WindowAction::Maximize] = std::make_shared<MaximizeCalculator>();
        m_calculators[WindowAction::AlmostMaximize] = std::make_shared<AlmostMaximizeCalculator>();
        m_calculators[WindowAction::MaximizeHeight] = std::make_shared<MaximizeHeightCalculator>();
        m_calculators[WindowAction::Larger] = std::make_shared<LargerCalculator>();
        m_calculators[WindowAction::Smaller] = std::make_shared<SmallerCalculator>();
        m_calculators[WindowAction::Center] = std::make_shared<CenterCalculator>();
        m_calculators[WindowAction::Restore] = std::make_shared<RestoreCalculator>();
        m_calculators[WindowAction::LargerWidth] = std::make_shared<LargerWidthCalculator>();
        m_calculators[WindowAction::SmallerWidth] = std::make_shared<SmallerWidthCalculator>();
        m_calculators[WindowAction::LargerHeight] = std::make_shared<LargerHeightCalculator>();
        m_calculators[WindowAction::SmallerHeight] = std::make_shared<SmallerHeightCalculator>();
    }

    std::shared_ptr<IRectCalculator> CalculatorFactory::GetCalculator(WindowAction action)
    {
        auto it = m_calculators.find(action);
        if (it != m_calculators.end())
        {
            return it->second;
        }
        return nullptr;
    }
}
