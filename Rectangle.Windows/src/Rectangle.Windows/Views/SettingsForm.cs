using Rectangle.Windows.Core;
using Rectangle.Windows.Services;
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

    // 颜色主题
    private static readonly Color BackgroundColor = Color.FromArgb(32, 32, 32);
    private static readonly Color CardColor = Color.FromArgb(45, 45, 45);
    private static readonly Color CardHoverColor = Color.FromArgb(55, 55, 55);
    private static readonly Color AccentColor = Color.FromArgb(0, 120, 212);
    private static readonly Color TextColor = Color.FromArgb(255, 255, 255);
    private static readonly Color SecondaryTextColor = Color.FromArgb(180, 180, 180);
    private static readonly Color BorderColor = Color.FromArgb(60, 60, 60);
    private static readonly Color InputBackColor = Color.FromArgb(38, 38, 38);

    // 导航
    private Panel _navPanel = null!;
    private Panel _contentPanel = null!;
    private int _selectedNavIndex = 0;
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
        Size = new Size(900, 640);
        MinimumSize = new Size(800, 500);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = BackgroundColor;
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
            BackColor = Color.FromArgb(28, 28, 28),
            Padding = new Padding(8, 20, 8, 20)
        };

        // 标题
        var titleLabel = new Label
        {
            Text = "Rectangle",
            Font = new Font("Microsoft YaHei UI", 14F, FontStyle.Bold),
            ForeColor = TextColor,
            Location = new Point(20, 20),
            AutoSize = true
        };
        _navPanel.Controls.Add(titleLabel);

        // 版本号
        var versionLabel = new Label
        {
            Text = "v1.0.0",
            Font = new Font("Microsoft YaHei UI", 8F),
            ForeColor = SecondaryTextColor,
            Location = new Point(20, 48),
            AutoSize = true
        };
        _navPanel.Controls.Add(versionLabel);

        // 导航按钮
        var navItems = new[]
        {
            ("⌨️", "键盘快捷键"),
            ("📐", "吸附区域"),
            ("⚙️", "设置")
        };

        int y = 90;
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
            BackColor = BackgroundColor,
            Padding = new Padding(30, 20, 30, 20)
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
        _selectedNavIndex = index;

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
            BackColor = BackgroundColor
        };

        var container = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(0, 0, 20, 20)
        };

        // 页面标题
        AddPageTitle(container, "键盘快捷键", "配置窗口管理的快捷键");

        // 左右两列布局
        var columnsPanel = new TableLayoutPanel
        {
            AutoSize = true,
            ColumnCount = 2,
            RowCount = 1,
            Margin = new Padding(0, 10, 0, 0)
        };
        columnsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 320));
        columnsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 320));

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
            Width = 300,
            Margin = new Padding(0, 0, 0, 12)
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
            ForeColor = TextColor,
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
            Width = 280,
            Height = 32,
            Margin = new Padding(0, 2, 0, 2)
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
            ForeColor = TextColor,
            Location = new Point(24, 7),
            AutoSize = true
        };
        row.Container.Controls.Add(row.NameLabel);

        // 快捷键输入框
        row.KeyLabel = new Label
        {
            Width = 100,
            Height = 24,
            Location = new Point(130, 4),
            BackColor = InputBackColor,
            ForeColor = SecondaryTextColor,
            TextAlign = ContentAlignment.MiddleCenter,
            Text = "记录快捷键",
            Cursor = Cursors.Hand
        };
        row.KeyLabel.Click += (s, e) => ShowShortcutCaptureDialog(action, row);
        row.KeyLabel.Paint += (s, e) =>
        {
            using var pen = new Pen(BorderColor, 1);
            var rect = new Rectangle(0, 0, row.KeyLabel.Width - 1, row.KeyLabel.Height - 1);
            using var path = CreateRoundedRect(rect, 4);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.DrawPath(pen, path);
        };
        row.Container.Controls.Add(row.KeyLabel);

        // 清除按钮
        row.ClearButton = new Label
        {
            Text = "✕",
            ForeColor = SecondaryTextColor,
            Location = new Point(238, 7),
            AutoSize = true,
            Cursor = Cursors.Hand
        };
        row.ClearButton.Click += (s, e) => ClearShortcut(action, row);
        row.ClearButton.MouseEnter += (s, e) => row.ClearButton.ForeColor = Color.White;
        row.ClearButton.MouseLeave += (s, e) => row.ClearButton.ForeColor = SecondaryTextColor;
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
            BackColor = BackgroundColor,
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
        var optionsCard = new ModernCard { Width = 500, Height = 200 };
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
        var previewCard = new ModernCard { Width = 500, Height = 280, Margin = new Padding(0, 20, 0, 0) };
        var previewLabel = new Label
        {
            Text = "吸附区域示意",
            Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold),
            ForeColor = TextColor,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 10)
        };

        var previewPanel = new SnapAreaPreview
        {
            Width = 460,
            Height = 200,
            Margin = new Padding(0, 10, 0, 0)
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
            BackColor = BackgroundColor,
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
        var generalCard = new ModernCard { Width = 500, Height = 160 };
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
        var gapPanel = new Panel { Width = 460, Height = 60 };
        var gapLabel = new Label
        {
            Text = "窗口间隙",
            ForeColor = TextColor,
            Location = new Point(0, 5),
            AutoSize = true
        };
        var gapDesc = new Label
        {
            Text = "窗口之间的间距",
            ForeColor = SecondaryTextColor,
            Font = new Font("Microsoft YaHei UI", 8F),
            Location = new Point(0, 25),
            AutoSize = true
        };
        _gapSizeSlider = new ModernSlider(0, 30)
        {
            Location = new Point(300, 10),
            Width = 160
        };
        _gapSizeSlider.ValueChanged += (s, e) => _config.GapSize = _gapSizeSlider.Value;
        gapPanel.Controls.Add(gapLabel);
        gapPanel.Controls.Add(gapDesc);
        gapPanel.Controls.Add(_gapSizeSlider);
        generalPanel.Controls.Add(gapPanel);

        generalCard.SetContent(generalPanel);
        container.Controls.Add(generalCard);

        // 关于卡片
        var aboutCard = new ModernCard { Width = 500, Height = 120, Margin = new Padding(0, 20, 0, 0) };
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
            ForeColor = TextColor,
            AutoSize = true
        };
        var aboutVersion = new Label
        {
            Text = "版本 1.0.0 · 基于 macOS Rectangle 移植",
            ForeColor = SecondaryTextColor,
            AutoSize = true,
            Margin = new Padding(0, 5, 0, 0)
        };
        var aboutLink = new LinkLabel
        {
            Text = "GitHub: rxhanson/Rectangle",
            LinkColor = AccentColor,
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
            Font = new Font("Microsoft YaHei UI", 16F, FontStyle.Bold),
            ForeColor = TextColor,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 5)
        };
        parent.Controls.Add(titleLabel);

        var descLabel = new Label
        {
            Text = description,
            ForeColor = SecondaryTextColor,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 20)
        };
        parent.Controls.Add(descLabel);
    }

    private Image? LoadIcon(string iconName)
    {
        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        string resourceName = $"Rectangle.Windows.Assets.WindowPositions.{iconName}";
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream != null)
        {
            try { return Image.FromStream(stream); }
            catch { }
        }
        return null;
    }

    private static GraphicsPath CreateRoundedRect(Rectangle rect, int radius)
    {
        var path = new GraphicsPath();
        var d = radius * 2;
        path.AddArc(rect.X, rect.Y, d, d, 180, 90);
        path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
        path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
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
            BackColor = BackgroundColor,
            ForeColor = TextColor,
            KeyPreview = true
        };

        var label = new Label
        {
            Text = "请按下新的快捷键组合\n例如: Ctrl+Alt+←",
            ForeColor = TextColor,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Top,
            Height = 80,
            Font = new Font("Microsoft YaHei UI", 10F)
        };

        var hintLabel = new Label
        {
            Text = "按 Escape 取消",
            ForeColor = SecondaryTextColor,
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
                row.KeyLabel.ForeColor = TextColor;

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
        row.KeyLabel.ForeColor = SecondaryTextColor;

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
                row.KeyLabel.ForeColor = TextColor;
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

#region Custom Controls

/// <summary>
/// 导航按钮
/// </summary>
internal class NavButton : Control
{
    public int Index { get; }
    public bool IsSelected { get; set; }

    private readonly string _icon;
    private readonly string _text;
    private bool _isHovered;

    private static readonly Color NormalColor = Color.Transparent;
    private static readonly Color HoverColor = Color.FromArgb(45, 45, 45);
    private static readonly Color SelectedColor = Color.FromArgb(55, 55, 55);
    private static readonly Color AccentColor = Color.FromArgb(0, 120, 212);

    public NavButton(string icon, string text, int index)
    {
        _icon = icon;
        _text = text;
        Index = index;
        Height = 40;
        Cursor = Cursors.Hand;
        DoubleBuffered = true;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var bgColor = IsSelected ? SelectedColor : (_isHovered ? HoverColor : NormalColor);
        var rect = new Rectangle(0, 0, Width - 1, Height - 1);

        using var path = CreateRoundedRect(rect, 6);
        using var brush = new SolidBrush(bgColor);
        g.FillPath(brush, path);

        if (IsSelected)
        {
            using var accentBrush = new SolidBrush(AccentColor);
            g.FillRectangle(accentBrush, 0, 8, 3, Height - 16);
        }

        using var textBrush = new SolidBrush(Color.White);
        g.DrawString(_icon, new Font("Segoe UI Emoji", 11F), textBrush, 12, 10);
        g.DrawString(_text, new Font("Microsoft YaHei UI", 9F), textBrush, 38, 11);
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        _isHovered = true;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        _isHovered = false;
        Invalidate();
    }

    private static GraphicsPath CreateRoundedRect(Rectangle rect, int radius)
    {
        var path = new GraphicsPath();
        var d = radius * 2;
        path.AddArc(rect.X, rect.Y, d, d, 180, 90);
        path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
        path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }
}

/// <summary>
/// 现代卡片
/// </summary>
internal class ModernCard : Panel
{
    private static readonly Color CardColor = Color.FromArgb(45, 45, 45);
    private static readonly Color BorderColor = Color.FromArgb(60, 60, 60);

    public ModernCard()
    {
        DoubleBuffered = true;
        Padding = new Padding(16);
    }

    public void SetContent(Control content)
    {
        content.Dock = DockStyle.Fill;
        Controls.Add(content);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        using var path = CreateRoundedRect(rect, 8);
        using var brush = new SolidBrush(CardColor);
        using var pen = new Pen(BorderColor, 1);

        g.FillPath(brush, path);
        g.DrawPath(pen, path);
    }

    private static GraphicsPath CreateRoundedRect(Rectangle rect, int radius)
    {
        var path = new GraphicsPath();
        var d = radius * 2;
        path.AddArc(rect.X, rect.Y, d, d, 180, 90);
        path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
        path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }
}

/// <summary>
/// 现代复选框
/// </summary>
internal class ModernCheckBox : Panel
{
    public bool Checked { get; set; }
    public event EventHandler? CheckedChanged;

    private readonly Label _titleLabel;
    private readonly Label _descLabel;
    private readonly Panel _checkBox;

    private static readonly Color CheckedColor = Color.FromArgb(0, 120, 212);
    private static readonly Color UncheckedColor = Color.FromArgb(60, 60, 60);

    public ModernCheckBox(string title, string description)
    {
        Width = 460;
        Height = 50;
        Cursor = Cursors.Hand;

        _titleLabel = new Label
        {
            Text = title,
            ForeColor = Color.White,
            Location = new Point(0, 5),
            AutoSize = true
        };
        Controls.Add(_titleLabel);

        _descLabel = new Label
        {
            Text = description,
            ForeColor = Color.FromArgb(150, 150, 150),
            Font = new Font("Microsoft YaHei UI", 8F),
            Location = new Point(0, 25),
            AutoSize = true
        };
        Controls.Add(_descLabel);

        _checkBox = new Panel
        {
            Size = new Size(44, 22),
            Location = new Point(416, 14)
        };
        _checkBox.Paint += CheckBox_Paint;
        _checkBox.Click += Toggle;
        Controls.Add(_checkBox);

        _titleLabel.Click += Toggle;
        _descLabel.Click += Toggle;
        Click += Toggle;
    }

    private void Toggle(object? sender, EventArgs e)
    {
        Checked = !Checked;
        _checkBox.Invalidate();
        CheckedChanged?.Invoke(this, EventArgs.Empty);
    }

    private void CheckBox_Paint(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var rect = new Rectangle(0, 0, 43, 21);
        using var path = CreateRoundedRect(rect, 10);

        var bgColor = Checked ? CheckedColor : UncheckedColor;
        using var brush = new SolidBrush(bgColor);
        g.FillPath(brush, path);

        var circleX = Checked ? 24 : 3;
        using var circleBrush = new SolidBrush(Color.White);
        g.FillEllipse(circleBrush, circleX, 3, 16, 16);
    }

    private static GraphicsPath CreateRoundedRect(Rectangle rect, int radius)
    {
        var path = new GraphicsPath();
        var d = radius * 2;
        path.AddArc(rect.X, rect.Y, d, d, 180, 90);
        path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
        path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }
}

/// <summary>
/// 现代滑块
/// </summary>
internal class ModernSlider : Control
{
    public int Value { get; set; }
    public int Minimum { get; }
    public int Maximum { get; }
    public event EventHandler? ValueChanged;

    private bool _isDragging;
    private static readonly Color TrackColor = Color.FromArgb(60, 60, 60);
    private static readonly Color FillColor = Color.FromArgb(0, 120, 212);
    private static readonly Color ThumbColor = Color.White;

    public ModernSlider(int min, int max)
    {
        Minimum = min;
        Maximum = max;
        Height = 30;
        DoubleBuffered = true;
        Cursor = Cursors.Hand;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var trackY = Height / 2 - 2;
        var trackRect = new Rectangle(8, trackY, Width - 16, 4);
        using var trackBrush = new SolidBrush(TrackColor);
        g.FillRectangle(trackBrush, trackRect);

        var ratio = (float)(Value - Minimum) / (Maximum - Minimum);
        var fillWidth = (int)((Width - 16) * ratio);
        using var fillBrush = new SolidBrush(FillColor);
        g.FillRectangle(fillBrush, 8, trackY, fillWidth, 4);

        var thumbX = 8 + fillWidth - 8;
        using var thumbBrush = new SolidBrush(ThumbColor);
        g.FillEllipse(thumbBrush, thumbX, Height / 2 - 8, 16, 16);

        using var valueBrush = new SolidBrush(Color.FromArgb(150, 150, 150));
        g.DrawString($"{Value} px", Font, valueBrush, Width - 45, 7);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        _isDragging = true;
        UpdateValue(e.X);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (_isDragging)
            UpdateValue(e.X);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        _isDragging = false;
    }

    private void UpdateValue(int x)
    {
        var ratio = Math.Clamp((float)(x - 8) / (Width - 16), 0, 1);
        var newValue = (int)(Minimum + ratio * (Maximum - Minimum));
        if (newValue != Value)
        {
            Value = newValue;
            Invalidate();
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}

/// <summary>
/// 现代按钮
/// </summary>
internal class ModernButton : Control
{
    private bool _isHovered;
    private static readonly Color NormalColor = Color.FromArgb(55, 55, 55);
    private static readonly Color HoverColor = Color.FromArgb(65, 65, 65);
    private static readonly Color BorderColor = Color.FromArgb(80, 80, 80);

    public ModernButton(string text)
    {
        Text = text;
        Height = 32;
        Cursor = Cursors.Hand;
        DoubleBuffered = true;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        using var path = CreateRoundedRect(rect, 6);

        var bgColor = _isHovered ? HoverColor : NormalColor;
        using var brush = new SolidBrush(bgColor);
        using var pen = new Pen(BorderColor, 1);

        g.FillPath(brush, path);
        g.DrawPath(pen, path);

        using var textBrush = new SolidBrush(Color.White);
        var textSize = g.MeasureString(Text, Font);
        g.DrawString(Text, Font, textBrush,
            (Width - textSize.Width) / 2,
            (Height - textSize.Height) / 2);
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        _isHovered = true;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        _isHovered = false;
        Invalidate();
    }

    private static GraphicsPath CreateRoundedRect(Rectangle rect, int radius)
    {
        var path = new GraphicsPath();
        var d = radius * 2;
        path.AddArc(rect.X, rect.Y, d, d, 180, 90);
        path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
        path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }
}

/// <summary>
/// 吸附区域预览
/// </summary>
internal class SnapAreaPreview : Control
{
    private static readonly Color ScreenColor = Color.FromArgb(35, 35, 35);
    private static readonly Color BorderColor = Color.FromArgb(80, 80, 80);
    private static readonly Color ZoneColor = Color.FromArgb(0, 120, 212);

    public SnapAreaPreview()
    {
        DoubleBuffered = true;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        // 屏幕背景
        var screenRect = new Rectangle(20, 20, Width - 40, Height - 40);
        using var screenBrush = new SolidBrush(ScreenColor);
        using var screenPen = new Pen(BorderColor, 2);
        g.FillRectangle(screenBrush, screenRect);
        g.DrawRectangle(screenPen, screenRect);

        // 吸附区域标注
        var zones = new[]
        {
            (new Rectangle(20, 20, 30, 40), "左上"),
            (new Rectangle(Width / 2 - 15, 20, 30, 30), "上"),
            (new Rectangle(Width - 70, 20, 30, 40), "右上"),
            (new Rectangle(20, Height / 2 - 15, 30, 30), "左"),
            (new Rectangle(Width - 70, Height / 2 - 15, 30, 30), "右"),
            (new Rectangle(20, Height - 80, 30, 40), "左下"),
            (new Rectangle(Width / 2 - 15, Height - 70, 30, 30), "下"),
            (new Rectangle(Width - 70, Height - 80, 30, 40), "右下")
        };

        using var zoneBrush = new SolidBrush(Color.FromArgb(40, ZoneColor));
        using var zonePen = new Pen(Color.FromArgb(100, ZoneColor), 1);
        using var textBrush = new SolidBrush(Color.FromArgb(120, 120, 120));
        var font = new Font("Microsoft YaHei UI", 7F);

        foreach (var (rect, label) in zones)
        {
            g.FillRectangle(zoneBrush, rect);
            g.DrawRectangle(zonePen, rect);
        }
    }
}

#endregion
