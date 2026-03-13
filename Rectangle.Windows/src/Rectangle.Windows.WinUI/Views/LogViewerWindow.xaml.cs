using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace Rectangle.Windows.WinUI.Views
{
    public sealed partial class LogViewerWindow : Window
    {
        private string _logFilePath = "";
        private FileSystemWatcher? _fileWatcher;
        private long _lastFileSize;

        public LogViewerWindow()
        {
            this.InitializeComponent();
            LoadLogFilePath();
            LoadLogs();
            SetupFileWatcher();
        }

        private void LoadLogFilePath()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _logFilePath = Path.Combine(appData, "Rectangle", "logs", "rectangle.log");
        }

        private async void LoadLogs()
        {
            try
            {
                if (!File.Exists(_logFilePath))
                {
                    LogTextBox.Text = "日志文件不存在";
                    return;
                }

                var content = await File.ReadAllTextAsync(_logFilePath);
                LogTextBox.Text = content;
                _lastFileSize = new FileInfo(_logFilePath).Length;

                UpdateStatus();

                // 自动滚动到底部
                if (AutoScrollButton.IsChecked == true)
                {
                    LogScrollViewer.ChangeView(null, LogScrollViewer.ScrollableHeight, null);
                }
            }
            catch (Exception ex)
            {
                LogTextBox.Text = $"读取日志失败: {ex.Message}";
            }
        }

        private void SetupFileWatcher()
        {
            try
            {
                var dir = Path.GetDirectoryName(_logFilePath);
                if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir)) return;

                _fileWatcher = new FileSystemWatcher(dir, Path.GetFileName(_logFilePath))
                {
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size
                };

                _fileWatcher.Changed += (s, e) =>
                {
                    DispatcherQueue.TryEnqueue(LoadLogs);
                };

                _fileWatcher.EnableRaisingEvents = true;
            }
            catch { }
        }

        private void UpdateStatus()
        {
            var lines = LogTextBox.Text.Split('\n').Length;
            LineCountText.Text = $"{lines} 行";
            StatusText.Text = $"{_logFilePath}";
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadLogs();
        }

        private async void Clear_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "确认清空",
                Content = "确定要清空日志文件吗？此操作不可恢复。",
                PrimaryButtonText = "清空",
                SecondaryButtonText = "取消",
                DefaultButton = ContentDialogButton.Secondary,
                XamlRoot = this.Content.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                try
                {
                    File.WriteAllText(_logFilePath, $"日志已清空于 {DateTime.Now}\n");
                    LoadLogs();
                }
                catch (Exception ex)
                {
                    await ShowErrorDialog($"清空日志失败: {ex.Message}");
                }
            }
        }

        private async void Export_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var savePicker = new FileSavePicker();
                savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                savePicker.SuggestedFileName = $"rectangle_export_{DateTime.Now:yyyyMMdd_HHmmss}.log";
                savePicker.FileTypeChoices.Add("日志文件", new[] { ".log" });
                savePicker.FileTypeChoices.Add("文本文件", new[] { ".txt" });

                // 需要设置窗口句柄
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
                WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);

                var file = await savePicker.PickSaveFileAsync();
                if (file != null)
                {
                    await File.WriteAllTextAsync(file.Path, LogTextBox.Text);

                    var successDialog = new ContentDialog
                    {
                        Title = "导出成功",
                        Content = $"日志已导出到: {file.Path}",
                        PrimaryButtonText = "确定",
                        XamlRoot = this.Content.XamlRoot
                    };
                    await successDialog.ShowAsync();
                }
            }
            catch (Exception ex)
            {
                await ShowErrorDialog($"导出日志失败: {ex.Message}");
            }
        }

        private async Task ShowErrorDialog(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "错误",
                Content = message,
                PrimaryButtonText = "确定",
                XamlRoot = this.Content.XamlRoot
            };
            await dialog.ShowAsync();
        }
    }
}
