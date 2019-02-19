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
            InitializeComponent();
        }

        public Color SystemChromeMediumColor => ((SolidColorBrush)Background).Color;
        public Color SystemChromeMediumLowColor => ((SolidColorBrush)BorderBrush).Color;
        public Color SystemChromeHighColor => ((SolidColorBrush)Foreground).Color;
        public Color SystemBaseMediumHighColor => ((SolidColorBrush)FocusVisualPrimaryBrush).Color;
        public Color SystemChromeDisabledLowColor => ((SolidColorBrush)FocusVisualSecondaryBrush).Color;
    }
}
