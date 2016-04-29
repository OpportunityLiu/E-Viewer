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
            OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs(propertyName));
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

        protected abstract IAsyncOperation<uint> LoadPage(int pageIndex);

        public bool IsEmpty => RecordCount == 0;

        private int loadedPageCount;

        internal int LoadedPageCount => loadedPageCount;

        public bool HasMoreItems => loadedPageCount < PageCount;

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            return Run(async token =>
            {
                if(!HasMoreItems)
                    return new LoadMoreItemsResult();
                var lp = LoadPage(loadedPageCount++);
                token.Register(lp.Cancel);
                var re = await lp;
                OnPropertyChanged(nameof(HasMoreItems));
                return new LoadMoreItemsResult() { Count = re };
            });
        }
    }
}
