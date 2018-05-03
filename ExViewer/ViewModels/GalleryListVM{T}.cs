using ExClient.Galleries;
using ExViewer.Views;
using Opportunity.MvvmUniverse;
using Opportunity.MvvmUniverse.Collections;
using Opportunity.MvvmUniverse.Commands;
using Opportunity.MvvmUniverse.Views;
using System;
using System.Collections.Generic;
using ICommand = System.Windows.Input.ICommand;

namespace ExViewer.ViewModels
{
    public abstract class GalleryListVM<T> : ViewModelBase
         where T : Gallery
    {
        protected GalleryListVM()
        {
        }

        private ObservableList<Gallery> galleries;
        public ObservableList<Gallery> Galleries
        {
            get => this.galleries;
            protected set => Set(ref this.galleries, value);
        }

        public AsyncCommand Refresh => Commands.Get<AsyncCommand>();

        public Command Clear => Commands.Get<Command>();

        public Command<Gallery> Delete => Commands.GetOrAdd(() =>
            Command<Gallery>.Create(async (sender, g) =>
            {
                GalleryVM.RemoveVM(g.ID);
                await g.DeleteAsync();
                ((GalleryListVM<T>)sender.Tag).Galleries?.Remove(g);
                RootControl.RootController.SendToast(Strings.Resources.Views.CachedPage.GalleryDeleted, null);
            }, (sender, g) => g != null));

        public Command<Gallery> Open => Commands.GetOrAdd(() =>
            Command<Gallery>.Create(async (sender, g) =>
            {
                GalleryVM.GetVM(g);
                await RootControl.RootController.Navigator.NavigateAsync(typeof(GalleryPage), g.ID);
            }, (sender, g) => g != null));
    }
}
