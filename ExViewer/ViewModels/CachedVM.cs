using ExClient;
using ExClient.Galleries;
using ExViewer.Views;
using Opportunity.MvvmUniverse.Commands;
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
            this.Clear = Command.Create(() =>
            {
                RootControl.RootController.TrackAsyncAction(CachedGallery.ClearCachedGalleriesAsync(), (s, e) =>
                {
                    this.Refresh.Execute();
                });
            });
            this.Refresh = AsyncCommand.Create(async () =>
            {
                this.Galleries = null;
                this.Galleries = await CachedGallery.LoadCachedGalleriesAsync();
            });
        }
    }
}
