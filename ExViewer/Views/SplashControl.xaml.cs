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
        }

        private void splash_Loaded(object sender, RoutedEventArgs e)
        {
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

        private void goToContent()
        {
            Window.Current.Content = new RootControl(homePageType, previousExecutionState);
            Themes.ThemeExtention.SetTitleBar();
        }

        private bool effectLoaded, applicationLoaded;
        private object syncRoot = new object();

        private async void loadEffect()
        {
            await Task.Delay(50);
            Window.Current.Activate();
            ShowPic.Begin();
            lock(syncRoot)
            {
                if(applicationLoaded)
                    goToContent();
                else
                    effectLoaded = true;
            }
        }

        private async void loadApplication()
        {
            await Task.Run(async () =>
            {
                var initdb = Task.Run(async () =>
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
            lock(syncRoot)
            {
                if(effectLoaded)
                    goToContent();
                else
                    applicationLoaded = true;
            }
        }

        private async Task verify()
        {
            Task<UserConsentVerificationResult> verifyTask = null;
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                verifyTask = UserConsentVerifier.RequestVerificationAsync(LocalizedStrings.Resources.VerifyDialogContent).AsTask();
            });
            string info = null;
            var succeed = false;
            var result = await verifyTask;
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
            case UserConsentVerificationResult.Canceled:
            default:
                info = LocalizedStrings.Resources.VerifyFailed;
                break;
            }
            if(!succeed)
            {
                if(info != null)
                {
                    Task showDialogTask = null;
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        var dialog = new ContentDialog
                        {
                            Title = LocalizedStrings.Resources.VerifyFailedDialogTitle,
                            Content = info,
                            PrimaryButtonText = LocalizedStrings.Resources.OK
                        };
                        showDialogTask = dialog.ShowAsync().AsTask();
                    });
                    await showDialogTask;
                }
                Application.Current.Exit();
            }
        }
    }
}
