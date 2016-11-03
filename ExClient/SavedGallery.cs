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
using Windows.UI.Xaml.Data;
using System.Collections;
using MetroLog;

namespace ExClient
{
    public sealed class SavedGallery : Gallery, ICanLog
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
                for(int i = 0; i < visibleRange.Length; i++)
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
                for(int i = 0; i < items.Count; i++)
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
                : base(model, false)
        {
            this.thumbFile = savedModel.ThumbData;
            this.PageCount = MathHelper.GetPageCount(RecordCount, PageSize);
            this.Owner = Client.Current;
        }

        protected override IAsyncAction InitOverrideAsync()
        {
            var temp = thumbFile;
            return DispatcherHelper.RunAsync(async () =>
            {
                if(temp == null)
                    return;
                using(var stream = temp.AsRandomAccessStream())
                {
                    await Thumb.SetSourceAsync(stream);
                }
                thumbFile = null;
            });
        }

        private List<ImageModel> imageModels;

        private byte[] thumbFile;

        private void loadImageModel()
        {
            this.Log().Debug($"Start loading image model, Id = {Id}");
            using(var db = new GalleryDb())
            {
                var gid = Id;
                imageModels = (from g in db.GallerySet
                               where g.Id == gid
                               select g.Images).Single();
                imageModels.Sort((i, j) => i.PageId - j.PageId);
            }
            this.Log().Debug($"Finish loading image model, Id = {Id}");
        }

        protected override IAsyncOperation<IList<GalleryImage>> LoadPageAsync(int pageIndex)
        {
            this.Log().Info($"Start loading page {pageIndex}, Id = {Id}");
            return Task.Run<IList<GalleryImage>>(async () =>
            {
                    await GetFolderAsync();
                if(imageModels == null)
                    loadImageModel();
                var currentPageSize = MathHelper.GetSizeOfPage(RecordCount, PageSize, pageIndex);
                var toAdd = new List<GalleryImage>(currentPageSize);
                for(int i = 0; i < currentPageSize; i++)
                {
                    // Load cache
                    var model = imageModels[Count + i];
                    var image = await GalleryImage.LoadCachedImageAsync(this, model);
                    if(image == null)
                    // when load fails
                    {
                        image = new GalleryImage(this, model.PageId, model.ImageKey, null);
                    }
                    toAdd.Add(image);

                }
                this.Log().Info($"Finish loading page {pageIndex}, Id = {Id}");
                return toAdd;
            }).AsAsyncOperation();
        }

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
