using ExClient;
using ExClient.Galleries;
using ExViewer.Views;
using Opportunity.MvvmUniverse;
using Opportunity.MvvmUniverse.Collections;
using Opportunity.MvvmUniverse.Commands;
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
            this.Delete = new Command<Gallery>(async g =>
            {
                await g.DeleteAsync();
                this.Galleries?.Remove(g);
                RootControl.RootController.SendToast(Strings.Resources.Views.CachedPage.GalleryDeleted, null);
            }, g => g != null);
            this.Open = new Command<Gallery>(g =>
            {
                GalleryVM.GetVM(g);
                RootControl.RootController.Frame.Navigate(typeof(GalleryPage), g.Id);
            }, g => g != null);
        }

        private ObservableCollection<Gallery> galleries;

        public ObservableCollection<Gallery> Galleries
        {
            get => this.galleries;
            protected set => Set(ref this.galleries, value);
        }

        public AsyncCommand Refresh
        {
            get;
            protected set;
        }

        public Command Clear
        {
            get;
            protected set;
        }

        public Command<Gallery> Delete
        {
            get;
        }

        public Command<Gallery> Open
        {
            get;
        }
    }
}
