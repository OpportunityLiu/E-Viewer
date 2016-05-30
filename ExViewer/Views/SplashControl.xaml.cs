using ExClient;
using ExViewer.Settings;
using ExViewer.ViewModels;
using GalaSoft.MvvmLight.Ioc;
using System;
using System.Linq;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Security.Credentials;
using Windows.Security.Credentials.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace ExViewer.Views
{
    public sealed partial class SplashControl : UserControl
    {
        private SplashScreen splashScreen;

        public SplashControl()
        {
            this.InitializeComponent();
            var imgN = new Random().Next(8);
            this.img_pic.Source = new BitmapImage(new Uri($"ms-appx:///Assets/Splashes/botm{imgN}.png"));
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
            ((Storyboard)Resources["ShowPic"]).Begin();
            Themes.ThemeExtention.SetDefaultTitleBar();

            SimpleIoc.Default.Register<CacheVM>();

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
            rc = new RootControl(typeof(SearchPage));
            IAsyncAction initSearch = null;
            if(!Client.Current.NeedLogOn)
            {
                initSearch = SearchVM.InitAsync();
            }

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
            if(initSearch != null)
                try
                {
                    await initSearch;
                }
                catch(Exception)
                {
                    rc = new RootControl(typeof(CachePage));
                }
            loaded = true;
            if(goToContent)
                GoToContent();
        }

        private void ShowPic_Completed(object sender, object e)
        {
            FindName(nameof(pr));
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
