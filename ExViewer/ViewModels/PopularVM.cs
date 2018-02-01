using ExClient;
using ExClient.Galleries;
using ExClient.Search;
using ExViewer.Views;
using Opportunity.MvvmUniverse;
using Opportunity.MvvmUniverse.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ExViewer.ViewModels
{
    public class PopularVM : ViewModelBase
    {
        public PopularVM()
        {
            this.Refresh.Tag = this;
            this.Open.Tag = this;
        }

        public PopularCollection Galleries { get; } = new PopularCollection();

        public Command Refresh { get; } = Command.Create(sender => ((PopularVM)sender.Tag).Galleries.Clear());

        public Command<Gallery> Open { get; } = Command.Create<Gallery>((sender, g) =>
        {
            GalleryVM.GetVM(g);
            RootControl.RootController.Frame.Navigate(typeof(GalleryPage), g.ID);
        }, (sender, g) => g != null);
    }
}
