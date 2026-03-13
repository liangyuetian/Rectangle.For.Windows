using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Rectangle.Windows.WinUI.Views
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();

            // 设置窗口背景为深色
            this.Content.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                Microsoft.UI.Colors.FromArgb(255, 24, 24, 24));

            // 导航到默认页面
            ContentFrame.Navigate(typeof(SettingsPage));
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is NavigationViewItem item)
            {
                var tag = item.Tag?.ToString();
                switch (tag)
                {
                    case "Shortcuts":
                        ContentFrame.Navigate(typeof(SettingsPage));
                        break;
                    case "SnapAreas":
                        ContentFrame.Navigate(typeof(SnapAreasPage));
                        break;
                    case "Settings":
                        // 可以创建一个通用的设置页面，或者与 Shortcuts 合并
                        ContentFrame.Navigate(typeof(GeneralSettingsPage));
                        break;
                }
            }
        }
    }
}
