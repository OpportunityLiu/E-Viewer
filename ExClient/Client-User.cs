using ExClient.Forums;
using ExClient.Internal;
using ExClient.Status;
using System;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Web.Http;

namespace ExClient
{
    public partial class Client
    {
        public static Uri ForumsUri { get; } = new Uri("https://forums.e-hentai.org/");

        public static Uri LogOnUri { get; } = new Uri(ForumsUri, "index.php?act=Login");

        public bool NeedLogOn
            => UserId <= 0
            || PassHash is null;

        internal void CheckLogOn()
        {
            if (NeedLogOn)
                throw new InvalidOperationException(LocalizedStrings.Resources.WrongAccountInfo);
        }

        internal static class CookieNames
        {
            public const string MemberID = "ipb_member_id";
            public const string PassHash = "ipb_pass_hash";
            public const string S = "s";
            public const string SK = "sk";
            public const string HathPerks = "hath_perks";
            public const string NeverWarn = "nw";
            public const string LastEventRefreshTime = "event";
        }

        internal static class Domains
        {
            public const string Eh = "e-hentai.org";
            public const string Ex = "exhentai.org";
        }

        internal void ResetExCookie()
        {
            foreach (var item in CookieManager.GetCookies(DomainProvider.Ex.RootUri))
                CookieManager.DeleteCookie(item);

            foreach (var item in CookieManager.GetCookies(DomainProvider.Eh.RootUri).Where(isImportantCookie))
            {
                var cookie = new HttpCookie(item.Name, Domains.Ex, "/")
                {
                    Expires = item.Expires,
                    Value = item.Value
                };
                CookieManager.SetCookie(cookie);
            }
            setDefaultCookies();
        }

        private static bool isImportantCookie(HttpCookie cookie)
        {
            var name = cookie?.Name;
            return name == CookieNames.MemberID
                || name == CookieNames.PassHash
                || name == CookieNames.S
                || name == CookieNames.SK
                || name == CookieNames.HathPerks;
        }

        private static bool isKeyCookie(HttpCookie cookie)
        {
            var name = cookie?.Name;
            return name == CookieNames.MemberID
                || name == CookieNames.PassHash;
        }

        public LogOnInfo GetLogOnInfo()
        {
            return new LogOnInfo(this);
        }

        public async Task RefreshCookiesAsync()
        {
            if (NeedLogOn)
                throw new InvalidOperationException("Must log on first.");

            var backup = GetLogOnInfo();
            try
            {
                await _RefreshCookieAndSettings();
                await _RefreshHathPerks();
                await UserStatus?.RefreshAsync();
            }
            catch (Exception)
            {
                RestoreLogOnInfo(backup);
                throw;
            }
        }

        public void RestoreLogOnInfo(LogOnInfo info)
        {
            if (info is null)
                throw new ArgumentNullException(nameof(info));

            ClearLogOnInfo();
            foreach (var item in info.Cookies)
                CookieManager.SetCookie(item);
            ResetExCookie();
        }

        public void ClearLogOnInfo()
        {
            foreach (var item in CookieManager.GetCookies(DomainProvider.Eh.RootUri)
                        .Concat(CookieManager.GetCookies(DomainProvider.Ex.RootUri)))
                CookieManager.DeleteCookie(item);
            setDefaultCookies();
        }

        private void setDefaultCookies()
        {
            CookieManager.SetCookie(new HttpCookie(CookieNames.NeverWarn, Domains.Eh, "/") { Value = "1" });
            CookieManager.SetCookie(new HttpCookie(CookieNames.LastEventRefreshTime, Domains.Eh, "/") { Value = "1" });
            CookieManager.SetCookie(new HttpCookie(CookieNames.NeverWarn, Domains.Ex, "/") { Value = "1" });
        }

        private async Task _RefreshCookieAndSettings()
        {
            foreach (var item in CookieManager.GetCookies(DomainProvider.Eh.RootUri).Where(c => !isKeyCookie(c)))
                CookieManager.DeleteCookie(item);
            ResetExCookie();
            var f1 = DomainProvider.Eh.Settings.FetchAsync();
            var f2 = DomainProvider.Ex.Settings.FetchAsync();
            await f1;
            await f2;

            var a1 = DomainProvider.Eh.Settings.SendAsync();
            var a2 = DomainProvider.Ex.Settings.SendAsync();
            await a1;
            await a2;
        }

        /// <summary>
        /// Log on with tokens.
        /// View <see cref="LogOnUri"/> to get the tokens.
        /// </summary>
        /// <param name="userID">cookie with name ipb_member_id</param>
        /// <param name="passHash">cookie with name ipb_pass_hash</param>
        public async Task LogOnAsync(long userID, string passHash)
        {
            if (userID <= 0)
                throw new ArgumentOutOfRangeException(nameof(userID));
            if (string.IsNullOrWhiteSpace(passHash))
                throw new ArgumentNullException(nameof(passHash));

            passHash = passHash.Trim().ToLowerInvariant();
            if (passHash.Length != 32 || !passHash.All(c => (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f')))
                throw new ArgumentException("Should be 32 hex chars.", nameof(passHash));

            var cookieBackUp = GetLogOnInfo();
            ClearLogOnInfo();

            CookieManager.SetCookie(new HttpCookie(CookieNames.MemberID, Domains.Eh, "/") { Value = userID.ToString(), Expires = DateTimeOffset.UtcNow.AddYears(5) });
            CookieManager.SetCookie(new HttpCookie(CookieNames.PassHash, Domains.Eh, "/") { Value = passHash, Expires = DateTimeOffset.UtcNow.AddYears(5) });

            try
            {
                await _RefreshCookieAndSettings();
                try
                {
                    await UserStatus?.RefreshAsync();
                    await _RefreshHathPerks();
                }
                catch { }

                if (NeedLogOn)
                    throw new ArgumentException("Invalid log on info.");
            }
            catch (Exception)
            {
                RestoreLogOnInfo(cookieBackUp);
                throw;
            }
        }

        private static readonly Uri _HathperksUri = new Uri(DomainProvider.Eh.RootUri, "hathperks.php");

        private async Task _RefreshHathPerks()
        {
            var cookie = CookieManager.GetCookies(DomainProvider.Eh.RootUri).SingleOrDefault(c => c.Name == CookieNames.HathPerks);
            if (cookie != null)
                CookieManager.DeleteCookie(cookie);

            await HttpClient.GetAsync(_HathperksUri, HttpCompletionOption.ResponseHeadersRead, true);
            CheckLogOn();
            ResetExCookie();
        }

        public long UserId
        {
            get
            {
                var cookie = CookieManager.GetCookies(DomainProvider.Eh.RootUri).SingleOrDefault(c => c.Name == CookieNames.MemberID);
                var value = cookie?.Value;
                if (value.IsNullOrWhiteSpace())
                    return -1;
                return long.Parse(value);
            }
        }

        internal string PassHash
        {
            get
            {
                var cookie = CookieManager.GetCookies(DomainProvider.Eh.RootUri).SingleOrDefault(c => c.Name == CookieNames.PassHash);
                var value = cookie?.Value;
                if (value.IsNullOrWhiteSpace())
                    return null;
                return value;
            }
        }

        public Task<UserInfo> FetchCurrentUserInfoAsync()
        {
            if (UserId < 0)
                throw new InvalidOperationException("Hasn't log in");
            return UserInfo.FeachAsync(UserId);
        }
    }
}