using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using ExClient.Models;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;
using MetroLog;
using GalaSoft.MvvmLight.Threading;
using Windows.UI.Xaml.Data;

namespace ExClient
{
    public sealed class CachedGallery : Gallery, ICanLog
    {
        private sealed class CachedGalleryList : Internal.GalleryList
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

            protected override IList<Gallery> LoadRange(ItemIndexRange visibleRange, GalleryDb db)
            {
                var list = new Gallery[visibleRange.Length];
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

        public static IAsyncOperation<IncrementalLoadingCollection<Gallery>> LoadCachedGalleriesAsync()
        {
            return Run<IncrementalLoadingCollection<Gallery>>(async token => await CachedGalleryList.LoadList());
        }

        internal CachedGallery(GalleryModel model)
            : base(model, false)
        {
        }

        private List<ImageModel> imageModels;

        private void loadImageModel()
        {
            this.Log().Debug($"Start loading image model, Id = {Id}");
            using(var db = new GalleryDb())
            {
                var gid = Id;
                imageModels = (from im in db.ImageSet
                               where im.OwnerId == gid
                               select im).ToList();
                // 倒序
                imageModels.Sort((a, b) => b.PageId - a.PageId);
            }
            this.Log().Debug($"Finish loading image model, Id = {Id}");
        }

        protected override IAsyncOperation<IList<GalleryImage>> LoadPageAsync(int pageIndex)
        {
            return Task.Run<IList<GalleryImage>>(async () =>
            {
                if(GalleryFolder == null)
                    await GetFolderAsync();
                if(this.imageModels == null)
                    this.loadImageModel();
                var currentPageSize = MathHelper.GetSizeOfPage(RecordCount, PageSize, pageIndex);
                var loadList = new List<GalleryImage>(currentPageSize);
                for(int i = 0; i < currentPageSize; i++)
                {
                    var currentPageId = Count + i + 1;
                    if(imageModels.Count == 0 || imageModels[imageModels.Count - 1].PageId != currentPageId)
                        loadList.Add(new GalleryImagePlaceHolder(this, currentPageId));
                    else
                    {
                        loadList.Add(await GalleryImage.LoadCachedImageAsync(this, imageModels[imageModels.Count - 1]));
                        imageModels.RemoveAt(imageModels.Count - 1);
                    }
                }
                return loadList;
            }).AsAsyncOperation();
        }

        internal IAsyncAction LoadImageAsync(GalleryImagePlaceHolder image)
        {
            return Run(async token =>
            {
                var pageIndex = MathHelper.GetPageIndexOfRecord(PageSize, image.PageId - 1);
                var images = await base.LoadPageAsync(pageIndex);
                var offset = MathHelper.GetStartIndexOfPage(PageSize, pageIndex);
                for(int i = 0; i < images.Count; i++)
                {
                    var ph = this[i + offset] as GalleryImagePlaceHolder;
                    if(ph == null)
                        continue;
                    ph.Init(images[i].ImageKey, images[i].Thumb);
                }
            });
        }

        public override IAsyncAction DeleteAsync()
        {
            imageModels = null;
            return base.DeleteAsync();
        }

        protected override IAsyncAction InitOverrideAsync()
        {
            return DispatcherHelper.RunAsync(async () =>
            {
                if(GalleryFolder == null)
                    await GetFolderAsync();
                var file = (await GalleryFolder.GetFilesAsync(Windows.Storage.Search.CommonFileQuery.DefaultQuery, 0, 1)).SingleOrDefault();
                if(file == null)
                    return;
                try
                {
                    using(var stream = await file.GetThumbnailAsync(Windows.Storage.FileProperties.ThumbnailMode.SingleItem))
                    {
                        await Thumb.SetSourceAsync(stream);
                    }
                }
                catch(Exception)
                {
                }
            });
        }
    }
}
