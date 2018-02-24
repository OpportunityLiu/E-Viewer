using ExClient.Internal;
using ExClient.Settings;
using ExClient.Status;
using Opportunity.MvvmUniverse;
using Windows.Web.Http;
using Windows.Web.Http.Filters;
using System.Threading.Tasks;
using System;

namespace ExClient
{
    public partial class Client : ObservableObject
    {
        public static Client Current { get; } = new Client();

        private Client()
        {
            var httpFilter = new HttpBaseProtocolFilter { AllowAutoRedirect = false };
            httpFilter.CacheControl.WriteBehavior = HttpCacheWriteBehavior.NoCache;
            this.CookieManager = httpFilter.CookieManager;
            this.HttpClient = new MyHttpClient(this, new HttpClient(new RedirectFilter(httpFilter)));

            ResetExCookie();

            var ignore = Task.Run(fetchSettings);
        }

        private async Task fetchSettings()
        {
            try
            {
                if (!NeedLogOn)
                {
                    await DomainProvider.Eh.Settings.FetchAsync();
                    await DomainProvider.Ex.Settings.FetchAsync();
                }
            }
            catch (System.Exception)
            {
            }
        }

        internal HttpCookieManager CookieManager { get; }

        internal MyHttpClient HttpClient { get; }

        internal DomainProvider Uris => this.Host == HostType.ExHentai ? DomainProvider.Ex : DomainProvider.Eh;

        private HostType host;
        public HostType Host
        {
            get => this.host;
            set
            {
                Set(nameof(Settings), ref this.host, value);
                Settings.FetchAsync().Completed = (s, e) => s.Close();
            }
        }

        public SettingCollection Settings => this.Uris.Settings;

        private readonly FavoriteCollection favotites = new FavoriteCollection();
        public FavoriteCollection Favorites => NeedLogOn ? null : this.favotites;

        private readonly UserStatus userStatus = new UserStatus();
        public UserStatus UserStatus => NeedLogOn ? null : this.userStatus;

        private readonly TaggingStatistics taggingStatistics = new TaggingStatistics();
        public TaggingStatistics TaggingStatistics => NeedLogOn ? null : this.taggingStatistics;
    }
}
