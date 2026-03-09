using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Rectangle.Windows.Views;

/// <summary>
/// 带亚克力/毛玻璃效果的上下文菜单
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

    public AcrylicContextMenu()
    {
        Renderer = new AcrylicMenuRenderer();
        ShowImageMargin = true;
        ShowCheckMargin = false;
        BackColor = Color.FromArgb(240, 32, 32, 32);
        ForeColor = Color.White;
        Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular);
        Padding = new Padding(4, 8, 4, 8);
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
            // 使用 Mica 效果
            int value = DWMSBT_TRANSIENTWINDOW;
            DwmSetWindowAttribute(Handle, DWMWA_SYSTEMBACKDROP_TYPE, ref value, sizeof(int));

            // 启用暗色模式
            int darkMode = 1;
            DwmSetWindowAttribute(Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref darkMode, sizeof(int));
        }
        else if (Environment.OSVersion.Version.Build >= 17763) // Windows 10 1809+
        {
            EnableAcrylicBlur();
        }
    }

    private void EnableAcrylicBlur()
    {
        var accent = new AccentPolicy
        {
            AccentState = 3, // ACCENT_ENABLE_BLURBEHIND
            AccentFlags = 2,
            GradientColor = unchecked((int)0x99000000) // Semi-transparent black
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
}

/// <summary>
/// 亚克力风格菜单渲染器
/// </summary>
public class AcrylicMenuRenderer : ToolStripProfessionalRenderer
{
    private static readonly Color BackgroundColor = Color.FromArgb(220, 32, 32, 32);
    private static readonly Color HoverColor = Color.FromArgb(255, 55, 55, 55);
    private static readonly Color BorderColor = Color.FromArgb(100, 255, 255, 255);
    private static readonly Color TextColor = Color.FromArgb(255, 255, 255, 255);
    private static readonly Color ShortcutColor = Color.FromArgb(180, 200, 200, 200);
    private static readonly Color SeparatorColor = Color.FromArgb(60, 255, 255, 255);
    private static readonly Color DisabledColor = Color.FromArgb(100, 150, 150, 150);

    public AcrylicMenuRenderer() : base(new AcrylicColorTable())
    {
        RoundedEdges = true;
    }

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
                e.Graphics.DrawImage(e.Image, rect);
            }
        }
    }

    protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
    {
        e.ArrowColor = e.Item.Enabled ? TextColor : DisabledColor;
        base.OnRenderArrow(e);
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
/// 亚克力风格颜色表
/// </summary>
public class AcrylicColorTable : ProfessionalColorTable
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
