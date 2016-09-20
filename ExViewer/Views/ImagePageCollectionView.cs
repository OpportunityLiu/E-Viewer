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
        public ImagePageCollectionView(Gallery collection)
        {
            if(collection == null)
                throw new ArgumentNullException(nameof(collection));
            this.collection = collection;
            this.array = new ImagePageImageView[collection.RecordCount];
            for(int i = 0; i < array.Length; i++)
            {
                array[i] = new ImagePageImageView(this, i);
            }
            collection.CollectionChanged += collection_CollectionChanged;
        }

        private ImagePageImageView[] array;

        private void collection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch(e.Action)
            {
            case NotifyCollectionChangedAction.Add:
                for(int i = 0; i < e.NewItems.Count; i++)
                {
                    array[e.NewStartingIndex + i].Refresh();
                }
                break;
            default:
                foreach(var item in array)
                {
                    item.Refresh();
                }
                break;
            }
        }

        private Gallery collection;

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
                    if(parent == null)
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
                return array[index];
            }
        }

        public int Count => collection.RecordCount;

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
                collection.CollectionChanged -= collection_CollectionChanged;
                collection = null;
                foreach(var item in array)
                {
                    item.Dispose();
                }
                array = null;
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
