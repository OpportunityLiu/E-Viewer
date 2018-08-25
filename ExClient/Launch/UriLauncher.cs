using ExClient.Internal;
using System;
using System.Collections.Generic;
using Windows.Foundation;

namespace ExClient.Launch
{
    public static class UriLauncher
    {
        private static readonly UriHandler[] handlers = new UriHandler[]
        {
            SearchHandler.Instance,
            SearchCategoryHandler.Instance,
            SearchUploaderAndTagHandler.Instance,
            FavoritesSearchHandler.Instance,
            new GalleryHandler(),
            new GalleryPopupHandler(),
            new GalleryImageHandler(),
        };

        private static UriHandlerData previousData;
        private static UriHandler previousHandler;

        public static bool CanHandle(Uri uri)
        {
            if (uri is null)
            {
                return false;
            }

            if (uri.Host != DomainProvider.Eh.RootUri.Host && uri.Host != DomainProvider.Ex.RootUri.Host)
            {
                return false;
            }

            var data = new UriHandlerData(uri);
            foreach (var item in handlers)
            {
                if (item.CanHandle(data))
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
            if (data?.Uri == uri && h != null && h.CanHandle(data))
            {
                return h.HandleAsync(data);
            }
            data = new UriHandlerData(uri);
            foreach (var item in handlers)
            {
                if (item.CanHandle(data))
                {
                    return item.HandleAsync(data);
                }
            }
            throw new NotSupportedException("Unsupported uri.");
        }
    }
}
