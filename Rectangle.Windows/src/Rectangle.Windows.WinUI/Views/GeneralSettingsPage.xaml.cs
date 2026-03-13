using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Rectangle.Windows.WinUI.ViewModels;

namespace Rectangle.Windows.WinUI.Views
{
    public sealed partial class GeneralSettingsPage : Page
    {
        public SettingsViewModel ViewModel { get; } = new SettingsViewModel();

        public GeneralSettingsPage()
        {
            this.InitializeComponent();
            this.Loaded += GeneralSettingsPage_Loaded;
        }

        private async void GeneralSettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.LoadSettingsAsync();
        }
    }
}
