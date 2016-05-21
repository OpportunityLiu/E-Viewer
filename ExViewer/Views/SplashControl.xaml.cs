using ExClient;
using ExViewer.Settings;
using ExViewer.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.Credentials;
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

        public IAsyncAction InitAsync()
        {
            return AsyncInfo.Run(async token =>
            {
                var imgN = new Random().Next(8);
                this.img_pic.Source = new BitmapImage(new Uri($"ms-appx:///Assets/Splashes/botm{imgN}.png"));
                var pic = new BitmapImage();
                var file = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/SplashScreen.png"));
                await pic.SetSourceAsync(await file.OpenReadAsync());
                this.img_splash.Source = pic;
            });
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
            string sr = null;
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
                    sr = Cache.GetSearchQuery(SettingCollection.Current.DefaultSearchString, SettingCollection.Current.DefaultSearchCategory);
                    await Cache.GetSearchResult(sr).LoadMoreItemsAsync(40);
                }
                catch(InvalidOperationException)
                {
                    //failed to search
                }
            }
            rc = new RootControl(sr);
            loaded = true;
            if(goToContent)
                GoToContent();
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
