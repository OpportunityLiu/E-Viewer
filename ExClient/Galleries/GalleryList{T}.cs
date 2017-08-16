using Opportunity.MvvmUniverse.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Data;

namespace ExClient.Galleries
{
    internal abstract class GalleryList<TGallery, TModel> : ObservableCollection<Gallery>, IItemsRangeInfo
         where TGallery : Gallery
    {
        public bool IsEmpty => this.Count == 0;

        private int loadedCount;

        internal GalleryList(List<TModel> models)
            : base(Enumerable.Repeat<TGallery>(null, models.Count))
        {
            var end = Math.Min(models.Count, 10);
            for (var i = 0; i < end; i++)
            {
                this[i] = Load(models[i]);
                models[i] = default(TModel);
                this.loadedCount++;
            }
            this.models = models;
        }

        private List<TModel> models;

        protected override void ClearItems()
        {
            this.models.Clear();
            this.loadedCount = 0;
            base.ClearItems();
            OnPropertyChanged(nameof(IsEmpty));
        }

        protected override void RemoveItems(int index, int count)
        {
            this.models.RemoveRange(index, count);
            for (var i = 0; i < count; i++)
            {
                if (this[index + i] != null)
                    this.loadedCount--;
            }
            base.RemoveItems(index, count);
            OnPropertyChanged(nameof(IsEmpty));
        }

        void IItemsRangeInfo.RangesChanged(ItemIndexRange visibleRange, IReadOnlyList<ItemIndexRange> trackedItems)
        {
            if (this.loadedCount == this.models.Count)
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
                if (this[i] != null)
                    continue;
                this[i] = Load(this.models[i]);
                this.models[i] = default(TModel);
                this.loadedCount++;
            }
        }

        protected abstract TGallery Load(TModel model);

        void IDisposable.Dispose() { this.Clear(); }
    }
}
