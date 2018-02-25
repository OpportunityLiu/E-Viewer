using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace ExViewer.Themes
{
    internal sealed partial class ThemeHelperControl : UserControl
    {
        public ThemeHelperControl()
        {
            this.InitializeComponent();
        }

        public Color SystemChromeMediumColor => ((SolidColorBrush)this.Background).Color;
        public Color SystemChromeMediumLowColor => ((SolidColorBrush)this.BorderBrush).Color;
        public Color SystemChromeHighColor => ((SolidColorBrush)this.Foreground).Color;
        public Color SystemBaseMediumHighColor => ((SolidColorBrush)this.FocusVisualPrimaryBrush).Color;
        public Color SystemChromeDisabledLowColor => ((SolidColorBrush)this.FocusVisualSecondaryBrush).Color;
    }
}
