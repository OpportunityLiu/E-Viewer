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
    public abstract class GalleryList<T> : IncrementalLoadingCollection<Gallery>, IItemsRangeInfo
         where T : Gallery
    {
        protected static Gallery DefaultGallery
        {
            get;
        } = new Gallery(-1, null, "", "", LocalizedStrings.Resources.DefaultTitle, "", "", "ms-appx:///", LocalizedStrings.Resources.DefaultUploader, "0", "0", 0, false, "2.5", "0", new string[0]);

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

        private int loadedCount;

        internal GalleryList(int recordCount)
            : base(1)
        {
            this.PageCount = 1;
            this.RecordCount = recordCount;
            AddRange(Enumerable.Repeat(DefaultGallery, RecordCount));
        }

        protected override void ClearItems()
        {
            RecordCount = 0;
            base.ClearItems();
            loadedCount = 0;
        }

        protected override void RemoveItem(int index)
        {
            RecordCount--;
            if(this[index] != DefaultGallery)
                loadedCount--;
            base.RemoveItem(index);
        }

        public void RangesChanged(ItemIndexRange visibleRange, IReadOnlyList<ItemIndexRange> trackedItems)
        {
            if(loadedCount == RecordCount)
            {
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
                if(this[i] == DefaultGallery)
                {
                    needLoad = true;
                    break;
                }
            }
            if(!needLoad)
                return;
            var list = LoadRange(visibleRange, db);
            for(int i = 0; i < visibleRange.Length; i++)
            {
                var index = i + visibleRange.FirstIndex;
                if(this[index] != DefaultGallery)
                    continue;
                this[index] = list[i];
                this.loadedCount++;
            }
        }

        protected abstract IList<T> LoadRange(ItemIndexRange visibleRange, GalleryDb db);

        protected override IAsyncOperation<IList<Gallery>> LoadPageAsync(int pageIndex)
        {
            return Run<IList<Gallery>>(async token =>
            {
                await Task.Yield();
                return Array.Empty<T>();
            });
        }

        public void Dispose()
        {
        }
    }
}
