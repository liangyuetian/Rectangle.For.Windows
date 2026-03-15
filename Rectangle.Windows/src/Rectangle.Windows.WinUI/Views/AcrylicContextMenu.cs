using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using Microsoft.Win32;
using Rectangle.Windows.WinUI.Core;
using Windows.UI;

namespace Rectangle.Windows.WinUI.Views;

/// <summary>
/// 带亚克力/毛玻璃效果的上下文菜单，自动适配系统主题
/// WinUI 3 版本 - 使用 MenuFlyout 和 AcrylicBrush
/// </summary>
public class AcrylicContextMenu : MenuFlyout
{
    private static bool _isDarkTheme;
    private static bool _themeInitialized = false;

    /// <summary>
    /// 菜单背景画笔
    /// </summary>
    public Brush? AcrylicBrush { get; private set; }

    public AcrylicContextMenu()
    {
        // 检测系统主题
        UpdateTheme();

        // 应用亚克力效果
        ApplyAcrylicEffect();
    }

    /// <summary>
    /// 应用亚克力效果
    /// </summary>
    private void ApplyAcrylicEffect()
    {
        // 创建亚克力背景画笔
        AcrylicBrush = CreateAcrylicBrush();

        // 设置 MenuFlyout 的样式
        this.MenuFlyoutPresenterStyle = CreateMenuFlyoutPresenterStyle();
    }

    /// <summary>
    /// 创建亚克力画笔
    /// </summary>
    private Brush CreateAcrylicBrush()
    {
        var tintColor = _isDarkTheme
            ? ColorHelper.FromArgb(220, 32, 32, 32)
            : ColorHelper.FromArgb(230, 243, 243, 243);

        var fallbackColor = _isDarkTheme
            ? ColorHelper.FromArgb(255, 32, 32, 32)
            : ColorHelper.FromArgb(255, 243, 243, 243);

        // WinUI 3 使用 DesktopAcrylicBrush
        var acrylicBrush = new AcrylicBrush
        {
            TintColor = tintColor,
            TintOpacity = 0.6,
            FallbackColor = fallbackColor
        };

        return acrylicBrush;
    }

    /// <summary>
    /// 创建 MenuFlyoutPresenter 样式
    /// </summary>
    private Style CreateMenuFlyoutPresenterStyle()
    {
        var style = new Style(typeof(MenuFlyoutPresenter));

        // 设置背景
        style.Setters.Add(new Setter(Control.BackgroundProperty, AcrylicBrush));

        // 设置圆角
        style.Setters.Add(new Setter(Control.CornerRadiusProperty, new CornerRadius(8)));

        // 设置内边距
        style.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(4)));

        // 设置边框
        var borderColor = _isDarkTheme
            ? ColorHelper.FromArgb(60, 255, 255, 255)
            : ColorHelper.FromArgb(80, 0, 0, 0);
        style.Setters.Add(new Setter(Control.BorderBrushProperty, new SolidColorBrush(borderColor)));
        style.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(1)));

        return style;
    }

    /// <summary>
    /// 添加菜单项（带图标）
    /// </summary>
    public MenuFlyoutItem AddItem(string text, WindowAction action, Action? onClick = null)
    {
        var icon = MenuIconGenerator.GenerateIcon(action);

        var item = new MenuFlyoutItem
        {
            Text = text,
            Icon = icon != null ? new FontIcon { Glyph = GetIconGlyph(action) } : null
        };

        if (onClick != null)
        {
            item.Click += (s, e) => onClick();
        }

        // 应用样式
        ApplyItemStyle(item);

        this.Items.Add(item);
        return item;
    }

    /// <summary>
    /// 添加菜单项（带快捷键）
    /// </summary>
    public MenuFlyoutItem AddItem(string text, string shortcutText, WindowAction action, Action? onClick = null)
    {
        var icon = MenuIconGenerator.GenerateIcon(action);

        var item = new MenuFlyoutItem
        {
            Text = text,
            Icon = icon != null ? new FontIcon { Glyph = GetIconGlyph(action) } : null,
            KeyboardAcceleratorTextOverride = shortcutText
        };

        if (onClick != null)
        {
            item.Click += (s, e) => onClick();
        }

        ApplyItemStyle(item);

        this.Items.Add(item);
        return item;
    }

    /// <summary>
    /// 添加子菜单
    /// </summary>
    public MenuFlyoutSubItem AddSubMenu(string text, WindowAction action)
    {
        var subItem = new MenuFlyoutSubItem
        {
            Text = text,
            Icon = new FontIcon { Glyph = GetIconGlyph(action) }
        };

        ApplySubItemStyle(subItem);

        this.Items.Add(subItem);
        return subItem;
    }

    /// <summary>
    /// 添加分隔符
    /// </summary>
    public MenuFlyoutSeparator AddSeparator()
    {
        var separator = new MenuFlyoutSeparator();
        this.Items.Add(separator);
        return separator;
    }

    /// <summary>
    /// 获取图标字符
    /// </summary>
    private string GetIconGlyph(WindowAction action)
    {
        // 使用 Segoe Fluent Icons 字体图标
        return action switch
        {
            WindowAction.LeftHalf => "\xE7C5",   // DockLeft
            WindowAction.RightHalf => "\xE7C6",  // DockRight
            WindowAction.TopHalf => "\xE7C4",    // DockTop
            WindowAction.BottomHalf => "\xE7C3", // DockBottom
            WindowAction.Maximize => "\xE740",   // FullScreen
            WindowAction.Restore => "\xE73F",    // BackToWindow
            WindowAction.Center => "\xECE4",      // Center
            _ => "\xE73E"                         // Page
        };
    }

    /// <summary>
    /// 应用菜单项样式
    /// </summary>
    private void ApplyItemStyle(MenuFlyoutItem item)
    {
        var hoverColor = _isDarkTheme
            ? ColorHelper.FromArgb(255, 55, 55, 55)
            : ColorHelper.FromArgb(255, 230, 230, 230);

        var foregroundColor = _isDarkTheme
            ? Colors.White
            : ColorHelper.FromArgb(255, 30, 30, 30);

        item.Foreground = new SolidColorBrush(foregroundColor);

        // 设置悬停效果
        item.Resources["MenuFlyoutItemBackgroundPointerOver"] = new SolidColorBrush(hoverColor);
        item.Resources["MenuFlyoutItemBackgroundPressed"] = new SolidColorBrush(hoverColor);
    }

    /// <summary>
    /// 应用子菜单样式
    /// </summary>
    private void ApplySubItemStyle(MenuFlyoutSubItem item)
    {
        var foregroundColor = _isDarkTheme
            ? Colors.White
            : ColorHelper.FromArgb(255, 30, 30, 30);

        item.Foreground = new SolidColorBrush(foregroundColor);
    }

    /// <summary>
    /// 更新主题
    /// </summary>
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

    /// <summary>
    /// 刷新主题
    /// </summary>
    public void RefreshTheme()
    {
        UpdateTheme();
        ApplyAcrylicEffect();
    }

    /// <summary>
    /// 当前是否为深色主题
    /// </summary>
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
}
