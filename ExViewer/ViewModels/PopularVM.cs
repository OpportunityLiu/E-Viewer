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
            this.Refresh.Tag = this;
            this.Open.Tag = this;
        }

        public PopularCollection Galleries => Client.Current.Popular;

        public Command Refresh { get; } = Command.Create(sender => ((PopularVM)sender.Tag).Galleries.Clear());

        public Command<Gallery> Open { get; } = Command.Create<Gallery>(async (sender, g) =>
        {
            GalleryVM.GetVM(g);
            await RootControl.RootController.Navigator.NavigateAsync(typeof(GalleryPage), g.ID);
        }, (sender, g) => g != null);
    }
}
