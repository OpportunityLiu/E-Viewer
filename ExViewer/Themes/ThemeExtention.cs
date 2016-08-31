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
        public static void SetTitleBar()
        {
            var resources = Application.Current.Resources;
            var SystmeChromeMediumColor = (Color)resources["SystemChromeMediumColor"];
            var SystemChromeMediumLowColor = (Color)resources["SystemChromeMediumLowColor"];
            var SystemChromeHighColor = (Color)resources["SystemChromeHighColor"];
            var SystemBaseMediumHighColor = (Color)resources["SystemBaseMediumHighColor"];
            var SystemChromeDisabledLowColor = (Color)resources["SystemChromeDisabledLowColor"];
            var tb = ApplicationView.GetForCurrentView().TitleBar;
            if(tb != null)
            {
                tb.BackgroundColor = SystmeChromeMediumColor;
                tb.InactiveBackgroundColor = SystmeChromeMediumColor;
                tb.ButtonBackgroundColor = SystmeChromeMediumColor;
                tb.ButtonHoverBackgroundColor = SystemChromeMediumLowColor;
                tb.ButtonInactiveBackgroundColor = SystmeChromeMediumColor;
                tb.ButtonPressedBackgroundColor = SystemChromeHighColor;

                tb.ForegroundColor = SystemBaseMediumHighColor;
                tb.InactiveForegroundColor = SystemChromeDisabledLowColor;
                tb.ButtonForegroundColor = SystemBaseMediumHighColor;
                tb.ButtonHoverForegroundColor = SystemBaseMediumHighColor;
                tb.ButtonInactiveForegroundColor = SystemChromeDisabledLowColor;
                tb.ButtonPressedForegroundColor = SystemBaseMediumHighColor;
            }
            if(ApiInfo.StatusBarSupported)
            {
                var sb = StatusBar.GetForCurrentView();
                sb.BackgroundColor = SystmeChromeMediumColor;
                sb.BackgroundOpacity = 1;
                sb.ForegroundColor = SystemBaseMediumHighColor;
                var ignore = sb.ShowAsync();
            }
        }

        public static void SetSplashTitleBar()
        {
            var splashColor = (Color)Application.Current.Resources["SplashColor"];
            var view = ApplicationView.GetForCurrentView();
            var tb = view.TitleBar;
            if(tb != null)
            {
                tb.BackgroundColor = splashColor;
                tb.InactiveBackgroundColor = splashColor;
                tb.ButtonBackgroundColor = splashColor;
                tb.ButtonInactiveBackgroundColor = splashColor;
            }
            if(ApiInfo.StatusBarSupported)
            {
                var statusBar = StatusBar.GetForCurrentView();
                var ignore = statusBar.HideAsync();
            }
        }
    }
}
