using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Web.Http;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;

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

        public static IAsyncOperation<StorageFile> GetBannerAsync()
        {
            return Run(async token =>
            {
                await init();
                var files = await bannerFolder.GetItemsAsync();
                if(files.Count == 0)
                    return await GetDefaultBanner();
                return (StorageFile)files[new Random().Next(0, files.Count)];
            });
        }

        public static IAsyncOperation<StorageFile> GetDefaultBanner()
        {
            return StorageFile.GetFileFromApplicationUriAsync(new Uri($"ms-appx:///BannerProvider/Images/DefaultBanner.png"));
        }

        public static IAsyncAction FetchBanners()
        {
            return Run(async token =>
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
            });
        }
    }
}
