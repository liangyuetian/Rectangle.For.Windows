using System;
using System.Collections.Generic;
using System.Linq;
using Rectangle.Windows.Services;

namespace Rectangle.Windows.Core;

/// <summary>
/// 记录程序对窗口的操作信息
/// </summary>
public struct RectangleAction
{
    /// <summary>
    /// 执行的操作类型
    /// </summary>
    public WindowAction Action { get; set; }

    /// <summary>
    /// 连续执行同一操作的次数
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// 最后一次执行的时间
    /// </summary>
    public DateTime LastExecutionTime { get; set; }

    /// <summary>
    /// 操作后的窗口位置
    /// </summary>
    public (int X, int Y, int W, int H) Rect { get; set; }
}

/// <summary>
/// 窗口状态 - 用于 Undo/Redo
/// </summary>
public struct WindowState
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public WindowAction Action { get; set; }
    public DateTime Timestamp { get; set; }
}

public class WindowHistory
{
    /// <summary>
    /// 保存用户原始窗口位置（用于恢复）
    /// </summary>
    private readonly Dictionary<nint, (int X, int Y, int W, int H)> _restoreRects = new();

    /// <summary>
    /// 记录程序最后一次对窗口的操作信息（用于重复执行检测）
    /// </summary>
    private readonly Dictionary<nint, RectangleAction> _lastActions = new();

    /// <summary>
    /// Undo 栈 - 每个窗口的操作历史
    /// </summary>
    private readonly Dictionary<nint, Stack<WindowState>> _undoStacks = new();

    /// <summary>
    /// Redo 栈 - 每个窗口的撤销历史
    /// </summary>
    private readonly Dictionary<nint, Stack<WindowState>> _redoStacks = new();

    /// <summary>
    /// 记录哪些窗口是由程序调整的（这些窗口的位置变化不应该被记录）
    /// </summary>
    private readonly HashSet<nint> _programAdjustedWindows = new();

    /// <summary>
    /// 记录窗口最后访问时间（用于清理过期记录）
    /// </summary>
    private readonly Dictionary<nint, DateTime> _lastAccessTimes = new();

    /// <summary>
    /// 重复执行的超时时间（秒）
    /// </summary>
    private const double RepeatTimeoutSeconds = 2.0;

    /// <summary>
    /// 历史记录最大数量
    /// </summary>
    private int _maxHistoryCount = 100;

    /// <summary>
    /// 历史记录过期时间（分钟）
    /// </summary>
    private int _expirationMinutes = 60;

    /// <summary>
    /// 最后清理时间
    /// </summary>
    private DateTime _lastCleanupTime = DateTime.MinValue;

    /// <summary>
    /// 清理间隔（分钟）
    /// </summary>
    private const int CleanupIntervalMinutes = 10;

    /// <summary>
    /// 配置服务
    /// </summary>
    private readonly ConfigService? _configService;

    public WindowHistory(ConfigService? configService = null)
    {
        _configService = configService;
        LoadConfig();
    }

    /// <summary>
    /// 从配置加载设置
    /// </summary>
    private void LoadConfig()
    {
        if (_configService == null) return;

        try
        {
            var config = _configService.Load();
            _maxHistoryCount = config.MaxWindowHistoryCount;
            _expirationMinutes = config.WindowHistoryExpirationMinutes;
        }
        catch
        {
            // 使用默认值
        }
    }

    /// <summary>
    /// 保存窗口位置到恢复点（总是覆盖）
    /// </summary>
    public void SaveRestoreRect(nint hwnd, int x, int y, int w, int h)
    {
        CleanupIfNeeded();
        _restoreRects[hwnd] = (x, y, w, h);
        _lastAccessTimes[hwnd] = DateTime.Now;
    }

    /// <summary>
    /// 检查并执行清理（如果需要）
    /// </summary>
    private void CleanupIfNeeded()
    {
        var now = DateTime.Now;
        if ((now - _lastCleanupTime).TotalMinutes < CleanupIntervalMinutes)
            return;

        CleanupExpiredRecords();
        _lastCleanupTime = now;
    }

    /// <summary>
    /// 清理过期记录
    /// </summary>
    private void CleanupExpiredRecords()
    {
        var now = DateTime.Now;
        var expiredWindows = new List<nint>();

        // 找出过期窗口
        foreach (var kvp in _lastAccessTimes)
        {
            if ((now - kvp.Value).TotalMinutes > _expirationMinutes)
            {
                expiredWindows.Add(kvp.Key);
            }
        }

        // 删除过期记录
        foreach (var hwnd in expiredWindows)
        {
            _restoreRects.Remove(hwnd);
            _lastActions.Remove(hwnd);
            _programAdjustedWindows.Remove(hwnd);
            _lastAccessTimes.Remove(hwnd);
        }

        // 如果超过最大数量，删除最旧的
        if (_restoreRects.Count > _maxHistoryCount)
        {
            var oldest = _lastAccessTimes.OrderBy(x => x.Value).Take(_restoreRects.Count - _maxHistoryCount);
            foreach (var item in oldest)
            {
                _restoreRects.Remove(item.Key);
                _lastActions.Remove(item.Key);
                _programAdjustedWindows.Remove(item.Key);
                _lastAccessTimes.Remove(item.Key);
            }
        }

        if (expiredWindows.Count > 0)
        {
            Logger.Debug("WindowHistory", $"清理了 {expiredWindows.Count} 个过期窗口记录");
        }
    }

    /// <summary>
    /// 只在窗口还没有恢复点时保存原始位置。
    /// 这样 Restore 会恢复到第一次使用快捷键前的位置。
    /// </summary>
    public void SaveRestoreRectIfNotExists(nint hwnd, int x, int y, int w, int h)
    {
        CleanupIfNeeded();
        if (!_restoreRects.ContainsKey(hwnd))
        {
            _restoreRects[hwnd] = (x, y, w, h);
            _lastAccessTimes[hwnd] = DateTime.Now;
        }
    }

    /// <summary>
    /// 记录程序对窗口的操作
    /// </summary>
    public void RecordAction(nint hwnd, WindowAction action, int x, int y, int w, int h)
    {
        CleanupIfNeeded();
        var now = DateTime.Now;
        _lastAccessTimes[hwnd] = now;
        
        if (_lastActions.TryGetValue(hwnd, out var lastAction))
        {
            // 检查是否是重复执行同一操作
            var timeSinceLastExecution = (now - lastAction.LastExecutionTime).TotalSeconds;
            
            if (lastAction.Action == action && timeSinceLastExecution <= RepeatTimeoutSeconds)
            {
                // 增加计数
                _lastActions[hwnd] = new RectangleAction
                {
                    Action = action,
                    Count = lastAction.Count + 1,
                    LastExecutionTime = now,
                    Rect = (x, y, w, h)
                };
            }
            else
            {
                // 新的操作或超时，重置计数
                _lastActions[hwnd] = new RectangleAction
                {
                    Action = action,
                    Count = 1,
                    LastExecutionTime = now,
                    Rect = (x, y, w, h)
                };
            }
        }
        else
        {
            // 第一次操作
            _lastActions[hwnd] = new RectangleAction
            {
                Action = action,
                Count = 1,
                LastExecutionTime = now,
                Rect = (x, y, w, h)
            };
        }
    }

    /// <summary>
    /// 获取窗口的最后操作信息
    /// </summary>
    public bool TryGetLastAction(nint hwnd, out RectangleAction action)
    {
        return _lastActions.TryGetValue(hwnd, out action);
    }

    /// <summary>
    /// 检测窗口是否被用户手动移动（与程序最后操作的位置不同）
    /// </summary>
    public bool IsWindowMovedExternally(nint hwnd, int x, int y, int w, int h)
    {
        if (!_lastActions.TryGetValue(hwnd, out var lastAction))
        {
            return false; // 没有记录，说明是第一次操作
        }
        
        var lastRect = lastAction.Rect;
        
        // 允许 2 像素的误差（有些应用会微调窗口位置）
        return Math.Abs(lastRect.X - x) > 2 
            || Math.Abs(lastRect.Y - y) > 2 
            || Math.Abs(lastRect.W - w) > 2 
            || Math.Abs(lastRect.H - h) > 2;
    }

    /// <summary>
    /// 保存用户手动调整的窗口位置（排除程序调整的窗口）
    /// </summary>
    public void SaveUserAdjustment(nint hwnd, int x, int y, int w, int h)
    {
        // 如果是程序调整的窗口，不记录
        if (_programAdjustedWindows.Contains(hwnd))
        {
            return;
        }
        
        _restoreRects[hwnd] = (x, y, w, h);
    }

    /// <summary>
    /// 标记窗口为由程序调整
    /// </summary>
    public void MarkAsProgramAdjusted(nint hwnd)
    {
        _programAdjustedWindows.Add(hwnd);
    }

    /// <summary>
    /// 清除程序调整标记（当用户手动调整窗口后）
    /// </summary>
    public void ClearProgramAdjustedMark(nint hwnd)
    {
        _programAdjustedWindows.Remove(hwnd);
    }

    /// <summary>
    /// 检查窗口是否由程序调整
    /// </summary>
    public bool IsProgramAdjusted(nint hwnd)
    {
        return _programAdjustedWindows.Contains(hwnd);
    }

    /// <summary>
    /// 获取恢复点位置
    /// </summary>
    public bool TryGetRestoreRect(nint hwnd, out (int X, int Y, int W, int H) rect) 
        => _restoreRects.TryGetValue(hwnd, out rect);

    /// <summary>
    /// 清除程序最后操作记录（恢复后调用）
    /// </summary>
    public void RemoveLastAction(nint hwnd)
    {
        _lastActions.Remove(hwnd);
    }

    // ========== Undo/Redo 支持 ==========

    /// <summary>
    /// 保存窗口状态到 Undo 栈
    /// </summary>
    public void PushUndoState(nint hwnd, int x, int y, int w, int h, WindowAction action)
    {
        CleanupIfNeeded();

        if (!_undoStacks.TryGetValue(hwnd, out var stack))
        {
            stack = new Stack<WindowState>();
            _undoStacks[hwnd] = stack;
        }

        // 限制栈大小
        if (stack.Count >= _maxHistoryCount)
        {
            var tempList = stack.ToList();
            tempList.RemoveAt(tempList.Count - 1);
            stack.Clear();
            foreach (var item in tempList.Reverse<WindowState>())
            {
                stack.Push(item);
            }
        }

        stack.Push(new WindowState
        {
            X = x,
            Y = y,
            Width = w,
            Height = h,
            Action = action,
            Timestamp = DateTime.Now
        });

        // 清空 Redo 栈（新操作后无法 redo）
        if (_redoStacks.TryGetValue(hwnd, out var redoStack))
        {
            redoStack.Clear();
        }

        _lastAccessTimes[hwnd] = DateTime.Now;
    }

    /// <summary>
    /// 尝试执行 Undo
    /// </summary>
    /// <returns>返回撤销前的窗口状态，如果没有可撤销的状态则返回 null</returns>
    public WindowState? TryUndo(nint hwnd)
    {
        if (!_undoStacks.TryGetValue(hwnd, out var stack) || stack.Count == 0)
            return null;

        var currentState = stack.Pop();

        // 保存到 Redo 栈
        if (!_redoStacks.TryGetValue(hwnd, out var redoStack))
        {
            redoStack = new Stack<WindowState>();
            _redoStacks[hwnd] = redoStack;
        }
        redoStack.Push(currentState);

        _lastAccessTimes[hwnd] = DateTime.Now;
        return currentState;
    }

    /// <summary>
    /// 尝试执行 Redo
    /// </summary>
    /// <returns>返回重做后的窗口状态，如果没有可重做的状态则返回 null</returns>
    public WindowState? TryRedo(nint hwnd)
    {
        if (!_redoStacks.TryGetValue(hwnd, out var stack) || stack.Count == 0)
            return null;

        var state = stack.Pop();

        // 保存回 Undo 栈
        if (_undoStacks.TryGetValue(hwnd, out var undoStack))
        {
            undoStack.Push(state);
        }

        _lastAccessTimes[hwnd] = DateTime.Now;
        return state;
    }

    /// <summary>
    /// 检查是否可以 Undo
    /// </summary>
    public bool CanUndo(nint hwnd)
    {
        return _undoStacks.TryGetValue(hwnd, out var stack) && stack.Count > 0;
    }

    /// <summary>
    /// 检查是否可以 Redo
    /// </summary>
    public bool CanRedo(nint hwnd)
    {
        return _redoStacks.TryGetValue(hwnd, out var stack) && stack.Count > 0;
    }

    /// <summary>
    /// 获取当前窗口的 Undo 栈深度
    /// </summary>
    public int GetUndoDepth(nint hwnd)
    {
        return _undoStacks.TryGetValue(hwnd, out var stack) ? stack.Count : 0;
    }

    /// <summary>
    /// 获取当前窗口的 Redo 栈深度
    /// </summary>
    public int GetRedoDepth(nint hwnd)
    {
        return _redoStacks.TryGetValue(hwnd, out var stack) ? stack.Count : 0;
    }

    /// <summary>
    /// 完全移除窗口的所有记录（窗口关闭时调用）
    /// </summary>
    public void RemoveWindow(nint hwnd)
    {
        _restoreRects.Remove(hwnd);
        _lastActions.Remove(hwnd);
        _programAdjustedWindows.Remove(hwnd);
        _lastAccessTimes.Remove(hwnd);
        _undoStacks.Remove(hwnd);
        _redoStacks.Remove(hwnd);
    }

    /// <summary>
    /// 检查窗口是否有恢复点
    /// </summary>
    public bool HasRestoreRect(nint hwnd) => _restoreRects.ContainsKey(hwnd);

    /// <summary>
    /// 清除所有历史记录
    /// </summary>
    public void Clear()
    {
        _restoreRects.Clear();
        _lastActions.Clear();
        _programAdjustedWindows.Clear();
        _lastAccessTimes.Clear();
        _undoStacks.Clear();
        _redoStacks.Clear();
        _lastCleanupTime = DateTime.MinValue;
        Logger.Info("WindowHistory", "历史记录已清空");
    }

    /// <summary>
    /// 获取当前历史记录数量
    /// </summary>
    public int GetHistoryCount() => _restoreRects.Count;

    /// <summary>
    /// 获取历史记录统计信息
    /// </summary>
    public (int RestoreRects, int LastActions, int ProgramAdjusted, int AccessTimes, int UndoStacks, int RedoStacks) GetStats()
    {
        return (
            _restoreRects.Count,
            _lastActions.Count,
            _programAdjustedWindows.Count,
            _lastAccessTimes.Count,
            _undoStacks.Sum(s => s.Value.Count),
            _redoStacks.Sum(s => s.Value.Count)
        );
    }
}
