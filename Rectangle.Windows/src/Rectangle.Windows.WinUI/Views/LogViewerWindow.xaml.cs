using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage.Pickers;

namespace Rectangle.Windows.WinUI.Views
{
    public sealed class LogViewerWindow : Window
    {
        private string _logFilePath = "";
        private FileSystemWatcher? _fileWatcher;

        private readonly AppBarToggleButton _autoScrollButton;
        private readonly ScrollViewer _logScrollViewer;
        private readonly TextBox _logTextBox;
        private readonly TextBlock _statusText;
        private readonly TextBlock _lineCountText;

        public LogViewerWindow()
        {
            Title = "日志查看器";

            // 工具栏
            var refreshBtn = new AppBarButton { Icon = new SymbolIcon(Symbol.Refresh), Label = "刷新" };
            refreshBtn.Click += (s, e) => LoadLogs();

            var clearBtn = new AppBarButton { Icon = new SymbolIcon(Symbol.Clear), Label = "清空" };
            clearBtn.Click += Clear_Click;

            var exportBtn = new AppBarButton { Icon = new SymbolIcon(Symbol.Save), Label = "导出" };
            exportBtn.Click += Export_Click;

            _autoScrollButton = new AppBarToggleButton { Icon = new SymbolIcon(Symbol.Sort), Label = "自动滚动", IsChecked = true };

            var commandBar = new CommandBar { DefaultLabelPosition = CommandBarDefaultLabelPosition.Right };
            commandBar.PrimaryCommands.Add(refreshBtn);
            commandBar.PrimaryCommands.Add(clearBtn);
            commandBar.PrimaryCommands.Add(new AppBarSeparator());
            commandBar.PrimaryCommands.Add(exportBtn);
            commandBar.PrimaryCommands.Add(new AppBarSeparator());
            commandBar.PrimaryCommands.Add(_autoScrollButton);

            // 日志文本框
            _logTextBox = new TextBox
            {
                IsReadOnly = true,
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                FontFamily = new FontFamily("Consolas"),
                FontSize = 12,
                BorderThickness = new Thickness(0),
                Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent)
            };

            _logScrollViewer = new ScrollViewer { Content = _logTextBox };

            // 状态栏
            _statusText = new TextBlock { Text = "就绪", VerticalAlignment = VerticalAlignment.Center };
            _lineCountText = new TextBlock { Text = "0 行", VerticalAlignment = VerticalAlignment.Center };

            var statusGrid = new Grid { Padding = new Thickness(12, 8, 12, 8) };
            statusGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            statusGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            Grid.SetColumn(_statusText, 0);
            Grid.SetColumn(_lineCountText, 1);
            statusGrid.Children.Add(_statusText);
            statusGrid.Children.Add(_lineCountText);

            // 主布局
            var rootGrid = new Grid();
            rootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            rootGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            rootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            Grid.SetRow(commandBar, 0);
            Grid.SetRow(_logScrollViewer, 1);
            Grid.SetRow(statusGrid, 2);
            rootGrid.Children.Add(commandBar);
            rootGrid.Children.Add(_logScrollViewer);
            rootGrid.Children.Add(statusGrid);

            Content = rootGrid;

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
                    _logTextBox.Text = "日志文件不存在";
                    return;
                }

                var content = await File.ReadAllTextAsync(_logFilePath);
                _logTextBox.Text = content;
                UpdateStatus();

                if (_autoScrollButton.IsChecked == true)
                    _logScrollViewer.ChangeView(null, _logScrollViewer.ScrollableHeight, null);
            }
            catch (Exception ex)
            {
                _logTextBox.Text = $"读取日志失败: {ex.Message}";
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
                _fileWatcher.Changed += (s, e) => DispatcherQueue.TryEnqueue(LoadLogs);
                _fileWatcher.EnableRaisingEvents = true;
            }
            catch { }
        }

        private void UpdateStatus()
        {
            var lines = _logTextBox.Text.Split('\n').Length;
            _lineCountText.Text = $"{lines} 行";
            _statusText.Text = _logFilePath;
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

            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                try
                {
                    File.WriteAllText(_logFilePath, $"日志已清空于 {DateTime.Now}\n");
                    LoadLogs();
                }
                catch (Exception ex)
                {
                    await ShowDialog($"清空日志失败: {ex.Message}");
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

                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
                WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);

                var file = await savePicker.PickSaveFileAsync();
                if (file != null)
                {
                    await File.WriteAllTextAsync(file.Path, _logTextBox.Text);
                    await ShowDialog($"日志已导出到: {file.Path}");
                }
            }
            catch (Exception ex)
            {
                await ShowDialog($"导出日志失败: {ex.Message}");
            }
        }

        private async Task ShowDialog(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "提示",
                Content = message,
                PrimaryButtonText = "确定",
                XamlRoot = this.Content.XamlRoot
            };
            await dialog.ShowAsync();
        }
    }
}
