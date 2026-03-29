using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Rectangle.Windows.Services;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Rectangle.Windows.Core;

/// <summary>
/// 窗口布局信息
/// </summary>
public class WindowLayoutInfo
{
    public string ProcessName { get; set; } = "";
    public string WindowTitle { get; set; } = "";
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public bool IsMaximized { get; set; }
}

/// <summary>
/// 保存的布局
/// </summary>
public class SavedLayout
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public List<WindowLayoutInfo> Windows { get; set; } = new();
}

/// <summary>
/// 布局管理器 - 保存和恢复窗口布局
/// </summary>
public class LayoutManager
{
    private const string LayoutsFileName = "layouts.json";
    private readonly string _layoutsPath;
    private readonly ConfigService _configService;
    private readonly Win32WindowService _win32;

    public LayoutManager(ConfigService configService, Win32WindowService win32)
    {
        _configService = configService;
        _win32 = win32;
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _layoutsPath = Path.Combine(appData, "Rectangle", LayoutsFileName);
    }

    /// <summary>
    /// 获取所有保存的布局
    /// </summary>
    public async Task<List<SavedLayout>> GetLayoutsAsync()
    {
        return await Task.Run(() =>
        {
            if (!File.Exists(_layoutsPath))
                return new List<SavedLayout>();

            try
            {
                var json = File.ReadAllText(_layoutsPath);
                return JsonSerializer.Deserialize<List<SavedLayout>>(json) ?? new List<SavedLayout>();
            }
            catch
            {
                return new List<SavedLayout>();
            }
        });
    }

    /// <summary>
    /// 保存当前窗口布局
    /// </summary>
    public async Task SaveCurrentLayoutAsync(string name)
    {
        var layouts = await GetLayoutsAsync();
        var layout = new SavedLayout
        {
            Name = name,
            CreatedAt = DateTime.Now,
            Windows = CaptureCurrentWindows()
        };

        layouts.Add(layout);

        // 只保留最近 20 个布局
        if (layouts.Count > 20)
        {
            layouts = layouts.OrderByDescending(l => l.CreatedAt).Take(20).ToList();
        }

        await SaveLayoutsAsync(layouts);
    }

    /// <summary>
    /// 恢复指定布局
    /// </summary>
    public async Task RestoreLayoutAsync(string layoutId)
    {
        var layouts = await GetLayoutsAsync();
        var layout = layouts.FirstOrDefault(l => l.Id == layoutId);
        if (layout == null) return;

        foreach (var windowInfo in layout.Windows)
        {
            try
            {
                // 查找匹配的窗口
                var hwnd = FindMatchingWindow(windowInfo);
                if (hwnd != 0)
                {
                    if (windowInfo.IsMaximized)
                    {
                        // 如果原来是最大化，先恢复再最大化
                        _win32.SetWindowRect(hwnd, windowInfo.X, windowInfo.Y, windowInfo.Width, windowInfo.Height);
                        // 使用 ShowWindow 最大化
                        PInvoke.ShowWindow(
                            new HWND(hwnd),
                            SHOW_WINDOW_CMD.SW_MAXIMIZE
                        );
                    }
                    else
                    {
                        _win32.SetWindowRect(hwnd, windowInfo.X, windowInfo.Y, windowInfo.Width, windowInfo.Height);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Warning("LayoutManager", $"恢复窗口失败: {windowInfo.ProcessName} - {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 删除指定布局
    /// </summary>
    public async Task DeleteLayoutAsync(string layoutId)
    {
        var layouts = await GetLayoutsAsync();
        layouts.RemoveAll(l => l.Id == layoutId);
        await SaveLayoutsAsync(layouts);
    }

    private async Task SaveLayoutsAsync(List<SavedLayout> layouts)
    {
        await Task.Run(() =>
        {
            try
            {
                var dir = Path.GetDirectoryName(_layoutsPath);
                if (!string.IsNullOrEmpty(dir))
                    Directory.CreateDirectory(dir);

                var json = JsonSerializer.Serialize(layouts, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_layoutsPath, json);
            }
            catch (Exception ex)
            {
                Logger.Error("LayoutManager", $"保存布局失败: {ex.Message}");
            }
        });
    }

    private List<WindowLayoutInfo> CaptureCurrentWindows()
    {
        var windows = new List<WindowLayoutInfo>();
        var allWindows = WindowEnumerator.GetAllValidWindows();

        foreach (var hwnd in allWindows)
        {
            try
            {
                var (x, y, w, h) = _win32.GetWindowRect(hwnd);
                var processName = WindowEnumerator.GetProcessNameFromWindow(hwnd);
                var windowTitle = WindowEnumerator.GetWindowTitle(hwnd);

                // 暂时不检测最大化状态（避免使用 GetWindowPlacement）
                var isMaximized = false;

                if (!string.IsNullOrEmpty(processName))
                {
                    windows.Add(new WindowLayoutInfo
                    {
                        ProcessName = processName,
                        WindowTitle = windowTitle,
                        X = x,
                        Y = y,
                        Width = w,
                        Height = h,
                        IsMaximized = isMaximized
                    });
                }
            }
            catch { }
        }

        return windows;
    }

    private nint FindMatchingWindow(WindowLayoutInfo info)
    {
        var allWindows = WindowEnumerator.GetAllValidWindows();

        foreach (var hwnd in allWindows)
        {
            try
            {
                var processName = WindowEnumerator.GetProcessNameFromWindow(hwnd);
                var windowTitle = WindowEnumerator.GetWindowTitle(hwnd);

                // 优先匹配进程名和窗口标题
                if (processName.Equals(info.ProcessName, StringComparison.OrdinalIgnoreCase))
                {
                    // 如果窗口标题也匹配，优先返回
                    if (windowTitle == info.WindowTitle)
                    {
                        return hwnd;
                    }
                    // 否则记录第一个匹配的进程
                    return hwnd;
                }
            }
            catch { }
        }

        return 0;
    }
}

/// <summary>
/// 窗口枚举辅助类
/// </summary>
public static class WindowEnumerator
{
    public static List<nint> GetAllValidWindows()
    {
        var windows = new List<nint>();

        PInvoke.EnumWindows((hwnd, _) =>
        {
            try
            {
                if (IsValidWindow(hwnd))
                {
                    windows.Add(hwnd.Value);
                }
            }
            catch { }
            return true;
        }, 0);

        return windows;
    }

    public static string GetProcessNameFromWindow(nint hwnd)
    {
        try
        {
            unsafe
            {
                uint processId;
                PInvoke.GetWindowThreadProcessId(new HWND(hwnd), &processId);
                if (processId == 0) return string.Empty;

                using var process = System.Diagnostics.Process.GetProcessById((int)processId);
                return process.ProcessName;
            }
        }
        catch
        {
            return string.Empty;
        }
    }

    public static string GetWindowTitle(nint hwnd)
    {
        try
        {
            var hWnd = new HWND(hwnd);
            int length = PInvoke.GetWindowTextLength(hWnd);
            if (length == 0) return string.Empty;

            var buffer = new char[length + 1];
            unsafe
            {
                fixed (char* pBuffer = buffer)
                {
                    PInvoke.GetWindowText(hWnd, pBuffer, length + 1);
                }
            }
            return new string(buffer, 0, length);
        }
        catch
        {
            return string.Empty;
        }
    }

    private static bool IsValidWindow(HWND hwnd)
    {
        try
        {
            if (!PInvoke.IsWindow(hwnd)) return false;
            if (!PInvoke.IsWindowVisible(hwnd)) return false;

            int titleLength = PInvoke.GetWindowTextLength(hwnd);
            if (titleLength == 0) return false;

            // 获取窗口样式
            var exStyle = (uint)PInvoke.GetWindowLong(hwnd, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE);
            if ((exStyle & 0x00000080) != 0) return false; // WS_EX_TOOLWINDOW

            var style = (uint)PInvoke.GetWindowLong(hwnd, WINDOW_LONG_PTR_INDEX.GWL_STYLE);
            if ((style & 0x80000000) != 0 && (style & 0x00C00000) == 0) return false; // WS_POPUP without caption

            // 获取窗口类名
            unsafe
            {
                char* className = stackalloc char[256];
                int len = PInvoke.GetClassName(hwnd, className, 256);
                string classNameStr = new string(className, 0, len);

                string[] systemClasses = {
                    "Shell_TrayWnd", "Shell_SecondaryTrayWnd", "NotifyIconOverflowWindow",
                    "Progman", "WorkerW", "tooltips_class32"
                };

                if (systemClasses.Any(c => c.Equals(classNameStr, StringComparison.OrdinalIgnoreCase)))
                    return false;
            }

            // 获取窗口大小
            if (PInvoke.GetWindowRect(hwnd, out var rect))
            {
                int width = rect.right - rect.left;
                int height = rect.bottom - rect.top;
                if (width < 50 || height < 50) return false;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }
}
