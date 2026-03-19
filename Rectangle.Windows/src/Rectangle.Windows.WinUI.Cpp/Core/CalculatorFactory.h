#pragma once
#include "pch.h"
#include "Core/IRectCalculator.h"

namespace winrt::Rectangle::Core
{
    class CalculatorFactory
    {
    public:
        CalculatorFactory();
        ~CalculatorFactory() = default;

        std::shared_ptr<IRectCalculator> GetCalculator(WindowAction action);

    private:
        void RegisterCalculators();
        std::map<WindowAction, std::shared_ptr<IRectCalculator>> m_calculators;
    };
}
