using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace ExViewer.Controls
{
    internal class SplitViewButton : Button
    {
        public SplitViewButton()
        {
            DefaultStyleKey = typeof(SplitViewButton);
        }

        public IconElement Icon
        {
            get => (IconElement)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        // Using a DependencyProperty as the backing store for Icon.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(IconElement), typeof(SplitViewButton), new PropertyMetadata(null));


    }
}
