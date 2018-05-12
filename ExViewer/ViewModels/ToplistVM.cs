using ExClient.Galleries;
using ExClient.Search;
using ExClient.Status;
using ExViewer.Views;
using Opportunity.MvvmUniverse.Commands;
using Opportunity.MvvmUniverse.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExViewer.ViewModels
{
    public sealed class ToplistVM : ViewModelBase
    {
        public IReadOnlyList<GalleryToplist> Toplists { get; } = new List<GalleryToplist>
        {
            new GalleryToplist(ToplistName.GalleriesYesterday),
            new GalleryToplist(ToplistName.GalleriesPastMonth),
            new GalleryToplist(ToplistName.GalleriesPastYear),
            new GalleryToplist(ToplistName.GalleriesAllTime),
        };

        public Command<Gallery> Open => Commands.GetOrAdd(() =>
            Command<Gallery>.Create(async (sender, g) =>
            {
                GalleryVM.GetVM(g);
                await RootControl.RootController.Navigator.NavigateAsync(typeof(GalleryPage), g.ID);
            }, (sender, g) => g != null));
        public Command<GalleryToplist> Refresh => Commands.GetOrAdd(() => Command<GalleryToplist>.Create((sender, gt) =>
            {
                gt.Clear();
            }, (sender, gt) => gt != null));
    }
}
