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
using ExClient.Search;

namespace ExViewer
{
    internal static class UriHandler
    {
        private static Uri reform(Uri uri)
        {
            if (isPrivateProtocal(uri))
                return new Uri($"https://exhentai.org/{uri.PathAndQuery.TrimStart('/', '\\', ' ', '\t')}");
            return uri;
        }

        private static bool isPrivateProtocal(Uri uri)
        {
            if (uri == null)
                return false;
            return uri.Scheme == "e-viewer-data";
        }

        public static bool CanHandleInApp(Uri uri)
        {
            if (isPrivateProtocal(uri))
                return true;
            return UriLauncher.CanHandle(reform(uri));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uri"></param>
        /// <returns>表示是否在应用内处理</returns>
        public static bool Handle(Uri uri)
        {
            if (uri == null)
                return true;
            var p = isPrivateProtocal(uri);
            uri = reform(uri);
            if (!CanHandleInApp(uri))
            {
                if (p)
                    // private protocal, handled by doing nothing
                    return true;
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
                    switch (r)
                    {
                        case GalleryLaunchResult g:
                            var page = RootControl.RootController.Frame.Content;
                            if (!(page is GalleryPage gPage && gPage.VM.Gallery.ID == g.GalleryInfo.ID))
                            {
                                await GalleryVM.GetVMAsync(g.GalleryInfo);
                                RootControl.RootController.Frame.Navigate(typeof(GalleryPage), g.GalleryInfo.ID);
                                await Task.Delay(500);
                            }
                            switch (g.Status)
                            {
                                case GalleryLaunchStatus.Image:
                                    RootControl.RootController.Frame.Navigate(typeof(ImagePage), g.GalleryInfo.ID);
                                    await Task.Delay(500);
                                    (RootControl.RootController.Frame.Content as ImagePage)?.SetImageIndex(g.CurrentIndex - 1);
                                    break;
                                case GalleryLaunchStatus.Torrent:
                                    (RootControl.RootController.Frame.Content as GalleryPage)?.ChangePivotSelection(2);
                                    break;
                                default:
                                    (RootControl.RootController.Frame.Content as GalleryPage)?.ChangePivotSelection(0);
                                    break;
                            }
                            return;
                        case SearchLaunchResult sr:
                            switch (sr.Data)
                            {
                                case CategorySearchResult ksr:
                                    var vm = SearchVM.GetVM(ksr);
                                    RootControl.RootController.Frame.Navigate(typeof(SearchPage), vm.SearchQuery.ToString());
                                    return;
                                case FavoritesSearchResult fsr:
                                    var fvm = FavoritesVM.GetVM(fsr);
                                    RootControl.RootController.Frame.Navigate(typeof(FavoritesPage), fvm.SearchQuery.ToString());
                                    return;
                            }
                            throw new InvalidOperationException();
                    }
                }
                catch (Exception e)
                {
                    RootControl.RootController.SendToast(e, null);
                }
            });
        }
    }
}
