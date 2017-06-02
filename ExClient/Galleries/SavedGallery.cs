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

namespace ExClient.Galleries
{
    public sealed class SavedGallery : CachedGallery
    {
        private sealed class SavedGalleryList : GalleryList<SavedGallery, SavedGalleryModel>
        {
            public static IAsyncOperation<ObservableCollection<Gallery>> LoadList()
            {
                return Task.Run<ObservableCollection<Gallery>>(() =>
                {
                    using (var db = new GalleryDb())
                    {
                        db.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
                        var query = db.SavedSet
                            .Include(s => s.Gallery)
                            .OrderByDescending(s => s.Saved);
                        return new SavedGalleryList(query.ToList());
                    }
                }).AsAsyncOperation();
            }

            private SavedGalleryList(List<SavedGalleryModel> galleries)
                : base(galleries)
            {
            }

            protected override SavedGallery Load(SavedGalleryModel model)
            {
                var sg = new SavedGallery(model.Gallery, model);
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

        internal SavedGallery(GalleryModel model, SavedGalleryModel savedModel)
                : base(model)
        {
            this.thumbFile = savedModel.ThumbData;
            this.PageCount = MathHelper.GetPageCount(this.RecordCount, PageSize);
        }

        protected override IAsyncAction InitOverrideAsync()
        {
            if (this.Thumb != null)
                return AsyncWrapper.CreateCompleted();
            var temp = this.thumbFile;
            this.thumbFile = null;
            if (temp == null)
                return AsyncWrapper.CreateCompleted();
            return DispatcherHelper.RunAsyncOnUIThread(async () =>
            {
                using (var stream = temp.AsRandomAccessStream())
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
