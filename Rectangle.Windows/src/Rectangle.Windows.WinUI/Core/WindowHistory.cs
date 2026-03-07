namespace Rectangle.Windows.WinUI.Core;

public class WindowHistory
{
    private readonly Dictionary<nint, (int X, int Y, int W, int H)> _history = new();

    public void Save(nint hwnd, int x, int y, int w, int h) => _history[hwnd] = (x, y, w, h);

    public bool TryGet(nint hwnd, out (int X, int Y, int W, int H) rect) => _history.TryGetValue(hwnd, out rect);

    public void Remove(nint hwnd) => _history.Remove(hwnd);
}
