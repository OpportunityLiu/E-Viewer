using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExClient.Internal
{
    class UriProvieder
    {
        public Uri RootUri { get; }
        public Uri ApiUri { get; }

        private UriProvieder(string root, string api)
        {
            RootUri = new Uri(root);
            ApiUri = new Uri(api);
        }

        public static UriProvieder Ex { get; } = new UriProvieder("https://exhentai.org/", "https://exhentai.org/api.php");
        public static UriProvieder Eh { get; } = new UriProvieder("https://e-hentai.org/", "https://api.e-hentai.org/api.php");
    }
}
