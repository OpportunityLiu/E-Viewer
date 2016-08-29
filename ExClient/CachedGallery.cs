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

namespace ExClient
{
    public class CachedGallery : Gallery
    {
        private class CachedGalleryList : IncrementalLoadingCollection<Gallery>, IItemsRangeInfo
        {
            private static Gallery defaultGallery = new Gallery(-1, null, "", "", LocalizedStrings.Resources.DefaultTitle, "", "", "ms-appx:///", LocalizedStrings.Resources.DefaultUploader, "0", "0", 0, false, "2.5", "0", new string[0]);

            private class ItemIndexRangeEqualityComparer : EqualityComparer<ItemIndexRange>
            {
                public static new ItemIndexRangeEqualityComparer Default { get; } = new ItemIndexRangeEqualityComparer();

                public override bool Equals(ItemIndexRange x, ItemIndexRange y)
                {
                    return x.FirstIndex == y.FirstIndex && x.Length == y.Length;
                }

                public override int GetHashCode(ItemIndexRange obj)
                {
                    return obj.FirstIndex ^ ((int)obj.Length << 16);
                }
            }

            private BitArray loadStateFlag;
            private int loadedCount;

            public CachedGalleryList() : base(1)
            {
                using(var db = new GalleryDb())
                {
                    RecordCount = db.CacheSet.Count();
                    PageCount = 1;
                    loadStateFlag = new BitArray(RecordCount);
                    AddRange(Enumerable.Repeat(defaultGallery, RecordCount));
                    loadRange(new ItemIndexRange(0, (uint)Math.Min(RecordCount, 10)), db);
                }
            }

            protected override void RemoveItem(int index)
            {
                RecordCount--;
                base.RemoveItem(index);
                if(loadStateFlag != null)
                {
                    if(loadStateFlag[index])
                        loadedCount--;
                    var oldFlag = loadStateFlag;
                    loadStateFlag = new BitArray(RecordCount);
                    for(int i = 0; i < index; i++)
                    {
                        loadStateFlag[i] = oldFlag[i];
                    }
                    for(int i = index; i < RecordCount; i++)
                    {
                        loadStateFlag[i] = oldFlag[i + 1];
                    }
                }
                else
                {
                    loadedCount--;
                }
            }

            public void RangesChanged(ItemIndexRange visibleRange, IReadOnlyList<ItemIndexRange> trackedItems)
            {
                if(loadedCount == RecordCount)
                {
                    loadStateFlag = null;
                    return;
                }
                using(var db = new GalleryDb())
                {
                    foreach(var item in trackedItems.Concat(Enumerable.Repeat(visibleRange, 1)).Distinct(ItemIndexRangeEqualityComparer.Default))
                    {
                        loadRange(item, db);
                    }
                }
            }

            private void loadRange(ItemIndexRange visibleRange, GalleryDb db)
            {
                var needLoad = false;
                for(int i = visibleRange.LastIndex; i >= visibleRange.FirstIndex; i--)
                {
                    if(!this.loadStateFlag[i])
                    {
                        needLoad = true;
                        break;
                    }
                }
                if(!needLoad)
                    return;
                var query = db.CacheSet
                    .OrderByDescending(c => c.Saved)
                    .Skip(visibleRange.FirstIndex)
                    .Take((int)visibleRange.Length)
                    .Select(c => new
                    {
                        c,
                        c.Gallery
                    }).ToList();
                for(int i = 0; i < visibleRange.Length; i++)
                {
                    var index = i + visibleRange.FirstIndex;
                    if(this.loadStateFlag[index])
                        continue;
                    var a = query[i];
                    var c = new CachedGallery(a.Gallery, a.c);
                    var ignore = c.InitAsync();
                    this[index] = c;
                    this.loadStateFlag[index] = true;
                    this.loadedCount++;
                }
            }

            protected override IAsyncOperation<uint> LoadPageAsync(int pageIndex)
            {
                return Task.Run(() =>
                {
                    return 0u;
                }).AsAsyncOperation();
            }
            
            public void Dispose()
            {
            }
        }

        private static int pageSize = 20;

        public static IAsyncOperation<IncrementalLoadingCollection<Gallery>> LoadCachedGalleriesAsync()
        {
            return Task.Run<IncrementalLoadingCollection<Gallery>>(() => new CachedGalleryList()).AsAsyncOperation();
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
                using(var db = new GalleryDb())
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
            this.thumbFile = cacheModel.ThumbData;
            this.PageCount = (int)Math.Ceiling(RecordCount / (double)pageSize);
            this.Owner = Client.Current;
        }

        public override IAsyncAction InitOverrideAsync()
        {
            var temp = thumbFile;
            return DispatcherHelper.RunAsync(async () =>
            {
                if(temp == null)
                    return;
                using(var stream = temp.AsBuffer().AsRandomAccessStream())
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
            if(imageModels != null)
                return;
            using(var db = new GalleryDb())
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
                        image = new GalleryImage(this, model.PageId, model.ImageKey, null);
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
                using(var db = new GalleryDb())
                {
                    db.CacheSet.Remove(db.CacheSet.Single(c => c.GalleryId == this.Id));
                    await db.SaveChangesAsync();
                }
                await base.DeleteAsync();
            }).AsAsyncAction();
        }
    }
}
