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
                this.CachedGalleries = new ObservableCollection<Gallery>(await CachedGallery.LoadCachedGalleriesAsync());
            });
            Clear = new RelayCommand(() => RootControl.RootController.TrackAsyncAction(CachedGallery.ClearCachedGalleriesAsync(), (s, e) => Refresh.Execute(null)));
            Delete = new RelayCommand<Gallery>(async g =>
            {
                await g.DeleteAsync();
                this.CachedGalleries?.Remove(g);
                RootControl.RootController.SendToast("Refreshed");
            });
            Open = new RelayCommand<Gallery>(g =>
            {
                GalleryVM.AddGallery(g);
                RootControl.RootController.Frame.Navigate(typeof(GalleryPage), g.Id);
            });
        }

        public RelayCommand Refresh
        {
            get;
        }

        public RelayCommand Clear
        {
            get;
        }

        public RelayCommand<Gallery> Delete
        {
            get;
        }

        public RelayCommand<Gallery> Open
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
