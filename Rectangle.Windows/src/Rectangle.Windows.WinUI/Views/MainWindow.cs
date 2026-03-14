using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Rectangle.Windows.WinUI.Views
{
    public sealed class MainWindow : Window
    {
        private readonly Frame _contentFrame;

        public MainWindow()
        {
            Title = "Rectangle 设置";

            _contentFrame = new Frame
            {
                Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent),
                Padding = new Thickness(0)
            };

            var navView = new NavigationView
            {
                PaneDisplayMode = NavigationViewPaneDisplayMode.Left,
                IsSettingsVisible = false,
                PaneTitle = "Rectangle",
                Content = _contentFrame
            };

            navView.MenuItems.Add(new NavigationViewItem
            {
                Icon = new SymbolIcon(Symbol.Keyboard),
                Content = "键盘快捷键",
                Tag = "Shortcuts"
            });
            navView.MenuItems.Add(new NavigationViewItem
            {
                Icon = new SymbolIcon(Symbol.ViewAll),
                Content = "吸附区域",
                Tag = "SnapAreas"
            });
            navView.MenuItems.Add(new NavigationViewItem
            {
                Icon = new SymbolIcon(Symbol.Setting),
                Content = "设置",
                Tag = "Settings"
            });

            var versionText = new TextBlock
            {
                Text = "v1.0.0",
                FontSize = 12,
                Foreground = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 160, 160, 160))
            };
            navView.PaneFooter = new StackPanel { Padding = new Thickness(16, 8, 16, 8), Children = { versionText } };

            navView.SelectionChanged += NavView_SelectionChanged;

            Content = new Grid { Children = { navView } };

            // 导航到默认页面
            _contentFrame.Navigate(typeof(SettingsPage));
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is NavigationViewItem item)
            {
                switch (item.Tag?.ToString())
                {
                    case "Shortcuts":
                        _contentFrame.Navigate(typeof(SettingsPage));
                        break;
                    case "SnapAreas":
                        _contentFrame.Navigate(typeof(SnapAreasPage));
                        break;
                    case "Settings":
                        _contentFrame.Navigate(typeof(GeneralSettingsPage));
                        break;
                }
            }
        }
    }
}
