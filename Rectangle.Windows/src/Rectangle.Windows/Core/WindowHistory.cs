using System.Collections.Generic;

namespace Rectangle.Windows.Core;

public class WindowHistory
{
    private readonly Dictionary<nint, (int X, int Y, int W, int H)> _history = new();
    
    /// <summary>
    /// 记录哪些窗口是由程序调整的（这些窗口的位置变化不应该被记录）
    /// </summary>
    private readonly HashSet<nint> _programAdjustedWindows = new();

    /// <summary>
    /// 保存窗口位置（总是覆盖）
    /// </summary>
    public void Save(nint hwnd, int x, int y, int w, int h)
    {
        _history[hwnd] = (x, y, w, h);
    }

    /// <summary>
    /// 只在窗口还没有历史记录时保存原始位置。
    /// 这样 Restore 会恢复到第一次使用快捷键前的位置。
    /// </summary>
    public void SaveIfNotExists(nint hwnd, int x, int y, int w, int h)
    {
        if (!_history.ContainsKey(hwnd))
        {
            _history[hwnd] = (x, y, w, h);
        }
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
        
        _history[hwnd] = (x, y, w, h);
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

    public bool TryGet(nint hwnd, out (int X, int Y, int W, int H) rect) => _history.TryGetValue(hwnd, out rect);

    public void Remove(nint hwnd)
    {
        _history.Remove(hwnd);
        _programAdjustedWindows.Remove(hwnd);
    }
    
    public bool Contains(nint hwnd) => _history.ContainsKey(hwnd);
    
    /// <summary>
    /// 清除所有历史记录
    /// </summary>
    public void Clear()
    {
        _history.Clear();
        _programAdjustedWindows.Clear();
    }
}
