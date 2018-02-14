using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExClient;
using Opportunity.MvvmUniverse;
using ExClient.Galleries;

namespace ExViewer.Views
{
    internal class ImagePageCollectionView : IReadOnlyList<ImagePageImageView>, IDisposable
    {
        private const int initialCapacity = 10;

        public ImagePageCollectionView()
        {
            ensureCacheSize(initialCapacity);
        }

        private List<ImagePageImageView> imageViewCache = new List<ImagePageImageView>(initialCapacity);

        private void ensureCacheSize(int needSize)
        {
            var needItemCount = needSize - this.imageViewCache.Count;
            if (needItemCount <= 0)
                return;
            this.imageViewCache.Capacity = needSize;
            this.imageViewCache.AddRange(from i in Enumerable.Range(this.imageViewCache.Count, needItemCount)
                                         select new ImagePageImageView(this, i));
        }

        private void collection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
            case NotifyCollectionChangedAction.Add:
                for (var i = 0; i < e.NewItems.Count; i++)
                {
                    this.imageViewCache[e.NewStartingIndex + i].Refresh();
                }
                break;
            default:
                foreach (var item in this.imageViewCache.GetRange(0, this.Count))
                {
                    item.Refresh();
                }
                break;
            }
        }

        private Gallery collection;

        public Gallery Collection
        {
            get => this.collection;
            set
            {
                if (this.collection != null)
                    this.collection.CollectionChanged -= this.collection_CollectionChanged;
                this.collection = value;
                if (this.collection != null)
                {
                    this.collection.CollectionChanged += this.collection_CollectionChanged;
                    ensureCacheSize(this.collection.RecordCount);
                }
            }
        }

        public ImagePageImageView this[int index]
        {
            get
            {
                if ((uint)index < (uint)this.Count)
                    return this.imageViewCache[index];
                throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        public int Count => this.collection?.RecordCount ?? 0;

        public IEnumerator<ImagePageImageView> GetEnumerator()
        {
            return this.imageViewCache.Take(this.Count).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                }
                this.Collection = null;
                foreach (var item in this.imageViewCache)
                {
                    item.Dispose();
                }
                this.imageViewCache = null;
                this.disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }

    internal sealed class ImagePageImageView : ObservableObject, IDisposable
    {
        private static readonly GalleryImage defaultImage = null;

        public ImagePageImageView(ImagePageCollectionView parent, int index)
        {
            this.parent = parent;
            this.index = index;
        }

        private ImagePageCollectionView parent;
        private readonly int index;

        public GalleryImage Image
        {
            get
            {
                if (this.parent == null || this.parent.Collection == null)
                    return defaultImage;
                if (this.index >= this.parent.Collection.Count)
                    return defaultImage;
                return this.parent.Collection[this.index];
            }
        }

        public void Refresh()
        {
            OnPropertyChanged(nameof(Image));
        }

        public void Dispose()
        {
            this.parent = null;
            Refresh();
        }
    }
}
