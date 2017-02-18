using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;
using System.Reflection;

namespace ExClient.Launch
{
    public static class UriLauncher
    {
        private static IReadOnlyList<UriHandler> handlers = new UriHandler[]
        {
            new SearchHandler(),
            new SearchCategoryHandler(),
            new SearchUploaderAndTagHandler(),
            new GalleryHandler(),
            new GalleryTorrentHandler(),
            new GalleryImageHandler()
        };

        private static UriHandlerData previousData;
        private static UriHandler previousHandler;

        public static bool CanHandle(Uri uri)
        {
            if(uri == null)
                return false;
            if(uri.Host != Client.RootUri.Host && uri.Host != Client.EhUri.Host)
                return false;
            var data = new UriHandlerData(uri);
            foreach(var item in handlers)
            {
                if(item.CanHandle(data))
                {
                    previousData = data;
                    previousHandler = item;
                    return true;
                }
            }
            return false;
        }

        public static IAsyncOperation<LaunchResult> HandleAsync(Uri uri)
        {
            var data = previousData;
            var h = previousHandler;
            if(data?.Uri == uri && h != null && h.CanHandle(data))
            {
                return h.HandleAsync(data);
            }
            data = new UriHandlerData(uri);
            foreach(var item in handlers)
            {
                if(item.CanHandle(data))
                    return item.HandleAsync(data);
            }
            throw new NotSupportedException("Unsupported uri.");
        }
    }
}
