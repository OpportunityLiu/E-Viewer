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
using Windows.Foundation.Collections;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using System.Collections.Specialized;

namespace ExClient
{
    public class CachedGallery : Gallery
    {
        private class CachedGalleryList : ObservableCollection<CachedGallery>, ICollectionView,ICollectionViewFactory
        {
            private static int pageSize = 20;

            public CachedGalleryList()
            {
                using(var db = CachedGalleryDb.Create())
                {
                    for(int i = 0; i < db.CacheSet.Count(); i++)
                    {
                        this.Add(null);
                    }
                }
            }

            protected IAsyncAction LoadPageAsync(int startIndex)
            {
                return Task.Run(() =>
                {
                    using(var db = CachedGalleryDb.Create())
                    {
                        var query = db.CacheSet.Skip(startIndex).Take(pageSize).Select(c => new
                        {
                            c,
                            c.Gallery
                        }).ToList();
                        var toAdd = query.Select(a =>
                        {
                            var c = new CachedGallery(a.Gallery, a.c);
                            var ignore = c.InitAsync();
                            return c;
                        });
                        foreach(var item in toAdd)
                        {
                            if(this[startIndex] == null)
                            {
                                this[startIndex] = item;
                            }
                            startIndex++;
                        }
                    }
                }).AsAsyncAction();
            }

            protected void Set<TProp>(ref TProp field, TProp value, [CallerMemberName]string propertyName = null)
            {
                if(Equals(field, value))
                    return;
                field = value;
                OnPropertyChanged(propertyName);
            }

            protected void OnPropertyChanged([CallerMemberName]string propertyName = null)
            {
                OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
            }

            protected override void OnPropertyChanged(PropertyChangedEventArgs e)
            {
                DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    base.OnPropertyChanged(e);
                });
            }

            protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
            {
                DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    base.OnCollectionChanged(e);
                    VectorChanged?.Invoke(this, new VectorChangedEventArgs(e));
                });
            }

            private class VectorChangedEventArgs : IVectorChangedEventArgs
            {
                public VectorChangedEventArgs(NotifyCollectionChangedEventArgs args)
                {
                    switch(args.Action)
                    {
                    case NotifyCollectionChangedAction.Add:
                        CollectionChange = CollectionChange.ItemInserted;
                        break;
                    case NotifyCollectionChangedAction.Replace:
                    case NotifyCollectionChangedAction.Move:
                        CollectionChange = CollectionChange.ItemChanged;
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        CollectionChange = CollectionChange.ItemRemoved;
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        CollectionChange = CollectionChange.Reset;
                        break;
                    }
                    Index = (uint)args.NewStartingIndex;
                }

                public CollectionChange CollectionChange
                {
                    get;
                }

                public uint Index
                {
                    get;
                }
            }

            public event EventHandler<object> CurrentChanged;
            public event CurrentChangingEventHandler CurrentChanging;
            public event VectorChangedEventHandler<object> VectorChanged;

            public IObservableVector<object> CollectionGroups
            {
                get
                {
                    return null;
                }
            }

            public object CurrentItem
            {
                get
                {
                    if(currentPosition < 0 || currentPosition >= this.Count)
                        return null;
                    return this[CurrentPosition];
                }
            }

            private int currentPosition = -1;

            public int CurrentPosition
            {
                get
                {
                    return currentPosition;
                }
            }

            protected bool SetCurrentPosition(int value)
            {
                var changingArgs = new CurrentChangingEventArgs();
                CurrentChanging?.Invoke(this, changingArgs);
                if(changingArgs.Cancel)
                    return false;
                Set(ref currentPosition, value, nameof(CurrentPosition));
                OnPropertyChanged(nameof(CurrentItem));
                CurrentChanged?.Invoke(this, CurrentItem);
                if(CurrentItem == null)
                {
                    var ignore = LoadPageAsync(value);
                }
                return true;
            }

            bool ICollectionView.HasMoreItems => false;

            public bool IsCurrentAfterLast => false;

            public bool IsCurrentBeforeFirst => false;

            public bool IsReadOnly => false;

            object IList<object>.this[int index]
            {
                get
                {
                    return this[index];
                }
                set
                {
                    this[index] = (CachedGallery)value;
                }
            }

            public bool MoveCurrentTo(object item)
            {
                var i = IndexOf((CachedGallery)item);
                if(i == -1)
                    return false;
                return SetCurrentPosition(i);
            }

            public bool MoveCurrentToPosition(int index)
            {
                return SetCurrentPosition(index);
            }

            public bool MoveCurrentToFirst()
            {
                return SetCurrentPosition(0);
            }

            public bool MoveCurrentToLast()
            {
                return SetCurrentPosition(Count - 1);
            }

            public bool MoveCurrentToNext()
            {
                if(currentPosition == Count - 1)
                    return false;
                return SetCurrentPosition(currentPosition + 1);
            }

            public bool MoveCurrentToPrevious()
            {
                if(currentPosition == 0)
                    return false;
                return SetCurrentPosition(currentPosition - 1);
            }

            IAsyncOperation<LoadMoreItemsResult> ICollectionView.LoadMoreItemsAsync(uint count)
            {
                throw new NotImplementedException();
            }

            int IList<object>.IndexOf(object item)
            {
                return IndexOf((CachedGallery)item);
            }

            void IList<object>.Insert(int index, object item)
            {
                Insert(index, (CachedGallery)item);
            }

            void ICollection<object>.Add(object item)
            {
                Add((CachedGallery)item);
            }

            bool ICollection<object>.Contains(object item)
            {
                return Contains((CachedGallery)item);
            }

            void ICollection<object>.CopyTo(object[] array, int arrayIndex)
            {
                CopyTo((CachedGallery[])array, arrayIndex);
            }

            bool ICollection<object>.Remove(object item)
            {
                return Remove((CachedGallery)item);
            }

            IEnumerator<object> IEnumerable<object>.GetEnumerator()
            {
                return GetEnumerator();
            }

            public ICollectionView CreateView()
            {
                return this;
            }
        }

        private class CachedGalleryFactory : ICollectionViewFactory
        {
            private CachedGalleryList view = new CachedGalleryList();

            public static IAsyncOperation<ICollectionViewFactory> CreateAsync()
            {
                return Task.Run(() =>
                {
                    return (ICollectionViewFactory)new CachedGalleryList();
                }).AsAsyncOperation();
            }

            private CachedGalleryFactory()
            {
            }

            public ICollectionView CreateView()
            {
                if(view == null)
                    return new CachedGalleryList();
                else
                {
                    var temp = view;
                    view = null;
                    return temp;
                }
            }
        }

        private static int pageSize = 20;

        public static IAsyncOperation<ICollectionViewFactory> LoadCachedGalleriesAsync()
        {
            return Run(async token => (ICollectionViewFactory)await CachedGalleryFactory.CreateAsync());
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
                using(var db = CachedGalleryDb.Create())
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
            this.ThumbFile = cacheModel.ThumbData;
            this.PageCount = (int)Math.Ceiling(RecordCount / (double)pageSize);
            this.Owner = Client.Current;
        }

        public override IAsyncAction InitAsync()
        {
            return DispatcherHelper.RunAsync(async () =>
            {
                using(var stream = ThumbFile.AsBuffer().AsRandomAccessStream())
                {
                    await Thumb.SetSourceAsync(stream);
                }
            });
        }

        private List<ImageModel> imageModels;

        protected byte[] ThumbFile
        {
            get;
            private set;
        }

        private void loadImageModel()
        {
            if(imageModels != null)
                return;
            using(var db = CachedGalleryDb.Create())
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
                using(var db = CachedGalleryDb.Create())
                {
                    db.CacheSet.Remove(db.CacheSet.Single(c => c.GalleryId == this.Id));
                    await db.SaveChangesAsync();
                }
                await base.DeleteAsync();
            }).AsAsyncAction();
        }
    }
}
