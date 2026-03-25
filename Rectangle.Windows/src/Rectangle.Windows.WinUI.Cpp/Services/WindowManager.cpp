#include "pch.h"
#include "Services/WindowManager.h"
#include "Services/Logger.h"
#include "Services/Win32WindowService.h"
#include "Services/OperationHistoryManager.h"
#include "Services/LastActiveWindowService.h"
#include "Core/CalculatorFactory.h"
#include "Core/WindowHistory.h"
#include "Services/ConfigService.h"

namespace winrt::Rectangle::Services
{
    WindowManager::WindowManager()
    {
        Logger::Instance().Info(L"WindowManager", L"WindowManager initialized");
    }

    WindowManager::~WindowManager() = default;

    void WindowManager::SetConfigService(ConfigService* configService)
    {
        m_configService = configService;
        ReloadConfig();
    }

    void WindowManager::SetLastActiveWindowService(void* service)
    {
        m_lastActiveService = service;
    }

    void WindowManager::SetOperationHistory(void* history)
    {
        m_operationHistory = history;
    }

    void WindowManager::ReloadConfig()
    {
        if (!m_configService) return;

        auto config = m_configService->Load();
        m_gapSize = config.GapSize;
    }

    bool WindowManager::IsIgnoredApp(const std::wstring& processName) const
    {
        if (!m_configService) return false;

        auto config = m_configService->Load();
        for (const auto& app : config.IgnoredApps)
        {
            if (_wcsicmp(app.c_str(), processName.c_str()) == 0 ||
                _wcsicmp((app + L".exe").c_str(), processName.c_str()) == 0)
            {
                return true;
            }
        }
        return false;
    }

    void WindowManager::Execute(WindowAction action, int64_t targetHwnd, bool forceDirectAction)
    {
        if (action == WindowAction::Restore)
        {
            ExecuteRestore(targetHwnd);
            return;
        }

        if (action == WindowAction::Maximize)
        {
            ExecuteMaximizeToggle(targetHwnd);
            return;
        }

        if (action == WindowAction::NextDisplay)
        {
            ExecuteNextDisplay(targetHwnd);
            return;
        }

        if (action == WindowAction::PreviousDisplay)
        {
            ExecutePreviousDisplay(targetHwnd);
            return;
        }

        if (action == WindowAction::Undo || action == WindowAction::Redo)
        {
            if (action == WindowAction::Undo) ExecuteUndo();
            else ExecuteRedo();
            return;
        }

        int64_t hwnd = targetHwnd ? targetHwnd : GetTargetWindow();
        if (hwnd == 0)
        {
            PlayBeep();
            return;
        }

        Win32WindowService win32;
        auto processName = win32.GetProcessNameFromWindow(hwnd);
        if (processName.empty())
        {
            Logger::Instance().Warning(L"WindowManager", L"无法获取窗口进程名");
            return;
        }

        if (IsIgnoredApp(processName))
        {
            Logger::Instance().Info(L"WindowManager", processName + L" 在忽略列表中，跳过操作");
            return;
        }

        if (win32.IsModalDialog(hwnd))
        {
            Logger::Instance().Info(L"WindowManager", processName + L" 是模态对话框，跳过操作");
            return;
        }

        int32_t x, y, width, height;
        if (!win32.GetWindowRect(hwnd, x, y, width, height))
        {
            return;
        }

        bool isMaximized = win32.IsMaximized(hwnd);
        bool useDirectResizeFromMaximized = false;

        auto isResizeAction = action == WindowAction::Larger || action == WindowAction::Smaller ||
            action == WindowAction::LargerWidth || action == WindowAction::SmallerWidth ||
            action == WindowAction::LargerHeight || action == WindowAction::SmallerHeight;

        if (isResizeAction && isMaximized)
        {
            bool isLarger = action == WindowAction::Larger || action == WindowAction::LargerWidth || action == WindowAction::LargerHeight;
            if (isLarger)
            {
                return;
            }
            useDirectResizeFromMaximized = true;
        }

        auto workArea = GetTargetWorkArea(hwnd);
        Core::WindowRect current(x, y, width, height);

        WindowHistory history;
        bool windowMovedExternally = history.IsWindowMovedExternally(hwnd, x, y, width, height);

        if (windowMovedExternally)
        {
            history.RemoveLastAction(hwnd);
            Logger::Instance().Info(L"WindowManager", L"检测到窗口被用户手动移动: " + processName);
        }

        auto [actualAction, targetDisplayIndex] = forceDirectAction
            ? std::make_pair(action, std::optional<int32_t>{})
            : GetActualAction(hwnd, action, windowMovedExternally);

        if (actualAction != action || targetDisplayIndex.has_value())
        {
            Logger::Instance().Info(L"WindowManager",
                targetDisplayIndex.has_value()
                    ? L"循环尺寸(显示器" + std::to_wstring(targetDisplayIndex.value() + 1) + L"): " + Core::ToString(action) + L" → " + Core::ToString(actualAction)
                    : L"循环尺寸: " + Core::ToString(action) + L" → " + Core::ToString(actualAction));
        }

        CalculatorFactory factory;
        auto calculator = factory.GetCalculator(actualAction);
        if (!calculator)
        {
            Logger::Instance().Warning(L"WindowManager", L"未找到对应的计算器: " + Core::ToString(actualAction));
            return;
        }

        if (targetDisplayIndex.has_value())
        {
            workArea = GetWorkAreaByDisplayIndex(targetDisplayIndex.value());
        }

        if (!history.HasRestoreRect(hwnd) || windowMovedExternally)
        {
            history.SaveRestoreRect(hwnd, x, y, width, height);
            Logger::Instance().Debug(L"WindowManager", L"保存恢复点: (" + std::to_wstring(x) + L", " + std::to_wstring(y) + L", " + std::to_wstring(width) + L", " + std::to_wstring(height) + L")");
        }

        history.MarkAsProgramAdjusted(hwnd);
        m_maximizedWindows.erase(hwnd);

        auto target = calculator->Calculate(workArea, current, actualAction, m_gapSize);
        target = ApplyWindowGap(target, workArea, actualAction);

        if (!win32.IsResizable(hwnd))
        {
            target = HandleFixedSizeWindow(current, target, workArea, actualAction);
        }

        target = ApplyMinimumSize(target);
        target = ClampToWorkArea(target, workArea);

        if (useDirectResizeFromMaximized)
            win32.SetWindowRectFromMaximized(hwnd, target.X, target.Y, target.Width, target.Height);
        else
            win32.SetWindowRect(hwnd, target.X, target.Y, target.Width, target.Height);

        history.RecordAction(hwnd, action, target.X, target.Y, target.Width, target.Height);

        RecordToOperationHistory(action, hwnd, current, target, processName);

        MoveCursorIfEnabled(hwnd, actualAction);

        Logger::Instance().Info(L"WindowManager", Core::ToString(actualAction) + L" 了 " + processName);
    }

    void WindowManager::ExecuteRestore(int64_t targetHwnd)
    {
        int64_t hwnd = targetHwnd ? targetHwnd : GetTargetWindow();
        if (hwnd == 0) return;

        Win32WindowService win32;
        WindowHistory history;

        if (!history.HasRestoreRect(hwnd))
        {
            Logger::Instance().Debug(L"WindowManager", L"窗口没有恢复点");
            return;
        }

        auto restoreRect = history.GetRestoreRect(hwnd);
        win32.SetWindowRect(hwnd, restoreRect.X, restoreRect.Y, restoreRect.Width, restoreRect.Height);
        history.ClearRestoreRect(hwnd);
        m_maximizedWindows.erase(hwnd);

        Logger::Instance().Info(L"WindowManager", L"已恢复窗口");
    }

    void WindowManager::ExecuteMaximizeToggle(int64_t targetHwnd)
    {
        int64_t hwnd = targetHwnd ? targetHwnd : GetTargetWindow();
        if (hwnd == 0) return;

        Win32WindowService win32;

        if (win32.IsMaximized(hwnd))
        {
            ExecuteRestore(hwnd);
        }
        else
        {
            auto workArea = GetTargetWorkArea(hwnd);
            win32.SetWindowRect(hwnd, workArea.Left, workArea.Top, workArea.Width(), workArea.Height());
            m_maximizedWindows.insert(hwnd);
        }
    }

    void WindowManager::ExecuteNextDisplay(int64_t targetHwnd)
    {
        int64_t hwnd = targetHwnd ? targetHwnd : GetTargetWindow();
        if (hwnd == 0) return;

        Win32WindowService win32;
        auto workAreas = win32.GetMonitorWorkAreas();
        if (workAreas.size() <= 1) return;

        auto currentWorkArea = GetTargetWorkArea(hwnd);
        int32_t currentIndex = -1;

        for (size_t i = 0; i < workAreas.size(); ++i)
        {
            if (workAreas[i].Left == currentWorkArea.Left &&
                workAreas[i].Top == currentWorkArea.Top)
            {
                currentIndex = static_cast<int32_t>(i);
                break;
            }
        }

        int32_t nextIndex = (currentIndex + 1) % workAreas.size();
        auto nextWorkArea = workAreas[nextIndex];

        int32_t x, y, width, height;
        if (!win32.GetWindowRect(hwnd, x, y, width, height)) return;

        int32_t newX = nextWorkArea.Left + (x - currentWorkArea.Left);
        int32_t newY = nextWorkArea.Top + (y - currentWorkArea.Top);

        newX = std::max(nextWorkArea.Left, std::min(newX, nextWorkArea.Right - width));
        newY = std::max(nextWorkArea.Top, std::min(newY, nextWorkArea.Bottom - height));

        win32.SetWindowRect(hwnd, newX, newY, width, height);
        Logger::Instance().Info(L"WindowManager", L"窗口已移动到下一个显示器");
    }

    void WindowManager::ExecutePreviousDisplay(int64_t targetHwnd)
    {
        int64_t hwnd = targetHwnd ? targetHwnd : GetTargetWindow();
        if (hwnd == 0) return;

        Win32WindowService win32;
        auto workAreas = win32.GetMonitorWorkAreas();
        if (workAreas.size() <= 1) return;

        auto currentWorkArea = GetTargetWorkArea(hwnd);
        int32_t currentIndex = -1;

        for (size_t i = 0; i < workAreas.size(); ++i)
        {
            if (workAreas[i].Left == currentWorkArea.Left &&
                workAreas[i].Top == currentWorkArea.Top)
            {
                currentIndex = static_cast<int32_t>(i);
                break;
            }
        }

        int32_t prevIndex = currentIndex <= 0 ? static_cast<int32_t>(workAreas.size()) - 1 : currentIndex - 1;
        auto prevWorkArea = workAreas[prevIndex];

        int32_t x, y, width, height;
        if (!win32.GetWindowRect(hwnd, x, y, width, height)) return;

        int32_t newX = prevWorkArea.Left + (x - currentWorkArea.Left);
        int32_t newY = prevWorkArea.Top + (y - currentWorkArea.Top);

        newX = std::max(prevWorkArea.Left, std::min(newX, prevWorkArea.Right - width));
        newY = std::max(prevWorkArea.Top, std::min(newY, prevWorkArea.Bottom - height));

        win32.SetWindowRect(hwnd, newX, newY, width, height);
        Logger::Instance().Info(L"WindowManager", L"窗口已移动到上一个显示器");
    }

    void WindowManager::ExecuteUndo()
    {
        if (m_operationHistory)
        {
            auto* historyManager = static_cast<OperationHistoryManager*>(m_operationHistory);
            if (historyManager->CanUndo())
            {
                historyManager->Undo();
                Logger::Instance().Info(L"WindowManager", L"撤销上一个操作");
                return;
            }
        }
        Logger::Instance().Info(L"WindowManager", L"没有可撤销的操作");
    }

    void WindowManager::ExecuteRedo()
    {
        if (m_operationHistory)
        {
            auto* historyManager = static_cast<OperationHistoryManager*>(m_operationHistory);
            if (historyManager->CanRedo())
            {
                historyManager->Redo();
                Logger::Instance().Info(L"WindowManager", L"重做上一个操作");
                return;
            }
        }
        Logger::Instance().Info(L"WindowManager", L"没有可重做的操作");
    }

    int64_t WindowManager::GetTargetWindow() const
    {
        if (m_cachedTargetWindow != 0 && Win32WindowService().IsWindow(m_cachedTargetWindow))
        {
            return m_cachedTargetWindow;
        }

        return GetTargetWindowCore();
    }

    int64_t WindowManager::GetTargetWindowCore() const
    {
        if (m_lastActiveService)
        {
            auto* lastActive = static_cast<LastActiveWindowService*>(m_lastActiveService);
            int64_t hwnd = lastActive->GetTargetWindow();
            if (hwnd != 0)
            {
                return hwnd;
            }
        }

        return Win32WindowService().GetForegroundWindow();
    }

    Core::WorkArea WindowManager::GetTargetWorkArea(int64_t hwnd) const
    {
        Win32WindowService win32;
        bool useCursorScreen = true;
        if (m_configService)
        {
            auto config = m_configService->Load();
            useCursorScreen = config.UseCursorScreenDetection;
        }

        return useCursorScreen
            ? win32.GetMonitorWorkAreaFromCursor()
            : win32.GetMonitorWorkAreaFromWindow(hwnd);
    }

    Core::WorkArea WindowManager::GetWorkAreaByDisplayIndex(int32_t index) const
    {
        Win32WindowService win32;
        auto workAreas = win32.GetMonitorWorkAreas();
        if (index < 0 || index >= static_cast<int32_t>(workAreas.size()))
        {
            return workAreas.size() > 0 ? workAreas[0] : Core::WorkArea(0, 0, 1920, 1080);
        }
        return workAreas[index];
    }

    std::pair<WindowAction, std::optional<int32_t>> WindowManager::GetActualAction(
        int64_t hwnd, WindowAction requestedAction, bool windowMovedExternally)
    {
        if (windowMovedExternally)
        {
            return { requestedAction, {} };
        }

        if (!m_configService)
        {
            return { requestedAction, {} };
        }

        auto config = m_configService->Load();
        auto mode = static_cast<Core::SubsequentExecutionMode>(config.SubsequentExecutionMode);

        if (mode != Core::SubsequentExecutionMode::CycleSize ||
            !Core::RepeatedExecutionsCalculator::SupportsCycle(requestedAction))
        {
            return { requestedAction, {} };
        }

        WindowHistory history;
        Core::WindowHistoryRecord record;
        if (!history.TryGetLastAction(hwnd, record))
        {
            return { requestedAction, {} };
        }

        if (record.Action != requestedAction)
        {
            return { requestedAction, {} };
        }

        Win32WindowService win32;
        auto workAreas = win32.GetMonitorWorkAreas();
        int32_t numDisplays = static_cast<int32_t>(workAreas.size());
        int32_t executionCount = record.Count + 1;

        if (numDisplays > 1 && Core::RepeatedExecutionsCalculator::SupportsCycle(requestedAction))
        {
            auto cycle = Core::RepeatedExecutionsCalculator::GetCycleGroup(requestedAction);
            int32_t groupLength = static_cast<int32_t>(cycle.size());
            int32_t totalCycleLength = groupLength * numDisplays;
            int32_t cycleIndex = (executionCount - 1) % totalCycleLength;
            int32_t displayIndex = cycleIndex / groupLength;
            int32_t positionInGroup = cycleIndex % groupLength;
            auto actualAction = cycle[positionInGroup];
            return { actualAction, displayIndex };
        }

        auto nextAction = Core::RepeatedExecutionsCalculator::GetNextCycleAction(requestedAction, executionCount);
        return { nextAction, {} };
    }

    Core::WindowRect WindowManager::ApplyWindowGap(Core::WindowRect target, const Core::WorkArea& workArea, WindowAction action)
    {
        if (m_gapSize <= 0) return target;

        const int32_t halfGap = m_gapSize / 2;

        switch (action)
        {
        case WindowAction::LeftHalf:
            target.Width -= halfGap;
            break;
        case WindowAction::RightHalf:
            target.X += halfGap;
            target.Width -= halfGap;
            break;
        case WindowAction::TopHalf:
            target.Height -= halfGap;
            break;
        case WindowAction::BottomHalf:
            target.Y += halfGap;
            target.Height -= halfGap;
            break;
        case WindowAction::TopLeft:
            target.Width -= halfGap;
            target.Height -= halfGap;
            break;
        case WindowAction::TopRight:
            target.X += halfGap;
            target.Width -= halfGap;
            target.Height -= halfGap;
            break;
        case WindowAction::BottomLeft:
            target.Y += halfGap;
            target.Width -= halfGap;
            target.Height -= halfGap;
            break;
        case WindowAction::BottomRight:
            target.X += halfGap;
            target.Y += halfGap;
            target.Width -= halfGap;
            target.Height -= halfGap;
            break;
        case WindowAction::FirstThird:
            target.Width -= halfGap;
            break;
        case WindowAction::CenterThird:
            target.X += halfGap;
            target.Width -= m_gapSize;
            break;
        case WindowAction::LastThird:
            target.X += halfGap;
            target.Width -= halfGap;
            break;
        default:
            break;
        }

        return target;
    }

    Core::WindowRect WindowManager::HandleFixedSizeWindow(const Core::WindowRect& current, const Core::WindowRect& target,
        const Core::WorkArea& workArea, WindowAction action)
    {
        switch (action)
        {
        case WindowAction::LeftHalf:
        case WindowAction::FirstThird:
        case WindowAction::FirstFourth:
            return Core::WindowRect(workArea.Left, target.Y, current.Width, current.Height);
        case WindowAction::RightHalf:
        case WindowAction::LastThird:
        case WindowAction::LastFourth:
            return Core::WindowRect(workArea.Right - current.Width, target.Y, current.Width, current.Height);
        case WindowAction::Center:
        case WindowAction::CenterHalf:
        case WindowAction::CenterThird:
        {
            int32_t centerX = workArea.Left + (workArea.Width() - current.Width) / 2;
            int32_t centerY = workArea.Top + (workArea.Height() - current.Height) / 2;
            return Core::WindowRect(centerX, centerY, current.Width, current.Height);
        }
        case WindowAction::TopHalf:
            return Core::WindowRect(target.X, workArea.Top, current.Width, current.Height);
        case WindowAction::BottomHalf:
            return Core::WindowRect(target.X, workArea.Bottom - current.Height, current.Width, current.Height);
        case WindowAction::MoveLeft:
        case WindowAction::MoveRight:
        case WindowAction::MoveUp:
        case WindowAction::MoveDown:
            return Core::WindowRect(target.X, target.Y, current.Width, current.Height);
        default:
        {
            int32_t centerX = workArea.Left + (workArea.Width() - current.Width) / 2;
            int32_t centerY = workArea.Top + (workArea.Height() - current.Height) / 2;
            return Core::WindowRect(centerX, centerY, current.Width, current.Height);
        }
        }
    }

    Core::WindowRect WindowManager::ApplyMinimumSize(Core::WindowRect target)
    {
        if (!m_configService) return target;

        auto config = m_configService->Load();
        int32_t minWidth = static_cast<int32_t>(config.MinimumWindowWidth);
        int32_t minHeight = static_cast<int32_t>(config.MinimumWindowHeight);

        if (target.Width < minWidth)
        {
            target.Width = minWidth;
        }
        if (target.Height < minHeight)
        {
            target.Height = minHeight;
        }

        return target;
    }

    Core::WindowRect WindowManager::ClampToWorkArea(Core::WindowRect target, const Core::WorkArea& workArea)
    {
        if (target.X < workArea.Left)
        {
            target.X = workArea.Left;
        }
        if (target.Y < workArea.Top)
        {
            target.Y = workArea.Top;
        }
        if (target.Right() > workArea.Right)
        {
            target.X = workArea.Right - target.Width;
        }
        if (target.Bottom() > workArea.Bottom)
        {
            target.Y = workArea.Bottom - target.Height;
        }

        return target;
    }

    void WindowManager::MoveCursorIfEnabled(int64_t hwnd, WindowAction action)
    {
        if (!m_configService) return;

        auto config = m_configService->Load();
        bool shouldMoveCursor = config.MoveCursor;

        if ((action == WindowAction::NextDisplay || action == WindowAction::PreviousDisplay)
            && !config.MoveCursorAcrossDisplays)
        {
            shouldMoveCursor = false;
        }

        if (shouldMoveCursor)
        {
            MoveCursorToWindowCenter(hwnd);
            Logger::Instance().Debug(L"WindowManager", L"光标已移动到窗口中心");
        }
    }

    void WindowManager::MoveCursorToWindowCenter(int64_t hwnd)
    {
        Win32WindowService win32;
        int32_t x, y, width, height;
        if (!win32.GetWindowRect(hwnd, x, y, width, height)) return;

        int32_t centerX = x + width / 2;
        int32_t centerY = y + height / 2;
        SetCursorPos(centerX, centerY);
    }

    void WindowManager::PlayBeep()
    {
        Beep(1000, 50);
    }

    void WindowManager::RecordToOperationHistory(WindowAction action, int64_t hwnd,
        const Core::WindowRect& oldRect, const Core::WindowRect& newRect,
        const std::wstring& processName)
    {
        if (!m_configService || !m_operationHistory)
        {
            return;
        }

        auto config = m_configService->Load();
        if (!config.History.Enabled)
        {
            return;
        }

        auto* historyManager = static_cast<OperationHistoryManager*>(m_operationHistory);
        historyManager->SetMaxHistoryCount(config.History.MaxHistoryCount);
        historyManager->RecordOperation(action, hwnd, oldRect, newRect, processName);
    }
}
