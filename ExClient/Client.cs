using ExClient.Internal;
using ExClient.Settings;
using ExClient.Status;

using Opportunity.MvvmUniverse;

using Windows.Web.Http;
using Windows.Web.Http.Filters;

namespace ExClient
{
    public partial class Client : ObservableObject
    {
        public static Client Current { get; } = new Client();

        private Client()
        {
            var httpFilter = new HttpBaseProtocolFilter { AllowAutoRedirect = false };
            httpFilter.IgnorableServerCertificateErrors.Add(Windows.Security.Cryptography.Certificates.ChainValidationResult.InvalidName);
            httpFilter.CacheControl.WriteBehavior = HttpCacheWriteBehavior.NoCache;
            CookieManager = httpFilter.CookieManager;
            HttpClient = new MyHttpClient(this, new HttpClient(new RedirectFilter(httpFilter)));
            _SetDefaultCookies();

            if (NeedLogOn)
                ClearLogOnInfo();
        }

        internal HttpCookieManager CookieManager { get; }

        internal MyHttpClient HttpClient { get; }

        internal DomainProvider Uris => Host == HostType.ExHentai ? DomainProvider.Ex : DomainProvider.Eh;

        private HostType host;
        public HostType Host
        {
            get => host;
            set => Set(nameof(Settings), ref host, value);
        }

        public SettingCollection Settings => Uris.Settings;


        private readonly FavoriteCollection favotites = new FavoriteCollection();
        public FavoriteCollection Favorites => NeedLogOn ? null : favotites;

        private readonly UserStatus userStatus = new UserStatus();
        public UserStatus UserStatus => NeedLogOn ? null : userStatus;

        private readonly TaggingStatistics taggingStatistics = new TaggingStatistics();
        public TaggingStatistics TaggingStatistics => NeedLogOn ? null : taggingStatistics;
    }
}
