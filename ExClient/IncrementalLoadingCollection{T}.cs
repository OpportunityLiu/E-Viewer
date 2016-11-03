using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Data;
using Windows.Foundation;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;
using System.Runtime.CompilerServices;
using GalaSoft.MvvmLight.Threading;
using System.ComponentModel;
using System.Collections.Specialized;
using Windows.UI.Xaml;
using System.Collections;

namespace ExClient
{
    public abstract class IncrementalLoadingCollection<T> : ObservableCollection<T>, ISupportIncrementalLoading
    {
        protected IncrementalLoadingCollection(int loadedPageCount)
        {
            this.loadedPageCount = loadedPageCount;
        }

        protected void Set<TProp>(ref TProp field, TProp value, [CallerMemberName]string propertyName = null)
        {
            if(Equals(field, value))
                return;
            field = value;
            OnPropertyChanged(propertyName);
        }

        /// <summary>
        /// Add items into collection.
        /// </summary>
        /// <param name="items">Items to add.</param>
        /// <returns>Count of added items.</returns>
        public int AddRange(IEnumerable<T> items)
        {
            CheckReentrancy();
            var count = 0;
            foreach(var item in items)
            {
                this.Items.Add(item);
                count++;
            }
            if(count == 0)
                return 0;
            var startingIndex = this.Count - count;
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new AddRangeInfo(this, startingIndex, count), startingIndex));
            OnPropertyChanged(nameof(Count));
            OnPropertyChanged("Item[]");
            return count;
        }

        private class AddRangeInfo : IList
        {
            private int count;
            private IncrementalLoadingCollection<T> parent;
            private int startingIndex;

            public AddRangeInfo(IncrementalLoadingCollection<T> parent, int startingIndex, int count)
            {
                this.parent = parent;
                this.startingIndex = startingIndex;
                this.count = count;
            }

            public object this[int index]
            {
                get
                {
                    if((uint)index > (uint)count)
                        throw new ArgumentOutOfRangeException(nameof(index));
                    return parent[startingIndex + index];
                }
                set
                {
                    throw new InvalidOperationException();
                }
            }

            public int Count => count;

            public bool IsFixedSize => true;

            public bool IsReadOnly => true;

            public bool IsSynchronized => false;

            public object SyncRoot => null;

            public int Add(object value)
            {
                throw new InvalidOperationException();
            }

            public void Clear()
            {
                throw new InvalidOperationException();
            }

            public bool Contains(object value)
            {
                foreach(var item in this)
                {
                    if(item == value)
                        return true;
                }
                return false;
            }

            public void CopyTo(Array array, int index)
            {
                for(int i = 0; i < count; i++)
                {
                    array.SetValue(this[i], i);
                }
            }

            public IEnumerator GetEnumerator()
            {
                for(int i = 0; i < count; i++)
                {
                    yield return this[i];
                }
            }

            public int IndexOf(object value)
            {
                for(int i = 0; i < count; i++)
                {
                    if(this[i] == value)
                        return i;
                }
                return -1;
            }

            public void Insert(int index, object value)
            {
                throw new InvalidOperationException();
            }

            public void Remove(object value)
            {
                throw new InvalidOperationException();
            }

            public void RemoveAt(int index)
            {
                throw new InvalidOperationException();
            }
        }

        protected void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected sealed override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                base.OnPropertyChanged(e);
            });
        }

        protected sealed override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                base.OnCollectionChanged(e);
            });
        }

        private int rc, pc;

        public int RecordCount
        {
            get
            {
                return rc;
            }
            protected set
            {
                Set(ref rc, value);
                OnPropertyChanged(nameof(IsEmpty));
            }
        }

        public int PageCount
        {
            get
            {
                return pc;
            }
            protected set
            {
                Set(ref pc, value);
                OnPropertyChanged(nameof(HasMoreItems));
            }
        }

        protected abstract IAsyncOperation<IList<T>> LoadPageAsync(int pageIndex);

        public bool IsEmpty => RecordCount == 0;

        private int loadedPageCount;

        protected int LoadedPageCount => loadedPageCount;

        public bool HasMoreItems => loadedPageCount < PageCount;

        protected void ResetAll()
        {
            PageCount = 0;
            RecordCount = 0;
            loadedPageCount = 0;
            Clear();
        }

        private IAsyncOperation<LoadMoreItemsResult> loading;

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            if(loading?.Status == AsyncStatus.Started)
            {
                var temp = loading;
                return Run(async token =>
                {
                    token.Register(temp.Cancel);
                    while(temp.Status == AsyncStatus.Started)
                    {
                        await Task.Delay(200);
                    }
                    switch(temp.Status)
                    {
                    case AsyncStatus.Completed:
                        return temp.GetResults();
                    case AsyncStatus.Error:
                        throw temp.ErrorCode;
                    default:
                        token.ThrowIfCancellationRequested();
                        throw new OperationCanceledException(token);
                    }
                });
            }
            return loading = Run(async token =>
            {
                if(!HasMoreItems)
                    return new LoadMoreItemsResult();
                var lp = LoadPageAsync(loadedPageCount);
                IList<T> re = null;
                token.Register(lp.Cancel);
                try
                {
                    re = await lp;
                    this.AddRange(re);
                    loadedPageCount++;
                    OnPropertyChanged(nameof(HasMoreItems));
                }
                catch(Exception ex)
                {
                    if(!await tryHandle(ex))
                        throw;
                }
                return new LoadMoreItemsResult() { Count = re == null ? 0u : (uint)re.Count };
            });
        }

        public event TypedEventHandler<IncrementalLoadingCollection<T>, LoadMoreItemsExceptionEventArgs> LoadMoreItemsException;

        private async Task<bool> tryHandle(Exception ex)
        {
            var temp = LoadMoreItemsException;
            if(temp == null)
                return false;
            var h = false;
            await DispatcherHelper.RunAsync(() =>
            {
                var args = new LoadMoreItemsExceptionEventArgs(ex);
                temp(this, args);
                h = args.Handled;
            });
            return h;
        }
    }

    public class LoadMoreItemsExceptionEventArgs : EventArgs
    {
        internal LoadMoreItemsExceptionEventArgs(Exception ex)
        {
            Exception = ex;
        }

        public Exception Exception
        {
            get;
        }

        public string Message => Exception?.Message;

        public bool Handled
        {
            get;
            set;
        }
    }
}
