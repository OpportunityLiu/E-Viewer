using ExClient.Internal;
using ExClient.Settings;
using System;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

namespace ExClient
{
    public partial class Client : IDisposable
    {
        public static Client Current { get; } = new Client();

        private Client()
        {
            var httpFilter = new HttpBaseProtocolFilter { AllowAutoRedirect = false };
            httpFilter.CacheControl.WriteBehavior = HttpCacheWriteBehavior.NoCache;
            this.CookieManager = httpFilter.CookieManager;
            this.HttpClient = new MyHttpClient(this, new HttpClient(new RedirectFilter(httpFilter)));

            this.Settings = new SettingCollection(this);
            this.Favorites = new FavoriteCollection(this);

            ResetExCookie();
        }

        internal HttpCookieManager CookieManager { get; }

        internal MyHttpClient HttpClient { get; }

        internal UriProvider Uris => this.Host == HostType.Exhentai ? UriProvider.Ex : UriProvider.Eh;

        public HostType Host { get; set; } = HostType.Ehentai;

        public SettingCollection Settings { get; }

        public FavoriteCollection Favorites { get; }

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if(!this.disposedValue)
            {
                if(disposing)
                {
                    this.HttpClient.Dispose();
                }
                this.disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
