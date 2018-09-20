using ExClient.Galleries;
using ExViewer.Views;
using Opportunity.MvvmUniverse.Commands;
using System;

namespace ExViewer.ViewModels
{
    public class CachedVM : GalleryListVM<CachedGallery>
    {
        public static CachedVM Instance { get; } = new CachedVM();

        private CachedVM()
        {
            Commands.Add(nameof(Clear), Command.Create(sender =>
            {
                var that = (CachedVM)sender.Tag;
                RootControl.RootController.TrackAsyncAction(CachedGallery.ClearCachedGalleriesAsync(), (s, e) =>
                {
                    that.Refresh.Execute();
                });
            }));
            Commands.Add(nameof(Refresh), AsyncCommand.Create(async (sender) =>
            {
                var that = (CachedVM)sender.Tag;
                that.Galleries = null;
                that.Galleries = await CachedGallery.LoadCachedGalleriesAsync();
            }));
        }
    }
}
