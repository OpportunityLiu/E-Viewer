using ExClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            var imgN = new Random().Next(8);
            this.img_pic.Source = new BitmapImage(new Uri($"ms-appx:///Assets/Splashes/botm{imgN}.png"));
        }

        public SplashControl(SplashScreen splashScreen) : this()
        {
            this.splashScreen = splashScreen;
        }

        private bool loaded, goToContent;

        public void GoToContent()
        {
            if(loaded)
                Window.Current.Content = new MainPage();
            else
                goToContent = true;
        }

        private async void splash_Loaded(object sender, RoutedEventArgs e)
        {
            ((Storyboard)Resources["ShowPic"]).Begin();
            if(Client.Current == null)
            {
                var pv = new PasswordVault();
                try
                {
                    var pass = pv.FindAllByResource("ex").First();
                    pass.RetrievePassword();
                    await Client.CreateClient(pass.UserName, pass.Password);
                }
                catch(Exception ex) when(ex.HResult == -2147023728)
                {
                }
            }
            loaded = true;
            if(goToContent)
                GoToContent();
        }

        private void splash_SizeChanged(object sender, SizeChangedEventArgs e)
        {
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
