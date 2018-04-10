using ExClient;
using ExViewer.Controls;
using ExViewer.Settings;
using ExViewer.ViewModels;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Security.Credentials.UI;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;
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
            if (DeviceTrigger.IsMobile)
            {
                this.gd_Foreground.VerticalAlignment = VerticalAlignment.Stretch;
            }
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
            {
                var s = e.NewSize;
                if (s.Height >= s.Width)
                {
                    this.gd_Foreground.Height = double.NaN;
                    this.img_pic.Height = s.Width / 620 * 136;
                }
                else
                {
                    this.gd_Foreground.Height = s.Height / 2.5;
                    this.img_pic.Height = s.Height / 2.5 / 300 * 136;
                }
            }
            else
            {
                var l = this.splashScreen.ImageLocation;
                this.gd_Foreground.Margin = new Thickness(0, l.Top, 0, 0);
                this.gd_Foreground.Height = l.Height;
                this.img_pic.Height = l.Height / 300 * 136;
            }

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
                        CoreApplication.MainView.Dispatcher.Begin(async () =>
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
                var ver = await VersionChecker.CheckAsync();
                if (ver is VersionChecker.VersionInfo v)
                {
                    var dialog = new UpdateDialog { Version = v };
                    await dialog.ShowAsync();
                }
            }
            catch (Exception)
            {
                //Ignore exceptions here.
            }
            try
            {
                await Client.Current.RefreshHathPerks();
                await ExClient.HentaiVerse.HentaiVerseInfo.FetchAsync();
            }
            catch (Exception)
            {
                //Ignore exceptions here.
            }
            try
            {
                if (await EhTagTranslatorClient.Client.NeedUpdateAsync())
                {
                    AboutControl.UpdateETT.Execute();
                }
            }
            catch (Exception)
            {
                RootControl.RootController.SendToast(Strings.Resources.Database.EhTagTranslatorClient.Update.Failed, null);
            }
            if (DateTimeOffset.Now - EhTagClient.Client.LastUpdate > new TimeSpan(7, 0, 0, 0))
            {
                AboutControl.UpdateEhWiki.Execute();
            }
            if (DateTimeOffset.Now - BannerProvider.Provider.LastUpdate > new TimeSpan(7, 0, 0, 0))
            {
                try
                {
                    await BannerProvider.Provider.FetchBanners();
                }
                catch (Exception)
                {
                    //Ignore exceptions here.
                }
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
                        CloseButtonText = Strings.Resources.General.Exit,
                    };
                    await dialog.ShowAsync();
                }
                Application.Current.Exit();
            }
        }
    }
}
