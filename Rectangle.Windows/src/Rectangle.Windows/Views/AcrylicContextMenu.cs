using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Rectangle.Windows.Views;

/// <summary>
/// 带亚克力/毛玻璃效果的上下文菜单，自动适配系统主题
/// </summary>
public class AcrylicContextMenu : ContextMenuStrip
{
    private const int CS_DROPSHADOW = 0x00020000;
    
    // DWM API for Windows 11 Mica/Acrylic
    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

    [DllImport("user32.dll")]
    private static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

    [StructLayout(LayoutKind.Sequential)]
    private struct WindowCompositionAttributeData
    {
        public int Attribute;
        public IntPtr Data;
        public int SizeOfData;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct AccentPolicy
    {
        public int AccentState;
        public int AccentFlags;
        public int GradientColor;
        public int AnimationId;
    }

    private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
    private const int DWMWA_SYSTEMBACKDROP_TYPE = 38;
    private const int DWMSBT_MAINWINDOW = 2; // Mica
    private const int DWMSBT_TRANSIENTWINDOW = 3; // Acrylic

    private static bool _isDarkTheme;
    private static bool _themeInitialized = false;

    public AcrylicContextMenu()
    {
        // 检测系统主题
        UpdateTheme();
        
        // 监听系统主题变化
        SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
        
        var renderer = new AcrylicMenuRenderer(_isDarkTheme);
        Renderer = renderer;
        ShowImageMargin = true;
        ShowCheckMargin = false;
        BackColor = _isDarkTheme ? Color.FromArgb(240, 32, 32, 32) : Color.FromArgb(240, 243, 243, 243);
        ForeColor = _isDarkTheme ? Color.White : Color.FromArgb(30, 30, 30);
        Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular);
        Padding = new Padding(4, 8, 4, 8);
    }

    private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
    {
        if (e.Category == UserPreferenceCategory.General || e.Category == UserPreferenceCategory.Color)
        {
            UpdateTheme();
            ApplyTheme();
        }
    }

    private static void UpdateTheme()
    {
        try
        {
            // 使用注册表检测 Windows 应用模式
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            if (key != null)
            {
                var appsUseLightTheme = key.GetValue("AppsUseLightTheme");
                _isDarkTheme = appsUseLightTheme == null || (int)appsUseLightTheme == 0;
            }
            else
            {
                // 默认使用浅色主题
                _isDarkTheme = false;
            }
            _themeInitialized = true;
        }
        catch
        {
            _isDarkTheme = false;
            _themeInitialized = true;
        }
    }

    private void ApplyTheme()
    {
        if (IsDisposed) return;
        
        var newRenderer = new AcrylicMenuRenderer(_isDarkTheme);
        Renderer = newRenderer;
        BackColor = _isDarkTheme ? Color.FromArgb(240, 32, 32, 32) : Color.FromArgb(240, 243, 243, 243);
        ForeColor = _isDarkTheme ? Color.White : Color.FromArgb(30, 30, 30);
        
        // 更新菜单项颜色
        foreach (ToolStripItem item in Items)
        {
            UpdateItemColor(item);
        }
        
        Invalidate();
    }

    private void UpdateItemColor(ToolStripItem item)
    {
        item.ForeColor = _isDarkTheme ? Color.White : Color.FromArgb(30, 30, 30);
        
        if (item is ToolStripMenuItem menuItem)
        {
            foreach (ToolStripItem subItem in menuItem.DropDownItems)
            {
                UpdateItemColor(subItem);
            }
        }
    }

    public static bool IsDarkTheme
    {
        get
        {
            if (!_themeInitialized)
            {
                UpdateTheme();
            }
            return _isDarkTheme;
        }
    }

    protected override CreateParams CreateParams
    {
        get
        {
            var cp = base.CreateParams;
            cp.ClassStyle |= CS_DROPSHADOW;
            return cp;
        }
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        EnableAcrylic();
    }

    private void EnableAcrylic()
    {
        if (Environment.OSVersion.Version.Build >= 22000) // Windows 11
        {
            // 使用 Acrylic 效果
            int value = DWMSBT_TRANSIENTWINDOW;
            DwmSetWindowAttribute(Handle, DWMWA_SYSTEMBACKDROP_TYPE, ref value, sizeof(int));

            // 根据主题设置暗色/亮色模式
            int darkMode = _isDarkTheme ? 1 : 0;
            DwmSetWindowAttribute(Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref darkMode, sizeof(int));
        }
        else if (Environment.OSVersion.Version.Build >= 17763) // Windows 10 1809+
        {
            EnableAcrylicBlur();
        }
    }

    private void EnableAcrylicBlur()
    {
        // 根据主题设置颜色
        int gradientColor = _isDarkTheme 
            ? unchecked((int)0x99000000)  // 深色主题：半透明黑色
            : unchecked((int)0x99FFFFFF); // 浅色主题：半透明白色

        var accent = new AccentPolicy
        {
            AccentState = 3, // ACCENT_ENABLE_BLURBEHIND
            AccentFlags = 2,
            GradientColor = gradientColor
        };

        var accentSize = Marshal.SizeOf(accent);
        var accentPtr = Marshal.AllocHGlobal(accentSize);
        Marshal.StructureToPtr(accent, accentPtr, false);

        var data = new WindowCompositionAttributeData
        {
            Attribute = 19, // WCA_ACCENT_POLICY
            Data = accentPtr,
            SizeOfData = accentSize
        };

        SetWindowCompositionAttribute(Handle, ref data);
        Marshal.FreeHGlobal(accentPtr);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            SystemEvents.UserPreferenceChanged -= SystemEvents_UserPreferenceChanged;
        }
        base.Dispose(disposing);
    }
}

/// <summary>
/// 亚克力风格菜单渲染器，支持深色和浅色主题
/// </summary>
public class AcrylicMenuRenderer : ToolStripProfessionalRenderer
{
    // 深色主题颜色
    private static readonly Color DarkBackground = Color.FromArgb(220, 32, 32, 32);
    private static readonly Color DarkHover = Color.FromArgb(255, 55, 55, 55);
    private static readonly Color DarkBorder = Color.FromArgb(60, 255, 255, 255);
    private static readonly Color DarkText = Color.FromArgb(255, 255, 255, 255);
    private static readonly Color DarkShortcut = Color.FromArgb(180, 200, 200, 200);
    private static readonly Color DarkSeparator = Color.FromArgb(60, 255, 255, 255);
    private static readonly Color DarkDisabled = Color.FromArgb(100, 150, 150, 150);

    // 浅色主题颜色
    private static readonly Color LightBackground = Color.FromArgb(230, 243, 243, 243);
    private static readonly Color LightHover = Color.FromArgb(255, 230, 230, 230);
    private static readonly Color LightBorder = Color.FromArgb(80, 0, 0, 0);
    private static readonly Color LightText = Color.FromArgb(30, 30, 30);
    private static readonly Color LightShortcut = Color.FromArgb(120, 80, 80, 80);
    private static readonly Color LightSeparator = Color.FromArgb(60, 0, 0, 0);
    private static readonly Color LightDisabled = Color.FromArgb(120, 160, 160, 160);

    private readonly bool _isDark;

    public AcrylicMenuRenderer(bool isDark) : base(isDark ? new DarkAcrylicColorTable() : new LightAcrylicColorTable())
    {
        _isDark = isDark;
        RoundedEdges = true;
    }

    private Color BackgroundColor => _isDark ? DarkBackground : LightBackground;
    private Color HoverColor => _isDark ? DarkHover : LightHover;
    private Color BorderColor => _isDark ? DarkBorder : LightBorder;
    private Color TextColor => _isDark ? DarkText : LightText;
    private Color ShortcutColor => _isDark ? DarkShortcut : LightShortcut;
    private Color SeparatorColor => _isDark ? DarkSeparator : LightSeparator;
    private Color DisabledColor => _isDark ? DarkDisabled : LightDisabled;

    protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
    {
        using var brush = new SolidBrush(BackgroundColor);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        
        var rect = new System.Drawing.Rectangle(0, 0, e.ToolStrip.Width, e.ToolStrip.Height);
        using var path = CreateRoundedRectangle(rect, 8);
        e.Graphics.FillPath(brush, path);
    }

    protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
    {
        using var pen = new Pen(BorderColor, 1);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        
        var rect = new System.Drawing.Rectangle(0, 0, e.ToolStrip.Width - 1, e.ToolStrip.Height - 1);
        using var path = CreateRoundedRectangle(rect, 8);
        e.Graphics.DrawPath(pen, path);
    }

    protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
    {
        var item = e.Item;
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var rect = new System.Drawing.Rectangle(4, 0, item.Width - 8, item.Height);

        if (item.Selected && item.Enabled)
        {
            using var brush = new SolidBrush(HoverColor);
            using var path = CreateRoundedRectangle(rect, 4);
            g.FillPath(brush, path);
        }
    }

    protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
    {
        var g = e.Graphics;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        
        var textColor = e.Item.Enabled ? TextColor : DisabledColor;
        var item = e.Item;
        
        // 计算文本区域（左侧，排除图标区域）
        int iconWidth = 24; // 图标区域宽度
        int leftPadding = 8;
        int rightPadding = 12;
        int shortcutWidth = 140; // 增加快捷键区域宽度
        
        var textRect = new System.Drawing.Rectangle(
            iconWidth + leftPadding,
            2,
            item.Width - iconWidth - leftPadding - rightPadding - shortcutWidth,
            item.Height - 4);
        
        // 绘制菜单项文本
        using var textBrush = new SolidBrush(textColor);
        using var format = new StringFormat
        {
            Alignment = StringAlignment.Near,
            LineAlignment = StringAlignment.Center,
            Trimming = StringTrimming.EllipsisCharacter,
            FormatFlags = StringFormatFlags.NoWrap
        };
        
        g.DrawString(item.Text, e.TextFont, textBrush, textRect, format);
        
        // 绘制快捷键文本（如果有）
        if (e.Item is ToolStripMenuItem menuItem && !string.IsNullOrEmpty(menuItem.ShortcutKeyDisplayString))
        {
            // 快捷键区域在右侧，固定宽度
            var shortcutRect = new System.Drawing.Rectangle(
                item.Width - shortcutWidth - rightPadding,
                2,
                shortcutWidth,
                item.Height - 4);
            
            using var shortcutBrush = new SolidBrush(ShortcutColor);
            using var shortcutFormat = new StringFormat
            {
                Alignment = StringAlignment.Far,
                LineAlignment = StringAlignment.Center,
                FormatFlags = StringFormatFlags.NoWrap
            };
            
            g.DrawString(menuItem.ShortcutKeyDisplayString, e.TextFont, shortcutBrush, shortcutRect, shortcutFormat);
        }
    }

    protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
    {
        var g = e.Graphics;
        var y = e.Item.Height / 2;
        using var pen = new Pen(SeparatorColor, 1);
        g.DrawLine(pen, 12, y, e.Item.Width - 12, y);
    }

    protected override void OnRenderItemImage(ToolStripItemImageRenderEventArgs e)
    {
        if (e.Image != null)
        {
            var rect = e.ImageRectangle;
            
            if (!e.Item.Enabled)
            {
                using var disabledImage = CreateDisabledImage(e.Image);
                e.Graphics.DrawImage(disabledImage, rect);
            }
            else
            {
                // 如果是浅色主题且图像是白色图标，需要着色
                if (!_isDark)
                {
                    using var tintedImage = TintImageForLightTheme(e.Image);
                    e.Graphics.DrawImage(tintedImage, rect);
                }
                else
                {
                    e.Graphics.DrawImage(e.Image, rect);
                }
            }
        }
    }

    protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
    {
        e.ArrowColor = e.Item.Enabled ? TextColor : DisabledColor;
        base.OnRenderArrow(e);
    }

    private Image TintImageForLightTheme(Image original)
    {
        // 为浅色主题将白色图标转换为深色
        var result = new Bitmap(original.Width, original.Height);
        using var g = Graphics.FromImage(result);
        using var attr = new System.Drawing.Imaging.ImageAttributes();
        
        var matrix = new System.Drawing.Imaging.ColorMatrix(new float[][]
        {
            new float[] { 0.3f, 0.3f, 0.3f, 0, 0 },
            new float[] { 0.3f, 0.3f, 0.3f, 0, 0 },
            new float[] { 0.3f, 0.3f, 0.3f, 0, 0 },
            new float[] { 0, 0, 0, 1, 0 },
            new float[] { 0, 0, 0, 0, 1 }
        });
        
        attr.SetColorMatrix(matrix);
        g.DrawImage(original, new System.Drawing.Rectangle(0, 0, original.Width, original.Height),
            0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attr);
        
        return result;
    }

    private static Image CreateDisabledImage(Image original)
    {
        var result = new Bitmap(original.Width, original.Height);
        using var g = Graphics.FromImage(result);
        using var attr = new System.Drawing.Imaging.ImageAttributes();
        
        var matrix = new System.Drawing.Imaging.ColorMatrix(new float[][]
        {
            new float[] { 0.3f, 0.3f, 0.3f, 0, 0 },
            new float[] { 0.3f, 0.3f, 0.3f, 0, 0 },
            new float[] { 0.3f, 0.3f, 0.3f, 0, 0 },
            new float[] { 0, 0, 0, 0.5f, 0 },
            new float[] { 0, 0, 0, 0, 1 }
        });
        
        attr.SetColorMatrix(matrix);
        g.DrawImage(original, new System.Drawing.Rectangle(0, 0, original.Width, original.Height),
            0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attr);
        
        return result;
    }

    private static GraphicsPath CreateRoundedRectangle(System.Drawing.Rectangle rect, int radius)
    {
        var path = new GraphicsPath();
        var diameter = radius * 2;
        
        path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
        path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
        path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();
        
        return path;
    }
}

/// <summary>
/// 深色主题颜色表
/// </summary>
public class DarkAcrylicColorTable : ProfessionalColorTable
{
    public override Color MenuBorder => Color.FromArgb(60, 255, 255, 255);
    public override Color MenuItemBorder => Color.Transparent;
    public override Color MenuItemSelected => Color.FromArgb(255, 55, 55, 55);
    public override Color MenuItemSelectedGradientBegin => Color.FromArgb(255, 55, 55, 55);
    public override Color MenuItemSelectedGradientEnd => Color.FromArgb(255, 55, 55, 55);
    public override Color MenuItemPressedGradientBegin => Color.FromArgb(255, 0, 120, 212);
    public override Color MenuItemPressedGradientEnd => Color.FromArgb(255, 0, 120, 212);
    public override Color MenuStripGradientBegin => Color.FromArgb(220, 32, 32, 32);
    public override Color MenuStripGradientEnd => Color.FromArgb(220, 32, 32, 32);
    public override Color ToolStripDropDownBackground => Color.FromArgb(220, 32, 32, 32);
    public override Color ImageMarginGradientBegin => Color.FromArgb(220, 32, 32, 32);
    public override Color ImageMarginGradientMiddle => Color.FromArgb(220, 32, 32, 32);
    public override Color ImageMarginGradientEnd => Color.FromArgb(220, 32, 32, 32);
    public override Color SeparatorDark => Color.FromArgb(60, 255, 255, 255);
    public override Color SeparatorLight => Color.Transparent;
}

/// <summary>
/// 浅色主题颜色表
/// </summary>
public class LightAcrylicColorTable : ProfessionalColorTable
{
    public override Color MenuBorder => Color.FromArgb(80, 0, 0, 0);
    public override Color MenuItemBorder => Color.Transparent;
    public override Color MenuItemSelected => Color.FromArgb(255, 230, 230, 230);
    public override Color MenuItemSelectedGradientBegin => Color.FromArgb(255, 230, 230, 230);
    public override Color MenuItemSelectedGradientEnd => Color.FromArgb(255, 230, 230, 230);
    public override Color MenuItemPressedGradientBegin => Color.FromArgb(255, 0, 120, 212);
    public override Color MenuItemPressedGradientEnd => Color.FromArgb(255, 0, 120, 212);
    public override Color MenuStripGradientBegin => Color.FromArgb(230, 243, 243, 243);
    public override Color MenuStripGradientEnd => Color.FromArgb(230, 243, 243, 243);
    public override Color ToolStripDropDownBackground => Color.FromArgb(230, 243, 243, 243);
    public override Color ImageMarginGradientBegin => Color.FromArgb(230, 243, 243, 243);
    public override Color ImageMarginGradientMiddle => Color.FromArgb(230, 243, 243, 243);
    public override Color ImageMarginGradientEnd => Color.FromArgb(230, 243, 243, 243);
    public override Color SeparatorDark => Color.FromArgb(60, 0, 0, 0);
    public override Color SeparatorLight => Color.Transparent;
}
