using Rectangle.Windows.Core;
using Rectangle.Windows.Services;
using Rectangle.Windows.Views.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Rectangle.Windows.Views;

/// <summary>
/// 现代化设置窗口 - Windows 11 Fluent Design 风格
/// </summary>
public class SettingsForm : Form
{
    private readonly ConfigService _configService;
    private AppConfig _config = new();
    private readonly Dictionary<string, ShortcutRow> _shortcutRows = new();

    // 导航
    private Panel _navPanel = null!;
    private Panel _contentPanel = null!;
    private readonly List<NavButton> _navButtons = new();

    // 页面
    private Panel _shortcutsPage = null!;
    private Panel _snapAreasPage = null!;
    private Panel _settingsPage = null!;

    // 设置控件
    private ModernSlider _gapSizeSlider = null!;
    private ModernCheckBox _launchOnLoginCheckBox = null!;
    private ModernCheckBox _dragToSnapCheckBox = null!;
    private ModernCheckBox _restoreSizeCheckBox = null!;
    private ModernCheckBox _snapAnimationCheckBox = null!;

    public SettingsForm()
    {
        _configService = Program.ConfigService ?? new ConfigService();
        InitializeForm();
        CreateNavigation();
        CreatePages();
        LoadSettings();
        ShowPage(0);
    }

    private void InitializeForm()
    {
        Text = "Rectangle 偏好设置";
        Size = new Size(960, 680); // slightly larger for breathing room
        MinimumSize = new Size(800, 500);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = SettingsTheme.BackgroundColor;
        Font = new Font("Microsoft YaHei UI", 9F);
        DoubleBuffered = true;

        FormClosed += SettingsForm_FormClosed;
    }

    private void CreateNavigation()
    {
        _navPanel = new Panel
        {
            Dock = DockStyle.Left,
            Width = 200,
            BackColor = SettingsTheme.NavBackgroundColor,
            Padding = new Padding(8, 20, 8, 20)
        };

        // 标题
        var titleLabel = new Label
        {
            Text = "Rectangle",
            Font = new Font("Microsoft YaHei UI", 16F, FontStyle.Bold),
            ForeColor = SettingsTheme.TextColor,
            Location = new Point(20, 24),
            AutoSize = true
        };
        _navPanel.Controls.Add(titleLabel);

        // 版本号
        var versionLabel = new Label
        {
            Text = "v1.0.0",
            Font = new Font("Microsoft YaHei UI", 8F),
            ForeColor = SettingsTheme.SecondaryTextColor,
            Location = new Point(20, 48),
            AutoSize = true
        };
        _navPanel.Controls.Add(versionLabel);

        var navItems = new[]
        {
            ("⌨️", "键盘快捷键"),
            ("📐", "吸附区域"),
            ("⚙️", "设置")
        };

        int y = 100;
        for (int i = 0; i < navItems.Length; i++)
        {
            var (icon, text) = navItems[i];
            var btn = new NavButton(icon, text, i)
            {
                Location = new Point(8, y),
                Width = 184,
                Height = 40
            };
            btn.Click += NavButton_Click;
            _navButtons.Add(btn);
            _navPanel.Controls.Add(btn);
            y += 44;
        }

        Controls.Add(_navPanel);

        // 内容区域
        _contentPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = SettingsTheme.BackgroundColor,
            Padding = new Padding(40, 30, 40, 30) // more generous padding
        };
        Controls.Add(_contentPanel);
    }

    private void NavButton_Click(object? sender, EventArgs e)
    {
        if (sender is NavButton btn)
        {
            ShowPage(btn.Index);
        }
    }

    private void ShowPage(int index)
    {
        foreach (var btn in _navButtons)
        {
            btn.IsSelected = btn.Index == index;
        }

        _shortcutsPage.Visible = index == 0;
        _snapAreasPage.Visible = index == 1;
        _settingsPage.Visible = index == 2;
    }

    private void CreatePages()
    {
        CreateShortcutsPage();
        CreateSnapAreasPage();
        CreateSettingsPage();
    }

    #region Shortcuts Page

    private void CreateShortcutsPage()
    {
        _shortcutsPage = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = SettingsTheme.BackgroundColor
        };

        var container = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(0, 0, 20, 40) // extra bottom padding for scrolling
        };

        // 页面标题
        AddPageTitle(container, "键盘快捷键", "配置窗口管理的快捷键");

        // 左右两列布局
        var columnsPanel = new TableLayoutPanel
        {
            AutoSize = true,
            ColumnCount = 2,
            RowCount = 1,
            Margin = new Padding(0, 16, 0, 0)
        };
        columnsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 340));
        columnsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 340));

        // 左列
        var leftColumn = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoSize = true,
            Margin = new Padding(0)
        };

        // 右列
        var rightColumn = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoSize = true,
            Margin = new Padding(0)
        };

        // 半屏操作 - 左列
        AddShortcutCard(leftColumn, "半屏", new[]
        {
            ("LeftHalf", "左半屏", "leftHalfTemplate.png"),
            ("RightHalf", "右半屏", "rightHalfTemplate.png"),
            ("CenterHalf", "中间半屏", "halfWidthCenterTemplate.png"),
            ("TopHalf", "上半屏", "topHalfTemplate.png"),
            ("BottomHalf", "下半屏", "bottomHalfTemplate.png")
        });

        // 四角 - 左列
        AddShortcutCard(leftColumn, "四角", new[]
        {
            ("TopLeft", "左上", "topLeftTemplate.png"),
            ("TopRight", "右上", "topRightTemplate.png"),
            ("BottomLeft", "左下", "bottomLeftTemplate.png"),
            ("BottomRight", "右下", "bottomRightTemplate.png")
        });

        // 三分屏 - 左列
        AddShortcutCard(leftColumn, "三分屏", new[]
        {
            ("FirstThird", "左首 1/3", "firstThirdTemplate.png"),
            ("CenterThird", "中间 1/3", "centerThirdTemplate.png"),
            ("LastThird", "右首 1/3", "lastThirdTemplate.png"),
            ("FirstTwoThirds", "左侧 2/3", "firstTwoThirdsTemplate.png"),
            ("CenterTwoThirds", "中间 2/3", "centerTwoThirdsTemplate.png"),
            ("LastTwoThirds", "右侧 2/3", "lastTwoThirdsTemplate.png")
        });

        // 最大化与缩放 - 右列
        AddShortcutCard(rightColumn, "最大化", new[]
        {
            ("Maximize", "最大化", "maximizeTemplate.png"),
            ("AlmostMaximize", "接近最大化", "almostMaximizeTemplate.png"),
            ("MaximizeHeight", "最大化高度", "maximizeHeightTemplate.png"),
            ("Smaller", "缩小", "makeSmallerTemplate.png"),
            ("Larger", "放大", "makeLargerTemplate.png"),
            ("Center", "居中", "centerTemplate.png"),
            ("Restore", "恢复", "restoreTemplate.png")
        });

        // 显示器 - 右列
        AddShortcutCard(rightColumn, "显示器", new[]
        {
            ("NextDisplay", "下一个显示器", "nextDisplayTemplate.png"),
            ("PreviousDisplay", "上一个显示器", "prevDisplayTemplate.png")
        });

        columnsPanel.Controls.Add(leftColumn, 0, 0);
        columnsPanel.Controls.Add(rightColumn, 1, 0);
        container.Controls.Add(columnsPanel);

        // 恢复默认按钮
        var restoreBtn = new ModernButton("恢复默认快捷键")
        {
            Width = 150,
            Margin = new Padding(0, 20, 0, 0)
        };
        restoreBtn.Click += RestoreDefaults_Click;
        container.Controls.Add(restoreBtn);

        _shortcutsPage.Controls.Add(container);
        _contentPanel.Controls.Add(_shortcutsPage);
    }

    private void AddShortcutCard(FlowLayoutPanel parent, string title, (string Action, string DisplayName, string Icon)[] shortcuts)
    {
        var card = new ModernCard
        {
            Width = 320,
            Margin = new Padding(0, 0, 0, 20) // better spacing between cards
        };

        var contentPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoSize = true,
            Padding = new Padding(0)
        };

        // 卡片标题
        var titleLabel = new Label
        {
            Text = title,
            Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold),
            ForeColor = SettingsTheme.TextColor,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 8)
        };
        contentPanel.Controls.Add(titleLabel);

        // 快捷键行
        foreach (var (action, displayName, iconName) in shortcuts)
        {
            var row = CreateShortcutRowControl(action, displayName, iconName);
            _shortcutRows[action] = row;
            contentPanel.Controls.Add(row.Container);
        }

        card.SetContent(contentPanel);
        card.Height = 50 + shortcuts.Length * 36;
        parent.Controls.Add(card);
    }

    private ShortcutRow CreateShortcutRowControl(string action, string displayName, string iconName)
    {
        var row = new ShortcutRow();

        row.Container = new Panel
        {
            Width = 290,
            Height = 40,
            Margin = new Padding(0, 4, 0, 4) // row breathing room
        };

        // 图标
        var icon = LoadIcon(iconName);
        if (icon != null)
        {
            var iconBox = new PictureBox
            {
                Image = icon,
                Size = new Size(18, 18),
                Location = new Point(0, 7),
                SizeMode = PictureBoxSizeMode.Zoom
            };
            row.Container.Controls.Add(iconBox);
        }

        // 名称
        row.NameLabel = new Label
        {
            Text = displayName,
            ForeColor = SettingsTheme.TextColor,
            Location = new Point(24, 11),
            AutoSize = true
        };
        row.Container.Controls.Add(row.NameLabel);

        // 快捷键输入框
        row.KeyLabel = new Label
        {
            Width = 110,
            Height = 28,
            Location = new Point(140, 6),
            BackColor = SettingsTheme.InputBackColor,
            ForeColor = SettingsTheme.SecondaryTextColor,
            TextAlign = ContentAlignment.MiddleCenter,
            Text = "记录快捷键",
            Cursor = Cursors.Hand
        };
        row.KeyLabel.Click += (s, e) => ShowShortcutCaptureDialog(action, row);
        row.KeyLabel.Paint += (s, e) =>
        {
            using var pen = new Pen(SettingsTheme.BorderColor, 1);
            var rect = new System.Drawing.Rectangle(0, 0, row.KeyLabel.Width - 1, row.KeyLabel.Height - 1);
            using var path = SettingsTheme.CreateRoundedRect(rect, 4);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            e.Graphics.DrawPath(pen, path);
        };
        row.Container.Controls.Add(row.KeyLabel);

        // 清除按钮
        row.ClearButton = new Label
        {
            Text = "✕",
            ForeColor = SettingsTheme.SecondaryTextColor,
            Location = new Point(260, 11),
            AutoSize = true,
            Cursor = Cursors.Hand
        };
        row.ClearButton.Click += (s, e) => ClearShortcut(action, row);
        row.ClearButton.MouseEnter += (s, e) => row.ClearButton.ForeColor = Color.White;
        row.ClearButton.MouseLeave += (s, e) => row.ClearButton.ForeColor = SettingsTheme.SecondaryTextColor;
        row.Container.Controls.Add(row.ClearButton);

        return row;
    }

    #endregion

    #region Snap Areas Page

    private void CreateSnapAreasPage()
    {
        _snapAreasPage = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = SettingsTheme.BackgroundColor,
            Visible = false
        };

        var container = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(0, 0, 20, 20)
        };

        AddPageTitle(container, "吸附区域", "配置窗口拖拽吸附行为");

        // 吸附选项卡片
        var optionsCard = new ModernCard { Width = 600, Height = 200 };
        var optionsPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoSize = true
        };

        _dragToSnapCheckBox = new ModernCheckBox("拖移以吸附", "将窗口拖动到屏幕边缘时自动吸附");
        _dragToSnapCheckBox.CheckedChanged += (s, e) => _config.SnapAreas.DragToSnap = _dragToSnapCheckBox.Checked;
        optionsPanel.Controls.Add(_dragToSnapCheckBox);

        _restoreSizeCheckBox = new ModernCheckBox("结束吸附时恢复窗口大小", "取消吸附后恢复原始窗口尺寸");
        _restoreSizeCheckBox.CheckedChanged += (s, e) => _config.SnapAreas.RestoreSizeOnSnapEnd = _restoreSizeCheckBox.Checked;
        optionsPanel.Controls.Add(_restoreSizeCheckBox);

        _snapAnimationCheckBox = new ModernCheckBox("显示吸附预览动画", "拖动时显示目标位置预览");
        _snapAnimationCheckBox.CheckedChanged += (s, e) => _config.SnapAreas.SnapAnimation = _snapAnimationCheckBox.Checked;
        optionsPanel.Controls.Add(_snapAnimationCheckBox);

        optionsCard.SetContent(optionsPanel);
        container.Controls.Add(optionsCard);

        // 吸附区域示意图
        var previewCard = new ModernCard { Width = 600, Height = 300, Margin = new Padding(0, 20, 0, 0) };
        var previewLabel = new Label
        {
            Text = "吸附区域示意",
            Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold),
            ForeColor = SettingsTheme.TextColor,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 16)
        };

        var previewPanel = new SnapAreaPreview
        {
            Width = 560,
            Height = 220,
            Margin = new Padding(0, 0, 0, 0)
        };

        var previewContainer = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoSize = true
        };
        previewContainer.Controls.Add(previewLabel);
        previewContainer.Controls.Add(previewPanel);

        previewCard.SetContent(previewContainer);
        container.Controls.Add(previewCard);

        _snapAreasPage.Controls.Add(container);
        _contentPanel.Controls.Add(_snapAreasPage);
    }

    #endregion

    #region Settings Page

    private void CreateSettingsPage()
    {
        _settingsPage = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = SettingsTheme.BackgroundColor,
            Visible = false
        };

        var container = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(0, 0, 20, 20)
        };

        AddPageTitle(container, "设置", "通用设置和偏好选项");

        // 通用设置卡片
        var generalCard = new ModernCard { Width = 600, Height = 180 };
        var generalPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoSize = true
        };

        _launchOnLoginCheckBox = new ModernCheckBox("登录时打开", "系统启动时自动运行 Rectangle");
        _launchOnLoginCheckBox.CheckedChanged += LaunchOnLoginCheckBox_CheckedChanged;
        generalPanel.Controls.Add(_launchOnLoginCheckBox);

        // 窗口间隙
        var gapPanel = new Panel { Width = 560, Height = 60, Margin = new Padding(0, 10, 0, 0) };
        var gapLabel = new Label
        {
            Text = "窗口间隙",
            ForeColor = SettingsTheme.TextColor,
            Location = new Point(0, 5),
            AutoSize = true
        };
        var gapDesc = new Label
        {
            Text = "窗口之间的间距",
            ForeColor = SettingsTheme.SecondaryTextColor,
            Font = new Font("Microsoft YaHei UI", 8F),
            Location = new Point(0, 25),
            AutoSize = true
        };
        _gapSizeSlider = new ModernSlider(0, 30)
        {
            Location = new Point(360, 10),
            Width = 180
        };
        _gapSizeSlider.ValueChanged += (s, e) => _config.GapSize = _gapSizeSlider.Value;
        gapPanel.Controls.Add(gapLabel);
        gapPanel.Controls.Add(gapDesc);
        gapPanel.Controls.Add(_gapSizeSlider);
        generalPanel.Controls.Add(gapPanel);

        generalCard.SetContent(generalPanel);
        container.Controls.Add(generalCard);

        // 关于卡片
        var aboutCard = new ModernCard { Width = 600, Height = 140, Margin = new Padding(0, 20, 0, 0) };
        var aboutPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoSize = true
        };

        var aboutTitle = new Label
        {
            Text = "Rectangle for Windows",
            Font = new Font("Microsoft YaHei UI", 11F, FontStyle.Bold),
            ForeColor = SettingsTheme.TextColor,
            AutoSize = true
        };
        var aboutVersion = new Label
        {
            Text = "版本 1.0.0 · 基于 macOS Rectangle 移植",
            ForeColor = SettingsTheme.SecondaryTextColor,
            AutoSize = true,
            Margin = new Padding(0, 5, 0, 0)
        };
        var aboutLink = new LinkLabel
        {
            Text = "GitHub: rxhanson/Rectangle",
            LinkColor = SettingsTheme.AccentColor,
            AutoSize = true,
            Margin = new Padding(0, 10, 0, 0)
        };
        aboutLink.Click += (s, e) => System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = "https://github.com/rxhanson/Rectangle",
            UseShellExecute = true
        });

        aboutPanel.Controls.Add(aboutTitle);
        aboutPanel.Controls.Add(aboutVersion);
        aboutPanel.Controls.Add(aboutLink);

        aboutCard.SetContent(aboutPanel);
        container.Controls.Add(aboutCard);

        _settingsPage.Controls.Add(container);
        _contentPanel.Controls.Add(_settingsPage);
    }

    #endregion

    #region Helper Methods

    private void AddPageTitle(FlowLayoutPanel parent, string title, string description)
    {
        var titleLabel = new Label
        {
            Text = title,
            Font = new Font("Microsoft YaHei UI", 20F, FontStyle.Bold),
            ForeColor = SettingsTheme.TextColor,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 8)
        };
        parent.Controls.Add(titleLabel);

        var descLabel = new Label
        {
            Text = description,
            Font = new Font("Microsoft YaHei UI", 10F),
            ForeColor = SettingsTheme.SecondaryTextColor,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 28)
        };
        parent.Controls.Add(descLabel);
    }

    private Image? LoadIcon(string iconName)
    {
        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        string resourceName = $"Rectangle.Windows.Assets.WindowPositions.{iconName}";
        var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream != null)
        {
            try
            {
                var ms = new System.IO.MemoryStream();
                stream.CopyTo(ms);
                ms.Position = 0;
                stream.Dispose();
                return Image.FromStream(ms);
            }
            catch
            {
                stream.Dispose();
            }
        }
        return null;
    }

    #endregion

    #region Shortcut Methods

    private void ShowShortcutCaptureDialog(string action, ShortcutRow row)
    {
        Program.HotkeyManager?.SetCapturingMode(true);

        var dialog = new Form
        {
            Text = "记录快捷键",
            Size = new Size(350, 180),
            FormBorderStyle = FormBorderStyle.FixedDialog,
            StartPosition = FormStartPosition.CenterParent,
            MaximizeBox = false,
            MinimizeBox = false,
            BackColor = SettingsTheme.BackgroundColor,
            ForeColor = SettingsTheme.TextColor,
            KeyPreview = true
        };

        var label = new Label
        {
            Text = "请按下新的快捷键组合\n例如: Ctrl+Alt+←",
            ForeColor = SettingsTheme.TextColor,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Top,
            Height = 80,
            Font = new Font("Microsoft YaHei UI", 10F)
        };

        var hintLabel = new Label
        {
            Text = "按 Escape 取消",
            ForeColor = SettingsTheme.SecondaryTextColor,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Top,
            Height = 30
        };

        dialog.Controls.Add(hintLabel);
        dialog.Controls.Add(label);

        dialog.KeyDown += (s, e) =>
        {
            e.Handled = true;
            if (e.KeyCode == Keys.Escape)
            {
                Program.HotkeyManager?.SetCapturingMode(false);
                dialog.Close();
                return;
            }

            if (e.KeyCode == Keys.ControlKey || e.KeyCode == Keys.LControlKey ||
                e.KeyCode == Keys.RControlKey || e.KeyCode == Keys.Menu ||
                e.KeyCode == Keys.LMenu || e.KeyCode == Keys.RMenu ||
                e.KeyCode == Keys.ShiftKey || e.KeyCode == Keys.LShiftKey ||
                e.KeyCode == Keys.RShiftKey)
            {
                return;
            }

            uint modifiers = 0;
            if (e.Control) modifiers |= 0x0002;
            if (e.Alt) modifiers |= 0x0001;
            if (e.Shift) modifiers |= 0x0004;
            int keyCode = (int)e.KeyCode;

            if (modifiers > 0 && keyCode > 0)
            {
                var text = FormatShortcut(keyCode, modifiers);
                row.KeyLabel.Text = text;
                row.KeyLabel.ForeColor = SettingsTheme.TextColor;

                if (!_config.Shortcuts.ContainsKey(action))
                    _config.Shortcuts[action] = new ShortcutConfig();

                _config.Shortcuts[action].KeyCode = keyCode;
                _config.Shortcuts[action].ModifierFlags = modifiers;
                _config.Shortcuts[action].Enabled = true;

                _configService.Save(_config);
                Program.HotkeyManager?.ReloadFromConfig(_config.Shortcuts);
                Program.HotkeyManager?.SetCapturingMode(false);
                dialog.Close();
            }
        };

        dialog.KeyPress += (s, e) => e.Handled = true;
        dialog.FormClosed += (s, e) => Program.HotkeyManager?.SetCapturingMode(false);
        dialog.ShowDialog(this);
    }

    private void ClearShortcut(string action, ShortcutRow row)
    {
        row.KeyLabel.Text = "记录快捷键";
        row.KeyLabel.ForeColor = SettingsTheme.SecondaryTextColor;

        if (_config.Shortcuts.ContainsKey(action))
        {
            _config.Shortcuts[action].KeyCode = 0;
            _config.Shortcuts[action].ModifierFlags = 0;
            _config.Shortcuts[action].Enabled = false;
        }

        _configService.Save(_config);
        Program.HotkeyManager?.ReloadFromConfig(_config.Shortcuts);
    }

    private string FormatShortcut(int keyCode, uint modifiers)
    {
        var parts = new List<string>();

        if ((modifiers & 0x0002) != 0) parts.Add("Ctrl");
        if ((modifiers & 0x0001) != 0) parts.Add("Alt");
        if ((modifiers & 0x0004) != 0) parts.Add("Shift");
        if ((modifiers & 0x0008) != 0) parts.Add("Win");

        var keyMappings = new Dictionary<Keys, string>
        {
            [Keys.Left] = "←", [Keys.Right] = "→",
            [Keys.Up] = "↑", [Keys.Down] = "↓",
            [Keys.Enter] = "↵", [Keys.Back] = "⌫",
            [Keys.Delete] = "Del", [Keys.Space] = "Space",
            [Keys.OemMinus] = "-", [Keys.Oemplus] = "="
        };

        var key = (Keys)keyCode;
        parts.Add(keyMappings.TryGetValue(key, out var mapped) ? mapped : key.ToString());
        return string.Join("+", parts);
    }

    #endregion

    #region Settings Loading/Saving

    private void LoadSettings()
    {
        _config = _configService.Load();
        var defaults = ConfigService.GetDefaultShortcuts();

        foreach (var (action, row) in _shortcutRows)
        {
            var config = _config.Shortcuts.TryGetValue(action, out var c) ? c :
                         defaults.TryGetValue(action, out var d) ? d : null;

            if (config != null && config.KeyCode > 0 && config.Enabled)
            {
                row.KeyLabel.Text = FormatShortcut(config.KeyCode, config.ModifierFlags);
                row.KeyLabel.ForeColor = SettingsTheme.TextColor;
            }
        }

        _gapSizeSlider.Value = _config.GapSize;
        _launchOnLoginCheckBox.Checked = _config.LaunchOnLogin;
        _dragToSnapCheckBox.Checked = _config.SnapAreas.DragToSnap;
        _restoreSizeCheckBox.Checked = _config.SnapAreas.RestoreSizeOnSnapEnd;
        _snapAnimationCheckBox.Checked = _config.SnapAreas.SnapAnimation;
    }

    private void LaunchOnLoginCheckBox_CheckedChanged(object? sender, EventArgs e)
    {
        _config.LaunchOnLogin = _launchOnLoginCheckBox.Checked;
        try
        {
            var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Run", true);

            if (_config.LaunchOnLogin)
            {
                var exePath = Environment.ProcessPath ?? Application.ExecutablePath;
                key?.SetValue("Rectangle", exePath);
            }
            else
            {
                key?.DeleteValue("Rectangle", false);
            }
        }
        catch { }
    }

    private void RestoreDefaults_Click(object? sender, EventArgs e)
    {
        _config.Shortcuts = ConfigService.GetDefaultShortcuts();
        _configService.Save(_config);
        LoadSettings();
        Program.HotkeyManager?.ReloadFromConfig(_config.Shortcuts);
    }

    private void SettingsForm_FormClosed(object? sender, FormClosedEventArgs e)
    {
        _configService.Save(_config);
        Program.HotkeyManager?.ReloadFromConfig(_config.Shortcuts);
    }

    #endregion

    private class ShortcutRow
    {
        public Panel Container { get; set; } = null!;
        public Label NameLabel { get; set; } = null!;
        public Label KeyLabel { get; set; } = null!;
        public Label ClearButton { get; set; } = null!;
    }
}
