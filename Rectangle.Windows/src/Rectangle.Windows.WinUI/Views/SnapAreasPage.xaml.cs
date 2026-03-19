using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Rectangle.Windows.WinUI.ViewModels;

namespace Rectangle.Windows.WinUI.Views
{
    public sealed partial class SnapAreasPage : Page
    {
        public SnapAreasViewModel ViewModel { get; } = new SnapAreasViewModel();

        public SnapAreasPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            this.Loaded += SnapAreasPage_Loaded;
        }

        private async void SnapAreasPage_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.LoadSettingsAsync();
        }
    }
}
