using ExClient.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;
using Windows.Storage.Streams;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using GalaSoft.MvvmLight.Threading;

namespace ExClient
{
    public class CachedGallery : Gallery
    {
        public static IAsyncOperation<IReadOnlyList<Gallery>> LoadCachedGalleriesAsync()
        {
            return Task.Run(async () =>
            {
                using(var db = CachedGalleryDb.Create())
                {
                    var query = (from c in db.CacheSet
                                 select new
                                 {
                                     c,
                                     c.Gallery
                                 }).ToList();
                    IReadOnlyList<Gallery> ret = null;
                    await DispatcherHelper.RunAsync(() =>
                    {
                        ret = query.Select(a => new CachedGallery(a.Gallery, a.c)).ToList();
                        foreach(var item in ret)
                        {
                            var ignore = item.InitAsync();
                        }
                    });
                    return ret;
                }
            }).AsAsyncOperation();
        }

        public static IAsyncActionWithProgress<double> ClearCachedGalleriesAsync()
        {
            return Run<double>(async (token, progress) =>
            {
                progress.Report(double.NaN);
                var items = await StorageHelper.LocalCache.GetItemsAsync();
                double c = items.Count;
                for(int i = 0; i < items.Count; i++)
                {
                    progress.Report(i / c);
                    await items[i].DeleteAsync(Windows.Storage.StorageDeleteOption.PermanentDelete);
                }
                progress.Report(1);
                using(var db = CachedGalleryDb.Create())
                {
                    db.ImageSet.RemoveRange(db.ImageSet);
                    db.CacheSet.RemoveRange(db.CacheSet);
                    await db.SaveChangesAsync();
                }
            });
        }

        internal CachedGallery(GalleryModel model, CachedGalleryModel cacheModel)
            : base(model.Id, model.Token, 0)
        {
            this.Id = model.Id;
            this.Available = model.Available;
            this.ArchiverKey = model.ArchiverKey;
            this.Token = model.Token;
            this.Title = model.Title;
            this.TitleJpn = model.TitleJpn;
            this.Category = model.Category;
            this.Uploader = model.Uploader;
            this.Posted = model.Posted;
            this.FileSize = model.FileSize;
            this.Expunged = model.Expunged;
            this.Rating = model.Rating;
            this.ThumbFile = cacheModel.ThumbData;
            this.Tags = JsonConvert.DeserializeObject<IEnumerable<string>>(model.Tags).Select(t => new Tag(this, t)).ToList();
            this.RecordCount = model.RecordCount;
            this.ThumbUri = new Uri(model.ThumbUri);
            this.PageCount = (int)Math.Ceiling(RecordCount / 10d);
            this.Owner = Client.Current;
        }

        public override IAsyncAction InitAsync()
        {
            return DispatcherHelper.RunAsync(async () =>
            {
                using(var stream = ThumbFile.AsBuffer().AsRandomAccessStream())
                {
                    await Thumb.SetSourceAsync(stream);
                }
            });
        }

        private List<ImageModel> imageModels;

        protected byte[] ThumbFile
        {
            get;
            private set;
        }

        private void loadImageModel()
        {
            if(imageModels != null)
                return;
            using(var db = CachedGalleryDb.Create())
            {
                imageModels = (from g in db.GallerySet
                               where g.Id == Id
                               select g.Images).Single();
            }
        }

        protected override IAsyncOperation<uint> LoadPageAsync(int pageIndex)
        {
            return Task.Run(async () =>
            {
                if(GalleryFolder == null)
                    await CreateFolderAsync();
                loadImageModel();
                var count = 0u;
                for(; count < 10 && Count < imageModels.Count; count++)
                {
                    // Load cache
                    var image = await GalleryImage.LoadCachedImageAsync(this, imageModels.Find(i => i.PageId == Count + 1));
                    if(image != null)
                    {
                        this.Add(image);
                        continue;
                    }
                }
                return count;
            }).AsAsyncOperation();
        }

        public override IAsyncAction DeleteAsync()
        {
            return Task.Run(async () =>
            {
                using(var db = CachedGalleryDb.Create())
                {
                    db.CacheSet.Remove(db.CacheSet.Single(c => c.GalleryId == this.Id));
                    await db.SaveChangesAsync();
                }
                await base.DeleteAsync();
            }).AsAsyncAction();
        }
    }
}
