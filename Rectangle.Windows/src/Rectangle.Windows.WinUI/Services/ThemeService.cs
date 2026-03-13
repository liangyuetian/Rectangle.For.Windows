using Microsoft.UI.Xaml;
using System;

namespace Rectangle.Windows.WinUI.Services
{
    /// <summary>
    /// 主题服务 - 管理应用主题
    /// </summary>
    public class ThemeService
    {
        private static ThemeService? _instance;
        public static ThemeService Instance => _instance ??= new ThemeService();

        private ElementTheme _currentTheme = ElementTheme.Default;

        public ElementTheme CurrentTheme => _currentTheme;

        public event EventHandler<ElementTheme>? ThemeChanged;

        private ThemeService() { }

        /// <summary>
        /// 设置主题
        /// </summary>
        public void SetTheme(ElementTheme theme)
        {
            if (_currentTheme == theme)
                return;

            _currentTheme = theme;

            // 应用到所有窗口
            ApplyThemeToWindow(App.MainWindow);

            ThemeChanged?.Invoke(this, theme);
        }

        /// <summary>
        /// 切换主题（深色/浅色）
        /// </summary>
        public void ToggleTheme()
        {
            var newTheme = _currentTheme switch
            {
                ElementTheme.Light => ElementTheme.Dark,
                ElementTheme.Dark => ElementTheme.Light,
                _ => ElementTheme.Dark
            };

            SetTheme(newTheme);
        }

        /// <summary>
        /// 应用主题到指定窗口
        /// </summary>
        public void ApplyThemeToWindow(Window? window)
        {
            if (window?.Content is FrameworkElement rootElement)
            {
                rootElement.RequestedTheme = _currentTheme;
            }
        }

        /// <summary>
        /// 获取当前是否为深色主题
        /// </summary>
        public bool IsDarkTheme()
        {
            return _currentTheme == ElementTheme.Dark ||
                   (_currentTheme == ElementTheme.Default &&
                    Application.Current.RequestedTheme == ApplicationTheme.Dark);
        }

        /// <summary>
        /// 从配置加载主题
        /// </summary>
        public void LoadThemeFromConfig()
        {
            try
            {
                var configService = new ConfigService();
                var config = configService.Load();

                var theme = config.Theme switch
                {
                    "Dark" => ElementTheme.Dark,
                    "Light" => ElementTheme.Light,
                    _ => ElementTheme.Default
                };

                SetTheme(theme);
            }
            catch { }
        }

        /// <summary>
        /// 保存主题到配置
        /// </summary>
        public void SaveThemeToConfig()
        {
            try
            {
                var configService = new ConfigService();
                var config = configService.Load();

                config.Theme = _currentTheme switch
                {
                    ElementTheme.Dark => "Dark",
                    ElementTheme.Light => "Light",
                    _ => "Default"
                };

                configService.Save(config);
            }
            catch { }
        }
    }
}
