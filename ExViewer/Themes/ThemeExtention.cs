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
        private static ThemeHelperControl helper = new ThemeHelperControl();

        public static void SetTitleBar()
        {
            helper.RequestedTheme = Settings.SettingCollection.Current.Theme.ToElementTheme();
            var SystmeChromeMediumColor = helper.SystemChromeMediumColor;
            var SystemChromeMediumLowColor = helper.SystemChromeMediumLowColor;
            var SystemChromeHighColor = helper.SystemChromeHighColor;
            var SystemBaseMediumHighColor = helper.SystemBaseMediumHighColor;
            var SystemChromeDisabledLowColor = helper.SystemChromeDisabledLowColor;
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
