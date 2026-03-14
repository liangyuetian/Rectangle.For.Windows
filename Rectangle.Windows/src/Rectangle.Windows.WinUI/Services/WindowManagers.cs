using System;
using System.Collections.Generic;
using System.Linq;
using Rectangle.Windows.WinUI.Core;

namespace Rectangle.Windows.WinUI.Services
{
    /// <summary>
    /// 层叠所有窗口管理器
    /// </summary>
    public class CascadeAllManager
    {
        private readonly Win32WindowService _win32;
        private readonly ConfigService _configService;

        public CascadeAllManager(Win32WindowService win32, ConfigService configService)
        {
            _win32 = win32;
            _configService = configService;
        }

        /// <summary>
        /// 层叠所有可见窗口
        /// </summary>
        public void CascadeAll()
        {
            var windows = WindowEnumerator.EnumerateVisibleWindows();
            var config = _configService.Load();
            var delta = config.CascadeAllDeltaSize;

            var screenService = new ScreenDetectionService(_win32);
            var workArea = screenService.GetPrimaryWorkArea();

            int x = workArea.X;
            int y = workArea.Y;
            int index = 0;

            foreach (var hwnd in windows)
            {
                int newX = x + (index * delta);
                int newY = y + (index * delta);

                // 确保不超出屏幕
                if (newX + 400 > workArea.X + workArea.Width)
                    newX = workArea.X + workArea.Width - 400;
                if (newY + 300 > workArea.Y + workArea.Height)
                    newY = workArea.Y + workArea.Height - 300;

                _win32.SetWindowRect(hwnd, newX, newY, 800, 600);
                index++;
            }

            Logger.Info("CascadeAllManager", $"层叠了 {index} 个窗口");
        }

        /// <summary>
        /// 层叠指定进程的所有窗口
        /// </summary>
        public void CascadeActiveApp(string processName)
        {
            var windows = WindowEnumerator.EnumerateWindowsByProcess(processName);
            var config = _configService.Load();
            var delta = config.CascadeAllDeltaSize;

            var screenService = new ScreenDetectionService(_win32);
            var workArea = screenService.GetPrimaryWorkArea();

            int x = workArea.X;
            int y = workArea.Y;
            int index = 0;

            foreach (var hwnd in windows)
            {
                int newX = x + (index * delta);
                int newY = y + (index * delta);

                if (newX + 400 > workArea.X + workArea.Width)
                    newX = workArea.X + workArea.Width - 400;
                if (newY + 300 > workArea.Y + workArea.Height)
                    newY = workArea.Y + workArea.Height - 300;

                _win32.SetWindowRect(hwnd, newX, newY, 800, 600);
                index++;
            }

            Logger.Info("CascadeAllManager", $"层叠了 {processName} 的 {index} 个窗口");
        }
    }

    /// <summary>
    /// 平铺所有窗口管理器
    /// </summary>
    public class TileAllManager
    {
        private readonly Win32WindowService _win32;
        private readonly ConfigService _configService;

        public TileAllManager(Win32WindowService win32, ConfigService configService)
        {
            _win32 = win32;
            _configService = configService;
        }

        /// <summary>
        /// 平铺所有可见窗口
        /// </summary>
        public void TileAll()
        {
            var windows = WindowEnumerator.EnumerateVisibleWindows();
            var screenService = new ScreenDetectionService(_win32);
            var workArea = screenService.GetPrimaryWorkArea();

            int count = windows.Count;
            if (count == 0) return;

            // 计算网格布局
            int cols = (int)Math.Ceiling(Math.Sqrt(count));
            int rows = (int)Math.Ceiling((double)count / cols);

            int cellWidth = workArea.Width / cols;
            int cellHeight = workArea.Height / rows;

            // 应用间隙
            var config = _configService.Load();
            int gap = config.GapSize;

            for (int i = 0; i < count; i++)
            {
                int col = i % cols;
                int row = i / cols;

                int x = workArea.X + (col * cellWidth) + (col > 0 ? gap / 2 : 0);
                int y = workArea.Y + (row * cellHeight) + (row > 0 ? gap / 2 : 0);
                int w = cellWidth - gap;
                int h = cellHeight - gap;

                _win32.SetWindowRect(windows[i], x, y, w, h);
            }

            Logger.Info("TileAllManager", $"平铺了 {count} 个窗口 ({cols}x{rows})");
        }
    }

    /// <summary>
    /// 恢复所有窗口管理器
    /// </summary>
    public class ReverseAllManager
    {
        private readonly Win32WindowService _win32;
        private readonly WindowHistory _history;

        public ReverseAllManager(Win32WindowService win32, WindowHistory history)
        {
            _win32 = win32;
            _history = history;
        }

        /// <summary>
        /// 恢复所有窗口到原始位置
        /// </summary>
        public void ReverseAll()
        {
            var windows = WindowEnumerator.EnumerateVisibleWindows();
            int restoredCount = 0;

            foreach (var hwnd in windows)
            {
                if (_history.TryGet(hwnd, out var rect))
                {
                    _win32.SetWindowRect(hwnd, rect.X, rect.Y, rect.W, rect.H);
                    _history.Remove(hwnd);
                    restoredCount++;
                }
            }

            Logger.Info("ReverseAllManager", $"恢复了 {restoredCount} 个窗口");
        }
    }
}
