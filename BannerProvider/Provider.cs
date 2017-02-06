using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Media.Imaging;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;
using Windows.Storage;
using Windows.Web.Http;
using GalaSoft.MvvmLight.Threading;

namespace BannerProvider
{
    public static class Provider
    {
        private static StorageFolder bannerFolder;

        private static async Task init()
        {
            if(bannerFolder != null)
                return;
            bannerFolder = await ApplicationData.Current.LocalCacheFolder.CreateFolderAsync("Banners", CreationCollisionOption.OpenIfExists);
        }

        public static IAsyncOperation<BitmapImage> GetBannerAsync()
        {
            return Run(async token =>
            {
                await init();
                var files = await bannerFolder.GetItemsAsync();
                if(files.Count == 0)
                    return GetDefaultBanner();
                var file = (StorageFile)files[new Random().Next(0, files.Count)];
                BitmapImage result = null;
                await DispatcherHelper.RunAsync(async () =>
                {
                    result = new BitmapImage();
                    using(var stream = await file.OpenAsync(FileAccessMode.Read))
                    {
                        await result.SetSourceAsync(stream);
                    }
                });
                return result;
            });
        }

        public static BitmapImage GetDefaultBanner()
        {
            return new BitmapImage(new Uri($"ms-appx:///BannerProvider/Images/DefaultBanner.png"));
        }

        public static IAsyncAction FetchBanners()
        {
            return Task.Run(async () =>
            {
                await init();
                using(var client = new HttpClient())
                {
                    for(int i = 1; i < 8; i++)
                    {
                        var r = await client.GetBufferAsync(new Uri($"https://ehgt.org/c/botm{i}.jpg"));
                        var f = await bannerFolder.CreateFileAsync($"{i}.jpg", CreationCollisionOption.ReplaceExisting);
                        await FileIO.WriteBufferAsync(f, r);
                    }
                }
            }).AsAsyncAction();
        }
    }
}
