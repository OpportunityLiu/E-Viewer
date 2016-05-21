using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.UI.Xaml.Media.Imaging;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;

namespace ExClient
{
    public class CachedGallery : Gallery
    {
        /// <summary>
        /// 查询已缓存的 <see cref="CachedGallery"/> 列表
        /// </summary>
        /// <returns>包含已缓存的 <see cref="CachedGallery"/> 的文件列表</returns>
        public static StorageFileQueryResult GetCachedGalleries()
        {
            var option = new QueryOptions(CommonFileQuery.DefaultQuery, new string[] { ".json" })
            {
                FolderDepth = FolderDepth.Shallow
            };
            var query = CacheHelper.LocalCache.CreateFileQueryWithOptions(option);
            return query;
        }

        /// <summary>
        /// 清空缓存（包含自动缓存的文件）
        /// </summary>
        public static IAsyncAction ClearCachedGalleriesAsync()
        {
            return Run(async token =>
            {
                foreach(var item in await CacheHelper.LocalCache.GetItemsAsync())
                {
                    await item.DeleteAsync();
                }
            });
        }

        /// <summary>
        /// 从缓存中载入相应的 <see cref="CachedGallery"/>
        /// </summary>
        /// <param name="galleryInfo">要载入的 <see cref="CachedGallery"/> 对应的储存文件，使用<see cref="GetCachedGalleries"/> 获得</param>
        /// <returns>相应的 <see cref="CachedGallery"/></returns>
        public static IAsyncOperation<CachedGallery> LoadGalleryAsync(StorageFile galleryInfo)
        {
            return LoadGalleryAsync(galleryInfo, Client.Current);
        }

        /// <summary>
        /// 从缓存中载入相应的 <see cref="CachedGallery"/>
        /// </summary>
        /// <param name="galleryInfo">要载入的 <see cref="CachedGallery"/> 对应的储存文件，使用<see cref="GetCachedGalleries"/> 获得</param>
        /// <param name="owner">要设置的 <see cref="Owner"/></param>
        /// <returns>相应的 <see cref="CachedGallery"/></returns>
        public static IAsyncOperation<CachedGallery> LoadGalleryAsync(StorageFile galleryInfo, Client owner)
        {
            if(galleryInfo == null)
                throw new ArgumentNullException(nameof(galleryInfo));
            return Run(async token =>
            {
                var cache = await GalleryCache.LoadCacheAsync(galleryInfo);
                var galleryFolder = await CacheHelper.LocalCache.TryGetFolderAsync(cache.Id.ToString());
                if(galleryFolder == null)
                    throw new InvalidOperationException("Can't find cache folder of given gallery.");
                var gallery = new CachedGallery(cache, owner, galleryFolder);
                await gallery.InitAsync();
                return gallery;
            });
        }

        private CachedGallery(GalleryCache cache, Client owner, StorageFolder folder)
            : base(cache.Id, cache.Token, 0)
        {
            this.cache = cache;
            ArchiverKey = cache.ArchiverKey;
            Available = cache.Available;
            Category = (Category)cache.Category;
            Expunged = cache.Expunged;
            FileSize = cache.FileSize;
            Owner = owner;
            Posted = DateTimeOffset.FromUnixTimeSeconds(cache.Posted);
            Rating = cache.Rating;
            RecordCount = cache.RecordCount;
            Tags = new ReadOnlyCollection<Tag>(cache.Tags.Select(tag => new Tag(this, tag)).ToList());
            Title = cache.Title;
            TitleJpn = cache.TitleJpn;
            Uploader = cache.Uploader;
            PageCount = RecordCount / 10 + 1;
            galleryFolder = folder;
        }

        private StorageFolder galleryFolder;
        public override StorageFolder GalleryFolder => galleryFolder;

        public bool Deleted => galleryFolder == null;

        public IAsyncAction DeleteAsync()
        {
            throwIfDeleted();
            lock(galleryFolder)
            {
                throwIfDeleted();
                var temp = galleryFolder;
                galleryFolder = null;
                return Run(async token =>
                {
                    await temp.DeleteAsync();
                    await cache.DeleteCacheAsync();
                });
            }
        }

        private GalleryCache cache;

        private void throwIfDeleted()
        {
            if(Deleted)
                throw new InvalidOperationException("The gallery has been deleted.");
        }

        internal IAsyncAction InitAsync()
        {
            return Run(async token =>
            {
                throwIfDeleted();
                await LoadStorageInfoAsync();
                BitmapImage thumb;
                var thumbFile = await GalleryFolder.GetFileAsync(ThumbFileName);
                if(thumbFile == null)
                    thumb = new BitmapImage(new Uri(cache.Thumb));
                else
                    using(var source = await thumbFile.OpenReadAsync())
                    {
                        thumb = new BitmapImage();
                        await thumb.SetSourceAsync(source);
                    }
                this.Thumb = thumb;
            });
        }

        private int loadedCount = 0;

        protected override IAsyncOperation<uint> LoadPageAsync(int pageIndex)
        {
            return Run(async token =>
            {
                throwIfDeleted();
                uint i = 0;
                var max = (pageIndex + 1) * 10;
                while(this.loadedCount < this.cache.ImageKeys.Count && this.loadedCount < max)
                {
                    var image = GalleryImage.LoadCachedImage(this, loadedCount + 1, ImageFiles[loadedCount + 1], this.cache.ImageKeys[loadedCount]);
                    await Task.Delay(1);
                    this.Add(image);
                    this.loadedCount++;
                    i++;
                }
                return i;
            });
        }

        private readonly IAsyncActionWithProgress<SaveGalleryProgress> saveGalleryAction = Run<SaveGalleryProgress>(async (token, progress) =>
         {
             await Task.Yield();
             return;
         });

        public override IAsyncActionWithProgress<SaveGalleryProgress> SaveGalleryAction => saveGalleryAction;

        public override IAsyncActionWithProgress<SaveGalleryProgress> SaveGalleryAsync(ConnectionStrategy strategy)
        {
            return saveGalleryAction;
        }
    }
}
