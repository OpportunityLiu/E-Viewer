using ExClient;
using ExViewer.Controls;
using ExViewer.Settings;
using ExViewer.ViewModels;
using JYAnalyticsUniversal;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.Security.Credentials.UI;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace ExViewer.Views
{
    public sealed partial class SplashControl : UserControl
    {
        private SplashScreen splashScreen;

        public SplashControl(SplashScreen splashScreen)
        {
            InitializeComponent();
            BannerProvider.Provider.GetBannerAsync().Completed = (s, e)
                => Dispatcher.Begin(() => loadBanner(s.GetResults()));
            loadApplication();
            this.splashScreen = splashScreen;
        }

        private async void loadBanner(StorageFile banner)
        {
            if (banner == null)
            {
                ((BitmapImage)this.img_pic.Source).UriSource = BannerProvider.Provider.DefaultBanner;
                return;
            }
            using (var stream = await banner.OpenReadAsync())
            {
                await ((BitmapImage)this.img_pic.Source).SetSourceAsync(stream);
            }
        }

        private void splash_Loading(FrameworkElement sender, object args)
        {
            Themes.ThemeExtention.SetTitleBar();
        }

        private void ShowPic_Completed(object sender, object e)
        {
            FindName(nameof(this.pr));
        }

        private void img_pic_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            loadBanner(null);
            // After the default image loaded, img_pic_ImageOpened() will be called.
        }

        private void img_pic_ImageOpened(object sender, RoutedEventArgs e)
        {
            loadEffect();
        }

        private void splash_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (DeviceTrigger.IsMobile)
                return;
            var l = this.splashScreen.ImageLocation;
            this.img_splash.Margin = new Thickness(l.Left, l.Top, l.Left, l.Top);
            this.img_splash.Width = l.Width;
            this.img_splash.Height = l.Height;

            this.img_pic.Height = l.Height / 300 * 136;
            this.img_pic.Width = l.Height / 300 * 770;
        }

        private RootControl rootControl;

        private async void goToContent()
        {
            if (SettingCollection.Current.NeedVerify)
                await verify();
            this.ccHided.Content = null;
            Window.Current.Content = this.rootControl;
            this.rootControl = null;
            afterActions();
        }

        private void setLoadingFinished()
        {
            if (this.goToContentEnabled)
                goToContent();
            else
                this.loadingFinished = true;
        }

        public void EnableGoToContent()
        {
            if (this.loadingFinished)
                goToContent();
            else
                this.goToContentEnabled = true;
        }

        private bool loadingFinished, goToContentEnabled;

        private bool effectLoaded, applicationLoaded;

        private async void loadEffect()
        {
            await Dispatcher.YieldIdle();
            Window.Current.Activate();
            this.ShowPic.Begin();
            if (this.applicationLoaded)
                setLoadingFinished();
            else
                this.effectLoaded = true;
        }

        private async void loadApplication()
        {
            var loadingTask = Task.Run(async () =>
            {
                var client = Client.Current;
                if (client.NeedLogOn)
                {
                    try
                    {
                        var pass = AccountManager.CurrentCredential;
                        if (pass != null)
                        {
                            pass.RetrievePassword();
                            await client.LogOnAsync(pass.UserName, pass.Password, null);
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
                if (!client.NeedLogOn)
                {
                    SettingCollection.Current.Apply();
                    client.ResetExCookie();
                    var initSearchTask = SearchVM.InitAsync();
                    var waitTime = 0;
                    while (waitTime < 7000)
                    {
                        await Task.Delay(250);
                        waitTime += 250;
                        if (initSearchTask.Status != Windows.Foundation.AsyncStatus.Started)
                        {
                            initSearchTask.Close();
                            break;
                        }
                    }
                }
                ExClient.HentaiVerse.HentaiVerseInfo.MonsterEncountered += (s, e) =>
                {
                    if (SettingCollection.Current.OpenHVOnMonsterEncountered)
                        Opportunity.MvvmUniverse.DispatcherHelper.BeginInvokeOnUIThread(async () =>
                        {
                            await Windows.System.Launcher.LaunchUriAsync(e.Uri);
                        });
                };
            });
            await Dispatcher.YieldIdle();
            this.rootControl = new RootControl();
            FindName(nameof(this.ccHided));
            this.ccHided.Content = this.rootControl;
            await loadingTask;
            if (Client.Current.NeedLogOn)
            {
                await RootControl.RootController.RequestLogOn();
            }
            if (this.effectLoaded)
                setLoadingFinished();
            else
                this.applicationLoaded = true;
        }

        private async void afterActions()
        {
            try
            {
                await Client.Current.UserStatus.RefreshAsync();
            }
            catch (Exception)
            {
                //Ignore exceptions here.
            }
            try
            {
                var ver = await VersionChecker.CheckAsync();
                if (ver is Windows.ApplicationModel.PackageVersion v)
                {
                    var dialog = new UpdateDialog { Version = v };
                    await dialog.ShowAsync();
                }
            }
            catch (Exception)
            {
                //Ignore exceptions here.
            }
            if (DateTimeOffset.Now - BannerProvider.Provider.LastUpdate > new TimeSpan(7, 0, 0, 0))
                try
                {
                    await BannerProvider.Provider.FetchBanners();
                }
                catch (Exception)
                {
                    //Ignore exceptions here.
                }
            if (DateTimeOffset.Now - EhTagClient.Client.LastUpdate > new TimeSpan(7, 0, 0, 0))
                try
                {
                    await EhTagClient.Client.UpdateAsync();
                    RootControl.RootController.SendToast(Strings.Resources.Database.EhTagClient.Update.Succeeded, null);
                }
                catch (Exception)
                {
                    RootControl.RootController.SendToast(Strings.Resources.Database.EhTagClient.Update.Failed, null);
                }
            try
            {
                if (await EhTagTranslatorClient.Client.NeedUpdateAsync())
                {
                    await EhTagTranslatorClient.Client.UpdateAsync();
                    RootControl.RootController.SendToast(Strings.Resources.Database.EhTagTranslatorClient.Update.Succeeded, null);
                }
            }
            catch (Exception)
            {
                RootControl.RootController.SendToast(Strings.Resources.Database.EhTagTranslatorClient.Update.Failed, null);
            }
        }

        private async Task verify()
        {
            string info = null;
            var succeed = false;
            var result = await UserConsentVerifier.RequestVerificationAsync(Strings.Resources.Verify.Dialog.Content);
            switch (result)
            {
            case UserConsentVerificationResult.Verified:
                succeed = true;
                break;
            case UserConsentVerificationResult.DeviceNotPresent:
            case UserConsentVerificationResult.NotConfiguredForUser:
                info = Strings.Resources.Verify.NotConfigured;
                break;
            case UserConsentVerificationResult.DisabledByPolicy:
                info = Strings.Resources.Verify.Disabled;
                break;
            case UserConsentVerificationResult.DeviceBusy:
                info = Strings.Resources.Verify.DeviceBusy;
                break;
            case UserConsentVerificationResult.RetriesExhausted:
                info = Strings.Resources.Verify.RetriesExhausted;
                break;
            case UserConsentVerificationResult.Canceled:
                info = Strings.Resources.Verify.Canceled;
                break;
            default:
                info = Strings.Resources.Verify.OtherFailure;
                break;
            }
            if (!succeed)
            {
                if (info != null)
                {
                    var dialog = new MyContentDialog
                    {
                        Title = Strings.Resources.Verify.FailedDialogTitle,
                        Content = info,
                        PrimaryButtonText = Strings.Resources.General.Exit
                    };
                    await dialog.ShowAsync();
                }
                Application.Current.Exit();
            }
        }
    }
}
