using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExClient;
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
            var h = UriLauncher.HandleAsync(uri);
            RootControl.RootController.TrackAsyncAction(h, (s, e) =>
            {
                switch(e)
                {
                case Windows.Foundation.AsyncStatus.Completed:
                    var r = s.GetResults();
                    var vm = GalleryVM.AddGallery(r.Item1);
                    RootControl.RootController.Frame.Navigate(typeof(GalleryPage), r.Item1.Id);
                    if(r.Item2 > 0)
                    {
                        vm.CurrentIndex = r.Item2 - 1;
                        RootControl.RootController.Frame.Navigate(typeof(ImagePage), r.Item1.Id);
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
