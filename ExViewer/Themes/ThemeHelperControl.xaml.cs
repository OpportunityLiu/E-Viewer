using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

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
