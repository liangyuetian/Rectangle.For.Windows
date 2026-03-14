using System;
using System.Runtime.InteropServices;
using Rectangle.Windows.Core;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;

namespace Rectangle.Windows.Services;

/// <summary>
/// 屏幕检测服务
/// 支持使用光标位置或窗口位置检测目标屏幕
/// </summary>
public class ScreenDetectionService
{
    private readonly Win32WindowService _win32;

    public ScreenDetectionService(Win32WindowService win32)
    {
        _win32 = win32;
    }

    /// <summary>
    /// 获取光标当前所在的屏幕工作区
    /// </summary>
    public WorkArea GetWorkAreaFromCursor()
    {
        // 获取光标位置
        var pt = GetCursorPosition();
        
        // 获取光标所在的显示器
        var hMonitor = PInvoke.MonitorFromPoint(pt, MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST);
        
        return GetWorkAreaFromMonitor(hMonitor);
    }

    /// <summary>
    /// 获取窗口所在的屏幕工作区
    /// </summary>
    public WorkArea GetWorkAreaFromWindow(nint hwnd)
    {
        return _win32.GetWorkAreaFromWindow(hwnd);
    }

    /// <summary>
    /// 获取包含指定点的屏幕工作区
    /// </summary>
    public WorkArea GetWorkAreaFromPoint(int x, int y)
    {
        var pt = new System.Drawing.Point(x, y);
        var hMonitor = PInvoke.MonitorFromPoint(pt, MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST);
        return GetWorkAreaFromMonitor(hMonitor);
    }

    /// <summary>
    /// 根据配置获取目标屏幕工作区
    /// </summary>
    public WorkArea GetTargetWorkArea(nint hwnd, ConfigService? configService)
    {
        var config = configService?.Load();
        
        // 如果配置启用光标位置检测
        if (config?.UseCursorScreenDetection == true)
        {
            return GetWorkAreaFromCursor();
        }
        
        // 默认使用窗口位置检测
        return GetWorkAreaFromWindow(hwnd);
    }

    /// <summary>
    /// 从显示器句柄获取工作区
    /// </summary>
    private WorkArea GetWorkAreaFromMonitor(HMONITOR hMonitor)
    {
        var mi = new MONITORINFO();
        mi.cbSize = (uint)Marshal.SizeOf<MONITORINFO>();
        
        if (PInvoke.GetMonitorInfo(hMonitor, ref mi))
        {
            var rcWork = mi.rcWork;
            return new WorkArea(rcWork.left, rcWork.top, rcWork.right, rcWork.bottom);
        }
        
        // 如果获取失败，返回主屏幕
        return GetPrimaryWorkArea();
    }

    /// <summary>
    /// 获取主屏幕工作区
    /// </summary>
    private WorkArea GetPrimaryWorkArea()
    {
        var hMonitor = PInvoke.MonitorFromPoint(new System.Drawing.Point(0, 0), MONITOR_FROM_FLAGS.MONITOR_DEFAULTTOPRIMARY);
        return GetWorkAreaFromMonitor(hMonitor);
    }

    /// <summary>
    /// 获取光标位置
    /// </summary>
    private System.Drawing.Point GetCursorPosition()
    {
        return MouseHookService.GetCursorPosition();
    }
}
