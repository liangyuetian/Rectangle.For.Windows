using Microsoft.UI.Xaml;
using System;

namespace Rectangle.Windows.WinUI.Services
{
    public class ThemeService
    {
        private static ThemeService? _instance;
        public static ThemeService Instance => _instance ??= new ThemeService();

        private ElementTheme _currentTheme = ElementTheme.Default;
        public ElementTheme CurrentTheme => _currentTheme;
        public event EventHandler<ElementTheme>? ThemeChanged;

        private ThemeService() { }

        public void SetTheme(ElementTheme theme)
        {
            if (_currentTheme == theme) return;
            _currentTheme = theme;
            ApplyThemeToWindow(App.MainWindow);
            ThemeChanged?.Invoke(this, theme);
        }

        public void ToggleTheme()
        {
            SetTheme(_currentTheme switch
            {
                ElementTheme.Light => ElementTheme.Dark,
                ElementTheme.Dark => ElementTheme.Light,
                _ => ElementTheme.Dark
            });
        }

        public void ApplyThemeToWindow(Window? window)
        {
            if (window?.Content is FrameworkElement rootElement)
                rootElement.RequestedTheme = _currentTheme;
        }

        public bool IsDarkTheme() =>
            _currentTheme == ElementTheme.Dark ||
            (_currentTheme == ElementTheme.Default &&
             Application.Current.RequestedTheme == ApplicationTheme.Dark);

        public void LoadThemeFromConfig()
        {
            try
            {
                var config = new ConfigService().Load();
                SetTheme(config.Theme switch
                {
                    "Dark" => ElementTheme.Dark,
                    "Light" => ElementTheme.Light,
                    _ => ElementTheme.Default
                });
            }
            catch { }
        }

        public void SaveThemeToConfig()
        {
            try
            {
                var svc = new ConfigService();
                var config = svc.Load();
                config.Theme = _currentTheme switch
                {
                    ElementTheme.Dark => "Dark",
                    ElementTheme.Light => "Light",
                    _ => "Default"
                };
                svc.Save(config);
            }
            catch { }
        }
    }
}
