using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExClient;

namespace ExViewer.Views
{
    internal class ImagePageCollectionView : IReadOnlyList<IImagePageImageView>, IDisposable
    {
        private const int initialCapacity = 16;

        public ImagePageCollectionView()
        {
            ensureCacheSize(initialCapacity);
        }

        private List<ImagePageImageView> imageViewCache = new List<ImagePageImageView>(initialCapacity);

        private void ensureCacheSize(int needSize)
        {
            var needItemCount = needSize - imageViewCache.Count;
            if(needItemCount <= 0)
                return;
            imageViewCache.Capacity = needSize;
            imageViewCache.AddRange(from i in Enumerable.Range(imageViewCache.Count, needItemCount)
                                    select new ImagePageImageView(this, i));
        }

        private void collection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch(e.Action)
            {
            case NotifyCollectionChangedAction.Add:
                for(int i = 0; i < e.NewItems.Count; i++)
                {
                    imageViewCache[e.NewStartingIndex + i].Refresh();
                }
                break;
            default:
                foreach(var item in imageViewCache.GetRange(0, Count))
                {
                    item.Refresh();
                }
                break;
            }
        }

        private Gallery collection;

        public Gallery Collection
        {
            get
            {
                return collection;
            }
            set
            {
                if(collection != null)
                    collection.CollectionChanged -= collection_CollectionChanged;
                collection = value;
                ensureCacheSize(collection.RecordCount);
                if(collection != null)
                    collection.CollectionChanged += collection_CollectionChanged;
            }
        }

        private sealed class ImagePageImageView : ObservableObject, IImagePageImageView, IDisposable
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
                    if(parent == null || parent.collection == null)
                        return defaultImage;
                    if(index < parent.collection.Count)
                        return parent.collection[index];
                    return defaultImage;
                }
            }

            public void Refresh()
            {
                RaisePropertyChanged(nameof(Image));
            }

            public void Dispose()
            {
                parent = null;
                Refresh();
            }
        }

        public IImagePageImageView this[int index]
        {
            get
            {
                if((uint)index < (uint)Count)
                    return imageViewCache[index];
                throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        public int Count => collection?.RecordCount ?? 0;

        public IEnumerator<IImagePageImageView> GetEnumerator()
        {
            for(int i = 0; i < Count; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if(!disposedValue)
            {
                if(disposing)
                {
                }
                if(collection != null)
                    collection.CollectionChanged -= collection_CollectionChanged;
                collection = null;
                foreach(var item in imageViewCache)
                {
                    item.Dispose();
                }
                imageViewCache = null;
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
