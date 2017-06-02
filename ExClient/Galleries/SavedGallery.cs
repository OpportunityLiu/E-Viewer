using ExClient.Models;
using Microsoft.EntityFrameworkCore;
using Opportunity.MvvmUniverse;
using Opportunity.MvvmUniverse.AsyncHelpers;
using Opportunity.MvvmUniverse.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;
using Windows.Graphics.Imaging;

namespace ExClient.Galleries
{
    public sealed class SavedGallery : CachedGallery
    {
        private sealed class SavedGalleryList : GalleryList<SavedGallery, GalleryModel>
        {
            public static IAsyncOperation<ObservableCollection<Gallery>> LoadList()
            {
                return Task.Run<ObservableCollection<Gallery>>(() =>
                {
                    using (var db = new GalleryDb())
                    {
                        db.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
                        var query = db.SavedSet
                            .OrderByDescending(s => s.Saved)
                            .Select(s => s.Gallery);
                        return new SavedGalleryList(query.ToList());
                    }
                }).AsAsyncOperation();
            }

            private SavedGalleryList(List<GalleryModel> galleries)
                : base(galleries)
            {
            }

            protected override SavedGallery Load(GalleryModel model)
            {
                var sg = new SavedGallery(model);
                var ignore = sg.InitAsync();
                return sg;
            }
        }

        public static IAsyncOperation<ObservableCollection<Gallery>> LoadSavedGalleriesAsync()
        {
            return SavedGalleryList.LoadList();
        }

        public static IAsyncActionWithProgress<double> ClearAllGalleriesAsync()
        {
            return Run<double>(async (token, progress) =>
            {
                progress.Report(double.NaN);
                var items = await ApplicationData.Current.LocalCacheFolder.GetItemsAsync();
                double c = items.Count;
                for (var i = 0; i < items.Count; i++)
                {
                    progress.Report(i / c);
                    if (long.TryParse(items[i].Name, out var r))
                        await items[i].DeleteAsync(StorageDeleteOption.PermanentDelete);
                }
                progress.Report(1);
                using (var db = new GalleryDb())
                {
                    db.SavedSet.RemoveRange(db.SavedSet);
                    db.ImageSet.RemoveRange(db.ImageSet);
                    await db.SaveChangesAsync();
                }
            });
        }

        internal SavedGallery(GalleryModel model)
                : base(model)
        {
            this.PageCount = MathHelper.GetPageCount(this.RecordCount, PageSize);
        }

        protected override IAsyncAction InitOverrideAsync()
        {
            return AsyncWrapper.CreateCompleted();
        }

        protected override IAsyncOperation<SoftwareBitmap> GetThumbAsync()
        {
            return Run(async token =>
            {
                byte[] thumbData;
                using (var db = new GalleryDb())
                {
                    db.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
                    var model = db.SavedSet.SingleOrDefault(s => s.GalleryId == this.Id);
                    thumbData = model?.ThumbData;
                }
                if (thumbData == null)
                    return null;
                try
                {
                    using (var stream = thumbData.AsRandomAccessStream())
                    {
                        var decoder = await BitmapDecoder.CreateAsync(stream);
                        return await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
                    }
                }
                catch (Exception)
                {
                    return null;
                }
            });
        }

        public override IAsyncAction DeleteAsync()
        {
            return Task.Run(async () =>
            {
                using (var db = new GalleryDb())
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
