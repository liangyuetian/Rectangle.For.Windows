using System;
using System.Collections.Generic;
using Rectangle.Windows.Core;

namespace Rectangle.Windows.Services;

/// <summary>
/// 窗口信息缓存
/// 减少频繁的 Win32 API 调用
/// </summary>
public class WindowInfoCache
{
    private readonly Dictionary<nint, CachedWindowInfo> _cache = new();
    private readonly Dictionary<nint, DateTime> _lastUpdateTime = new();
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMilliseconds(500);
    private readonly TimeSpan _staleCleanupInterval = TimeSpan.FromMinutes(5);
    private DateTime _lastCleanupTime = DateTime.MinValue;

    /// <summary>
    /// 获取窗口矩形（带缓存）
    /// </summary>
    public (int X, int Y, int W, int H) GetWindowRect(nint hwnd, Win32WindowService win32)
    {
        CleanupStaleEntries();

        if (_cache.TryGetValue(hwnd, out var info) &&
            _lastUpdateTime.TryGetValue(hwnd, out var lastUpdate) &&
            (DateTime.Now - lastUpdate) < _cacheDuration)
        {
            return (info.X, info.Y, info.Width, info.Height);
        }

        // 缓存未命中，从 Win32 获取
        var rect = win32.GetWindowRect(hwnd);
        UpdateCache(hwnd, rect);
        return rect;
    }

    /// <summary>
    /// 更新缓存
    /// </summary>
    private void UpdateCache(nint hwnd, (int X, int Y, int W, int H) rect)
    {
        _cache[hwnd] = new CachedWindowInfo
        {
            X = rect.X,
            Y = rect.Y,
            Width = rect.W,
            Height = rect.H,
            LastAccessTime = DateTime.Now
        };
        _lastUpdateTime[hwnd] = DateTime.Now;
    }

    /// <summary>
    /// 使窗口缓存失效
    /// </summary>
    public void Invalidate(nint hwnd)
    {
        _cache.Remove(hwnd);
        _lastUpdateTime.Remove(hwnd);
    }

    /// <summary>
    /// 清理所有缓存
    /// </summary>
    public void Clear()
    {
        _cache.Clear();
        _lastUpdateTime.Clear();
    }

    /// <summary>
    /// 清理过期条目
    /// </summary>
    private void CleanupStaleEntries()
    {
        var now = DateTime.Now;
        if ((now - _lastCleanupTime) < _staleCleanupInterval)
            return;

        var staleHandles = new List<nint>();
        foreach (var kvp in _lastUpdateTime)
        {
            if ((now - kvp.Value) > _staleCleanupInterval)
            {
                staleHandles.Add(kvp.Key);
            }
        }

        foreach (var hwnd in staleHandles)
        {
            _cache.Remove(hwnd);
            _lastUpdateTime.Remove(hwnd);
        }

        if (staleHandles.Count > 0)
        {
            Logger.Debug("WindowInfoCache", $"清理了 {staleHandles.Count} 个过期缓存条目");
        }

        _lastCleanupTime = now;
    }

    /// <summary>
    /// 获取缓存统计
    /// </summary>
    public (int Count, TimeSpan AvgAge) GetStats()
    {
        var now = DateTime.Now;
        double totalAgeMs = 0;
        int count = 0;

        foreach (var kvp in _lastUpdateTime)
        {
            totalAgeMs += (now - kvp.Value).TotalMilliseconds;
            count++;
        }

        var avgAge = count > 0 ? TimeSpan.FromMilliseconds(totalAgeMs / count) : TimeSpan.Zero;
        return (count, avgAge);
    }
}

/// <summary>
/// 缓存的窗口信息
/// </summary>
public class CachedWindowInfo
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public DateTime LastAccessTime { get; set; }
}
