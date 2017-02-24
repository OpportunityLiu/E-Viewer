using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using ExClient.Models;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;
using GalaSoft.MvvmLight.Threading;
using Windows.UI.Xaml.Data;
using Windows.Foundation.Diagnostics;

namespace ExClient
{
    public class CachedGallery : Gallery
    {
        private sealed class CachedGalleryList : GalleryList<CachedGallery>
        {
            public static IAsyncOperation<CachedGalleryList> LoadList()
            {
                return Task.Run(() =>
                {
                    using(var db = new GalleryDb())
                    {
                        var query = from gm in db.GallerySet
                                    where gm.Images.Count != 0
                                    where db.SavedSet.FirstOrDefault(sm => sm.GalleryId == gm.Id) == null
                                    select gm;
                        return new CachedGalleryList(query.ToList());
                    }
                }).AsAsyncOperation();
            }

            private CachedGalleryList(List<GalleryModel> galleries)
                : base(galleries.Count)
            {
                this.galleries = galleries;
            }

            private List<GalleryModel> galleries;

            protected override void RemoveItem(int index)
            {
                galleries.RemoveAt(index);
                base.RemoveItem(index);
            }

            protected override void ClearItems()
            {
                this.galleries.Clear();
                base.ClearItems();
            }

            protected override IList<CachedGallery> LoadRange(ItemIndexRange visibleRange, GalleryDb db)
            {
                var list = new CachedGallery[visibleRange.Length];
                for(int i = 0; i < visibleRange.Length; i++)
                {
                    var index = visibleRange.FirstIndex + i;
                    if(this[index] != DefaultGallery)
                        continue;
                    var c = new CachedGallery(galleries[index]);
                    var ignore = c.InitAsync();
                    this[index] = c;
                }
                return list;
            }
        }

        public static IAsyncOperation<GalleryList<CachedGallery>> LoadCachedGalleriesAsync()
        {
            return Run<GalleryList<CachedGallery>>(async token => await CachedGalleryList.LoadList());
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
                    int i = 0;
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
            if(ImageModels != null)
                return;
            ImageModels = new ImageModel[RecordCount];
            using(var db = new GalleryDb())
            {
                var gid = Id;
                var models = from im in db.ImageSet
                             where im.OwnerId == gid
                             select im;
                foreach(var item in models)
                {
                    ImageModels[item.PageId - 1] = item;
                }
            }
        }

        protected override IAsyncOperation<IList<GalleryImage>> LoadPageAsync(int pageIndex)
        {
            return Task.Run<IList<GalleryImage>>(async () =>
            {
                await GetFolderAsync();
                this.LoadImageModels();
                var currentPageSize = MathHelper.GetSizeOfPage(RecordCount, PageSize, pageIndex);
                var loadList = new GalleryImage[currentPageSize];
                for(int i = 0; i < currentPageSize; i++)
                {
                    var model = ImageModels[Count + i];
                    if(model == null)
                    {
                        loadList[i] = new GalleryImagePlaceHolder(this, Count + i + 1);
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
                this.action.Completed = action_Completed;
            }

            private void action_Completed(IAsyncAction sender, AsyncStatus e)
            {
                foreach(var item in completed)
                {
                    item(this, e);
                }
                action = null;
                completed = null;
            }

            public bool Disposed => action == null;

            public AsyncActionCompletedHandler Completed
            {
                get
                {
                    return this.action.Completed;
                }
                set
                {
                    completed.Add(value);
                }
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
            if(loadingPageDic.TryGetValue(pageIndex, out lpAc))
            {
                if(!lpAc.Disposed)
                    return lpAc;
                else
                    loadingPageDic.Remove(pageIndex);
            }
            var action = Run(async token =>
            {
                var images = await base.LoadPageAsync(pageIndex);
                var offset = MathHelper.GetStartIndexOfPage(PageSize, pageIndex);
                for(int i = 0; i < images.Count; i++)
                {
                    var ph = this[i + offset] as GalleryImagePlaceHolder;
                    if(ph == null)
                        continue;
                    this[i + offset] = images[i];
                }
            });
            lpAc = new LoadPageAction(action);
            loadingPageDic[pageIndex] = lpAc;
            return lpAc;
        }

        public override IAsyncAction DeleteAsync()
        {
            ImageModels = null;
            return base.DeleteAsync();
        }

        protected override IAsyncAction InitOverrideAsync()
        {
            return DispatcherHelper.RunAsync(async () =>
            {
                await GetFolderAsync();
                var file = (await GalleryFolder.GetFilesAsync(Windows.Storage.Search.CommonFileQuery.DefaultQuery, 0, 1)).SingleOrDefault();
                if(file == null)
                    return;
                try
                {
                    using(var stream = await file.GetThumbnailAsync(Windows.Storage.FileProperties.ThumbnailMode.SingleItem))
                    {
                        var decoder = await Windows.Graphics.Imaging.BitmapDecoder.CreateAsync(stream);
                        Thumb = await decoder.GetSoftwareBitmapAsync(Windows.Graphics.Imaging.BitmapPixelFormat.Bgra8, Windows.Graphics.Imaging.BitmapAlphaMode.Premultiplied);
                    }
                }
                catch(Exception)
                {
                }
            });
        }
    }
}
