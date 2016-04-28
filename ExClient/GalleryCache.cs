using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;
using Windows.Foundation;
using Windows.UI.Xaml.Media.Imaging;

namespace ExClient
{
    [JsonObject]
    internal class GalleryCache
    {
        private const string cacheFileName = "info.json";

        public static IAsyncOperation<GalleryCache> LoadCacheAsync(long galleryId)
        {
            return Run(async token =>
            {
                var str = await CacheHelper.LoadStringAsync(galleryId.ToString(), cacheFileName);
                if(str == null)
                    return null;
                return JsonConvert.DeserializeObject<GalleryCache>(str);
            });
        }

        public IAsyncOperation<Windows.Storage.StorageFile> SaveCacheAsync()
        {
            var str = JsonConvert.SerializeObject(this);
            return CacheHelper.SaveStringAsync(Id.ToString(), cacheFileName, str);
        }

        [JsonConstructor]
        internal GalleryCache()
        {
        }

        public GalleryCache(Gallery toCache)
        {
            this.Id = toCache.Id;
            this.Available = toCache.Available;
            this.ArchiverKey = toCache.ArchiverKey;
            this.Token = toCache.Token;
            this.Title = toCache.Title;
            this.TitleJpn = toCache.TitleJpn;
            this.Category = (int)toCache.Category;
            this.Uploader = toCache.Uploader;
            this.Posted = toCache.Posted.ToUnixTimeSeconds();
            this.FileSize = toCache.FileSize;
            this.Expunged = toCache.Expunged;
            this.Rating = toCache.Rating;
            this.Tags = toCache.Tags.ToList();
            this.RecordCount = toCache.RecordCount;
            this.ImageKeys = new string[toCache.Count];
            this.Thumb = ((BitmapImage)toCache.Thumb).UriSource.ToString();
            for(int i = 0; i < toCache.Count; i++)
            {
                this.ImageKeys[i] = toCache[i].ImageKey;
            }
        }

        public long Id
        {
            get; set;
        }

        public bool Available
        {
            get; set;
        }

        public string Token
        {
            get; set;
        }

        public string ArchiverKey
        {
            get; set;
        }

        public string Title
        {
            get; set;
        }

        public string TitleJpn
        {
            get; set;
        }

        public int Category
        {
            get; set;
        }

        public string Uploader
        {
            get; set;
        }

        public long Posted
        {
            get; set;
        }

        public long FileSize
        {
            get; set;
        }

        public bool Expunged
        {
            get; set;
        }

        public double Rating
        {
            get; set;
        }

        public string Thumb
        {
            get; set;
        }

        public int RecordCount
        {
            get;set;
        }

        public IList<string> Tags
        {
            get; set;
        }

        public IList<string> ImageKeys
        {
            get; set;
        }
    }
}
