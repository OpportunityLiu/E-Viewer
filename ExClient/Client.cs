using ExClient.Internal;
using ExClient.Settings;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

namespace ExClient
{
    public partial class Client
    {
        public static Client Current { get; } = new Client();

        private Client()
        {
            var httpFilter = new HttpBaseProtocolFilter { AllowAutoRedirect = false };
            httpFilter.CacheControl.WriteBehavior = HttpCacheWriteBehavior.NoCache;
            this.CookieManager = httpFilter.CookieManager;
            this.HttpClient = new MyHttpClient(this, new HttpClient(new RedirectFilter(httpFilter)));
            this.Settings = new SettingCollection(this);

            ResetExCookie();
        }

        internal HttpCookieManager CookieManager { get; }

        internal MyHttpClient HttpClient { get; }

        internal UriProvider Uris => this.Host == HostType.Exhentai ? UriProvider.Ex : UriProvider.Eh;

        public HostType Host { get; set; } = HostType.Ehentai;

        public SettingCollection Settings { get; }

        private readonly FavoriteCollection favotites = new FavoriteCollection();
        public FavoriteCollection Favorites => NeedLogOn ? null : this.favotites;

        private readonly UserStatus userStatus = new UserStatus();
        public UserStatus UserStatus => NeedLogOn ? null : this.userStatus;
    }
}
