using System;
using System.Collections.Generic;
using Windows.Win32.Foundation;

namespace Rectangle.Windows.WinUI.Services;

/// <summary>
/// 窗口信息缓存
/// </summary>
public class WindowCache
{
    public nint Hwnd { get; set; }
    public string ProcessName { get; set; } = "";
    public string Title { get; set; } = "";
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public DateTime LastUpdateTime { get; set; }
}

/// <summary>
/// 窗口缓存服务
/// 缓存窗口信息以减少频繁的 Windows API 调用
/// </summary>
public class WindowCacheService
{
    private readonly Dictionary<nint, WindowCache> _cache = new();
    private readonly object _lock = new();
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromSeconds(1);
    private readonly Win32WindowService _win32;

    public WindowCacheService(Win32WindowService win32)
    {
        _win32 = win32;
    }

    /// <summary>
    /// 获取窗口信息（优先从缓存读取）
    /// </summary>
    public WindowCache? GetWindowInfo(nint hwnd)
    {
        lock (_lock)
        {
            if (_cache.TryGetValue(hwnd, out var cached))
            {
                if (DateTime.Now - cached.LastUpdateTime < _cacheExpiration)
                {
                    return cached;
                }
            }

            // 缓存过期或不存在，重新获取
            return RefreshCache(hwnd);
        }
    }

    /// <summary>
    /// 刷新窗口缓存
    /// </summary>
    public WindowCache? RefreshCache(nint hwnd)
    {
        try
        {
            var (x, y, w, h) = _win32.GetWindowRect(hwnd);
            var processName = WindowEnumerator.GetProcessNameFromWindow(hwnd);

            var cache = new WindowCache
            {
                Hwnd = hwnd,
                ProcessName = processName,
                X = x,
                Y = y,
                Width = w,
                Height = h,
                LastUpdateTime = DateTime.Now
            };

            lock (_lock)
            {
                _cache[hwnd] = cache;
            }

            return cache;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 使缓存失效
    /// </summary>
    public void Invalidate(nint hwnd)
    {
        lock (_lock)
        {
            _cache.Remove(hwnd);
        }
    }

    /// <summary>
    /// 清空所有缓存
    /// </summary>
    public void Clear()
    {
        lock (_lock)
        {
            _cache.Clear();
        }
    }

    /// <summary>
    /// 清理过期缓存
    /// </summary>
    public void CleanupExpired()
    {
        lock (_lock)
        {
            var now = DateTime.Now;
            var keysToRemove = new List<nint>();

            foreach (var kvp in _cache)
            {
                if (now - kvp.Value.LastUpdateTime > TimeSpan.FromMinutes(5))
                {
                    keysToRemove.Add(kvp.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                _cache.Remove(key);
            }
        }
    }

    /// <summary>
    /// 获取缓存统计信息
    /// </summary>
    public int GetCacheCount()
    {
        lock (_lock)
        {
            return _cache.Count;
        }
    }
}
