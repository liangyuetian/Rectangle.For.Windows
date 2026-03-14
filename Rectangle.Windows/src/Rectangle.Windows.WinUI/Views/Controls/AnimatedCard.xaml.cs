using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace Rectangle.Windows.WinUI.Views.Controls
{
    public sealed partial class AnimatedCard : UserControl
    {
        public AnimatedCard()
        {
            this.InitializeComponent();
            CardBorder.PointerEntered += (s, e) => HoverInStoryboard.Begin();
            CardBorder.PointerExited += (s, e) => HoverOutStoryboard.Begin();
            this.Loaded += (s, e) => CardContent.Content = this.Content;
        }
    }
}
