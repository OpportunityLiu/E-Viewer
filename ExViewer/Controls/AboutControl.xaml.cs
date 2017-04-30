using System;
using System.Globalization;
using Windows.ApplicationModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ExViewer.Controls
{
    public sealed partial class AboutControl : UserControl
    {
        public AboutControl()
        {
            this.InitializeComponent();
            var version = Package.Current.Id.Version;
            this.tb_AppVersion.Text = string.Format(CultureInfo.CurrentCulture, "{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
            this.tb_AppName.Text = Package.Current.DisplayName;
#if !RELEASE
            FindName(nameof(this.tb_DebugTag));
#endif
            this.tb_AppAuthor.Text = Package.Current.PublisherDisplayName;
            this.tb_AppDescription.Text = Package.Current.Description;
            this.refreshTimer.Tick += this.RefreshTimer_Tick;
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
    }
}
