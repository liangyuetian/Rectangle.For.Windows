using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;
using Rectangle.Windows.Services;

namespace Rectangle.Windows.Views;

/// <summary>
/// 日志查看器窗口
/// </summary>
public class LogViewerForm : Form
{
    private ListBox _logListBox = null!;
    private ComboBox _levelFilter = null!;
    private TextBox _searchBox = null!;
    private Button _refreshButton = null!;
    private Button _exportButton = null!;
    private Button _clearButton = null!;
    private CheckBox _autoScroll = null!;
    private Timer? _refreshTimer;
    private string _logFilePath = "";

    public LogViewerForm()
    {
        InitializeComponent();
        LoadLogFilePath();
    }

    private void InitializeComponent()
    {
        Text = "Rectangle 日志查看器";
        Size = new Size(900, 600);
        StartPosition = FormStartPosition.CenterScreen;

        // 工具栏
        var toolBar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 40,
            Padding = new Padding(5)
        };

        _levelFilter = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 100,
            Location = new Point(5, 8)
        };
        _levelFilter.Items.AddRange(new object[] { "All", "Debug", "Info", "Warning", "Error" });
        _levelFilter.SelectedIndex = 0;
        _levelFilter.SelectedIndexChanged += OnFilterChanged;

        _searchBox = new TextBox
        {
            Width = 200,
            Location = new Point(110, 8),
            PlaceholderText = "搜索日志..."
        };
        _searchBox.TextChanged += OnFilterChanged;

        _refreshButton = new Button
        {
            Text = "刷新",
            Location = new Point(320, 6),
            Width = 60
        };
        _refreshButton.Click += OnRefreshClicked;

        _exportButton = new Button
        {
            Text = "导出",
            Location = new Point(390, 6),
            Width = 60
        };
        _exportButton.Click += OnExportClicked;

        _clearButton = new Button
        {
            Text = "清空",
            Location = new Point(460, 6),
            Width = 60
        };
        _clearButton.Click += OnClearClicked;

        _autoScroll = new CheckBox
        {
            Text = "自动滚动",
            Location = new Point(530, 10),
            Checked = true
        };

        toolBar.Controls.AddRange(new Control[] {
            _levelFilter, _searchBox, _refreshButton, _exportButton, _clearButton, _autoScroll
        });

        // 日志列表
        _logListBox = new ListBox
        {
            Dock = DockStyle.Fill,
            Font = new Font("Consolas", 9F),
            HorizontalScrollbar = true,
            IntegralHeight = false,
            SelectionMode = SelectionMode.MultiExtended
        };

        Controls.Add(_logListBox);
        Controls.Add(toolBar);

        // 自动刷新定时器
        _refreshTimer = new Timer
        {
            Interval = 2000 // 2秒
        };
        _refreshTimer.Tick += OnRefreshClicked;
        _refreshTimer.Start();
    }

    private void LoadLogFilePath()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _logFilePath = Path.Combine(appData, "Rectangle", "logs", "rectangle.log");
    }

    private void OnFilterChanged(object? sender, EventArgs e)
    {
        LoadLogs();
    }

    private void OnRefreshClicked(object? sender, EventArgs e)
    {
        LoadLogs();
    }

    private void LoadLogs()
    {
        if (!File.Exists(_logFilePath))
        {
            _logListBox.Items.Clear();
            _logListBox.Items.Add("日志文件不存在: " + _logFilePath);
            return;
        }

        var selectedLevel = _levelFilter.SelectedItem?.ToString() ?? "All";
        var searchText = _searchBox.Text?.ToLower() ?? "";

        try
        {
            var lines = File.ReadAllLines(_logFilePath);
            var filteredLines = new List<string>();

            foreach (var line in lines)
            {
                // 级别过滤
                if (selectedLevel != "All")
                {
                    var levelPrefix = $"[{selectedLevel.ToUpper()}]";
                    if (!line.Contains(levelPrefix))
                        continue;
                }

                // 搜索过滤
                if (!string.IsNullOrEmpty(searchText))
                {
                    if (!line.ToLower().Contains(searchText))
                        continue;
                }

                filteredLines.Add(line);
            }

            // 只显示最后 1000 行以提高性能
            var displayLines = filteredLines.Count > 1000
                ? filteredLines.Skip(filteredLines.Count - 1000).ToList()
                : filteredLines;

            _logListBox.Items.Clear();
            _logListBox.Items.AddRange(displayLines.ToArray());

            if (_autoScroll.Checked && _logListBox.Items.Count > 0)
            {
                _logListBox.SelectedIndex = _logListBox.Items.Count - 1;
                _logListBox.TopIndex = _logListBox.Items.Count - 1;
            }
        }
        catch (Exception ex)
        {
            _logListBox.Items.Clear();
            _logListBox.Items.Add($"读取日志失败: {ex.Message}");
        }
    }

    private void OnExportClicked(object? sender, EventArgs e)
    {
        using var dialog = new SaveFileDialog
        {
            Filter = "日志文件 (*.log)|*.log|文本文件 (*.txt)|*.txt|所有文件 (*.*)|*.*",
            FileName = $"rectangle_export_{DateTime.Now:yyyyMMdd_HHmmss}.log"
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            try
            {
                var content = string.Join(Environment.NewLine, _logListBox.Items.Cast<string>());
                File.WriteAllText(dialog.FileName, content);
                MessageBox.Show("日志导出成功！", "导出成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"导出失败: {ex.Message}", "导出失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void OnClearClicked(object? sender, EventArgs e)
    {
        var result = MessageBox.Show(
            "确定要清空日志文件吗？此操作不可恢复。",
            "确认清空",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (result == DialogResult.Yes)
        {
            try
            {
                if (File.Exists(_logFilePath))
                {
                    File.WriteAllText(_logFilePath, "");
                }
                LoadLogs();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"清空失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _refreshTimer?.Stop();
        _refreshTimer?.Dispose();
        base.OnFormClosing(e);
    }
}
