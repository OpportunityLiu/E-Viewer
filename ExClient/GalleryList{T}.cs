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
            if (this[index] != DefaultGallery)
                this.loadedCount--;
            base.RemoveItem(index);
        }

        void IItemsRangeInfo.RangesChanged(ItemIndexRange visibleRange, IReadOnlyList<ItemIndexRange> trackedItems)
        {
            if (this.loadedCount == this.RecordCount)
            {
                return;
            }
            var ranges = trackedItems.Concat(Enumerable.Repeat(visibleRange, 1)).ToList();
            var start = ranges.Min(r => r.FirstIndex);
            var end = ranges.Max(r => r.LastIndex) + 1;
            if (start < 0)
                start = 0;
            if (end > this.Count)
                end = this.Count;
            for (var i = start; i < end; i++)
            {
                if (this[i] != DefaultGallery)
                    continue;
                this[i] = Load(this.models[i]);
                this.models[i] = default(TModel);
                this.loadedCount++;
            }
        }

        protected abstract TGallery Load(TModel model);

        protected override IAsyncOperation<IReadOnlyList<Gallery>> LoadPageAsync(int pageIndex)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
        }
    }
}
