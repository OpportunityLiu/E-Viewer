using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExClient.Launch;
using ExViewer.Views;
using ExViewer.ViewModels;
using Windows.System;
using Windows.Foundation;
using System.Runtime.InteropServices.WindowsRuntime;

namespace ExViewer
{
    internal static class UriHandler
    {
        public static bool CanHandleInApp(Uri uri)
        {
            return UriLauncher.CanHandle(uri);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uri"></param>
        /// <returns>表示是否在应用内处理</returns>
        public static bool Handle(Uri uri)
        {
            if(uri == null)
                return true;
            if(!CanHandleInApp(uri))
            {
                var ignore = Launcher.LaunchUriAsync(uri, new LauncherOptions
                {
                    DesiredRemainingView = Windows.UI.ViewManagement.ViewSizePreference.UseMore,
                    IgnoreAppUriHandlers = true,
                    TreatAsUntrusted = false
                });
                return false;
            }
            RootControl.RootController.TrackAsyncAction(handle(uri));
            return true;
        }

        private static IAsyncAction handle(Uri uri)
        {
            return AsyncInfo.Run(async token =>
            {
                try
                {
                    var r = await UriLauncher.HandleAsync(uri);
                    switch(r)
                    {
                    case GalleryLaunchResult g:
                        var page = RootControl.RootController.Frame.Content;
                        if(!(page is GalleryPage gPage && gPage.VM.Gallery.Id == g.GalleryInfo.Id))
                        {
                            await GalleryVM.GetVMAsync(g.GalleryInfo);
                            RootControl.RootController.Frame.Navigate(typeof(GalleryPage), g.GalleryInfo.Id);
                            await Task.Delay(500);
                        }
                        switch(g.Status)
                        {
                        case GalleryLaunchStatus.Default:
                            (RootControl.RootController.Frame.Content as GalleryPage)?.ChangePivotSelection(0);
                            break;
                        case GalleryLaunchStatus.Image:
                            RootControl.RootController.Frame.Navigate(typeof(ImagePage), g.GalleryInfo.Id);
                            await Task.Delay(500);
                            (RootControl.RootController.Frame.Content as ImagePage)?.SetImageIndex(g.CurrentIndex - 1);
                            break;
                        case GalleryLaunchStatus.Torrent:
                            (RootControl.RootController.Frame.Content as GalleryPage)?.ChangePivotSelection(2);
                            break;
                        }
                        return;
                    case SearchLaunchResult sr:
                        var vm = SearchVM.GetVM(sr.Data);
                        RootControl.RootController.Frame.Navigate(typeof(SearchPage), vm.SearchQuery);
                        return;
                    case FavoritesSearchLaunchResult fr:
                        var fvm = FavoritesVM.GetVM(fr.Data);
                        RootControl.RootController.Frame.Navigate(typeof(FavoritesPage), fvm.SearchQuery);
                        return;
                    }
                }
                catch(Exception e)
                {
                    RootControl.RootController.SendToast(e, null);
                }
            });
        }
    }
}
