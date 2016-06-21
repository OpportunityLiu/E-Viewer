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
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage;

namespace ExClient
{
    public class CachedGallery : Gallery
    {
        private class CachedGalleryList : IncrementalLoadingCollection<Gallery>
        {
            private static int pageSize = 20;

            public CachedGalleryList() : base(0)
            {
                using(var db = CachedGalleryDb.Create())
                {
                    RecordCount = db.CacheSet.Count();
                    PageCount = (int)Math.Ceiling(RecordCount / (double)pageSize);
                }
            }

            protected override IAsyncOperation<uint> LoadPageAsync(int pageIndex)
            {
                return Task.Run(() =>
                {
                    using(var db = CachedGalleryDb.Create())
                    {
                        var query = db.CacheSet.Skip(this.Count).Take(pageSize).Select(c => new
                        {
                            c,
                            c.Gallery
                        }).ToList();
                        var toAdd = query.Select(a =>
                        {
                            var c = new CachedGallery(a.Gallery, a.c);
                            var ignore = c.InitAsync();
                            return c;
                        });
                        return (uint)this.AddRange(toAdd);
                    }
                }).AsAsyncOperation();
            }
        }

        private static int pageSize = 1;

        public static IAsyncOperation<IncrementalLoadingCollection<Gallery>> LoadCachedGalleriesAsync()
        {
            return Task.Run<IncrementalLoadingCollection<Gallery>>(async () =>
            {
                var r = new CachedGalleryList();
                if(r.HasMoreItems)
                    await r.LoadMoreItemsAsync(10);
                return r;
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
                    await items[i].DeleteAsync(StorageDeleteOption.PermanentDelete);
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
            : base(model, false)
        {
            this.ThumbFile = cacheModel.ThumbData;
            this.PageCount = (int)Math.Ceiling(RecordCount / (double)pageSize);
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
                    await GetFolderAsync();
                loadImageModel();
                var toAdd = new List<GalleryImage>(pageSize);
                while(toAdd.Count < pageSize && Count + toAdd.Count < imageModels.Count)
                {
                    // Load cache
                    var model = imageModels.Find(i => i.PageId == Count + toAdd.Count + 1);
                    var image = await GalleryImage.LoadCachedImageAsync(this, model);
                    if(image == null)
                    // when load fails
                    {
                        var ext = Path.GetExtension(model.FileName);
                        var thumb = await StorageHelper.GetIconOfExtension(ext);
                        BitmapImage tb = null;
                        await DispatcherHelper.RunAsync(async () =>
                        {
                            tb = new BitmapImage();
                            using(thumb)
                            {
                                await tb.SetSourceAsync(thumb);
                            }
                        });
                        image = new GalleryImage(this, model.PageId, model.ImageKey, tb);
                    }
                    toAdd.Add(image);
                }
                return (uint)this.AddRange(toAdd);
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
