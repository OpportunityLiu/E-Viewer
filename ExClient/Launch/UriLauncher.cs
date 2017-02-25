using System;
using System.Collections.Generic;
using Windows.Foundation;

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
            new GalleryImageHandler(),
            new FavoritesSearchHandler()
        };

        private static UriHandlerData previousData;
        private static UriHandler previousHandler;

        public static bool CanHandle(Uri uri)
        {
            if(uri == null)
                return false;
            if(uri.Host != Client.ExUri.Host && uri.Host != Client.EhUri.Host)
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
