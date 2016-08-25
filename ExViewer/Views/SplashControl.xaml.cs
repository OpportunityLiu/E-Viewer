using ExClient;
using ExViewer.Settings;
using ExViewer.ViewModels;
using GalaSoft.MvvmLight.Ioc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Security.Credentials;
using Windows.Security.Credentials.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using System.Collections.Generic;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace ExViewer.Views
{
    public sealed partial class SplashControl : UserControl
    {
        private SplashScreen splashScreen;

        public SplashControl()
        {
            this.InitializeComponent();
            var imgN = new Random().Next(1, 8);
            this.img_pic.Source = new BitmapImage(new Uri($"http://ehgt.org/c/botm{imgN}.jpg"));
        }

        private void splash_Loading(FrameworkElement sender, object args)
        {
            Themes.ThemeExtention.SetSplashTitleBar();
        }

        private void splash_Loaded(object sender, RoutedEventArgs e)
        {
        }

        public async void prepareCompleted()
        {
            var db = await EhTagTranslatorClient.EhTagDatabase.LoadDatabaseAsync();
            foreach(var item in db)
            {
                var ii = item.Introduction.Analyze().ToList();
            }
            var initdb = Task.Run(() =>
            {
                ExClient.Models.GalleryDb.Migrate();
                Database.SearchHistoryDb.Migrate();
            });
            await Task.Delay(50);
            Window.Current.Activate();
            ShowPic.Begin();

            if(Client.Current.NeedLogOn)
            {
                try
                {
                    var pass = AccountManager.CurrentCredential;
                    if(pass != null)
                    {
                        pass.RetrievePassword();
                        await Client.Current.LogOnAsync(pass.UserName, pass.Password, null);
                    }
                }
                catch(Exception)
                {
                }
            }
            await initdb;
            IAsyncAction initSearch = null;
            if(!Client.Current.NeedLogOn)
            {
                initSearch = SearchVM.InitAsync();
            }

            if(SettingCollection.Current.NeedVerify)
            {
                await verify();
            }
            if(initSearch != null)
            {
                try
                {
                    await await Task.WhenAny(initSearch.AsTask(), Task.Delay(7000));
                    rc = new RootControl(typeof(SearchPage), previousExecutionState);
                }
                catch(Exception)
                {
                    rc = new RootControl(typeof(CachePage), previousExecutionState);
                }
            }
            else
                rc = new RootControl(typeof(SearchPage), previousExecutionState);
            GoToContent();
        }

        private static async Task verify()
        {
            var result = await UserConsentVerifier.RequestVerificationAsync("Because of your settings, we need to request the verification.");
            string info = null;
            var succeed = false;
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
                    var dialog = new ContentDialog
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

        public SplashControl(SplashScreen splashScreen, ApplicationExecutionState previousExecutionState)
            : this()
        {
            this.splashScreen = splashScreen;
            this.previousExecutionState = previousExecutionState;
        }

        private RootControl rc;
        private ApplicationExecutionState previousExecutionState;

        public void GoToContent()
        {
            Window.Current.Content = rc;
            rc = null;
            Themes.ThemeExtention.SetTitleBar();
        }

        private void ShowPic_Completed(object sender, object e)
        {
            FindName(nameof(pr));
        }

        private void img_pic_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            this.img_pic.Source = new BitmapImage(new Uri($"ms-appx:///Assets/Splashes/botm.png"));
            // After the default image loaded, prepareCompleted() will be called.
        }

        private void img_pic_ImageOpened(object sender, RoutedEventArgs e)
        {
            prepareCompleted();
        }

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
