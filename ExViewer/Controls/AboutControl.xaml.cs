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
            tb_AppVersion.Text = string.Format(CultureInfo.CurrentCulture, "{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
            tb_AppName.Text = Package.Current.DisplayName;
            var config = GetType().GetTypeInfo().Assembly.GetCustomAttribute<AssemblyConfigurationAttribute>();
            if (!"release".Equals(config.Configuration, StringComparison.OrdinalIgnoreCase))
            {
                FindName(nameof(tb_VersionInfoTag));
                tb_VersionInfoTag.Visibility = Visibility.Visible;
                tb_VersionInfoTag.Text = config.Configuration;
            }
            tb_AppAuthor.Text = Package.Current.PublisherDisplayName;
            tb_AppDescription.Text = Package.Current.Description;
            refreshTimer.Tick += RefreshTimer_Tick;
            hlbHV.NavigateUri = ExClient.HentaiVerseInfo.LogOnUri;
        }

        private void RefreshTimer_Tick(object sender, object e)
        {
            Bindings.Update();
        }

        private DispatcherTimer refreshTimer = new DispatcherTimer { Interval = new TimeSpan(10_000_000) };

        private void UserControl_Loading(FrameworkElement sender, object args)
        {
            refreshTimer.Start();
            Bindings.Update();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            refreshTimer.Stop();
        }

        private async void btnUpdateEt_Click(object sender, RoutedEventArgs e)
        {
            btnUpdateEt.IsEnabled = false;
            try
            {
                await EhTagClient.Client.UpdateAsync();
                RootControl.RootController.SendToast(Strings.Resources.Database.EhTagClient.Update.Succeeded, null);
            }
            catch (Exception ex)
            {
                RootControl.RootController.SendToast(ex, null);
            }
            finally
            {
                btnUpdateEt.IsEnabled = true;
            }
        }

        private async void btnUpdateEht_Click(object sender, RoutedEventArgs e)
        {
            btnUpdateEht.IsEnabled = false;
            try
            {
                await EhTagTranslatorClient.Client.UpdateAsync();
                RootControl.RootController.SendToast(Strings.Resources.Database.EhTagTranslatorClient.Update.Succeeded, null);
            }
            catch (Exception ex)
            {
                RootControl.RootController.SendToast(ex, null);
            }
            finally
            {
                btnUpdateEht.IsEnabled = true;
            }
        }
    }
}
