using System.Collections.Generic;

namespace Rectangle.Windows.Core;

public class WindowHistory
{
    /// <summary>
    /// 保存用户原始窗口位置（用于恢复）
    /// </summary>
    private readonly Dictionary<nint, (int X, int Y, int W, int H)> _restoreRects = new();
    
    /// <summary>
    /// 记录程序最后一次对窗口的操作位置（用于检测用户手动移动）
    /// </summary>
    private readonly Dictionary<nint, (int X, int Y, int W, int H)> _lastRectangleActions = new();
    
    /// <summary>
    /// 记录哪些窗口是由程序调整的（这些窗口的位置变化不应该被记录）
    /// </summary>
    private readonly HashSet<nint> _programAdjustedWindows = new();

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
    /// 记录程序最后一次操作的窗口位置
    /// </summary>
    public void SaveLastRectangleAction(nint hwnd, int x, int y, int w, int h)
    {
        _lastRectangleActions[hwnd] = (x, y, w, h);
    }

    /// <summary>
    /// 检测窗口是否被用户手动移动（与程序最后操作的位置不同）
    /// </summary>
    public bool IsWindowMovedExternally(nint hwnd, int x, int y, int w, int h)
    {
        if (!_lastRectangleActions.TryGetValue(hwnd, out var lastAction))
        {
            return false; // 没有记录，说明是第一次操作
        }
        
        // 允许 2 像素的误差（有些应用会微调窗口位置）
        return Math.Abs(lastAction.X - x) > 2 
            || Math.Abs(lastAction.Y - y) > 2 
            || Math.Abs(lastAction.W - w) > 2 
            || Math.Abs(lastAction.H - h) > 2;
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
    public void RemoveLastRectangleAction(nint hwnd)
    {
        _lastRectangleActions.Remove(hwnd);
    }

    /// <summary>
    /// 完全移除窗口的所有记录（窗口关闭时调用）
    /// </summary>
    public void RemoveWindow(nint hwnd)
    {
        _restoreRects.Remove(hwnd);
        _lastRectangleActions.Remove(hwnd);
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
        _lastRectangleActions.Clear();
        _programAdjustedWindows.Clear();
    }
}
