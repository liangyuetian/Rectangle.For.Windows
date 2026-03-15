namespace Rectangle.Windows.WinUI.Core;

/// <summary>
/// 记录程序对窗口的操作信息
/// </summary>
public struct RectangleAction
{
    public WindowAction Action { get; set; }
    public int Count { get; set; }
    public DateTime LastExecutionTime { get; set; }
    public (int X, int Y, int W, int H) Rect { get; set; }
}

public class WindowHistory
{
    /// <summary>用户原始窗口位置（用于 Restore 恢复到最初位置）</summary>
    private readonly Dictionary<nint, (int X, int Y, int W, int H)> _restoreRects = new();

    /// <summary>程序最后一次对窗口的操作（用于重复执行检测）</summary>
    private readonly Dictionary<nint, RectangleAction> _lastActions = new();

    /// <summary>由程序调整过的窗口集合（位置变化不应被记录为用户操作）</summary>
    private readonly HashSet<nint> _programAdjustedWindows = new();

    private const double RepeatTimeoutSeconds = 2.0;

    // ── 恢复点 ────────────────────────────────────────────────────

    /// <summary>保存恢复点（总是覆盖）</summary>
    public void SaveRestoreRect(nint hwnd, int x, int y, int w, int h)
        => _restoreRects[hwnd] = (x, y, w, h);

    /// <summary>只在没有恢复点时保存（保留最初位置）</summary>
    public void SaveRestoreRectIfNotExists(nint hwnd, int x, int y, int w, int h)
    {
        if (!_restoreRects.ContainsKey(hwnd))
            _restoreRects[hwnd] = (x, y, w, h);
    }

    public bool TryGetRestoreRect(nint hwnd, out (int X, int Y, int W, int H) rect)
        => _restoreRects.TryGetValue(hwnd, out rect);

    public bool HasRestoreRect(nint hwnd) => _restoreRects.ContainsKey(hwnd);

    // ── 操作记录 ──────────────────────────────────────────────────

    public void RecordAction(nint hwnd, WindowAction action, int x, int y, int w, int h)
    {
        var now = DateTime.Now;
        if (_lastActions.TryGetValue(hwnd, out var last))
        {
            var elapsed = (now - last.LastExecutionTime).TotalSeconds;
            if (last.Action == action && elapsed <= RepeatTimeoutSeconds)
            {
                _lastActions[hwnd] = new RectangleAction { Action = action, Count = last.Count + 1, LastExecutionTime = now, Rect = (x, y, w, h) };
                return;
            }
        }
        _lastActions[hwnd] = new RectangleAction { Action = action, Count = 1, LastExecutionTime = now, Rect = (x, y, w, h) };
    }

    public bool TryGetLastAction(nint hwnd, out RectangleAction action)
        => _lastActions.TryGetValue(hwnd, out action);

    public void RemoveLastAction(nint hwnd) => _lastActions.Remove(hwnd);

    /// <summary>检测窗口是否被用户手动移动（与程序最后操作位置不同）</summary>
    public bool IsWindowMovedExternally(nint hwnd, int x, int y, int w, int h)
    {
        if (!_lastActions.TryGetValue(hwnd, out var last)) return false;
        var r = last.Rect;
        return Math.Abs(r.X - x) > 2 || Math.Abs(r.Y - y) > 2
            || Math.Abs(r.W - w) > 2 || Math.Abs(r.H - h) > 2;
    }

    // ── 程序调整标记 ──────────────────────────────────────────────

    public void MarkAsProgramAdjusted(nint hwnd) => _programAdjustedWindows.Add(hwnd);
    public void ClearProgramAdjustedMark(nint hwnd) => _programAdjustedWindows.Remove(hwnd);
    public bool IsProgramAdjusted(nint hwnd) => _programAdjustedWindows.Contains(hwnd);

    // ── 清理 ──────────────────────────────────────────────────────

    public void RemoveWindow(nint hwnd)
    {
        _restoreRects.Remove(hwnd);
        _lastActions.Remove(hwnd);
        _programAdjustedWindows.Remove(hwnd);
    }

    public void Clear()
    {
        _restoreRects.Clear();
        _lastActions.Clear();
        _programAdjustedWindows.Clear();
    }
}
