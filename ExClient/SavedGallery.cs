using ExClient.Models;
using GalaSoft.MvvmLight.Threading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Xaml.Data;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;

namespace ExClient
{
    public sealed class SavedGallery : CachedGallery
    {
        private sealed class SavedGalleryList : GalleryList<SavedGallery>
        {
            private static int getRecordCount()
            {
                using(var db = new GalleryDb())
                {
                    return db.SavedSet.Count();
                }
            }

            public SavedGalleryList()
                : base(getRecordCount())
            {
            }

            protected override IList<SavedGallery> LoadRange(ItemIndexRange visibleRange, GalleryDb db)
            {
                var query = db.SavedSet
                    .OrderByDescending(c => c.Saved)
                    .Skip(visibleRange.FirstIndex)
                    .Take((int)visibleRange.Length)
                    .Select(savedModel => new
                    {
                        savedModel,
                        savedModel.Gallery
                    }).ToList();
                var list = new SavedGallery[query.Count];
                for(var i = 0; i < visibleRange.Length; i++)
                {
                    var index = i + visibleRange.FirstIndex;
                    if(this[index] != DefaultGallery)
                        continue;
                    var model = query[i];
                    var sg = new SavedGallery(model.Gallery, model.savedModel);
                    var ignore = sg.InitAsync();
                    list[i] = sg;
                }
                return list;
            }
        }

        public static IAsyncOperation<GalleryList<SavedGallery>> LoadSavedGalleriesAsync()
        {
            return Task.Run<GalleryList<SavedGallery>>(() => new SavedGalleryList()).AsAsyncOperation();
        }

        public static IAsyncActionWithProgress<double> ClearAllGalleriesAsync()
        {
            return Run<double>(async (token, progress) =>
            {
                progress.Report(double.NaN);
                var items = await StorageHelper.LocalCache.GetItemsAsync();
                double c = items.Count;
                for(var i = 0; i < items.Count; i++)
                {
                    progress.Report(i / c);
                    await items[i].DeleteAsync(StorageDeleteOption.PermanentDelete);
                }
                progress.Report(1);
                using(var db = new GalleryDb())
                {
                    db.SavedSet.RemoveRange(db.SavedSet);
                    db.ImageSet.RemoveRange(db.ImageSet);
                    await db.SaveChangesAsync();
                }
            });
        }

        internal SavedGallery(GalleryModel model, SavedGalleryModel savedModel)
                : base(model)
        {
            this.thumbFile = savedModel.ThumbData;
            this.PageCount = MathHelper.GetPageCount(this.RecordCount, PageSize);
        }

        protected override IAsyncAction InitOverrideAsync()
        {
            var temp = this.thumbFile;
            this.thumbFile = null;
            if(temp == null)
                return base.InitOverrideAsync();
            return DispatcherHelper.RunAsync(async () =>
            {
                using(var stream = temp.AsRandomAccessStream())
                {
                    var decoder = await Windows.Graphics.Imaging.BitmapDecoder.CreateAsync(stream);
                    this.Thumb = await decoder.GetSoftwareBitmapAsync(Windows.Graphics.Imaging.BitmapPixelFormat.Bgra8, Windows.Graphics.Imaging.BitmapAlphaMode.Premultiplied);
                }
            });
        }

        private byte[] thumbFile;

        public override IAsyncAction DeleteAsync()
        {
            return Task.Run(async () =>
            {
                using(var db = new GalleryDb())
                {
                    var gid = this.Id;
                    db.SavedSet.Remove(db.SavedSet.Single(c => c.GalleryId == gid));
                    await db.SaveChangesAsync();
                }
                await base.DeleteAsync();
            }).AsAsyncAction();
        }
    }
}
