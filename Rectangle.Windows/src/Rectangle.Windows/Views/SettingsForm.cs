using Rectangle.Windows.Core;
using Rectangle.Windows.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Rectangle.Windows.Views;

public class SettingsForm : Form
{
    private readonly ConfigService _configService;
    private AppConfig _config = new();
    private readonly Dictionary<string, ShortcutRow> _shortcutRows = new();
    private readonly List<CheckBox> _cycleSizeCheckBoxes = new();
    
    // 设置选项卡控件
    private TabControl _tabControl = null!;
    private TabPage _shortcutsTab = null!;
    private TabPage _snapAreasTab = null!;
    private TabPage _settingsTab = null!;
    
    // 设置选项卡控件
    private TrackBar _gapSizeSlider = null!;
    private Label _gapSizeLabel = null!;
    private CheckBox _launchOnLoginCheckBox = null!;
    private CheckBox _hideTrayIconCheckBox = null!;
    private ComboBox _subsequentExecutionCombo = null!;
    
    // 吸附区域选项卡控件
    private CheckBox _dragToSnapCheckBox = null!;
    private CheckBox _restoreSizeCheckBox = null!;
    private CheckBox _hapticFeedbackCheckBox = null!;
    private CheckBox _snapAnimationCheckBox = null!;

    public SettingsForm()
    {
        _configService = Program.ConfigService ?? new ConfigService();
        InitializeComponents();
        LoadSettings();
    }

    private void InitializeComponents()
    {
        this.Text = "Rectangle 偏好设置";
        this.Size = new Size(620, 580);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.StartPosition = FormStartPosition.CenterScreen;

        // 创建 TabControl
        _tabControl = new TabControl
        {
            Dock = DockStyle.Fill,
            Font = new Font("Microsoft YaHei UI", 9F)
        };

        InitializeShortcutsTab();
        InitializeSnapAreasTab();
        InitializeSettingsTab();

        // 添加选项卡
        _tabControl.TabPages.Add(_shortcutsTab);
        _tabControl.TabPages.Add(_snapAreasTab);
        _tabControl.TabPages.Add(_settingsTab);

        this.Controls.Add(_tabControl);
        
        // 窗口关闭时保存
        this.FormClosed += SettingsForm_FormClosed;
    }

    private void InitializeShortcutsTab()
    {
        _shortcutsTab = new TabPage("键盘快捷键");
        
        var mainPanel = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true
        };
        
        var layoutPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoSize = true,
            Padding = new Padding(10)
        };

        // 恢复默认按钮
        var restoreDefaultsBtn = new Button
        {
            Text = "恢复默认快捷键",
            Width = 150,
            Height = 28,
            Margin = new Padding(0, 0, 0, 10)
        };
        restoreDefaultsBtn.Click += RestoreDefaults_Click;

        // 半屏分组
        AddShortcutGroup(layoutPanel, "半屏操作", new[]
        {
            ("LeftHalf", "左半屏"),
            ("RightHalf", "右半屏"),
            ("TopHalf", "上半屏"),
            ("BottomHalf", "下半屏"),
            ("CenterHalf", "中间半屏")
        });

        // 四角分组
        AddShortcutGroup(layoutPanel, "四角操作", new[]
        {
            ("TopLeft", "左上角"),
            ("TopRight", "右上角"),
            ("BottomLeft", "左下角"),
            ("BottomRight", "右下角")
        });

        // 三分之一分组
        AddShortcutGroup(layoutPanel, "三分之一屏", new[]
        {
            ("FirstThird", "左首 1/3"),
            ("CenterThird", "中间 1/3"),
            ("LastThird", "右首 1/3"),
            ("FirstTwoThirds", "左侧 2/3"),
            ("CenterTwoThirds", "中间 2/3"),
            ("LastTwoThirds", "右侧 2/3")
        });

        // 最大化与缩放分组
        AddShortcutGroup(layoutPanel, "最大化与缩放", new[]
        {
            ("Maximize", "最大化"),
            ("AlmostMaximize", "接近最大化"),
            ("MaximizeHeight", "最大化高度"),
            ("Larger", "放大"),
            ("Smaller", "缩小"),
            ("Center", "居中"),
            ("Restore", "恢复")
        });

        // 显示器分组
        AddShortcutGroup(layoutPanel, "显示器", new[]
        {
            ("NextDisplay", "下一个显示器"),
            ("PreviousDisplay", "上一个显示器")
        });

        // 四等分分组
        AddShortcutGroup(layoutPanel, "四等分", new[]
        {
            ("FirstFourth", "左首 1/4"),
            ("SecondFourth", "左二 1/4"),
            ("ThirdFourth", "右二 1/4"),
            ("LastFourth", "右首 1/4"),
            ("FirstThreeFourths", "左侧 3/4"),
            ("CenterThreeFourths", "中间 3/4"),
            ("LastThreeFourths", "右侧 3/4")
        });

        // 六等分分组
        AddShortcutGroup(layoutPanel, "六等分", new[]
        {
            ("TopLeftSixth", "左上 1/6"),
            ("TopCenterSixth", "中上 1/6"),
            ("TopRightSixth", "右上 1/6"),
            ("BottomLeftSixth", "左下 1/6"),
            ("BottomCenterSixth", "中下 1/6"),
            ("BottomRightSixth", "右下 1/6")
        });

        // 移动到边缘分组
        AddShortcutGroup(layoutPanel, "移动到边缘", new[]
        {
            ("MoveLeft", "向左移动"),
            ("MoveRight", "向右移动"),
            ("MoveUp", "向上移动"),
            ("MoveDown", "向下移动")
        });

        layoutPanel.Controls.Add(restoreDefaultsBtn);
        mainPanel.Controls.Add(layoutPanel);
        _shortcutsTab.Controls.Add(mainPanel);
    }

    private void AddShortcutGroup(FlowLayoutPanel parent, string groupName, (string Action, string DisplayName)[] actions)
    {
        var groupPanel = new Panel
        {
            Width = 560,
            Height = actions.Length * 32 + 30,
            Margin = new Padding(0, 5, 0, 5)
        };

        // 分组标题
        var groupLabel = new Label
        {
            Text = groupName,
            Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold),
            Location = new Point(0, 0),
            AutoSize = true
        };
        groupPanel.Controls.Add(groupLabel);

        // 快捷键行
        int y = 25;
        foreach (var (action, displayName) in actions)
        {
            var row = CreateShortcutRow(action, displayName, y);
            _shortcutRows[action] = row;
            foreach (var control in row.Controls)
            {
                groupPanel.Controls.Add(control);
            }
            y += 32;
        }

        parent.Controls.Add(groupPanel);
    }

    private ShortcutRow CreateShortcutRow(string action, string displayName, int y)
    {
        var row = new ShortcutRow();

        row.CheckBox = new CheckBox
        {
            Text = displayName,
            Location = new Point(10, y),
            Width = 120,
            Checked = true
        };
        row.CheckBox.CheckedChanged += (s, e) => UpdateShortcutConfig(action, row);

        row.KeyLabel = new Label
        {
            Location = new Point(140, y + 3),
            Width = 150,
            Text = "",
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(3)
        };

        row.ChangeButton = new Button
        {
            Text = "修改",
            Location = new Point(300, y),
            Width = 60,
            Height = 24
        };
        row.ChangeButton.Click += (s, e) => ShowShortcutCaptureDialog(action, row);

        row.ClearButton = new Button
        {
            Text = "清除",
            Location = new Point(370, y),
            Width = 60,
            Height = 24
        };
        row.ClearButton.Click += (s, e) => ClearShortcut(action, row);

        row.Controls = new Control[] { row.CheckBox, row.KeyLabel, row.ChangeButton, row.ClearButton };
        return row;
    }

    private void ShowShortcutCaptureDialog(string action, ShortcutRow row)
    {
        var captureForm = new Form
        {
            Text = "按下新的快捷键...",
            Size = new Size(300, 120),
            FormBorderStyle = FormBorderStyle.FixedDialog,
            StartPosition = FormStartPosition.CenterParent,
            MaximizeBox = false,
            MinimizeBox = false
        };

        var label = new Label
        {
            Text = "请按下新的快捷键组合\n(例如: Ctrl+Alt+Left)",
            Dock = DockStyle.Top,
            TextAlign = ContentAlignment.MiddleCenter,
            Height = 50
        };

        var cancelButton = new Button
        {
            Text = "取消",
            Dock = DockStyle.Bottom,
            Height = 30
        };
        cancelButton.Click += (s, e) => captureForm.Close();

        captureForm.Controls.Add(label);
        captureForm.Controls.Add(cancelButton);

        uint modifiers = 0;
        int keyCode = 0;

        captureForm.KeyDown += (s, e) =>
        {
            modifiers = 0;
            if (e.Control) modifiers |= 0x0002; // MOD_CONTROL
            if (e.Alt) modifiers |= 0x0001;     // MOD_ALT
            if (e.Shift) modifiers |= 0x0004;   // MOD_SHIFT
            keyCode = (int)e.KeyCode;

            if (modifiers > 0 && keyCode > 0)
            {
                var shortcutText = FormatShortcut(keyCode, modifiers);
                row.KeyLabel.Text = shortcutText;
                UpdateShortcutConfig(action, row);
                captureForm.Close();
            }
        };

        captureForm.KeyPreview = true;
        captureForm.ShowDialog(this);
    }

    private void ClearShortcut(string action, ShortcutRow row)
    {
        row.KeyLabel.Text = "";
        if (_config.Shortcuts.ContainsKey(action))
        {
            _config.Shortcuts[action].KeyCode = 0;
            _config.Shortcuts[action].ModifierFlags = 0;
            _config.Shortcuts[action].Enabled = false;
        }
    }

    private void UpdateShortcutConfig(string action, ShortcutRow row)
    {
        if (!_config.Shortcuts.ContainsKey(action))
        {
            _config.Shortcuts[action] = new ShortcutConfig();
        }

        _config.Shortcuts[action].Enabled = row.CheckBox.Checked;
        
        // 解析快捷键文本
        var text = row.KeyLabel.Text;
        if (!string.IsNullOrEmpty(text))
        {
            // 这里需要从文本解析回 KeyCode 和 ModifierFlags
            // 简化处理：如果已经有值就保持，否则设置默认值
            if (_config.Shortcuts[action].KeyCode == 0)
            {
                var defaults = ConfigService.GetDefaultShortcuts();
                if (defaults.TryGetValue(action, out var defaultConfig))
                {
                    _config.Shortcuts[action].KeyCode = defaultConfig.KeyCode;
                    _config.Shortcuts[action].ModifierFlags = defaultConfig.ModifierFlags;
                }
            }
        }
    }

    private string FormatShortcut(int keyCode, uint modifiers)
    {
        var parts = new List<string>();
        
        if ((modifiers & 0x0002) != 0) parts.Add("Ctrl");
        if ((modifiers & 0x0001) != 0) parts.Add("Alt");
        if ((modifiers & 0x0004) != 0) parts.Add("Shift");
        if ((modifiers & 0x0008) != 0) parts.Add("Win");

        string keyName = ((Keys)keyCode).ToString();
        
        // 特殊键名映射
        var keyMappings = new Dictionary<Keys, string>
        {
            [Keys.Left] = "←",
            [Keys.Right] = "→",
            [Keys.Up] = "↑",
            [Keys.Down] = "↓",
            [Keys.Enter] = "Enter",
            [Keys.Back] = "Backspace",
            [Keys.Delete] = "Delete",
            [Keys.Space] = "Space"
        };

        if (keyMappings.TryGetValue((Keys)keyCode, out var mapped))
        {
            keyName = mapped;
        }

        parts.Add(keyName);
        return string.Join("+", parts);
    }

    private void InitializeSnapAreasTab()
    {
        _snapAreasTab = new TabPage("吸附区域");
        
        var layoutPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(15),
            AutoScroll = true
        };

        // 拖拽吸附选项
        var dragToSnapPanel = new Panel { Width = 500, Height = 30 };
        _dragToSnapCheckBox = new CheckBox
        {
            Text = "拖移以吸附",
            Location = new Point(0, 5),
            AutoSize = true
        };
        _dragToSnapCheckBox.CheckedChanged += DragToSnapCheckBox_CheckedChanged;
        dragToSnapPanel.Controls.Add(_dragToSnapCheckBox);
        layoutPanel.Controls.Add(dragToSnapPanel);

        // 恢复窗口大小选项
        var restorePanel = new Panel { Width = 500, Height = 30 };
        _restoreSizeCheckBox = new CheckBox
        {
            Text = "结束吸附时恢复窗口大小",
            Location = new Point(0, 5),
            AutoSize = true
        };
        _restoreSizeCheckBox.CheckedChanged += RestoreSizeCheckBox_CheckedChanged;
        restorePanel.Controls.Add(_restoreSizeCheckBox);
        layoutPanel.Controls.Add(restorePanel);

        // 触觉反馈选项
        var hapticPanel = new Panel { Width = 500, Height = 30 };
        _hapticFeedbackCheckBox = new CheckBox
        {
            Text = "吸附时触觉反馈",
            Location = new Point(0, 5),
            AutoSize = true
        };
        _hapticFeedbackCheckBox.CheckedChanged += HapticFeedbackCheckBox_CheckedChanged;
        hapticPanel.Controls.Add(_hapticFeedbackCheckBox);
        layoutPanel.Controls.Add(hapticPanel);

        // 吸附动画选项
        var animPanel = new Panel { Width = 500, Height = 30 };
        _snapAnimationCheckBox = new CheckBox
        {
            Text = "显示吸附预览动画",
            Location = new Point(0, 5),
            AutoSize = true
        };
        _snapAnimationCheckBox.CheckedChanged += SnapAnimationCheckBox_CheckedChanged;
        animPanel.Controls.Add(_snapAnimationCheckBox);
        layoutPanel.Controls.Add(animPanel);

        // 分隔线
        var separator = new Label
        {
            Text = "",
            Width = 500,
            Height = 2,
            BackColor = Color.LightGray,
            Margin = new Padding(0, 15, 0, 15)
        };
        layoutPanel.Controls.Add(separator);

        // 吸附区域网格标题
        var gridTitle = new Label
        {
            Text = "吸附区域动作配置",
            Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold),
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 10)
        };
        layoutPanel.Controls.Add(gridTitle);

        // 创建吸附区域网格
        CreateSnapAreaGrid(layoutPanel);

        _snapAreasTab.Controls.Add(layoutPanel);
    }

    private void CreateSnapAreaGrid(FlowLayoutPanel parent)
    {
        var gridPanel = new TableLayoutPanel
        {
            ColumnCount = 3,
            RowCount = 3,
            Width = 450,
            Height = 200,
            CellBorderStyle = TableLayoutPanelCellBorderStyle.Single,
            Margin = new Padding(0, 0, 0, 10)
        };

        // 设置列宽
        gridPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        gridPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        gridPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));

        // 设置行高
        gridPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));
        gridPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));
        gridPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));

        // 创建 3x3 网格的下拉框
        var snapAreaActions = new string[,]
        {
            { "TopLeft", "Top", "TopRight" },
            { "Left", "", "Right" },
            { "BottomLeft", "Bottom", "BottomRight" }
        };

        var actionNames = new Dictionary<string, string>
        {
            ["LeftHalf"] = "左半屏",
            ["RightHalf"] = "右半屏",
            ["TopHalf"] = "上半屏",
            ["BottomHalf"] = "下半屏",
            ["TopLeft"] = "左上角",
            ["TopRight"] = "右上角",
            ["BottomLeft"] = "左下角",
            ["BottomRight"] = "右下角",
            ["Maximize"] = "最大化",
            ["FirstThird"] = "左 1/3",
            ["LastThird"] = "右 1/3"
        };

        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                var key = snapAreaActions[row, col];
                if (string.IsNullOrEmpty(key))
                {
                    gridPanel.Controls.Add(new Panel(), col, row);
                    continue;
                }

                var combo = new ComboBox
                {
                    Dock = DockStyle.Fill,
                    DropDownStyle = ComboBoxStyle.DropDownList
                };

                combo.Items.Add("(无)");
                foreach (var action in actionNames)
                {
                    combo.Items.Add(action.Value);
                }
                combo.SelectedIndex = 0;

                // 存储区域标识
                combo.Tag = key;

                gridPanel.Controls.Add(combo, col, row);
            }
        }

        parent.Controls.Add(gridPanel);
    }

    private void InitializeSettingsTab()
    {
        _settingsTab = new TabPage("设置");
        
        var layoutPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(15)
        };

        // 间隙大小
        var gapPanel = new Panel { Width = 500, Height = 60 };
        var gapLabel = new Label
        {
            Text = "窗口间隙大小:",
            Location = new Point(0, 10),
            AutoSize = true
        };
        
        _gapSizeSlider = new TrackBar
        {
            Location = new Point(0, 35),
            Width = 200,
            Minimum = 0,
            Maximum = 20,
            Value = 0
        };
        _gapSizeSlider.Scroll += GapSizeSlider_Scroll;
        
        _gapSizeLabel = new Label
        {
            Text = "0 px",
            Location = new Point(210, 37),
            AutoSize = true
        };
        
        gapPanel.Controls.Add(gapLabel);
        gapPanel.Controls.Add(_gapSizeSlider);
        gapPanel.Controls.Add(_gapSizeLabel);
        layoutPanel.Controls.Add(gapPanel);

        // 开机启动
        var launchPanel = new Panel { Width = 500, Height = 30 };
        _launchOnLoginCheckBox = new CheckBox
        {
            Text = "开机启动",
            Location = new Point(0, 5),
            AutoSize = true
        };
        _launchOnLoginCheckBox.CheckedChanged += LaunchOnLoginCheckBox_CheckedChanged;
        launchPanel.Controls.Add(_launchOnLoginCheckBox);
        layoutPanel.Controls.Add(launchPanel);

        // 隐藏托盘图标
        var trayPanel = new Panel { Width = 500, Height = 30 };
        _hideTrayIconCheckBox = new CheckBox
        {
            Text = "隐藏菜单栏图标",
            Location = new Point(0, 5),
            AutoSize = true
        };
        _hideTrayIconCheckBox.CheckedChanged += HideTrayIconCheckBox_CheckedChanged;
        trayPanel.Controls.Add(_hideTrayIconCheckBox);
        layoutPanel.Controls.Add(trayPanel);

        // 后续执行行为
        var subsequentPanel = new Panel { Width = 500, Height = 50 };
        var subsequentLabel = new Label
        {
            Text = "重复执行行为:",
            Location = new Point(0, 10),
            AutoSize = true
        };
        
        _subsequentExecutionCombo = new ComboBox
        {
            Location = new Point(0, 30),
            Width = 200,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _subsequentExecutionCombo.Items.AddRange(new object[] { "循环调整大小", "切换显示器", "无动作" });
        _subsequentExecutionCombo.SelectedIndex = 0;
        
        subsequentPanel.Controls.Add(subsequentLabel);
        subsequentPanel.Controls.Add(_subsequentExecutionCombo);
        layoutPanel.Controls.Add(subsequentPanel);

        // 分隔线
        var separator = new Label
        {
            Text = "",
            Width = 500,
            Height = 2,
            BackColor = Color.LightGray,
            Margin = new Padding(0, 15, 0, 15)
        };
        layoutPanel.Controls.Add(separator);

        // 循环尺寸选项标题
        var cycleTitle = new Label
        {
            Text = "循环调整尺寸选项",
            Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold),
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 10)
        };
        layoutPanel.Controls.Add(cycleTitle);

        // 循环尺寸复选框
        var cycleSizes = new[] { "1/2", "1/3", "2/3", "1/4", "3/4" };
        foreach (var size in cycleSizes)
        {
            var cb = new CheckBox
            {
                Text = size,
                AutoSize = true,
                Margin = new Padding(5)
            };
            _cycleSizeCheckBoxes.Add(cb);
            layoutPanel.Controls.Add(cb);
        }

        _settingsTab.Controls.Add(layoutPanel);
    }

    private void LoadSettings()
    {
        _config = _configService.Load();
        
        // 加载快捷键设置
        var defaults = ConfigService.GetDefaultShortcuts();
        foreach (var row in _shortcutRows)
        {
            var action = row.Key;
            var shortcutConfig = _config.Shortcuts.TryGetValue(action, out var c) ? c : 
                                  defaults.TryGetValue(action, out var d) ? d : null;
            
            if (shortcutConfig != null)
            {
                row.Value.CheckBox.Checked = shortcutConfig.Enabled;
                if (shortcutConfig.KeyCode > 0)
                {
                    row.Value.KeyLabel.Text = FormatShortcut(shortcutConfig.KeyCode, shortcutConfig.ModifierFlags);
                }
            }
        }
        
        // 设置选项卡
        _gapSizeSlider.Value = _config.GapSize;
        _gapSizeLabel.Text = $"{_config.GapSize} px";
        _launchOnLoginCheckBox.Checked = _config.LaunchOnLogin;
        
        // 吸附区域选项卡
        _dragToSnapCheckBox.Checked = _config.SnapAreas.DragToSnap;
        _restoreSizeCheckBox.Checked = _config.SnapAreas.RestoreSizeOnSnapEnd;
        _hapticFeedbackCheckBox.Checked = _config.SnapAreas.HapticFeedback;
        _snapAnimationCheckBox.Checked = _config.SnapAreas.SnapAnimation;
    }

    private void GapSizeSlider_Scroll(object? sender, EventArgs e)
    {
        var value = _gapSizeSlider.Value;
        _gapSizeLabel.Text = $"{value} px";
        _config.GapSize = value;
    }

    private void LaunchOnLoginCheckBox_CheckedChanged(object? sender, EventArgs e)
    {
        _config.LaunchOnLogin = _launchOnLoginCheckBox.Checked;
        SetLaunchOnLogin(_config.LaunchOnLogin);
    }

    private void HideTrayIconCheckBox_CheckedChanged(object? sender, EventArgs e)
    {
        // TODO: 实现隐藏托盘图标功能
    }

    private void DragToSnapCheckBox_CheckedChanged(object? sender, EventArgs e)
    {
        _config.SnapAreas.DragToSnap = _dragToSnapCheckBox.Checked;
    }

    private void RestoreSizeCheckBox_CheckedChanged(object? sender, EventArgs e)
    {
        _config.SnapAreas.RestoreSizeOnSnapEnd = _restoreSizeCheckBox.Checked;
    }

    private void HapticFeedbackCheckBox_CheckedChanged(object? sender, EventArgs e)
    {
        _config.SnapAreas.HapticFeedback = _hapticFeedbackCheckBox.Checked;
    }

    private void SnapAnimationCheckBox_CheckedChanged(object? sender, EventArgs e)
    {
        _config.SnapAreas.SnapAnimation = _snapAnimationCheckBox.Checked;
    }

    private void RestoreDefaults_Click(object? sender, EventArgs e)
    {
        _config.Shortcuts = ConfigService.GetDefaultShortcuts();
        _configService.Save(_config);
        LoadSettings();
        MessageBox.Show("已恢复默认快捷键", "Rectangle", MessageBoxButtons.OK, MessageBoxIcon.Information);
        
        // 通知 HotkeyManager 重新注册
        Program.HotkeyManager?.ReloadFromConfig(_config.Shortcuts);
    }

    private void SetLaunchOnLogin(bool enabled)
    {
        try
        {
            var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Run", true);
            
            if (enabled)
            {
                var exePath = Environment.ProcessPath ?? Application.ExecutablePath;
                key?.SetValue("Rectangle", exePath);
            }
            else
            {
                key?.DeleteValue("Rectangle", false);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"设置开机启动失败: {ex.Message}", "Rectangle", 
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void SettingsForm_FormClosed(object? sender, FormClosedEventArgs e)
    {
        // 保存所有快捷键配置
        foreach (var row in _shortcutRows)
        {
            if (_config.Shortcuts.ContainsKey(row.Key))
            {
                _config.Shortcuts[row.Key].Enabled = row.Value.CheckBox.Checked;
            }
        }
        
        _configService.Save(_config);
        
        // 通知 HotkeyManager 重新注册
        Program.HotkeyManager?.ReloadFromConfig(_config.Shortcuts);
    }

    private class ShortcutRow
    {
        public CheckBox CheckBox { get; set; } = null!;
        public Label KeyLabel { get; set; } = null!;
        public Button ChangeButton { get; set; } = null!;
        public Button ClearButton { get; set; } = null!;
        public Control[] Controls { get; set; } = Array.Empty<Control>();
    }
}
