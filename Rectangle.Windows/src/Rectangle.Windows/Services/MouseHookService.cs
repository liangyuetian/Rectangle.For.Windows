using System;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Rectangle.Windows.Services;

/// <summary>
/// 全局鼠标钩子服务
/// 监听全局鼠标事件用于拖拽吸附功能
/// </summary>
public class MouseHookService : IDisposable
{
    // Win32 常量
    private const int WH_MOUSE_LL = 14;  // 低层鼠标钩子
    private const int WM_LBUTTONDOWN = 0x0201;  // 左键按下
    private const int WM_LBUTTONUP = 0x0202;    // 左键释放
    private const int WM_MOUSEMOVE = 0x0200;    // 鼠标移动
    private const int WM_RBUTTONDOWN = 0x0204;  // 右键按下
    private const int WM_RBUTTONUP = 0x0205;    // 右键释放

    // 钩子句柄
    private HHOOK _hookHandle;
    private HOOKPROC? _hookProc;
    private bool _isHookInstalled = false;

    // 事件
    public event EventHandler<MouseHookEventArgs>? MouseDown;
    public event EventHandler<MouseHookEventArgs>? MouseUp;
    public event EventHandler<MouseHookEventArgs>? MouseMove;

    /// <summary>
    /// 安装鼠标钩子
    /// </summary>
    public bool InstallHook()
    {
        if (_isHookInstalled) return true;

        try
        {
            // 创建钩子回调
            _hookProc = HookCallback;

            // 安装低层鼠标钩子
            _hookHandle = PInvoke.SetWindowsHookEx(
                WINDOWS_HOOK_ID.WH_MOUSE_LL,
                _hookProc,
                new HINSTANCE(System.Diagnostics.Process.GetCurrentProcess().Handle),
                0);

            if (_hookHandle.Value == IntPtr.Zero)
            {
                Console.WriteLine("[MouseHookService] 安装鼠标钩子失败");
                return false;
            }

            _isHookInstalled = true;
            Console.WriteLine("[MouseHookService] 鼠标钩子安装成功");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MouseHookService] 安装钩子异常: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 卸载鼠标钩子
    /// </summary>
    public void UninstallHook()
    {
        if (!_isHookInstalled) return;

        try
        {
            if (_hookHandle.Value != IntPtr.Zero)
            {
                PInvoke.UnhookWindowsHookEx(_hookHandle);
                _hookHandle = new HHOOK(IntPtr.Zero);
            }

            _isHookInstalled = false;
            _hookProc = null;
            Console.WriteLine("[MouseHookService] 鼠标钩子已卸载");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MouseHookService] 卸载钩子异常: {ex.Message}");
        }
    }

    /// <summary>
    /// 钩子回调函数
    /// </summary>
    private LRESULT HookCallback(int nCode, WPARAM wParam, LPARAM lParam)
    {
        if (nCode >= 0)
        {
            try
            {
                // 获取鼠标钩子结构
                var mouseHookStruct = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
                var args = new MouseHookEventArgs
                {
                    X = mouseHookStruct.pt.x,
                    Y = mouseHookStruct.pt.y,
                    Timestamp = (int)mouseHookStruct.time
                };

                uint msg = (uint)wParam.Value;

                switch (msg)
                {
                    case WM_LBUTTONDOWN:
                        MouseDown?.Invoke(this, args);
                        break;
                    case WM_LBUTTONUP:
                        MouseUp?.Invoke(this, args);
                        break;
                    case WM_MOUSEMOVE:
                        MouseMove?.Invoke(this, args);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MouseHookService] 钩子回调异常: {ex.Message}");
            }
        }

        // 继续传递消息
        return PInvoke.CallNextHookEx(HHOOK.Null, nCode, wParam, lParam);
    }

    // P/Invoke for GetCursorPos
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetCursorPos(out POINT lpPoint);

    /// <summary>
    /// 获取当前光标位置
    /// </summary>
    public static System.Drawing.Point GetCursorPosition()
    {
        if (GetCursorPos(out var pt))
        {
            return new System.Drawing.Point(pt.x, pt.y);
        }
        return new System.Drawing.Point(0, 0);
    }

    /// <summary>
    /// 获取光标下的窗口句柄
    /// </summary>
    public static nint GetWindowUnderCursor()
    {
        var pt = GetCursorPosition();
        return (nint)PInvoke.WindowFromPoint(pt).Value;
    }

    public void Dispose()
    {
        UninstallHook();
    }
}

/// <summary>
/// 鼠标钩子事件参数
/// </summary>
public class MouseHookEventArgs : EventArgs
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Timestamp { get; set; }
    public System.Windows.Forms.MouseButtons Button { get; set; }

    public System.Drawing.Point Point => new(X, Y);
}

/// <summary>
/// Win32 POINT 结构
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct POINT
{
    public int x;
    public int y;
}

/// <summary>
/// 低层鼠标钩子结构
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct MSLLHOOKSTRUCT
{
    public POINT pt;
    public uint mouseData;
    public uint flags;
    public uint time;
    public UIntPtr dwExtraInfo;
}
