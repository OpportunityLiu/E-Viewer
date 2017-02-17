using ExClient;
using ExViewer.Views;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExViewer.ViewModels
{
    public abstract class GalleryListVM<T> : ViewModelBase
         where T : Gallery
    {
        protected GalleryListVM()
        {
            Delete = new RelayCommand<Gallery>(async g =>
            {
                await g.DeleteAsync();
                this.Galleries?.Remove(g);
                RootControl.RootController.SendToast(LocalizedStrings.Resources.GalleryDeleted, typeof(SavedPage));
            });
            Open = new RelayCommand<Gallery>(g =>
            {
                GalleryVM.GetVM(g);
                RootControl.RootController.Frame.Navigate(typeof(GalleryPage), g.Id);
            });
        }

        private GalleryList<T> galleries;

        public GalleryList<T> Galleries
        {
            get
            {
                return galleries;
            }
            protected set
            {
                Set(ref galleries, value);
            }
        }

        public RelayCommand Refresh
        {
            get;
            protected set;
        }

        public RelayCommand Clear
        {
            get;
            protected set;
        }

        public RelayCommand<Gallery> Delete
        {
            get;
        }

        public RelayCommand<Gallery> Open
        {
            get;
        }
    }
}
