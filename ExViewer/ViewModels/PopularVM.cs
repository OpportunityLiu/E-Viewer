using ExClient;
using ExClient.Galleries;
using ExClient.Search;
using ExViewer.Views;
using Opportunity.MvvmUniverse;
using Opportunity.MvvmUniverse.Commands;
using Opportunity.MvvmUniverse.Views;
using System;

namespace ExViewer.ViewModels
{
    public class PopularVM : ViewModelBase
    {
        public PopularVM()
        {
            Refresh.Tag = this;
            Open.Tag = this;
        }

        public PopularCollection Galleries => Client.Current.Popular;

        public Command Refresh => Commands.GetOrAdd(() => Command.Create(sender => ((PopularVM)sender.Tag).Galleries.Clear()));

        public Command<Gallery> Open => Commands.GetOrAdd(() =>
            Command<Gallery>.Create(async (sender, g) =>
            {
                GalleryVM.GetVM(g);
                await RootControl.RootController.Navigator.NavigateAsync(typeof(GalleryPage), g.Id);
            }, (sender, g) => g != null));
    }
}
