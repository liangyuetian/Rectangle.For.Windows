using System.Collections.Generic;

namespace Rectangle.Windows.Core;

public class WindowHistory
{
    private readonly Dictionary<nint, (int X, int Y, int W, int H)> _history = new();

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

    public bool TryGet(nint hwnd, out (int X, int Y, int W, int H) rect) => _history.TryGetValue(hwnd, out rect);

    public void Remove(nint hwnd) => _history.Remove(hwnd);
    
    public bool Contains(nint hwnd) => _history.ContainsKey(hwnd);
}
