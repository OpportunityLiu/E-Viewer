using ExClient.Galleries;
using System;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

namespace ExViewer.Themes
{
    static class ThemeExtention
    {
        private static ThemeHelperControl helper = new ThemeHelperControl();
        private static StatusBar sb = ApiInfo.StatusBarSupported ? StatusBar.GetForCurrentView() : null;

        public static void SetTitleBar()
        {
            helper.RequestedTheme = Settings.SettingCollection.Current.Theme.ToElementTheme();
            var SystemChromeMediumLowColor = helper.SystemChromeMediumLowColor;
            var SystemChromeHighColor = helper.SystemChromeHighColor;
            var SystemBaseMediumHighColor = helper.SystemBaseMediumHighColor;
            var SystemChromeDisabledLowColor = helper.SystemChromeDisabledLowColor;
            var view = ApplicationView.GetForCurrentView();
            var tb = view.TitleBar;
            if (tb != null)
            {
                tb.BackgroundColor = Colors.Transparent;
                tb.InactiveBackgroundColor = Colors.Transparent;
                tb.ButtonBackgroundColor = Colors.Transparent;
                tb.ButtonHoverBackgroundColor = SystemChromeMediumLowColor;
                tb.ButtonInactiveBackgroundColor = Colors.Transparent;
                tb.ButtonPressedBackgroundColor = SystemChromeHighColor;

                tb.ForegroundColor = SystemBaseMediumHighColor;
                tb.InactiveForegroundColor = SystemChromeDisabledLowColor;
                tb.ButtonForegroundColor = SystemBaseMediumHighColor;
                tb.ButtonHoverForegroundColor = SystemBaseMediumHighColor;
                tb.ButtonInactiveForegroundColor = SystemChromeDisabledLowColor;
                tb.ButtonPressedForegroundColor = SystemBaseMediumHighColor;
            }
            if (ApiInfo.StatusBarSupported)
            {
                sb.BackgroundOpacity = 0;
                sb.ProgressIndicator.ProgressValue = 0;
                sb.ProgressIndicator.Text = " ";
                sb.ForegroundColor = SystemBaseMediumHighColor;
            }
        }

        public static async void SetStatusBarInfoVisibility(Visibility visibility)
        {
            if (ApiInfo.StatusBarSupported)
            {
                switch (visibility)
                {
                case Visibility.Visible:
                    await sb.ProgressIndicator.HideAsync();
                    break;
                case Visibility.Collapsed:
                    await sb.ProgressIndicator.ShowAsync();
                    break;
                default:
                    break;
                }
            }
        }

        public static void SetDefaltImage()
        {
            switch (Settings.SettingCollection.Current.Theme)
            {
            case ApplicationTheme.Light:
                GalleryImage.DefaultThumb = new BitmapImage(new Uri("ms-appx:///Assets/Theme/Light/ImageLoading.png"));
                break;
            default:
                GalleryImage.DefaultThumb = new BitmapImage(new Uri("ms-appx:///Assets/Theme/Dark/ImageLoading.png"));
                break;
            }
        }
    }
}
