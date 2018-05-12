using ExViewer.Controls;
using Opportunity.Helpers.Universal;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Web.Http;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace ExViewer.Views
{
    public sealed partial class UpdateDialog : MyContentDialog
    {
        internal UpdateDialog(VersionChecker.GitHubRelease release)
        {
            this.release = release;
            this.InitializeComponent();
            if (!(ApiInfo.IsDesktop || ApiInfo.IsMobile))
            {
                this.PrimaryButtonText = "";
            }
        }

        private long currentDownloaded = 0;
        private long totalDownloaded = 0;

        private static string[] downloadExt = new[] { ".cer", ".appx", ".appxbundle" };

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            this.gdRoot.Width = this.gdRoot.ActualWidth;
            var def = args.GetDeferral();
            this.PrimaryButtonText = "";
            this.SecondaryButtonText = "";
            this.CloseButtonText = "";
            FindName(nameof(this.rpDownload));

            var folder = await DownloadsFolder.CreateFolderAsync(StorageHelper.ToValidFileName(this.release.tag_name), CreationCollisionOption.GenerateUniqueName);
            try
            {
                var arch = Package.Current.Id.Architecture.ToString();
                if (this.release.assets.IsNullOrEmpty())
                {
                    throw new InvalidOperationException("Find appx file failed.");
                }

                var assets = (from asset in this.release.assets
                              where asset.name.IndexOf(arch, StringComparison.OrdinalIgnoreCase) > 0 && downloadExt.Any(e => asset.name.EndsWith(e))
                              select asset).ToList();

                if (assets.IsEmpty())
                {
                    throw new InvalidOperationException("Find appx file failed.");
                }
                var files = new List<StorageFile>();
                var fileLaunched = false;
                using (var client = new HttpClient())
                {
                    try
                    {
                        this.totalDownloaded = assets.Sum(a => (long)a.size);
                        foreach (var item in assets)
                        {
                            var op = client.GetBufferAsync(new Uri(item.browser_download_url));
                            ReportProgress(op, default);
                            op.Progress = ReportProgress;
                            var buf = await op;
                            var file = await folder.CreateFileAsync(item.name, CreationCollisionOption.ReplaceExisting);
                            await FileIO.WriteBufferAsync(file, buf);
                            this.currentDownloaded += item.size;
                            files.Add(file);
                        }
                        foreach (var item in files)
                        {
                            fileLaunched = fileLaunched || await Launcher.LaunchFileAsync(item);
                        }
                    }
                    finally
                    {
                        if ((assets.Count > 1 || !fileLaunched) && !await Launcher.LaunchFolderAsync(folder))
                        {
                            throw new InvalidOperationException("Launch download folder failed.");
                        }
                    }
                }
            }
            catch
            {
                await Launcher.LaunchUriAsync(new Uri(this.release.html_url));
            }
            finally
            {
                def.Complete();
                Application.Current.Exit();
            }
        }

        private async void ReportProgress(IAsyncOperationWithProgress<IBuffer, HttpProgress> asyncInfo, HttpProgress p)
        {
            await this.Dispatcher.Yield();

            var c = this.currentDownloaded + (long)p.BytesReceived;
            this.pb.Value = 100d * c / this.totalDownloaded;
            this.pb.IsIndeterminate = false;
            this.tbCurrent.Text = Opportunity.Converters.XBind.ByteSize.ToBinaryString(c);
            this.tbTotal.Text = Opportunity.Converters.XBind.ByteSize.ToBinaryString(this.totalDownloaded);
        }

        private async void MyContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var d = args.GetDeferral();
            try
            {
                args.Cancel = true;
                await Launcher.LaunchUriAsync(new Uri(this.release.html_url));
            }
            finally
            {
                d.Complete();
            }
        }

        private readonly VersionChecker.GitHubRelease release;

        private void MyContentDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            if (this.rpDownload != null)
            {
                args.Cancel = true;
            }
        }
    }
}
