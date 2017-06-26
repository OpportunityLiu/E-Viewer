using ExViewer.Views;
using System;
using System.Globalization;
using System.Reflection;
using Windows.ApplicationModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ExViewer.Controls
{
    public sealed partial class AboutControl : UserControl
    {
        public AboutControl()
        {
            InitializeComponent();
            var version = Package.Current.Id.Version;
            this.tb_AppVersion.Text = string.Format(CultureInfo.CurrentCulture, "{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
            this.tb_AppName.Text = Package.Current.DisplayName;
            var config = GetType().GetTypeInfo().Assembly.GetCustomAttribute<AssemblyConfigurationAttribute>();
            if (!"release".Equals(config.Configuration, StringComparison.OrdinalIgnoreCase))
            {
                FindName(nameof(tb_VersionInfoTag));
                this.tb_VersionInfoTag.Visibility = Visibility.Visible;
                this.tb_VersionInfoTag.Text = config.Configuration;
            }
            this.tb_AppAuthor.Text = Package.Current.PublisherDisplayName;
            this.tb_AppDescription.Text = Package.Current.Description;
            this.refreshTimer.Tick += RefreshTimer_Tick;
            this.hlbHV.NavigateUri = ExClient.HentaiVerse.HentaiVerseInfo.LogOnUri;
        }

        private void RefreshTimer_Tick(object sender, object e)
        {
            this.Bindings.Update();
        }

        private DispatcherTimer refreshTimer = new DispatcherTimer { Interval = new TimeSpan(10_000_000) };

        private void UserControl_Loading(FrameworkElement sender, object args)
        {
            this.refreshTimer.Start();
            this.Bindings.Update();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            this.refreshTimer.Stop();
        }

        private async void btnUpdateEt_Click(object sender, RoutedEventArgs e)
        {
            this.btnUpdateEt.IsEnabled = false;
            try
            {
                await EhTagClient.Client.UpdateAsync();
                RootControl.RootController.SendToast(Strings.Resources.Database.EhTagClient.Update.Succeeded, null);
                this.Bindings.Update();
            }
            catch (Exception ex)
            {
                RootControl.RootController.SendToast(ex, null);
            }
            finally
            {
                this.btnUpdateEt.IsEnabled = true;
            }
        }

        private async void btnUpdateEht_Click(object sender, RoutedEventArgs e)
        {
            this.btnUpdateEht.IsEnabled = false;
            try
            {
                await EhTagTranslatorClient.Client.UpdateAsync();
                RootControl.RootController.SendToast(Strings.Resources.Database.EhTagTranslatorClient.Update.Succeeded, null);
                this.Bindings.Update();
            }
            catch (Exception ex)
            {
                RootControl.RootController.SendToast(ex, null);
            }
            finally
            {
                this.btnUpdateEht.IsEnabled = true;
            }
        }
    }
}
