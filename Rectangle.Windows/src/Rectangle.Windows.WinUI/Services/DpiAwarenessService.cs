using Rectangle.Windows.WinUI.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.HiDpi;

namespace Rectangle.Windows.WinUI.Services
{
    public class DisplayDpiInfo
    {
        public string DeviceName { get; set; } = "";
        public nint MonitorHandle { get; set; }
        public float DpiX { get; set; } = 96f;
        public float DpiY { get; set; } = 96f;
        public float ScaleFactor => DpiX / 96f;
        public WindowRect Bounds { get; set; }
        public WindowRect WorkArea { get; set; }
        public bool IsPrimary { get; set; }
        public Size PhysicalSizeMm { get; set; }
        public int BitsPerPixel { get; set; }
        public int RefreshRate { get; set; }

        public int LogicalToPhysical(int logicalPixels) => (int)Math.Round(logicalPixels * ScaleFactor);
        public int PhysicalToLogical(int physicalPixels) => (int)Math.Round(physicalPixels / ScaleFactor);

        public WindowRect LogicalToPhysicalRect(WindowRect logical) => new()
        {
            X = LogicalToPhysical(logical.X), Y = LogicalToPhysical(logical.Y),
            Width = LogicalToPhysical(logical.Width), Height = LogicalToPhysical(logical.Height)
        };

        public WindowRect PhysicalToLogicalRect(WindowRect physical) => new()
        {
            X = PhysicalToLogical(physical.X), Y = PhysicalToLogical(physical.Y),
            Width = PhysicalToLogical(physical.Width), Height = PhysicalToLogical(physical.Height)
        };
    }

    public class DpiAwarenessService
    {
        private readonly Dictionary<nint, DisplayDpiInfo> _dpiCache = new();
        private readonly object _lock = new();

        // Native structs
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct MONITORINFOEX
        {
            public uint cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public uint dwFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string szDevice;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT { public int left, top, right, bottom; }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT { public int x, y; }

        // DllImports
        [DllImport("user32.dll")] private static extern bool GetMonitorInfo(nint hMonitor, ref MONITORINFOEX lpmi);
        [DllImport("user32.dll")] private static extern nint MonitorFromPoint(POINT pt, uint dwFlags);
        [DllImport("user32.dll")] private static extern nint MonitorFromWindow(nint hwnd, uint dwFlags);
        [DllImport("user32.dll")] private static extern bool EnumDisplayMonitors(nint hdc, nint lprcClip, MonitorEnumProc lpfnEnum, nint dwData);
        [DllImport("shcore.dll")] private static extern int GetDpiForMonitor(nint hmonitor, int dpiType, out uint dpiX, out uint dpiY);
        [DllImport("gdi32.dll")]  private static extern int GetDeviceCaps(nint hdc, int nIndex);
        [DllImport("user32.dll")] private static extern nint GetDC(nint hwnd);
        [DllImport("user32.dll")] private static extern int ReleaseDC(nint hwnd, nint hdc);

        private delegate bool MonitorEnumProc(nint hMonitor, nint hdcMonitor, nint lprcMonitor, nint dwData);

        private const uint MONITOR_DEFAULTTONEAREST = 2;
        private const int MDT_EFFECTIVE_DPI = 0;
        private const int LOGPIXELSX = 88;

        public DpiAwarenessService()
        {
            InitializeDpiAwareness();
        }

        private unsafe void InitializeDpiAwareness()
        {
            try
            {
                // DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 = -4
                var result = PInvoke.SetThreadDpiAwarenessContext(new DPI_AWARENESS_CONTEXT((nint)(-4)));
                if ((nint)result.Value != 0)
                    Logger.Info("DpiAwareness", "[DpiAwareness] 已启用 Per-Monitor V2 DPI Awareness");
                else
                {
                    result = PInvoke.SetThreadDpiAwarenessContext(new DPI_AWARENESS_CONTEXT((nint)(-3)));
                    if ((nint)result.Value != 0)
                        Logger.Info("DpiAwareness", "[DpiAwareness] 已启用 Per-Monitor V1 DPI Awareness");
                    else
                        Logger.Warning("DpiAwareness", "[DpiAwareness] 无法设置 DPI Awareness");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("DpiAwareness", $"[DpiAwareness] 初始化失败: {ex.Message}");
            }
        }

        public IReadOnlyList<DisplayDpiInfo> GetAllDisplayDpiInfo()
        {
            lock (_lock)
            {
                _dpiCache.Clear();
                var displays = new List<DisplayDpiInfo>();

                EnumDisplayMonitors(0, 0, (monitor, _, _, _) =>
                {
                    var info = GetMonitorDpiInfoInternal(monitor);
                    if (info != null)
                    {
                        displays.Add(info);
                        _dpiCache[monitor] = info;
                    }
                    return true;
                }, 0);

                return displays.AsReadOnly();
            }
        }

        public DisplayDpiInfo? GetMonitorDpiInfo(nint monitorHandle)
        {
            lock (_lock)
            {
                if (_dpiCache.TryGetValue(monitorHandle, out var cached)) return cached;
                var info = GetMonitorDpiInfoInternal(monitorHandle);
                if (info != null) _dpiCache[monitorHandle] = info;
                return info;
            }
        }

        private DisplayDpiInfo? GetMonitorDpiInfoInternal(nint monitorHandle)
        {
            try
            {
                var mi = new MONITORINFOEX { cbSize = (uint)Marshal.SizeOf<MONITORINFOEX>() };
                if (!GetMonitorInfo(monitorHandle, ref mi)) return null;

                uint dpiX = 96, dpiY = 96;
                try { GetDpiForMonitor(monitorHandle, MDT_EFFECTIVE_DPI, out dpiX, out dpiY); }
                catch
                {
                    var hdc = GetDC(0);
                    if (hdc != 0) { dpiX = (uint)GetDeviceCaps(hdc, LOGPIXELSX); dpiY = dpiX; ReleaseDC(0, hdc); }
                }

                return new DisplayDpiInfo
                {
                    DeviceName = mi.szDevice ?? "",
                    MonitorHandle = monitorHandle,
                    DpiX = dpiX, DpiY = dpiY,
                    Bounds = new WindowRect { X = mi.rcMonitor.left, Y = mi.rcMonitor.top, Width = mi.rcMonitor.right - mi.rcMonitor.left, Height = mi.rcMonitor.bottom - mi.rcMonitor.top },
                    WorkArea = new WindowRect { X = mi.rcWork.left, Y = mi.rcWork.top, Width = mi.rcWork.right - mi.rcWork.left, Height = mi.rcWork.bottom - mi.rcWork.top },
                    IsPrimary = (mi.dwFlags & 1) != 0
                };
            }
            catch (Exception ex)
            {
                Logger.Error("DpiAwareness", $"[DpiAwareness] 获取显示器DPI失败: {ex.Message}");
                return null;
            }
        }

        public DisplayDpiInfo? GetDpiInfoForPoint(Point point)
        {
            try
            {
                var monitor = MonitorFromPoint(new POINT { x = point.X, y = point.Y }, MONITOR_DEFAULTTONEAREST);
                return GetMonitorDpiInfo(monitor);
            }
            catch (Exception ex) { Logger.Error("DpiAwareness", $"[DpiAwareness] 获取点DPI失败: {ex.Message}"); return null; }
        }

        public DisplayDpiInfo? GetDpiInfoForWindow(nint hwnd)
        {
            try
            {
                var monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);
                return GetMonitorDpiInfo(monitor);
            }
            catch (Exception ex) { Logger.Error("DpiAwareness", $"[DpiAwareness] 获取窗口DPI失败: {ex.Message}"); return null; }
        }

        public WindowRect ConvertRectToDpi(WindowRect rect, float targetDpi)
        {
            float scale = targetDpi / 96f;
            return new WindowRect { X = (int)Math.Round(rect.X * scale), Y = (int)Math.Round(rect.Y * scale), Width = (int)Math.Round(rect.Width * scale), Height = (int)Math.Round(rect.Height * scale) };
        }

        public WindowRect ConvertRectDpi(WindowRect rect, float sourceDpi, float targetDpi)
        {
            float scale = targetDpi / sourceDpi;
            return new WindowRect { X = (int)Math.Round(rect.X * scale), Y = (int)Math.Round(rect.Y * scale), Width = (int)Math.Round(rect.Width * scale), Height = (int)Math.Round(rect.Height * scale) };
        }

        public float GetDpiForWindow(nint hwnd)
        {
            try { return PInvoke.GetDpiForWindow((HWND)hwnd); }
            catch { return 96f; }
        }

        public Size CreateSizeForDpi(int width, int height, float dpi)
        {
            float scale = dpi / 96f;
            return new Size((int)Math.Round(width * scale), (int)Math.Round(height * scale));
        }

        public float GetSystemDpi()
        {
            var hdc = GetDC(0);
            if (hdc != 0) { int dpi = GetDeviceCaps(hdc, LOGPIXELSX); ReleaseDC(0, hdc); return dpi; }
            return 96f;
        }

        public unsafe void MakeWindowDpiAware(nint hwnd)
        {
            try
            {
                var context = PInvoke.GetWindowDpiAwarenessContext((HWND)hwnd);
                Logger.Debug("DpiAwareness", $"[DpiAwareness] 窗口DPI Context: {(nint)context.Value}");
            }
            catch (Exception ex) { Logger.Error("DpiAwareness", $"[DpiAwareness] 失败: {ex.Message}"); }
        }

        public bool IsMixedDpiEnvironment()
        {
            var displays = GetAllDisplayDpiInfo();
            if (displays.Count <= 1) return false;
            var firstDpi = displays[0].DpiX;
            return displays.Any(d => Math.Abs(d.DpiX - firstDpi) > 1);
        }

        public MixedDpiStatistics GetMixedDpiStatistics()
        {
            var displays = GetAllDisplayDpiInfo();
            return new MixedDpiStatistics
            {
                TotalDisplays = displays.Count,
                UniqueDpiValues = displays.Select(d => d.DpiX).Distinct().Count(),
                MinDpi = displays.Min(d => d.DpiX),
                MaxDpi = displays.Max(d => d.DpiX),
                AverageScaleFactor = displays.Average(d => d.ScaleFactor),
                IsMixedDpi = displays.Select(d => d.DpiX).Distinct().Count() > 1
            };
        }

        public void ClearCache()
        {
            lock (_lock) { _dpiCache.Clear(); }
        }
    }

    public class MixedDpiStatistics
    {
        public int TotalDisplays { get; set; }
        public int UniqueDpiValues { get; set; }
        public float MinDpi { get; set; }
        public float MaxDpi { get; set; }
        public double AverageScaleFactor { get; set; }
        public bool IsMixedDpi { get; set; }
    }
}
