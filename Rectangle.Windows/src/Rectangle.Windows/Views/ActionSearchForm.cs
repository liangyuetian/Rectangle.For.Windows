using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Rectangle.Windows.Core;
using Rectangle.Windows.Services;

namespace Rectangle.Windows.Views;

/// <summary>
/// 动作搜索窗口 - 快速搜索并执行窗口操作
/// </summary>
public class ActionSearchForm : Form
{
    private TextBox _searchBox;
    private ListView _resultList;
    private WindowManager _windowManager;
    private ConfigService _configService;
    private List<ActionItem> _allActions;
    private List<ActionItem> _filteredActions;

    public ActionSearchForm(WindowManager windowManager, ConfigService configService)
    {
        _windowManager = windowManager;
        _configService = configService;
        InitializeComponents();
        LoadAllActions();
    }

    private void InitializeComponents()
    {
        Text = "搜索动作";
        Size = new Size(500, 400);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.Sizable;
        MinimizeBox = false;
        MaximizeBox = false;
        ShowInTaskbar = false;
        KeyPreview = true;

        // 搜索框
        _searchBox = new TextBox
        {
            Dock = DockStyle.Top,
            Font = new Font("Microsoft YaHei UI", 12F),
            Padding = new Padding(8),
            Margin = new Padding(8),
            Height = 36
        };
        _searchBox.TextChanged += SearchBox_TextChanged;
        _searchBox.KeyDown += SearchBox_KeyDown;

        // 结果列表
        _resultList = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true,
            GridLines = false,
            BorderStyle = BorderStyle.None,
            HeaderStyle = ColumnHeaderStyle.None,
            MultiSelect = false,
            Font = new Font("Microsoft YaHei UI", 11F)
        };
        _resultList.Columns.Add("动作", 280);
        _resultList.Columns.Add("快捷键", 150);
        _resultList.DoubleClick += ResultList_DoubleClick;
        _resultList.KeyDown += ResultList_KeyDown;

        // 添加控件
        Controls.Add(_resultList);
        Controls.Add(_searchBox);

        // 设置主题
        ApplyTheme();
    }

    private void ApplyTheme()
    {
        var isDark = IsDarkTheme();
        BackColor = isDark ? Color.FromArgb(32, 32, 32) : Color.White;
        ForeColor = isDark ? Color.White : Color.Black;
        _searchBox.BackColor = BackColor;
        _searchBox.ForeColor = ForeColor;
        _resultList.BackColor = BackColor;
        _resultList.ForeColor = ForeColor;
    }

    private static bool IsDarkTheme()
    {
        try
        {
            using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            if (key != null)
            {
                var value = key.GetValue("AppsUseLightTheme");
                return value == null || (int)value == 0;
            }
        }
        catch { }
        return false;
    }

    private void LoadAllActions()
    {
        _allActions = new List<ActionItem>
        {
            new("左半屏", WindowAction.LeftHalf, "LeftHalf"),
            new("右半屏", WindowAction.RightHalf, "RightHalf"),
            new("中间半屏", WindowAction.CenterHalf, "CenterHalf"),
            new("上半屏", WindowAction.TopHalf, "TopHalf"),
            new("下半屏", WindowAction.BottomHalf, "BottomHalf"),
            new("左上", WindowAction.TopLeft, "TopLeft"),
            new("右上", WindowAction.TopRight, "TopRight"),
            new("左下", WindowAction.BottomLeft, "BottomLeft"),
            new("右下", WindowAction.BottomRight, "BottomRight"),
            new("左首 1/3", WindowAction.FirstThird, "FirstThird"),
            new("中间 1/3", WindowAction.CenterThird, "CenterThird"),
            new("右首 1/3", WindowAction.LastThird, "LastThird"),
            new("左侧 2/3", WindowAction.FirstTwoThirds, "FirstTwoThirds"),
            new("中间 2/3", WindowAction.CenterTwoThirds, "CenterTwoThirds"),
            new("右侧 2/3", WindowAction.LastTwoThirds, "LastTwoThirds"),
            new("左首 1/4", WindowAction.FirstFourth, "FirstFourth"),
            new("左二 1/4", WindowAction.SecondFourth, "SecondFourth"),
            new("右二 1/4", WindowAction.ThirdFourth, "ThirdFourth"),
            new("右首 1/4", WindowAction.LastFourth, "LastFourth"),
            new("左侧 3/4", WindowAction.FirstThreeFourths, "FirstThreeFourths"),
            new("中间 3/4", WindowAction.CenterThreeFourths, "CenterThreeFourths"),
            new("右侧 3/4", WindowAction.LastThreeFourths, "LastThreeFourths"),
            new("左上 1/6", WindowAction.TopLeftSixth, "TopLeftSixth"),
            new("中上 1/6", WindowAction.TopCenterSixth, "TopCenterSixth"),
            new("右上 1/6", WindowAction.TopRightSixth, "TopRightSixth"),
            new("左下 1/6", WindowAction.BottomLeftSixth, "BottomLeftSixth"),
            new("中下 1/6", WindowAction.BottomCenterSixth, "BottomCenterSixth"),
            new("右下 1/6", WindowAction.BottomRightSixth, "BottomRightSixth"),
            new("上部 1/3", WindowAction.TopVerticalThird, "TopVerticalThird"),
            new("中间 1/3 (垂直)", WindowAction.MiddleVerticalThird, "MiddleVerticalThird"),
            new("下部 1/3", WindowAction.BottomVerticalThird, "BottomVerticalThird"),
            new("上部 2/3", WindowAction.TopVerticalTwoThirds, "TopVerticalTwoThirds"),
            new("下部 2/3", WindowAction.BottomVerticalTwoThirds, "BottomVerticalTwoThirds"),
            new("最大化", WindowAction.Maximize, "Maximize"),
            new("接近最大化", WindowAction.AlmostMaximize, "AlmostMaximize"),
            new("最大化高度", WindowAction.MaximizeHeight, "MaximizeHeight"),
            new("放大", WindowAction.Larger, "Larger"),
            new("缩小", WindowAction.Smaller, "Smaller"),
            new("加宽", WindowAction.LargerWidth, "LargerWidth"),
            new("减宽", WindowAction.SmallerWidth, "SmallerWidth"),
            new("加高", WindowAction.LargerHeight, "LargerHeight"),
            new("减高", WindowAction.SmallerHeight, "SmallerHeight"),
            new("居中", WindowAction.Center, "Center"),
            new("恢复", WindowAction.Restore, "Restore"),
            new("左移", WindowAction.MoveLeft, "MoveLeft"),
            new("右移", WindowAction.MoveRight, "MoveRight"),
            new("上移", WindowAction.MoveUp, "MoveUp"),
            new("下移", WindowAction.MoveDown, "MoveDown"),
            new("下一个显示器", WindowAction.NextDisplay, "NextDisplay"),
            new("上一个显示器", WindowAction.PreviousDisplay, "PreviousDisplay"),
            new("撤销", WindowAction.Undo, "Undo"),
            new("重做", WindowAction.Redo, "Redo"),
        };

        _filteredActions = new List<ActionItem>(_allActions);
        UpdateResultList();
    }

    private void UpdateResultList()
    {
        _resultList.Items.Clear();
        var shortcuts = _configService.Load().Shortcuts;
        var defaultShortcuts = ConfigService.GetDefaultShortcuts();

        foreach (var action in _filteredActions)
        {
            var shortcutText = GetShortcutText(action.Tag, shortcuts, defaultShortcuts);
            var item = new ListViewItem(new[] { action.DisplayName, shortcutText })
            {
                Tag = action
            };
            _resultList.Items.Add(item);
        }

        if (_resultList.Items.Count > 0)
        {
            _resultList.Items[0].Selected = true;
        }
    }

    private string GetShortcutText(string actionName, Dictionary<string, ShortcutConfig> shortcuts, Dictionary<string, ShortcutConfig> defaultShortcuts)
    {
        if (!shortcuts.TryGetValue(actionName, out var config) || !config.Enabled || config.KeyCode <= 0)
        {
            // 尝试默认值
            if (!defaultShortcuts.TryGetValue(actionName, out config) || !config.Enabled || config.KeyCode <= 0)
                return string.Empty;
        }

        var parts = new List<string>();
        if ((config.ModifierFlags & 0x0002) != 0) parts.Add("Ctrl");
        if ((config.ModifierFlags & 0x0001) != 0) parts.Add("Alt");
        if ((config.ModifierFlags & 0x0004) != 0) parts.Add("Shift");
        if ((config.ModifierFlags & 0x0008) != 0) parts.Add("Win");
        parts.Add(VkToString(config.KeyCode));
        return string.Join("+", parts);
    }

    private static string VkToString(int vk) => vk switch
    {
        0x25 => "←",
        0x26 => "↑",
        0x27 => "→",
        0x28 => "↓",
        0x0D => "Enter",
        0x08 => "Backspace",
        0x2E => "Delete",
        0x20 => "Space",
        0x1B => "Esc",
        0x09 => "Tab",
        >= 0x41 and <= 0x5A => ((char)vk).ToString(),
        >= 0x30 and <= 0x39 => ((char)vk).ToString(),
        0x70 => "F1", 0x71 => "F2", 0x72 => "F3", 0x73 => "F4",
        0x74 => "F5", 0x75 => "F6", 0x76 => "F7", 0x77 => "F8",
        0x78 => "F9", 0x79 => "F10", 0x7A => "F11", 0x7B => "F12",
        _ => $"0x{vk:X}"
    };

    private void SearchBox_TextChanged(object? sender, EventArgs e)
    {
        var searchText = _searchBox.Text.Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(searchText))
        {
            _filteredActions = new List<ActionItem>(_allActions);
        }
        else
        {
            _filteredActions = _allActions.Where(a =>
                a.DisplayName.ToLowerInvariant().Contains(searchText) ||
                a.Tag.ToLowerInvariant().Contains(searchText)
            ).ToList();
        }
        UpdateResultList();
    }

    private void SearchBox_KeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.KeyCode)
        {
            case Keys.Down:
                e.Handled = true;
                MoveSelection(1);
                break;
            case Keys.Up:
                e.Handled = true;
                MoveSelection(-1);
                break;
            case Keys.Enter:
                e.Handled = true;
                ExecuteSelectedAction();
                break;
            case Keys.Escape:
                e.Handled = true;
                Close();
                break;
        }
    }

    private void ResultList_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            e.Handled = true;
            ExecuteSelectedAction();
        }
        else if (e.KeyCode == Keys.Escape)
        {
            e.Handled = true;
            Close();
        }
    }

    private void ResultList_DoubleClick(object? sender, EventArgs e)
    {
        ExecuteSelectedAction();
    }

    private void MoveSelection(int direction)
    {
        if (_resultList.Items.Count == 0) return;

        int newIndex = 0;
        if (_resultList.SelectedItems.Count > 0)
        {
            newIndex = _resultList.SelectedItems[0].Index + direction;
        }

        newIndex = Math.Max(0, Math.Min(newIndex, _resultList.Items.Count - 1));

        _resultList.SelectedItems.Clear();
        _resultList.Items[newIndex].Selected = true;
        _resultList.Items[newIndex].EnsureVisible();
    }

    private void ExecuteSelectedAction()
    {
        if (_resultList.SelectedItems.Count == 0) return;

        var item = _resultList.SelectedItems[0].Tag as ActionItem;
        if (item == null) return;

        try
        {
            _windowManager.Execute(item.Action);
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"执行动作失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        _searchBox.Focus();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.KeyCode == Keys.Escape)
        {
            Close();
        }
    }

    private class ActionItem
    {
        public string DisplayName { get; }
        public WindowAction Action { get; }
        public string Tag { get; }

        public ActionItem(string displayName, WindowAction action, string tag)
        {
            DisplayName = displayName;
            Action = action;
            Tag = tag;
        }
    }
}
