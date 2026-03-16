using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Rectangle.Windows.WinUI.Core;

namespace Rectangle.Windows.WinUI.Services
{
    /// <summary>
    /// 窗口布局管理服务 - 保存和恢复窗口布局
    /// </summary>
    public class LayoutManager
    {
        private readonly ConfigService _configService;
        private readonly Win32WindowService _win32;
        private readonly string _layoutsFilePath;

        public LayoutManager(ConfigService configService, Win32WindowService win32)
        {
            _configService = configService;
            _win32 = win32;

            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _layoutsFilePath = Path.Combine(appData, "Rectangle", "layouts.json");
        }

        /// <summary>
        /// 保存当前窗口布局
        /// </summary>
        public async Task<WindowLayout> SaveCurrentLayoutAsync(string name)
        {
            var layout = new WindowLayout
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                CreatedAt = DateTime.Now,
                Windows = new List<WindowPositionInfo>()
            };

            // 枚举所有可见窗口
            var windows = WindowEnumerator.EnumerateVisibleWindows();
            var screenService = new ScreenDetectionService(_win32);
            var workArea = screenService.GetPrimaryWorkArea();

            foreach (var hwnd in windows)
            {
                try
                {
                    var (x, y, w, h) = _win32.GetWindowRect(hwnd);
                    var title = GetWindowTitle(hwnd);
                    var processName = WindowEnumerator.GetProcessNameFromWindow(hwnd);

                    // 只保存在工作区内的窗口
                    if (IsWindowInWorkArea(x, y, w, h, workArea))
                    {
                        layout.Windows.Add(new WindowPositionInfo
                        {
                            ProcessName = processName,
                            WindowTitle = title,
                            RelativeX = (double)(x - workArea.X) / workArea.Width,
                            RelativeY = (double)(y - workArea.Y) / workArea.Height,
                            RelativeWidth = (double)w / workArea.Width,
                            RelativeHeight = (double)h / workArea.Height
                        });
                    }
                }
                catch { }
            }

            // 保存到文件
            var layouts = await LoadLayoutsAsync();
            layouts.Add(layout);
            await SaveLayoutsAsync(layouts);

            Logger.Info("LayoutManager", $"保存布局: {name}, 包含 {layout.Windows.Count} 个窗口");
            return layout;
        }

        /// <summary>
        /// 恢复窗口布局
        /// </summary>
        public async Task RestoreLayoutAsync(string layoutId)
        {
            var layouts = await LoadLayoutsAsync();
            var layout = layouts.Find(l => l.Id == layoutId);

            if (layout == null)
            {
                Logger.Warning("LayoutManager", $"未找到布局: {layoutId}");
                return;
            }

            var screenService = new ScreenDetectionService(_win32);
            var workArea = screenService.GetPrimaryWorkArea();

            int restoredCount = 0;
            foreach (var window in layout.Windows)
            {
                try
                {
                    // 查找匹配的窗口
                    var hwnd = FindWindowByProcessAndTitle(window.ProcessName, window.WindowTitle);

                    if (hwnd != 0)
                    {
                        var x = workArea.X + (int)(window.RelativeX * workArea.Width);
                        var y = workArea.Y + (int)(window.RelativeY * workArea.Height);
                        var w = (int)(window.RelativeWidth * workArea.Width);
                        var h = (int)(window.RelativeHeight * workArea.Height);

                        _win32.SetWindowRect(hwnd, x, y, w, h);
                        restoredCount++;
                    }
                }
                catch { }
            }

            Logger.Info("LayoutManager", $"恢复布局: {layout.Name}, 成功 {restoredCount}/{layout.Windows.Count}");
        }

        /// <summary>
        /// 删除布局
        /// </summary>
        public async Task DeleteLayoutAsync(string layoutId)
        {
            var layouts = await LoadLayoutsAsync();
            layouts.RemoveAll(l => l.Id == layoutId);
            await SaveLayoutsAsync(layouts);

            Logger.Info("LayoutManager", $"删除布局: {layoutId}");
        }

        /// <summary>
        /// 获取所有布局
        /// </summary>
        public async Task<List<WindowLayout>> GetLayoutsAsync()
        {
            return await LoadLayoutsAsync();
        }

        /// <summary>
        /// 导出布局
        /// </summary>
        public async Task<string> ExportLayoutAsync(string layoutId)
        {
            var layouts = await LoadLayoutsAsync();
            var layout = layouts.Find(l => l.Id == layoutId);

            if (layout == null) return string.Empty;

            return JsonSerializer.Serialize(layout, AppJsonContext.Default.WindowLayout);
        }

        /// <summary>
        /// 导入布局
        /// </summary>
        public async Task<WindowLayout?> ImportLayoutAsync(string json)
        {
            try
            {
                var layout = JsonSerializer.Deserialize(json, AppJsonContext.Default.WindowLayout);
                if (layout != null)
                {
                    layout.Id = Guid.NewGuid().ToString();
                    layout.CreatedAt = DateTime.Now;

                    var layouts = await LoadLayoutsAsync();
                    layouts.Add(layout);
                    await SaveLayoutsAsync(layouts);

                    Logger.Info("LayoutManager", $"导入布局: {layout.Name}");
                    return layout;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("LayoutManager", $"导入布局失败: {ex.Message}");
            }

            return null;
        }

        private async Task<List<WindowLayout>> LoadLayoutsAsync()
        {
            try
            {
                if (File.Exists(_layoutsFilePath))
                {
                    var json = await File.ReadAllTextAsync(_layoutsFilePath);
                    return JsonSerializer.Deserialize(json, AppJsonContext.Default.ListWindowLayout) ?? new List<WindowLayout>();
                }
            }
            catch { }

            return new List<WindowLayout>();
        }

        private async Task SaveLayoutsAsync(List<WindowLayout> layouts)
        {
            try
            {
                var dir = Path.GetDirectoryName(_layoutsFilePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                var json = JsonSerializer.Serialize(layouts, AppJsonContext.Default.ListWindowLayout);
                await File.WriteAllTextAsync(_layoutsFilePath, json);
            }
            catch (Exception ex)
            {
                Logger.Error("LayoutManager", $"保存布局失败: {ex.Message}");
            }
        }

        private string GetWindowTitle(nint hwnd)
        {
            try
            {
                var length = global::Windows.Win32.PInvoke.GetWindowTextLength(new global::Windows.Win32.Foundation.HWND(hwnd));
                if (length == 0) return string.Empty;

                unsafe
                {
                    char* buffer = stackalloc char[length + 1];
                    global::Windows.Win32.PInvoke.GetWindowText(new global::Windows.Win32.Foundation.HWND(hwnd), buffer, length + 1);
                    return new string(buffer);
                }
            }
            catch { }
            return string.Empty;
        }

        private bool IsWindowInWorkArea(int x, int y, int w, int h, WindowRect workArea)
        {
            return x >= workArea.X && y >= workArea.Y &&
                   x + w <= workArea.X + workArea.Width &&
                   y + h <= workArea.Y + workArea.Height;
        }

        private nint FindWindowByProcessAndTitle(string processName, string windowTitle)
        {
            var windows = WindowEnumerator.EnumerateWindowsByProcess(processName, false);

            foreach (var hwnd in windows)
            {
                var title = GetWindowTitle(hwnd);
                if (title == windowTitle)
                {
                    return hwnd;
                }
            }

            // 如果没找到完全匹配的，返回第一个
            return windows.Count > 0 ? windows[0] : 0;
        }
    }

    public class WindowLayout
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public List<WindowPositionInfo> Windows { get; set; } = new();
    }

    public class WindowPositionInfo
    {
        public string ProcessName { get; set; } = "";
        public string WindowTitle { get; set; } = "";
        public double RelativeX { get; set; }
        public double RelativeY { get; set; }
        public double RelativeWidth { get; set; }
        public double RelativeHeight { get; set; }
    }
}
