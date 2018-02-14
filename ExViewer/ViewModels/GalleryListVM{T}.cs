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
            this.Delete.Tag = this;
            this.Open.Tag = this;
        }

        private ObservableList<Gallery> galleries;
        public ObservableList<Gallery> Galleries
        {
            get => this.galleries;
            protected set => Set(ref this.galleries, value);
        }

        public abstract AsyncCommand Refresh { get; }

        public abstract Command Clear { get; }

        public virtual Command<Gallery> Delete { get; } = Command.Create<Gallery>(async (sender, g) =>
        {
            await g.DeleteAsync();
            ((GalleryListVM<T>)sender.Tag).Galleries?.Remove(g);
            RootControl.RootController.SendToast(Strings.Resources.Views.CachedPage.GalleryDeleted, null);
        }, (sender, g) => g != null);

        public virtual Command<Gallery> Open { get; } = Command.Create<Gallery>(async (sender, g) =>
        {
            GalleryVM.GetVM(g);
            await RootControl.RootController.Navigator.NavigateAsync(typeof(GalleryPage), g.ID);
        }, (sender, g) => g != null);
    }
}
