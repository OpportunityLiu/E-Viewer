using ExClient.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml.Data;

namespace ExClient
{
    internal abstract class GalleryList<TGallery, TModel> : IncrementalLoadingCollection<Gallery>, IItemsRangeInfo
         where TGallery : Gallery
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

        internal GalleryList(IEnumerable<TModel> models)
            : base(1)
        {
            this.PageCount = 1;
            this.models = models.ToList();
            this.RecordCount = this.models.Count;
            AddRange(Enumerable.Repeat(DefaultGallery, this.RecordCount));
        }

        private List<TModel> models;
        protected IReadOnlyList<TModel> Models => this.models;

        protected override void ClearItems()
        {
            this.models.Clear();
            this.RecordCount = 0;
            base.ClearItems();
            this.loadedCount = 0;
        }

        protected override void RemoveItem(int index)
        {
            this.models.RemoveAt(index);
            this.RecordCount--;
            if(this[index] != DefaultGallery)
                this.loadedCount--;
            base.RemoveItem(index);
        }

        public void RangesChanged(ItemIndexRange visibleRange, IReadOnlyList<ItemIndexRange> trackedItems)
        {
            if(this.loadedCount == this.RecordCount)
            {
                return;
            }
            foreach(var item in trackedItems.Concat(Enumerable.Repeat(visibleRange, 1)).Distinct(ItemIndexRangeEqualityComparer.Default))
            {
                loadRange(item);
            }
        }

        private void loadRange(ItemIndexRange visibleRange)
        {
            if(visibleRange.FirstIndex < 0)
                visibleRange = new ItemIndexRange(0, (uint)visibleRange.LastIndex + 1);
            if(visibleRange.LastIndex >= this.Count)
                visibleRange = new ItemIndexRange(visibleRange.FirstIndex, (uint)(this.Count - visibleRange.FirstIndex));

            for(var i = visibleRange.FirstIndex; i <= visibleRange.LastIndex; i++)
            {
                var index = i + visibleRange.FirstIndex;
                if(this[index] != DefaultGallery)
                    continue;
                this[index] = Load(index);
                this.loadedCount++;
            }
        }

        protected abstract TGallery Load(int index);

        protected override IAsyncOperation<IReadOnlyList<Gallery>> LoadPageAsync(int pageIndex)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
        }
    }
}
