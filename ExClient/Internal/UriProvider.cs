using System;

namespace ExClient.Internal
{
    internal class UriProvider
    {
        public Uri RootUri { get; }
        public Uri ApiUri { get; }

        private UriProvider(string root, string api)
        {
            RootUri = new Uri(root);
            ApiUri = new Uri(api);
        }

        public static UriProvider Ex { get; } = new UriProvider("https://exhentai.org/", "https://exhentai.org/api.php");
        public static UriProvider Eh { get; } = new UriProvider("https://e-hentai.org/", "https://api.e-hentai.org/api.php");
    }
}
