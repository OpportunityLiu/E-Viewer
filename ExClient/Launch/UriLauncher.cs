using ExClient.Internal;

using System;
using System.Threading.Tasks;

namespace ExClient.Launch
{
    public static class UriLauncher
    {
        private static readonly UriHandler[] handlers = new UriHandler[]
        {
            SearchHandler.Instance,
            WatchedHandler.Instance,
            SearchCategoryHandler.Instance,
            SearchUploaderAndTagHandler.Instance,
            FavoritesSearchHandler.Instance,
            new GalleryHandler(),
            new GalleryPopupHandler(),
            new GalleryImageHandler(),
        };

        private static UriHandlerData _PreviousData;
        private static UriHandler _PreviousHandler;

        private static void _RewriteUri(ref Uri uri)
        {
            if (uri.Host == "lofi.e-hentai.org")
                uri = new Uri("https://" + DomainProvider.Eh.RootUri.Host + uri.PathAndQuery + uri.Fragment);
            if (uri.PathAndQuery.StartsWith("/lofi/"))
                uri = new Uri("https://" + uri.Host + uri.PathAndQuery.Substring("/lofi".Length) + uri.Fragment);
        }

        public static bool CanHandle(Uri uri)
        {
            if (uri is null)
                return false;
            _RewriteUri(ref uri);
            if (uri.Host != DomainProvider.Eh.RootUri.Host && uri.Host != DomainProvider.Ex.RootUri.Host)
                return false;

            var data = new UriHandlerData(uri);
            foreach (var item in handlers)
            {
                if (item.CanHandle(data))
                {
                    _PreviousData = data;
                    _PreviousHandler = item;
                    return true;
                }
            }
            return false;
        }

        public static Task<LaunchResult> HandleAsync(Uri uri)
        {
            if (uri is null)
                throw new ArgumentNullException(nameof(uri));
            _RewriteUri(ref uri);
            var data = _PreviousData;
            var h = _PreviousHandler;
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
