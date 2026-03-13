using Rectangle.Windows.WinUI.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.HiDpi;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Rectangle.Windows.WinUI.Services
{
    /// <summary>
    /// 显示器DPI信息
    /// </summary>
    public class DisplayDpiInfo
    {
        /// <summary>
        /// 显示器设备名称
        /// </summary>
        public string DeviceName { get; set; } = "";

        /// <summary>
        /// 显示器句柄
        /// </summary>
        public nint MonitorHandle { get; set; }

        /// <summary>
        /// 水平DPI
        /// </summary>
        public float DpiX { get; set; } = 96f;

        /// <summary>
        /// 垂直DPI
        /// </summary>
        public float DpiY { get; set; } = 96f;

        /// <summary>
        /// DPI缩放比例（相对于96 DPI）
        /// </summary>
        public float ScaleFactor => DpiX / 96f;

        /// <summary>
        /// 显示器边界（设备独立像素）
        /// </summary>
        public WindowRect Bounds { get; set; }

        /// <summary>
        /// 工作区域（设备独立像素，排除任务栏）
        /// </summary>
        public WindowRect WorkArea { get; set; }

        /// <summary>
        /// 是否为主显示器
        /// </summary>
        public bool IsPrimary { get; set; }

        /// <summary>
        /// 物理尺寸（毫米）
        /// </summary>
        public Size PhysicalSizeMm { get; set; }

        /// <summary>
        /// 位深度
        /// </summary>
        public int BitsPerPixel { get; set; }

        /// <summary>
        /// 刷新率
        /// </summary>
        public int RefreshRate { get; set; }

        /// <summary>
        /// 将设备独立像素转换为物理像素
        /// </summary>
        public int LogicalToPhysical(int logicalPixels)
        {
            return (int)Math.Round(logicalPixels * ScaleFactor);
        }

        /// <summary>
        /// 将物理像素转换为设备独立像素
        /// </summary>
        public int PhysicalToLogical(int physicalPixels)
        {
            return (int)Math.Round(physicalPixels / ScaleFactor);
        }

        /// <summary>
        /// 将设备独立像素Rect转换为物理像素Rect
        /// </summary>
        public WindowRect LogicalToPhysicalRect(WindowRect logical)
        {
            return new WindowRect
            {
                X = LogicalToPhysical(logical.X),
                Y = LogicalToPhysical(logical.Y),
                Width = LogicalToPhysical(logical.Width),
                Height = LogicalToPhysical(logical.Height)
            };
        }

        /// <summary>
        /// 将物理像素Rect转换为设备独立像素Rect
        /// </summary>
        public WindowRect PhysicalToLogicalRect(WindowRect physical)
        {
            return new WindowRect
            {
                X = PhysicalToLogical(physical.X),
                Y = PhysicalToLogical(physical.Y),
                Width = PhysicalToLogical(physical.Width),
                Height = PhysicalToLogical(physical.Height)
            };
        }
    }

    /// <summary>
    /// DPI感知服务 - 处理多显示器DPI缩放
    /// </summary>
    public class DpiAwarenessService
    {
        private readonly Logger _logger;
        private readonly Dictionary<nint, DisplayDpiInfo> _dpiCache = new();
        private readonly object _lock = new();

        /// <summary>
        /// 系统DPI感知模式
        /// </summary>
        public enum DpiAwarenessMode
        {
            /// <summary>
            ///  unaware - 系统DPI
            /// </summary>
            Unaware,

            /// <summary>
            /// System aware - 主显示器DPI
            /// </summary>
            SystemAware,

            /// <summary>
            /// Per-monitor aware - 每个显示器独立DPI（推荐）
            /// </summary>
            PerMonitorAware,

            /// <summary>
            /// Per-monitor V2 - 改进的每显示器DPI（Windows 10 1703+）
            /// </summary>
            PerMonitorAwareV2
        }

        public DpiAwarenessService(Logger logger)
        {
            _logger = logger;
            InitializeDpiAwareness();
        }

        /// <summary>
        /// 初始化DPI感知模式
        /// </summary>
        private void InitializeDpiAwareness()
        {
            try
            {
                // 尝试设置 Per-Monitor V2 DPI Awareness（Windows 10 1703+）
                var awareness = DPI_AWARENESS_CONTEXT.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2;
                var result = PInvoke.SetThreadDpiAwarenessContext(awareness);

                if (result != 0)
                {
                    _logger?.LogInfo("[DpiAwareness] 已启用 Per-Monitor V2 DPI Awareness");
                }
                else
                {
                    // 回退到 Per-Monitor V1
                    awareness = DPI_AWARENESS_CONTEXT.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE;
                    result = PInvoke.SetThreadDpiAwarenessContext(awareness);

                    if (result != 0)
                    {
                        _logger?.LogInfo("[DpiAwareness] 已启用 Per-Monitor V1 DPI Awareness");
                    }
                    else
                    {
                        _logger?.LogWarning("[DpiAwareness] 无法设置 DPI Awareness，使用系统默认");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError($"[DpiAwareness] 初始化 DPI Awareness 失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取所有显示器的DPI信息
        /// </summary>
        public IReadOnlyList<DisplayDpiInfo> GetAllDisplayDpiInfo()
        {
            lock (_lock)
            {
                _dpiCache.Clear();

                var displays = new List<DisplayDpiInfo>();

                // 枚举所有显示器
                PInvoke.EnumDisplayMonitors(null, null, (monitor, hdc, rect, data) =>
                {
                    var info = GetMonitorDpiInfo(monitor);
                    if (info != null)
                    {
                        displays.Add(info);
                        _dpiCache[monitor.Value] = info;
                    }
                    return true;
                }, 0);

                return displays.AsReadOnly();
            }
        }

        /// <summary>
        /// 获取指定显示器的DPI信息
        /// </summary>
        public DisplayDpiInfo? GetMonitorDpiInfo(nint monitorHandle)
        {
            lock (_lock)
            {
                if (_dpiCache.TryGetValue(monitorHandle, out var cached))
                {
                    return cached;
                }

                try
                {
                    var info = new MONITORINFOEXW();
                    info.monitorInfo.cbSize = (uint)Marshal.SizeOf<MONITORINFOEXW>();

                    if (!PInvoke.GetMonitorInfo((HMONITOR)monitorHandle, ref info.monitorInfo))
                    {
                        return null;
                    }

                    // 获取DPI
                    uint dpiX = 96, dpiY = 96;
                    try
                    {
                        PInvoke.GetDpiForMonitor((HMONITOR)monitorHandle, MONITOR_DPI_TYPE.MDT_EFFECTIVE_DPI, out dpiX, out dpiY);
                    }
                    catch
                    {
                        // 如果 API 不可用，尝试从设备上下文获取
                        var hdc = PInvoke.GetDC((HWND)0);
                        if (hdc != 0)
                        {
                            dpiX = (uint)PInvoke.GetDeviceCaps(hdc, GET_DEVICE_CAPS_INDEX.LOGPIXELSX);
                            dpiY = (uint)PInvoke.GetDeviceCaps(hdc, GET_DEVICE_CAPS_INDEX.LOGPIXELSY);
                            PInvoke.ReleaseDC((HWND)0, hdc);
                        }
                    }

                    // 获取设备名称
                    string deviceName = System.Text.Encoding.Unicode.GetString(info.szDevice).TrimEnd('\0');

                    // 获取显示设备信息
                    var displayDevice = new DISPLAY_DEVICEW();
                    displayDevice.cb = (uint)Marshal.SizeOf<DISPLAY_DEVICEW>();
                    PInvoke.EnumDisplayDevices(deviceName, 0, ref displayDevice, 0);

                    var dpiInfo = new DisplayDpiInfo
                    {
                        DeviceName = deviceName,
                        MonitorHandle = monitorHandle,
                        DpiX = dpiX,
                        DpiY = dpiY,
                        Bounds = new WindowRect
                        {
                            X = info.monitorInfo.rcMonitor.left,
                            Y = info.monitorInfo.rcMonitor.top,
                            Width = info.monitorInfo.rcMonitor.right - info.monitorInfo.rcMonitor.left,
                            Height = info.monitorInfo.rcMonitor.bottom - info.monitorInfo.rcMonitor.top
                        },
                        WorkArea = new WindowRect
                        {
                            X = info.monitorInfo.rcWork.left,
                            Y = info.monitorInfo.rcWork.top,
                            Width = info.monitorInfo.rcWork.right - info.monitorInfo.rcWork.left,
                            Height = info.monitorInfo.rcWork.bottom - info.monitorInfo.rcWork.top
                        },
                        IsPrimary = (info.monitorInfo.dwFlags & 1) != 0 // MONITORINFOF_PRIMARY
                    };

                    _dpiCache[monitorHandle] = dpiInfo;
                    return dpiInfo;
                }
                catch (Exception ex)
                {
                    _logger?.LogError($"[DpiAwareness] 获取显示器 DPI 信息失败: {ex.Message}");
                    return null;
                }
            }
        }

        /// <summary>
        /// 获取指定点的显示器DPI信息
        /// </summary>
        public DisplayDpiInfo? GetDpiInfoForPoint(Point point)
        {
            try
            {
                var monitor = PInvoke.MonitorFromPoint(
                    new Windows.Win32.Foundation.POINT { x = point.X, y = point.Y },
                    MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST);

                return GetMonitorDpiInfo(monitor.Value);
            }
            catch (Exception ex)
            {
                _logger?.LogError($"[DpiAwareness] 获取点 DPI 信息失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 获取指定窗口所在显示器的DPI信息
        /// </summary>
        public DisplayDpiInfo? GetDpiInfoForWindow(nint hwnd)
        {
            try
            {
                var monitor = PInvoke.MonitorFromWindow(
                    (HWND)hwnd,
                    MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST);

                return GetMonitorDpiInfo(monitor.Value);
            }
            catch (Exception ex)
            {
                _logger?.LogError($"[DpiAwareness] 获取窗口 DPI 信息失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 将窗口Rect转换为指定DPI下的Rect
        /// </summary>
        public WindowRect ConvertRectToDpi(WindowRect rect, float targetDpi)
        {
            float scale = targetDpi / 96f;
            return new WindowRect
            {
                X = (int)Math.Round(rect.X * scale),
                Y = (int)Math.Round(rect.Y * scale),
                Width = (int)Math.Round(rect.Width * scale),
                Height = (int)Math.Round(rect.Height * scale)
            };
        }

        /// <summary>
        /// 将窗口Rect从源DPI转换为目标DPI
        /// </summary>
        public WindowRect ConvertRectDpi(WindowRect rect, float sourceDpi, float targetDpi)
        {
            float scale = targetDpi / sourceDpi;
            return new WindowRect
            {
                X = (int)Math.Round(rect.X * scale),
                Y = (int)Math.Round(rect.Y * scale),
                Width = (int)Math.Round(rect.Width * scale),
                Height = (int)Math.Round(rect.Height * scale)
            };
        }

        /// <summary>
        /// 获取窗口的DPI
        /// </summary>
        public float GetDpiForWindow(nint hwnd)
        {
            try
            {
                uint dpi = PInvoke.GetDpiForWindow((HWND)hwnd);
                return dpi;
            }
            catch
            {
                // 如果 API 不可用，返回默认DPI
                return 96f;
            }
        }

        /// <summary>
        /// 在指定DPI下创建尺寸
        /// </summary>
        public Size CreateSizeForDpi(int width, int height, float dpi)
        {
            float scale = dpi / 96f;
            return new Size(
                (int)Math.Round(width * scale),
                (int)Math.Round(height * scale));
        }

        /// <summary>
        /// 获取系统DPI
        /// </summary>
        public float GetSystemDpi()
        {
            try
            {
                var hdc = PInvoke.GetDC((HWND)0);
                if (hdc != 0)
                {
                    int dpi = PInvoke.GetDeviceCaps(hdc, GET_DEVICE_CAPS_INDEX.LOGPIXELSX);
                    PInvoke.ReleaseDC((HWND)0, hdc);
                    return dpi;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError($"[DpiAwareness] 获取系统 DPI 失败: {ex.Message}");
            }
            return 96f;
        }

        /// <summary>
        /// 使窗口DPI感知
        /// </summary>
        public void MakeWindowDpiAware(nint hwnd)
        {
            try
            {
                // 使用 SetThreadDpiAwarenessContext 已经设置了线程级别的DPI感知
                // 对于特定窗口，可以使用 SetWindowDpiAwarenessContext
                var context = PInvoke.GetWindowDpiAwarenessContext((HWND)hwnd);
                _logger?.LogDebug($"[DpiAwareness] 窗口 DPI Awareness Context: {context.Value}");
            }
            catch (Exception ex)
            {
                _logger?.LogError($"[DpiAwareness] 设置窗口 DPI 感知失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 检测是否处于混合DPI环境
        /// </summary>
        public bool IsMixedDpiEnvironment()
        {
            var displays = GetAllDisplayDpiInfo();
            if (displays.Count <= 1)
            {
                return false;
            }

            var firstDpi = displays[0].DpiX;
            return displays.Any(d => Math.Abs(d.DpiX - firstDpi) > 1);
        }

        /// <summary>
        /// 获取混合DPI环境的统计信息
        /// </summary>
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

        /// <summary>
        /// 清除DPI缓存
        /// </summary>
        public void ClearCache()
        {
            lock (_lock)
            {
                _dpiCache.Clear();
            }
        }
    }

    /// <summary>
    /// 混合DPI环境统计
    /// </summary>
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
