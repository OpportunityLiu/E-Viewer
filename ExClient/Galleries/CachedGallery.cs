using ExClient.Models;
using Microsoft.EntityFrameworkCore;
using Opportunity.MvvmUniverse.AsyncHelpers;
using Opportunity.MvvmUniverse.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;

namespace ExClient.Galleries
{
    public class CachedGallery : Gallery
    {
        private sealed class CachedGalleryList : GalleryList<CachedGallery, GalleryModel>
        {
            public static IAsyncOperation<ObservableCollection<Gallery>> LoadList()
            {
                return Task.Run<ObservableCollection<Gallery>>(() =>
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

        public static IAsyncOperation<ObservableCollection<Gallery>> LoadCachedGalleriesAsync()
        {
            return CachedGalleryList.LoadList();
        }

        public static IAsyncActionWithProgress<double> ClearCachedGalleriesAsync()
        {
            return Run<double>(async (token, progress) =>
            {
                using (var db = new GalleryDb())
                {
                    var folder = GalleryImage.ImageFolder ?? await GalleryImage.GetImageFolderAsync();
                    var todelete = await db.ImageSet
                        .Where(im => !db.SavedSet.Any(sm => im.UsingBy.Any(gi => gi.GalleryId == sm.GalleryId)))
                        .ToListAsync(token);
                    double count = todelete.Count;
                    var i = 0;
                    foreach (var item in todelete)
                    {
                        var file = await folder.TryGetFileAsync(item.FileName);
                        if (file != null)
                            await file.DeleteAsync();
                        progress.Report(++i / count);
                    }
                    db.ImageSet.RemoveRange(todelete);
                    await db.SaveChangesAsync();
                }
            });
        }

        internal CachedGallery(GalleryModel model)
            : base(model)
        {
            this.loadingPageArray = new IAsyncAction[MathHelper.GetPageCount(model.RecordCount, PageSize)];
        }

        internal GalleryImageModel[] GalleryImageModels { get; private set; }

        internal void LoadImageModels()
        {
            if (this.GalleryImageModels != null)
                return;
            this.GalleryImageModels = new GalleryImageModel[this.RecordCount];
            using (var db = new GalleryDb())
            {
                db.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
                var gid = this.ID;
                var models = db.GalleryImageSet
                    .Include(gi => gi.Image)
                    .Where(gi => gi.GalleryId == gid);
                foreach (var item in models)
                {
                    this.GalleryImageModels[item.PageId - 1] = item;
                }
            }
        }

        protected override IAsyncOperation<IList<GalleryImage>> LoadPageAsync(int pageIndex)
        {
            return Run(async token =>
            {
                try
                {
                    return await base.LoadPageAsync(pageIndex);
                }
                catch
                {
                    return await LoadPageLocalilyAsync(pageIndex);
                }
            });
        }

        protected IAsyncOperation<IList<GalleryImage>> LoadPageLocalilyAsync(int pageIndex)
        {
            return Task.Run<IList<GalleryImage>>(async () =>
            {
                this.LoadImageModels();
                var currentPageSize = MathHelper.GetSizeOfPage(this.RecordCount, PageSize, pageIndex);
                var loadList = new GalleryImage[currentPageSize];
                for (var i = 0; i < currentPageSize; i++)
                {
                    var model = this.GalleryImageModels[this.Count + i];
                    if (model == null)
                    {
                        loadList[i] = new GalleryImagePlaceHolder(this, this.Count + i + 1);
                    }
                    else
                    {
                        loadList[i] = await GalleryImage.LoadCachedImageAsync(this, model, model.Image);
                    }
                }
                return loadList;
            }).AsAsyncOperation();
        }

        private readonly IAsyncAction[] loadingPageArray;

        internal IAsyncAction LoadImageAsync(GalleryImagePlaceHolder image)
        {
            var pageIndex = MathHelper.GetPageIndexOfRecord(PageSize, image.PageID - 1);
            var lpAc = this.loadingPageArray[pageIndex];
            if (lpAc != null && lpAc.Status == AsyncStatus.Started)
                return lpAc;
            return this.loadingPageArray[pageIndex] = Run(async token =>
            {
                var images = await base.LoadPageAsync(pageIndex);
                var offset = MathHelper.GetStartIndexOfPage(PageSize, pageIndex);
                for (var i = 0; i < images.Count; i++)
                {
                    var ph = this[i + offset] as GalleryImagePlaceHolder;
                    if (ph == null)
                        continue;
                    this[i + offset] = images[i];
                }
                await Task.Yield();
                this.loadingPageArray[pageIndex] = null;
            }).AsMulticast();
        }

        public override IAsyncAction DeleteAsync()
        {
            this.GalleryImageModels = null;
            Array.Clear(this.loadingPageArray, 0, this.loadingPageArray.Length);
            return base.DeleteAsync();
        }

        public override IAsyncActionWithProgress<SaveGalleryProgress> SaveAsync(ConnectionStrategy strategy)
        {
            return Run<SaveGalleryProgress>(async (token, progress) =>
            {
                var toReport = new SaveGalleryProgress
                {
                    ImageCount = this.RecordCount,
                    ImageLoadedInternal = -1
                };
                progress.Report(toReport);
                while (this.HasMoreItems)
                {
                    await this.LoadMoreItemsAsync((uint)PageSize);
                    token.ThrowIfCancellationRequested();
                }
                for (var i = 0; i < this.Count; i++)
                {
                    if (this[i] is GalleryImagePlaceHolder ph)
                    {
                        await ph.LoadImageAsync(false, strategy, true);
                        token.ThrowIfCancellationRequested();
                    }
                }

                var load = base.SaveAsync(strategy);
                load.Progress = (sender, pro) => progress.Report(pro);
                token.Register(load.Cancel);
                await load;
            });
        }

        protected override IAsyncAction InitOverrideAsync()
        {
            return AsyncWrapper.CreateCompleted();
        }

        protected override IAsyncOperation<SoftwareBitmap> GetThumbAsync()
        {
            return Run(async token =>
            {
                var r = await base.GetThumbAsync();
                if (r != null)
                    return r;
                return await GetThumbLocalilyAsync();
            });
        }

        protected IAsyncOperation<SoftwareBitmap> GetThumbLocalilyAsync()
        {
            return Run(async token =>
            {
                using (var db = new GalleryDb())
                {
                    db.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
                    var gId = this.ID;
                    var imageModel = db.GalleryImageSet
                        .Include(gi => gi.Image)
                        .Where(gi => gi.GalleryId == gId)
                        .OrderBy(gi => gi.PageId)
                        .FirstOrDefault();
                    if (imageModel == null)
                        return null;
                    var folder = GalleryImage.ImageFolder ?? await GalleryImage.GetImageFolderAsync();
                    var file = await folder.TryGetFileAsync(imageModel.Image.FileName);
                    if (file == null)
                        return null;
                    try
                    {
                        using (var stream = await file.GetThumbnailAsync(Windows.Storage.FileProperties.ThumbnailMode.SingleItem))
                        {
                            var decoder = await BitmapDecoder.CreateAsync(stream);
                            return await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
                        }
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }
            });
        }
    }
}
