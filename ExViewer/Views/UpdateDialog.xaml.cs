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
            //            release.body = @"* Show user rating  
            //  ![image](https://user-images.githubusercontent.com/13471233/36630230-f487930e-199d-11e8-8336-5ab6515c419b.png)  
            //* Scan QR code in gallery images  
            //  ![image](https://user-images.githubusercontent.com/13471233/36630227-eceeb6f4-199d-11e8-8398-f34f4d36132f.png)
            //* Bug fix & other improvements";
            this.release = release;
            InitializeComponent();
            if (!(ApiInfo.IsDesktop || ApiInfo.IsMobile))
            {
                PrimaryButtonText = "";
            }
        }

        private long currentDownloaded = 0;
        private long totalDownloaded = 0;

        private static string[] downloadExt = new[] { ".cer", ".msix", ".appx", ".appxbundle" };

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            gdRoot.Width = gdRoot.ActualWidth;
            var def = args.GetDeferral();
            PrimaryButtonText = "";
            SecondaryButtonText = "";
            CloseButtonText = "";
            FindName(nameof(rpDownload));

            var folder = await DownloadsFolder.CreateFolderAsync(StorageHelper.ToValidFileName(release.tag_name), CreationCollisionOption.GenerateUniqueName);
            try
            {
                var arch = Package.Current.Id.Architecture.ToString();
                if (release.assets.IsNullOrEmpty())
                    throw new InvalidOperationException("Find appx file failed.");
                var assets = (from asset in release.assets
                              where asset.name.IndexOf(arch, StringComparison.OrdinalIgnoreCase) > 0 && downloadExt.Any(e => asset.name.EndsWith(e))
                              select asset).ToList();

                if (assets.IsEmpty())
                    throw new InvalidOperationException("Find appx file failed.");
                var files = new List<StorageFile>();
                var fileLaunched = false;
                using (var client = new HttpClient())
                {
                    try
                    {
                        totalDownloaded = assets.Sum(a => (long)a.size);
                        foreach (var item in assets)
                        {
                            var op = client.GetBufferAsync(new Uri(item.browser_download_url));
                            ReportProgress(op, default);
                            op.Progress = ReportProgress;
                            var buf = await op;
                            var file = await folder.CreateFileAsync(item.name, CreationCollisionOption.ReplaceExisting);
                            await FileIO.WriteBufferAsync(file, buf);
                            currentDownloaded += item.size;
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
                await Launcher.LaunchUriAsync(new Uri(release.html_url));
            }
            finally
            {
                def.Complete();
                Application.Current.Exit();
            }
        }

        private async void ReportProgress(IAsyncOperationWithProgress<IBuffer, HttpProgress> asyncInfo, HttpProgress p)
        {
            await Dispatcher.Yield();

            var c = currentDownloaded + (long)p.BytesReceived;
            pb.Value = 100d * c / totalDownloaded;
            pb.IsIndeterminate = false;
            tbCurrent.Text = Opportunity.UWP.Converters.XBind.ByteSize.ToBinaryString(c);
            tbTotal.Text = Opportunity.UWP.Converters.XBind.ByteSize.ToBinaryString(totalDownloaded);
        }

        private async void MyContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var d = args.GetDeferral();
            try
            {
                args.Cancel = true;
                await Launcher.LaunchUriAsync(new Uri(release.html_url));
            }
            finally
            {
                d.Complete();
            }
        }

        private readonly VersionChecker.GitHubRelease release;

        private void MyContentDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            if (rpDownload != null)
            {
                args.Cancel = true;
            }
        }
    }
}
