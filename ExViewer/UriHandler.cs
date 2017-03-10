using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExClient.Launch;
using ExViewer.Views;
using ExViewer.ViewModels;
using Windows.System;

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
            RootControl.RootController.TrackAsyncAction(UriLauncher.HandleAsync(uri), async (s, e) =>
            {
                switch(e)
                {
                case Windows.Foundation.AsyncStatus.Completed:
                    var r = s.GetResults();
                    switch(r)
                    {
                    case GalleryLaunchResult g:
                        GalleryVM.GetVM(g.Gallery);
                        RootControl.RootController.Frame.Navigate(typeof(GalleryPage), g.Gallery.Id);
                        await Task.Delay(200);
                        switch(g.Status)
                        {
                        case GalleryLaunchStatus.Default:
                            break;
                        case GalleryLaunchStatus.Image:
                            RootControl.RootController.Frame.Navigate(typeof(ImagePage), g.Gallery.Id);
                            await Task.Delay(200);
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
                    }
                    break;
                case Windows.Foundation.AsyncStatus.Error:
                    RootControl.RootController.SendToast(s.ErrorCode, null);
                    break;
                }
            });
            return true;
        }
    }
}
