using ExClient;
using ExViewer.Settings;
using ExViewer.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.Credentials;
using Windows.Security.Credentials.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace ExViewer.Views
{
    public sealed partial class SplashControl : UserControl
    {
        private SplashScreen splashScreen;

        public SplashControl()
        {
            this.InitializeComponent();
        }

        public SplashControl(SplashScreen splashScreen) : this()
        {
            this.splashScreen = splashScreen;
        }

        private bool loaded, goToContent;

        private RootControl rc;

        public void GoToContent()
        {
            if(loaded)
            {
                Themes.ThemeExtention.SetTitleBar();
                Window.Current.Content = rc;
                rc = null;
            }
            else
                goToContent = true;
        }

        private async void splash_Loaded(object sender, RoutedEventArgs e)
        {
            var imgN = new Random().Next(8);
            this.img_pic.Source = new BitmapImage(new Uri($"ms-appx:///Assets/Splashes/botm{imgN}.png"));

            ((Storyboard)Resources["ShowPic"]).Begin();
            Themes.ThemeExtention.SetDefaultTitleBar();

            if(SettingCollection.Current.NeedVerify)
            {
                var result = await UserConsentVerifier.RequestVerificationAsync("Because of your settings, we need to request the verification.");
                string info = null;
                bool succeed = false;
                switch(result)
                {
                case UserConsentVerificationResult.Verified:
                    succeed = true;
                    break;
                case UserConsentVerificationResult.DeviceNotPresent:
                case UserConsentVerificationResult.NotConfiguredForUser:
                    info = "Please set up a PIN first. \n\n"
                        + "Go \"Settings -> Accounts - Sign-in options -> PIN -> Add\" to do this.";
                    break;
                case UserConsentVerificationResult.DisabledByPolicy:
                    info = "Verification has been disabled by group policy. Please contact your administrator.";
                    break;
                case UserConsentVerificationResult.DeviceBusy:
                    info = "Device is busy. Please try again later.";
                    break;
                case UserConsentVerificationResult.RetriesExhausted:
                case UserConsentVerificationResult.Canceled:
                default:
                    break;
                }
                if(!succeed)
                {
                    if(info != null)
                    {
                        var dialog = new ContentDialog()
                        {
                            Title = "VERIFICATION FAILED",
                            Content = info,
                            PrimaryButtonText = "Ok"
                        };
                        await dialog.ShowAsync();
                    }
                    Application.Current.Exit();
                }
            }

            string sr = Cache.GetSearchQuery(SettingCollection.Current.DefaultSearchString, SettingCollection.Current.DefaultSearchCategory);

            if(Client.Current.NeedLogOn)
            {
                var pv = new PasswordVault();
                try
                {
                    var pass = pv.FindAllByResource("ex").First();
                    pass.RetrievePassword();
                    await Client.Current.LogOnAsync(pass.UserName, pass.Password, null);
                }
                catch(Exception)
                {
                }
            }
            if(!Client.Current.NeedLogOn)
            {
                try
                {
                    SettingCollection.SetHah();
                    await Cache.GetSearchResult(sr).LoadMoreItemsAsync(40);
                }
                catch(InvalidOperationException)
                {
                    //failed to search
                    sr = null;
                }
            }
            rc = new RootControl(sr);
            loaded = true;
            if(goToContent)
                GoToContent();
        }

        private void ShowPic_Completed(object sender, object e)
        {
            pr.IsActive = true;
        }

        [System.Runtime.CompilerServices.PlatformSpecific]
        private void splash_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if(DeviceTrigger.IsMobile)
                return;
            var l = splashScreen.ImageLocation;
            this.img_splash.Margin = new Thickness(l.Left, l.Top, l.Left, l.Top);
            this.img_splash.Width = l.Width;
            this.img_splash.Height = l.Height;

            this.img_pic.Margin = new Thickness(l.Left, l.Top, l.Left, l.Top);
            this.img_pic.Width = l.Width;
            this.img_pic.Height = l.Height;
        }
    }
}
