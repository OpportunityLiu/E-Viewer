using Opportunity.MvvmUniverse.Commands;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Storage;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

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
                FindName(nameof(this.hlbOpenData));
                this.tb_VersionInfoTag.Text = config.Configuration;
            }
            this.tb_AppAuthor.Text = Package.Current.PublisherDisplayName;
            this.tb_AppDescription.Text = Package.Current.Description;
            this.refreshTimer.Tick += RefreshTimer_Tick;
            this.hlbHV.NavigateUri = ExClient.HentaiVerse.HentaiVerseInfo.LogOnUri;
            this.hlb_GithubVersion.NavigateUri = new Uri($"https://github.com/OpportunityLiu/ExViewer/tree/{Github.COMMIT}");
            this.tb_GithubVersion.Text = Strings.Resources.Controls.AboutControl.GithubVersionFormat(Github.BRANCH, Github.COMMIT.Substring(0, 8));
            UpdateEhWiki.Executed += (s, e) => this.Bindings.Update();
            UpdateETT.Executed += (s, e) => this.Bindings.Update();
        }

        private async void hlbOpenData_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchFolderAsync(ApplicationData.Current.LocalCacheFolder);
        }

        private async void init()
        {
            var source = await BannerProvider.Provider.GetBannersAsync()
                ?? new[] { await StorageFile.GetFileFromApplicationUriAsync(BannerProvider.Provider.DefaultBanner) };

            var data = new List<BitmapImage>();
            foreach (var item in source)
            {
                var img = new BitmapImage();
                data.Add(img);
                using (var stream = await item.OpenReadAsync())
                {
                    var ignore = img.SetSourceAsync(stream);
                }
            }
            if (data.Count >= 2)
            {  // 循环滚动
                data.Insert(0, data[data.Count - 1]);
                data.Add(data[1]);
            }
            this.fv_Banners.ItemsSource = data;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var margin = this.fv_Banners.Margin;
            this.fv_Banners.Height = Math.Min((availableSize.Width - margin.Left - margin.Right) / 770 * 136, 136);
            return base.MeasureOverride(availableSize);
        }

        private void RefreshTimer_Tick(object sender, object e)
        {
            this.fv_Banners_Counter++;
            if (this.fv_Banners_Counter <= 7)
            {
                return;
            }

            var c = this.fv_Banners.SelectedIndex;
            c++;
            if (((ICollection)this.fv_Banners.ItemsSource).Count <= c)
            {
                c = 0;
            }

            this.fv_Banners.SelectedIndex = c;
        }

        private DispatcherTimer refreshTimer = new DispatcherTimer { Interval = new TimeSpan(10_000_000) };

        private int fv_Banners_Counter = 0;

        private async void fv_Banners_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.fv_Banners_Counter = 0;
            var data = (ICollection)this.fv_Banners.ItemsSource;
            if (data.Count < 4)
            {
                return;
            }

            if (data.Count == this.fv_Banners.SelectedIndex + 1)
            {
                await Task.Delay(500);
                this.fv_Banners.SelectedIndex = 1;
            }
            else if (this.fv_Banners.SelectedIndex == 0)
            {
                await Task.Delay(500);
                this.fv_Banners.SelectedIndex = data.Count - 2;
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

        public static AsyncCommand UpdateEhWiki { get; } = AsyncCommand.Create(_ => EhTagClient.Client.UpdateAsync());
        public static AsyncCommand UpdateETT { get; } = AsyncCommand.Create(async _ => await EhTagTranslatorClient.Client.TryUpdateAsync());
    }
}
