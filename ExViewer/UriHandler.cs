using ExClient.Launch;
using ExClient.Search;
using ExViewer.ViewModels;
using ExViewer.Views;
using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.System;

namespace ExViewer
{
    internal static class UriHandler
    {
        private static Uri reform(Uri uri)
        {
            if (isPrivateProtocal(uri))
            {
                return new Uri("https://exhentai.org/" + uri.PathAndQuery.TrimStart('/', '\\', ' ', '\t'));
            }

            return uri;
        }

        private static bool isPrivateProtocal(Uri uri)
        {
            if (uri is null)
            {
                return false;
            }

            return uri.Scheme == "e-viewer-data";
        }

        public static bool CanHandleInApp(Uri uri)
        {
            if (isPrivateProtocal(uri))
            {
                return true;
            }

            return UriLauncher.CanHandle(reform(uri));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uri"></param>
        /// <returns>表示是否在应用内处理</returns>
        public static bool Handle(Uri uri)
        {
            if (uri is null)
            {
                return true;
            }

            var p = isPrivateProtocal(uri);
            uri = reform(uri);
            if (!CanHandleInApp(uri))
            {
                if (p)
                {
                    // private protocal, handled by doing nothing
                    return true;
                }

                _ = Launcher.LaunchUriAsync(uri, new LauncherOptions
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
                    {
                        var page = RootControl.RootController.Frame.Content;
                        var vm = await GalleryVM.GetVMAsync(g.GalleryInfo);
                        if (!(page is GalleryPage gPage && gPage.ViewModel.Gallery.Id == g.GalleryInfo.ID))
                        {
                            await RootControl.RootController.Navigator.NavigateAsync(typeof(GalleryPage), g.GalleryInfo.ID);
                        }
                        switch (g.Status)
                        {
                        case GalleryLaunchStatus.Image:
                            await Task.Delay(500);
                            vm.View.MoveCurrentToPosition(g.CurrentIndex);
                            await Task.Delay(500);
                            await RootControl.RootController.Navigator.NavigateAsync(typeof(ImagePage), g.GalleryInfo.ID);
                            break;
                        case GalleryLaunchStatus.Torrent:
                            await Task.Delay(500);
                            (RootControl.RootController.Frame.Content as GalleryPage)?.ChangePivotSelection(2);
                            break;
                        default:
                            await Task.Delay(500);
                            (RootControl.RootController.Frame.Content as GalleryPage)?.ChangePivotSelection(0);
                            break;
                        }
                        return;
                    }
                    case SearchLaunchResult sr:
                        switch (sr.Data)
                        {
                        case WatchedSearchResult wr:
                        {
                            var vm = SearchVM.GetVM(wr);
                            await RootControl.RootController.Navigator.NavigateAsync(typeof(WatchedPage), vm.SearchQuery);
                            return;
                        }
                        case CategorySearchResult ksr:
                        {
                            var vm = SearchVM.GetVM(ksr);
                            await RootControl.RootController.Navigator.NavigateAsync(typeof(SearchPage), vm.SearchQuery);
                            return;
                        }
                        case FavoritesSearchResult fsr:
                        {
                            var vm = FavoritesVM.GetVM(fsr);
                            await RootControl.RootController.Navigator.NavigateAsync(typeof(FavoritesPage), vm.SearchQuery);
                            return;
                        }
                        }
                        throw new InvalidOperationException();
                    case PopularLaunchResult pr:
                        await RootControl.RootController.Navigator.NavigateAsync(typeof(PopularPage));
                        return;
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
