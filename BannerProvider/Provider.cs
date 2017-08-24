using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Web.Http;
using Windows.Web.Http.Filters;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;

namespace BannerProvider
{
    public static class Provider
    {
        private static StorageFolder bannerFolder;
        private const string LAST_UPDATE = "BannerProvider.LastUpdate";

        public static Uri DefaultBanner { get; } = new Uri($"ms-appx:///BannerProvider/Images/Default.png");

        public static Uri BannerBackground { get; } = new Uri($"ms-appx:///BannerProvider/Images/Background.png");

        public static DateTimeOffset LastUpdate
        {
            get
            {
                if (ApplicationData.Current.LocalSettings.Values.TryGetValue(LAST_UPDATE, out var r))
                    return (DateTimeOffset)r;
                return DateTimeOffset.MinValue;
            }
            private set => ApplicationData.Current.LocalSettings.Values[LAST_UPDATE] = value;
        }

        private static async Task init()
        {
            if (bannerFolder != null)
                return;
            bannerFolder = await ApplicationData.Current.LocalCacheFolder.CreateFolderAsync("Banners", CreationCollisionOption.OpenIfExists);
        }

        public static IAsyncOperation<StorageFile> GetBannerAsync()
        {
            return Run(async token =>
            {
                await init();
                var files = await bannerFolder.GetItemsAsync();
                if (files.Count == 0)
                {
                    LastUpdate = DateTimeOffset.MinValue;
                    return null;
                }
                return (StorageFile)files[new Random().Next(0, files.Count)];
            });
        }

        public static IAsyncOperation<IList<StorageFile>> GetBannersAsync()
        {
            return Run<IList<StorageFile>>(async token =>
            {
                await init();
                var files = await bannerFolder.GetItemsAsync();
                if (files.Count == 0)
                {
                    LastUpdate = DateTimeOffset.MinValue;
                    return null;
                }
                return files.Cast<StorageFile>().ToList();
            });
        }

        public static IAsyncAction FetchBanners()
        {
            return Run(async token =>
            {
                await init();
                using (var f = new HttpBaseProtocolFilter())
                {
                    f.CacheControl.ReadBehavior = HttpCacheReadBehavior.NoCache;
                    f.CacheControl.WriteBehavior = HttpCacheWriteBehavior.NoCache;
                    using (var client = new HttpClient(f))
                    {
                        for (var i = 1; i < 8; i++)
                        {
                            var buf = await client.GetBufferAsync(new Uri($"https://ehgt.org/c/botm{i}.jpg"));
                            var file = await bannerFolder.CreateFileAsync($"{i}.jpg", CreationCollisionOption.ReplaceExisting);
                            await FileIO.WriteBufferAsync(file, buf);
                        }
                    }
                }
                LastUpdate = DateTimeOffset.Now;
            });
        }
    }
}
