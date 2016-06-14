using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace ExViewer.Themes
{
    static class ThemeExtention
    {
        public static Color SplashColor = Color.FromArgb(0xff, 0xe7, 0xdf, 0xca);

        public static void SetTitleBar()
        {
            var resources = Application.Current.Resources;
            var tb = ApplicationView.GetForCurrentView().TitleBar;
            if(tb != null)
            {
                tb.BackgroundColor = (Color)resources["SystemChromeMediumColor"];
                tb.InactiveBackgroundColor = (Color)resources["SystemChromeMediumColor"];
                tb.ButtonBackgroundColor = (Color)resources["SystemChromeMediumColor"];
                tb.ButtonHoverBackgroundColor = (Color)resources["SystemChromeMediumLowColor"];
                tb.ButtonInactiveBackgroundColor = (Color)resources["SystemChromeMediumColor"];
                tb.ButtonPressedBackgroundColor = (Color)resources["SystemChromeHighColor"];

                tb.ForegroundColor = (Color)resources["SystemBaseMediumHighColor"];
                tb.InactiveForegroundColor = (Color)resources["SystemChromeDisabledLowColor"];
                tb.ButtonForegroundColor = (Color)resources["SystemBaseMediumHighColor"];
                tb.ButtonHoverForegroundColor = (Color)resources["SystemBaseMediumHighColor"];
                tb.ButtonInactiveForegroundColor = (Color)resources["SystemChromeDisabledLowColor"];
                tb.ButtonPressedForegroundColor = (Color)resources["SystemBaseMediumHighColor"];
            }
            if(Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                var sb = StatusBar.GetForCurrentView();
                sb.BackgroundColor = (Color)resources["SystemChromeMediumColor"];
                sb.BackgroundOpacity = 1;
                sb.ForegroundColor = (Color)resources["SystemBaseMediumHighColor"];
            }
        }

        public static void SetDefaultTitleBar()
        {
            var view = ApplicationView.GetForCurrentView();
            var tb = view.TitleBar;
            if(tb != null)
            {
                tb.BackgroundColor = SplashColor;
                tb.InactiveBackgroundColor = SplashColor;
                tb.ButtonBackgroundColor = SplashColor;
                tb.ButtonInactiveBackgroundColor = SplashColor;
            }
        }
    }
}
