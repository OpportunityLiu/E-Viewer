using ExViewer.Views;
using System;
using System.Globalization;
using System.Reflection;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Foundation;
using System.Collections.Generic;
using Windows.UI.Core;
using System.Threading.Tasks;

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
                FindName(nameof(this.tb_VersionInfoTag));
                this.tb_VersionInfoTag.Visibility = Visibility.Visible;
                this.tb_VersionInfoTag.Text = config.Configuration;
            }
            this.tb_AppAuthor.Text = Package.Current.PublisherDisplayName;
            this.tb_AppDescription.Text = Package.Current.Description;
            this.refreshTimer.Tick += RefreshTimer_Tick;
            this.hlbHV.NavigateUri = ExClient.HentaiVerse.HentaiVerseInfo.LogOnUri;
        }

        private async void init()
        {
            var source = await BannerProvider.Provider.GetBannersAsync();
            if (source.Count == 0)
                source = new[] { await StorageFile.GetFileFromApplicationUriAsync(BannerProvider.Provider.DefaultBanner) };
            source.Add(source[0]);
            this.fv_Banners.ItemsSource = source;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var margin = this.fv_Banners.Margin;
            this.fv_Banners.Height = Math.Min((availableSize.Width - margin.Left - margin.Right) / 770 * 136, 136);
            return base.MeasureOverride(availableSize);
        }

        private void RefreshTimer_Tick(object sender, object e)
        {
            this.Bindings.Update();
            this.fv_Banners_Counter++;
            if (this.fv_Banners_Counter > 7)
            {
                var c = this.fv_Banners.SelectedIndex;
                c++;
                if (((IList<StorageFile>)this.fv_Banners.ItemsSource).Count <= c)
                    c = 0;
                this.fv_Banners.SelectedIndex = c;
            }
        }

        private DispatcherTimer refreshTimer = new DispatcherTimer { Interval = new TimeSpan(10_000_000) };

        private int fv_Banners_Counter = 0;

        private async void fv_Banners_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.fv_Banners_Counter = 0;
            if (((IList<StorageFile>)this.fv_Banners.ItemsSource).Count == this.fv_Banners.SelectedIndex + 1)
            {
                await Task.Delay(500);
                this.fv_Banners.SelectedIndex = 0;
            }
        }

        private void UserControl_Loading(FrameworkElement sender, object args)
        {
            init();
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
