using System;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Rectangle.Windows.Services;

/// <summary>
/// 窗口类型检测服务
/// 检测窗口的各种属性（对话框、可调整大小等）
/// </summary>
public class WindowTypeService
{
    // 窗口样式常量
    private const uint WS_THICKFRAME = 0x00040000;      // 可调整大小边框
    private const uint WS_DLGFRAME = 0x00400000;        // 对话框边框
    private const uint WS_BORDER = 0x00800000;          // 普通边框
    private const uint WS_CAPTION = 0x00C00000;         // 标题栏（WS_BORDER | WS_DLGFRAME）
    private const uint WS_MAXIMIZEBOX = 0x00010000;     // 最大化按钮
    private const uint WS_MINIMIZEBOX = 0x00020000;     // 最小化按钮
    private const uint WS_SYSMENU = 0x00080000;         // 系统菜单
    
    // 扩展窗口样式
    private const uint WS_EX_DLGMODALFRAME = 0x00000001; // 模态对话框边框
    private const uint WS_EX_TOOLWINDOW = 0x00000080;    // 工具窗口
    private const uint WS_EX_NOACTIVATE = 0x08000000;    // 不激活

    private readonly Win32WindowService _win32;

    public WindowTypeService(Win32WindowService win32)
    {
        _win32 = win32;
    }

    /// <summary>
    /// 检查窗口是否可调整大小
    /// </summary>
    public bool IsResizable(nint hwnd)
    {
        if (hwnd == 0) return false;
        
        try
        {
            var style = GetWindowStyle(hwnd);
            
            // 有 WS_THICKFRAME 样式表示可调整大小
            return (style & WS_THICKFRAME) != 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 检查窗口是否是对话框
    /// </summary>
    public bool IsDialog(nint hwnd)
    {
        if (hwnd == 0) return false;
        
        try
        {
            var style = GetWindowStyle(hwnd);
            var exStyle = GetWindowExStyle(hwnd);
            
            // 检查对话框样式
            bool hasDlgFrame = (style & WS_DLGFRAME) != 0 && (style & WS_THICKFRAME) == 0;
            bool hasModalFrame = (exStyle & WS_EX_DLGMODALFRAME) != 0;
            
            return hasDlgFrame || hasModalFrame;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 检查窗口是否是模态对话框
    /// </summary>
    public bool IsModalDialog(nint hwnd)
    {
        if (hwnd == 0) return false;
        
        try
        {
            var exStyle = GetWindowExStyle(hwnd);
            return (exStyle & WS_EX_DLGMODALFRAME) != 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 检查窗口是否是工具窗口
    /// </summary>
    public bool IsToolWindow(nint hwnd)
    {
        if (hwnd == 0) return false;
        
        try
        {
            var exStyle = GetWindowExStyle(hwnd);
            return (exStyle & WS_EX_TOOLWINDOW) != 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 检查窗口是否有标题栏
    /// </summary>
    public bool HasCaption(nint hwnd)
    {
        if (hwnd == 0) return false;
        
        try
        {
            var style = GetWindowStyle(hwnd);
            return (style & WS_CAPTION) != 0;
        }
        catch
        {
            return false;
        }
    }

    // 使用 P/Invoke 直接调用 GetWindowLong
    [DllImport("user32.dll", EntryPoint = "GetWindowLongW")]
    private static extern nint GetWindowLong32(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW")]
    private static extern nint GetWindowLong64(IntPtr hWnd, int nIndex);

    private static nint GetWindowLong(HWND hWnd, int nIndex)
    {
        if (nint.Size == 8)
            return GetWindowLong64(hWnd.Value, nIndex);
        else
            return GetWindowLong32(hWnd.Value, nIndex);
    }

    /// <summary>
    /// 检查窗口是否是最小化状态
    /// </summary>
    public bool IsMinimized(nint hwnd)
    {
        if (hwnd == 0) return false;
        
        try
        {
            return PInvoke.IsIconic(new HWND(hwnd));
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 检查窗口是否是最大化状态
    /// </summary>
    public bool IsMaximized(nint hwnd)
    {
        if (hwnd == 0) return false;
        
        try
        {
            return PInvoke.IsZoomed(new HWND(hwnd));
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 获取窗口样式
    /// </summary>
    private uint GetWindowStyle(nint hwnd)
    {
        return (uint)GetWindowLong(new HWND(hwnd), -16); // GWL_STYLE = -16
    }

    /// <summary>
    /// 获取窗口扩展样式
    /// </summary>
    private uint GetWindowExStyle(nint hwnd)
    {
        return (uint)GetWindowLong(new HWND(hwnd), -20); // GWL_EXSTYLE = -20
    }


}
