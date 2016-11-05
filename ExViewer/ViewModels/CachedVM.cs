using ExClient;
using ExViewer.Views;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExViewer.ViewModels
{
    public class CachedVM : GalleryListVM<CachedGallery>
    {
        public static CachedVM Instance
        {
            get;
        } = new CachedVM();

        private CachedVM()
        {
            Clear = new RelayCommand(() =>
            {
                RootControl.RootController.TrackAsyncAction(CachedGallery.ClearCachedGalleriesAsync(), (s, e) =>
                {
                    Refresh.Execute(null);
                });
            });
            Refresh = new RelayCommand(async () =>
            {
                this.Galleries = null;
                this.Refresh.RaiseCanExecuteChanged();
                this.Galleries = await CachedGallery.LoadCachedGalleriesAsync();
                this.Refresh.RaiseCanExecuteChanged();
            });
        }
    }
}
