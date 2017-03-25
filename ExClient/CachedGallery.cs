using ExClient.Models;
using GalaSoft.MvvmLight.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Data;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;

namespace ExClient
{
    public class CachedGallery : Gallery
    {
        private sealed class CachedGalleryList : GalleryList<CachedGallery, GalleryModel>
        {
            public static IAsyncOperation<CachedGalleryList> LoadList()
            {
                return Task.Run(() =>
                {
                    using(var db = new GalleryDb())
                    {
                        db.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                        var query = from gm in db.GallerySet
                                    where gm.Images.Count != 0
                                    where db.SavedSet.FirstOrDefault(sm => sm.GalleryId == gm.Id) == null
                                    select gm;
                        return new CachedGalleryList(query);
                    }
                }).AsAsyncOperation();
            }

            private CachedGalleryList(IEnumerable<GalleryModel> galleries)
                : base(galleries)
            {
            }

            protected override CachedGallery Load(int index)
            {
                var c = new CachedGallery(this.Models[index]);
                var ignore = c.InitAsync();
                return c;
            }
        }

        public static IAsyncOperation<IncrementalLoadingCollection<Gallery>> LoadCachedGalleriesAsync()
        {
            return Run<IncrementalLoadingCollection<Gallery>>(async token => await CachedGalleryList.LoadList());
        }

        public static IAsyncActionWithProgress<double> ClearCachedGalleriesAsync()
        {
            return Run<double>(async (token, progress) =>
            {
                progress.Report(double.NaN);
                using(var db = new GalleryDb())
                {
                    var query = from gm in db.GallerySet
                                where gm.Images.Count != 0
                                where db.SavedSet.FirstOrDefault(sm => sm.GalleryId == gm.Id) == null
                                select gm.Images;
                    var cacheDic = query.ToDictionary(dm => dm.First().OwnerId.ToString());
                    var saveDic = db.SavedSet.Select(sm => sm.GalleryId).ToDictionary(id => id.ToString());
                    double count = cacheDic.Count;
                    var i = 0;
                    foreach(var item in cacheDic)
                    {
                        progress.Report(i / count);
                        var folder = await StorageHelper.LocalCache.CreateFolderAsync(item.Key, Windows.Storage.CreationCollisionOption.OpenIfExists);
                        await folder.DeleteAsync(Windows.Storage.StorageDeleteOption.PermanentDelete);
                        db.ImageSet.RemoveRange(item.Value);
                        i++;
                    }
                    var folders = await StorageHelper.LocalCache.GetItemsAsync();
                    foreach(var item in folders)
                    {
                        if(!saveDic.ContainsKey(item.Name))
                            await item.DeleteAsync();
                    }
                    await db.SaveChangesAsync();
                }
            });
        }

        internal CachedGallery(GalleryModel model)
            : base(model)
        {
        }

        internal ImageModel[] ImageModels { get; private set; }

        internal void LoadImageModels()
        {
            if(this.ImageModels != null)
                return;
            this.ImageModels = new ImageModel[this.RecordCount];
            using(var db = new GalleryDb())
            {
                db.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                var gid = this.Id;
                var models = from im in db.ImageSet
                             where im.OwnerId == gid
                             select im;
                foreach(var item in models)
                {
                    this.ImageModels[item.PageId - 1] = item;
                }
            }
        }

        protected override IAsyncOperation<IReadOnlyList<GalleryImage>> LoadPageAsync(int pageIndex)
        {
            return Task.Run<IReadOnlyList<GalleryImage>>(async () =>
            {
                await GetFolderAsync();
                this.LoadImageModels();
                var currentPageSize = MathHelper.GetSizeOfPage(this.RecordCount, PageSize, pageIndex);
                var loadList = new GalleryImage[currentPageSize];
                for(var i = 0; i < currentPageSize; i++)
                {
                    var model = this.ImageModels[this.Count + i];
                    if(model == null)
                    {
                        loadList[i] = new GalleryImagePlaceHolder(this, this.Count + i + 1);
                    }
                    else
                    {
                        loadList[i] = await GalleryImage.LoadCachedImageAsync(this, model)
                                ?? new GalleryImage(this, model.PageId, model.ImageKey, null);
                    }
                }
                return loadList;
            }).AsAsyncOperation();
        }

        private Dictionary<int, LoadPageAction> loadingPageDic = new Dictionary<int, LoadPageAction>();

        private class LoadPageAction : IAsyncAction
        {
            private IAsyncAction action;

            public LoadPageAction(IAsyncAction action)
            {
                this.action = action;
                this.action.Completed = this.action_Completed;
            }

            private void action_Completed(IAsyncAction sender, AsyncStatus e)
            {
                foreach(var item in this.completed)
                {
                    item(this, e);
                }
                this.action = null;
                this.completed = null;
            }

            public bool Disposed => this.action == null;

            public AsyncActionCompletedHandler Completed
            {
                get => this.action.Completed;
                set => completed.Add(value);
            }

            private List<AsyncActionCompletedHandler> completed = new List<AsyncActionCompletedHandler>();

            public Exception ErrorCode => this.action.ErrorCode;

            public uint Id => this.action.Id;

            public AsyncStatus Status => this.action.Status;

            public void Cancel() => this.action.Cancel();

            public void Close() => this.action.Close();

            public void GetResults() => this.action.GetResults();
        }

        internal IAsyncAction LoadImageAsync(GalleryImagePlaceHolder image)
        {
            var pageIndex = MathHelper.GetPageIndexOfRecord(PageSize, image.PageId - 1);
            var lpAc = (LoadPageAction)null;
            if(this.loadingPageDic.TryGetValue(pageIndex, out lpAc))
            {
                if(!lpAc.Disposed)
                    return lpAc;
                else
                    this.loadingPageDic.Remove(pageIndex);
            }
            var action = Run(async token =>
            {
                var images = await base.LoadPageAsync(pageIndex);
                var offset = MathHelper.GetStartIndexOfPage(PageSize, pageIndex);
                for(var i = 0; i < images.Count; i++)
                {
                    var ph = this[i + offset] as GalleryImagePlaceHolder;
                    if(ph == null)
                        continue;
                    this[i + offset] = images[i];
                }
            });
            lpAc = new LoadPageAction(action);
            this.loadingPageDic[pageIndex] = lpAc;
            return lpAc;
        }

        public override IAsyncAction DeleteAsync()
        {
            this.ImageModels = null;
            return base.DeleteAsync();
        }

        public override IAsyncActionWithProgress<SaveGalleryProgress> SaveGalleryAsync(ConnectionStrategy strategy)
        {
            return Run<SaveGalleryProgress>(async (token, p) =>
            {
                p.Report(new SaveGalleryProgress { ImageCount = this.RecordCount, ImageLoaded = -1 });
                for(var i = 0; i < this.Count; i++)
                {
                    if(this[i] is GalleryImagePlaceHolder ph)
                    {
                        token.ThrowIfCancellationRequested();
                        await ph.LoadImageAsync(false, strategy, true);
                    }
                }
                var load = base.SaveGalleryAsync(strategy);
                load.Progress = (sender, pro) => p.Report(pro);
                token.Register(load.Cancel);
                await load;
            });
        }

        protected override IAsyncAction InitOverrideAsync()
        {
            return DispatcherHelper.RunAsync(async () =>
            {
                var f = await GetFolderAsync();
                if(this.Thumb != null)
                    return;
                var file = (await f.GetFilesAsync(Windows.Storage.Search.CommonFileQuery.DefaultQuery, 0, 1)).SingleOrDefault();
                if(file == null)
                    return;
                try
                {
                    using(var stream = await file.GetThumbnailAsync(Windows.Storage.FileProperties.ThumbnailMode.SingleItem))
                    {
                        var decoder = await Windows.Graphics.Imaging.BitmapDecoder.CreateAsync(stream);
                        this.Thumb = await decoder.GetSoftwareBitmapAsync(Windows.Graphics.Imaging.BitmapPixelFormat.Bgra8, Windows.Graphics.Imaging.BitmapAlphaMode.Premultiplied);
                    }
                }
                catch(Exception)
                {
                }
            });
        }
    }
}
