using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

namespace Rectangle.Windows.WinUI.Views.Controls
{
    public sealed partial class SnapAreaPreview : UserControl
    {
        public SnapAreaPreview()
        {
            this.InitializeComponent();
        }

        private void Area_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Border border)
            {
                border.Background = (SolidColorBrush)Application.Current.Resources["AccentBrush"];
                border.Opacity = 0.5;
            }
        }

        private void Area_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Border border)
            {
                border.Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
                border.Opacity = 0.3;
            }
        }
    }
}
