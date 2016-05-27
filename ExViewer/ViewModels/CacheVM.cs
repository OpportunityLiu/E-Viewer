using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using ExClient;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using System.Collections.ObjectModel;
using ExViewer.Views;

namespace ExViewer.ViewModels
{
    public class CacheVM : ViewModelBase
    {
        public CacheVM()
        {
            Refresh = new RelayCommand(async () =>
            {
                this.CachedGalleries = null;
                this.CachedGalleries = new ObservableCollection<Gallery>(await CachedGallery.GetCachedGalleriesAsync());
            });
            Clear = new RelayCommand(() => RootControl.RootController.TrackAsyncAction(CachedGallery.ClearCachedGalleriesAsync(), (s, e) => Refresh.Execute(null)));
            Delete = new RelayCommand<Gallery>(async g =>
            {
                await g.DeleteAsync();
                this.CachedGalleries?.Remove(g);
            });
        }

        public ICommand Refresh
        {
            get;
        }

        public ICommand Clear
        {
            get;
        }

        public ICommand Delete
        {
            get;
        }

        private ObservableCollection<Gallery> cachedGalleries;

        public ObservableCollection<Gallery> CachedGalleries
        {
            get
            {
                return cachedGalleries;
            }
            private set
            {
                Set(ref cachedGalleries, value);
            }
        }
    }
}
