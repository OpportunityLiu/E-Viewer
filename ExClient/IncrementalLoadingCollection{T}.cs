using GalaSoft.MvvmLight.Threading;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Data;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;

namespace ExClient
{
    public abstract class IncrementalLoadingCollection<T> : ObservableCollection<T>, ISupportIncrementalLoading
    {
        protected IncrementalLoadingCollection(int loadedPageCount)
        {
            this.loadedPageCount = loadedPageCount;
        }

        private int rc, pc;

        public int RecordCount
        {
            get => this.rc;
            protected set => Set(ref this.rc, value, nameof(IsEmpty));
        }

        public int PageCount
        {
            get => this.pc;
            protected set => Set(ref this.pc, value, nameof(HasMoreItems), nameof(LoadedPageCount));
        }

        protected abstract IAsyncOperation<IList<T>> LoadPageAsync(int pageIndex);

        public bool IsEmpty => this.RecordCount == 0;

        private int loadedPageCount;

        protected int LoadedPageCount => this.loadedPageCount;

        public bool HasMoreItems => this.loadedPageCount < this.PageCount;

        protected void ResetAll()
        {
            this.PageCount = 0;
            this.RecordCount = 0;
            this.loadedPageCount = 0;
            Clear();
        }

        private IAsyncOperation<LoadMoreItemsResult> loading;

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            if(this.loading?.Status == AsyncStatus.Started)
            {
                var temp = this.loading;
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
            return this.loading = Run(async token =>
            {
                if(!this.HasMoreItems)
                    return new LoadMoreItemsResult();
                var lp = LoadPageAsync(this.loadedPageCount);
                IList<T> re = null;
                token.Register(lp.Cancel);
                try
                {
                    re = await lp;
                    this.AddRange(re);
                    this.loadedPageCount++;
                    RaisePropertyChanged(nameof(HasMoreItems));
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
            this.Exception = ex;
        }

        public Exception Exception
        {
            get;
        }

        public string Message => this.Exception?.Message;

        public bool Handled
        {
            get;
            set;
        }
    }
}
