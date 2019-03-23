using ExClient.Models;
using Microsoft.EntityFrameworkCore;
using Opportunity.Helpers.Universal.AsyncHelpers;
using Opportunity.MvvmUniverse.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;

namespace ExClient.Galleries
{
    public class CachedGallery : Gallery
    {
        private sealed class CachedGalleryList : GalleryList<CachedGallery, GalleryModel>
        {
            public static IAsyncOperation<ObservableList<Gallery>> LoadList()
            {
                return Task.Run<ObservableList<Gallery>>(() =>
                {
                    using (var db = new GalleryDb())
                    {
                        db.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
                        var query = from gm in db.GallerySet
                                    where gm.Images.Count != 0
                                    where !db.SavedSet.Any(sm => sm.GalleryId == gm.GalleryModelId)
                                    orderby gm.posted descending
                                    select gm;
                        return new CachedGalleryList(query.ToList());
                    }
                }).AsAsyncOperation();
            }

            private CachedGalleryList(List<GalleryModel> galleries)
                : base(galleries) { }

            protected override CachedGallery Load(GalleryModel model)
            {
                var c = new CachedGallery(model);
                var ignore = c.InitAsync();
                return c;
            }
        }

        public static IAsyncOperation<ObservableList<Gallery>> LoadCachedGalleriesAsync()
        {
            return CachedGalleryList.LoadList();
        }

        public static IAsyncActionWithProgress<double> ClearCachedGalleriesAsync()
        {
            return Run<double>(async (token, progress) =>
            {
                using (var db = new GalleryDb())
                {
                    var todelete = await db.ImageSet
                        .Where(im => !db.SavedSet.Any(sm => im.UsingBy.Any(gi => gi.GalleryId == sm.GalleryId)))
                        .ToListAsync(token);
                    double count = todelete.Count;
                    var i = 0;
                    foreach (var item in todelete)
                    {
                        var file = await StorageHelper.ImageFolder.TryGetFileAsync(item.FileName);
                        if (file != null)
                        {
                            await file.DeleteAsync();
                        }

                        progress.Report(++i / count);
                    }
                    db.ImageSet.RemoveRange(todelete);
                    await db.SaveChangesAsync();
                }
            });
        }

        internal CachedGallery(GalleryModel model)
            : base(model) { }

        protected override IAsyncAction InitOverrideAsync()
        {
            return Task.Run(async () =>
            {
                using (var db = new GalleryDb())
                {
                    db.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
                    var gid = Id;
                    var models = db.GalleryImageSet
                        .Include(gi => gi.Image)
                        .Where(gi => gi.GalleryId == gid);
                    foreach (var item in models)
                    {
                        await this[item.PageId - 1].PopulateCachedImageAsync(item, item.Image);
                        LoadedItems[item.PageId - 1] = true;
                    }
                }
            }).AsAsyncAction();
        }
    }
}
