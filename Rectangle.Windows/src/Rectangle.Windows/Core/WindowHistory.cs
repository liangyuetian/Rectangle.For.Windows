using System;
using System.Collections.Generic;
using System.Linq;

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
    /// 记录哪些窗口是由程序调整的（这些窗口的位置变化不应该被记录）
    /// </summary>
    private readonly HashSet<nint> _programAdjustedWindows = new();
    
    /// <summary>
    /// 重复执行的超时时间（秒）
    /// </summary>
    private const double RepeatTimeoutSeconds = 2.0;

    /// <summary>
    /// 最大历史记录数量
    /// </summary>
    private const int MaxHistoryCount = 100;

    /// <summary>
    /// 历史记录的过期时间（分钟）
    /// </summary>
    private const int HistoryExpirationMinutes = 60;

    /// <summary>
    /// 最后清理时间
    /// </summary>
    private DateTime _lastCleanupTime = DateTime.MinValue;

    /// <summary>
    /// 清理间隔（分钟）
    /// </summary>
    private const int CleanupIntervalMinutes = 5;

    /// <summary>
    /// 保存窗口位置到恢复点（总是覆盖）
    /// </summary>
    public void SaveRestoreRect(nint hwnd, int x, int y, int w, int h)
    {
        _restoreRects[hwnd] = (x, y, w, h);
    }

    /// <summary>
    /// 只在窗口还没有恢复点时保存原始位置。
    /// 这样 Restore 会恢复到第一次使用快捷键前的位置。
    /// </summary>
    public void SaveRestoreRectIfNotExists(nint hwnd, int x, int y, int w, int h)
    {
        if (!_restoreRects.ContainsKey(hwnd))
        {
            _restoreRects[hwnd] = (x, y, w, h);
        }
    }

    /// <summary>
    /// 记录程序对窗口的操作
    /// </summary>
    public void RecordAction(nint hwnd, WindowAction action, int x, int y, int w, int h)
    {
        var now = DateTime.Now;
        
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

    /// <summary>
    /// 完全移除窗口的所有记录（窗口关闭时调用）
    /// </summary>
    public void RemoveWindow(nint hwnd)
    {
        _restoreRects.Remove(hwnd);
        _lastActions.Remove(hwnd);
        _programAdjustedWindows.Remove(hwnd);
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
    }

    /// <summary>
    /// 清理过期和超量的历史记录
    /// </summary>
    public void CleanupIfNeeded()
    {
        var now = DateTime.Now;

        // 检查是否需要清理
        if ((now - _lastCleanupTime).TotalMinutes < CleanupIntervalMinutes)
            return;

        _lastCleanupTime = now;

        // 清理过期的操作记录
        var expiredKeys = new List<nint>();
        foreach (var kvp in _lastActions)
        {
            if ((now - kvp.Value.LastExecutionTime).TotalMinutes > HistoryExpirationMinutes)
            {
                expiredKeys.Add(kvp.Key);
            }
        }

        foreach (var key in expiredKeys)
        {
            _lastActions.Remove(key);
        }

        // 如果记录数量超过限制，清理最旧的
        if (_restoreRects.Count > MaxHistoryCount)
        {
            var keysToRemove = _lastActions
                .OrderBy(x => x.Value.LastExecutionTime)
                .Take(_restoreRects.Count - MaxHistoryCount)
                .Select(x => x.Key)
                .ToList();

            foreach (var key in keysToRemove)
            {
                _restoreRects.Remove(key);
                _lastActions.Remove(key);
            }
        }
    }

    /// <summary>
    /// 获取历史记录统计信息
    /// </summary>
    public (int RestoreRects, int LastActions, int ProgramAdjusted) GetStatistics()
    {
        return (_restoreRects.Count, _lastActions.Count, _programAdjustedWindows.Count);
    }
}
