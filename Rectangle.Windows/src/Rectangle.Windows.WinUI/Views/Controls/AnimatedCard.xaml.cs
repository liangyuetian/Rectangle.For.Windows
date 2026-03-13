using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace Rectangle.Windows.WinUI.Views.Controls
{
    public sealed partial class AnimatedCard : UserControl
    {
        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register(nameof(Content), typeof(object), typeof(AnimatedCard),
                new PropertyMetadata(null));

        public object Content
        {
            get => GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        public AnimatedCard()
        {
            this.InitializeComponent();
            CardBorder.PointerEntered += CardBorder_PointerEntered;
            CardBorder.PointerExited += CardBorder_PointerExited;
        }

        private void CardBorder_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            HoverInStoryboard.Begin();
        }

        private void CardBorder_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            HoverOutStoryboard.Begin();
        }
    }
}
