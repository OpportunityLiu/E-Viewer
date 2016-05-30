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

        protected abstract IAsyncOperation<uint> LoadPageAsync(int pageIndex);

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
                    return await temp;
                });
            }
            return loading = Run(async token =>
            {
                if(!HasMoreItems)
                    return new LoadMoreItemsResult();
                var lp = LoadPageAsync(loadedPageCount);
                uint re = 0;
                token.Register(lp.Cancel);
                try
                {
                    re = await lp;
                    loadedPageCount++;
                    OnPropertyChanged(nameof(HasMoreItems));
                }
                catch(Exception ex)
                {
                    raiseLoadMoreItemsException(ex);
                }
                return new LoadMoreItemsResult() { Count = re };
            });
        }

        public event TypedEventHandler<IncrementalLoadingCollection<T>, LoadMoreItemsExceptionEventArgs> LoadMoreItemsException;

        private void raiseLoadMoreItemsException(Exception ex)
        {
            var temp = LoadMoreItemsException;
            if(temp == null)
                throw new InvalidOperationException($"LoadMoreItemsException did not handled in {{{this}}}.", ex);
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                var args = new LoadMoreItemsExceptionEventArgs(ex);
                temp(this, args);
                if(!args.Handled)
                    throw new InvalidOperationException($"LoadMoreItemsException did not handled in {{{this}}}.", ex);
            });
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
