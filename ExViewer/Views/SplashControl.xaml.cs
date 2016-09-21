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
using JYAnalyticsUniversal;

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
            this.loadApplication();
        }

        private void splash_Loading(FrameworkElement sender, object args)
        {
            Themes.ThemeExtention.SetSplashTitleBar();
            JYAnalytics.TrackPageStart(nameof(SplashControl));
        }

        public SplashControl(SplashScreen splashScreen, ApplicationExecutionState previousExecutionState)
            : this()
        {
            this.splashScreen = splashScreen;
            this.previousExecutionState = previousExecutionState;
        }

        private Type homePageType;
        private ApplicationExecutionState previousExecutionState;

        private void ShowPic_Completed(object sender, object e)
        {
            FindName(nameof(pr));
        }

        private void img_pic_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            this.img_pic.Source = new BitmapImage(new Uri($"ms-appx:///Images/Splash.png"));
            // After the default image loaded, prepareCompleted() will be called.
        }

        private void img_pic_ImageOpened(object sender, RoutedEventArgs e)
        {
            loadEffect();
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

        private RootControl rootControl;

        private async void goToContent()
        {
            if(SettingCollection.Current.NeedVerify)
               await verify();
            rootControl.PreviousState = previousExecutionState;
            rootControl.HomePageType = homePageType;
            Themes.ThemeExtention.SetTitleBar();
            Window.Current.Content = rootControl;
            rootControl = null;
            JYAnalytics.TrackPageEnd(nameof(SplashControl));
        }

        private void setLoadingFinished()
        {
            lock(goToContentSyncRoot)
            {
                if(goToContentEnabled)
                    goToContent();
                else
                    loadingFinished = true;
            }
        }

        public void EnableGoToContent()
        {
            lock(goToContentSyncRoot)
            {
                if(loadingFinished)
                    goToContent();
                else
                    goToContentEnabled = true;
            }
        }

        private bool loadingFinished, goToContentEnabled;
        private object goToContentSyncRoot = new object();

        private bool effectLoaded, applicationLoaded;
        private object loadingSyncRoot = new object();

        private async void loadEffect()
        {
            await Task.Delay(50);
            Window.Current.Activate();
            ShowPic.Begin();
            lock(loadingSyncRoot)
            {
                if(applicationLoaded)
                    setLoadingFinished();
                else
                    effectLoaded = true;
            }
        }

        private async void loadApplication()
        {
            await Task.Run(async () =>
            {
                var initDbTask = Task.Run(async () =>
                {
                    ExClient.Models.GalleryDb.Migrate();
                    Database.SearchHistoryDb.Migrate();
                    await TagExtension.Init();
                });

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
                await initDbTask;
                var initSearchTask = (Task)null;
                if(!Client.Current.NeedLogOn)
                {
                    initSearchTask = SearchVM.InitAsync().AsTask();
                }
                if(initSearchTask != null)
                {
                    try
                    {
                        await await Task.WhenAny(initSearchTask, Task.Delay(7000));
                        homePageType = typeof(SearchPage);
                    }
                    catch(Exception)
                    {
                        homePageType = typeof(CachePage);
                    }
                }
                else
                    homePageType = typeof(SearchPage);
            });
            rootControl = new RootControl();
            lock(loadingSyncRoot)
            {
                if(effectLoaded)
                    setLoadingFinished();
                else
                    applicationLoaded = true;
            }
        }

        private async Task verify()
        {
            string info = null;
            var succeed = false;
            var result = await UserConsentVerifier.RequestVerificationAsync(LocalizedStrings.Resources.VerifyDialogContent);
            switch(result)
            {
            case UserConsentVerificationResult.Verified:
                succeed = true;
                break;
            case UserConsentVerificationResult.DeviceNotPresent:
            case UserConsentVerificationResult.NotConfiguredForUser:
                info = LocalizedStrings.Resources.VerifyNotConfigured;
                break;
            case UserConsentVerificationResult.DisabledByPolicy:
                info = LocalizedStrings.Resources.VerifyDisabled;
                break;
            case UserConsentVerificationResult.DeviceBusy:
                info = LocalizedStrings.Resources.VerifyDeviceBusy;
                break;
            case UserConsentVerificationResult.RetriesExhausted:
                info = LocalizedStrings.Resources.VerifyRetriesExhausted;
                break;
            case UserConsentVerificationResult.Canceled:
                info = LocalizedStrings.Resources.VerifyCanceled;
                break;
            default:
                info = LocalizedStrings.Resources.VerifyOtherFailure;
                break;
            }
            if(!succeed)
            {
                if(info != null)
                {
                    var dialog = new ContentDialog
                    {
                        Title = LocalizedStrings.Resources.VerifyFailedDialogTitle,
                        Content = info,
                        PrimaryButtonText = LocalizedStrings.Resources.Exit
                    };
                    await dialog.ShowAsync();
                }
                Application.Current.Exit();
            }
        }
    }
}
