using ExClient.Forums;
using ExClient.Internal;
using ExClient.Status;
using System;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
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
            public const string SettingsKey = "sk";
            public const string HathPerks = "hath_perks";
            public const string NeverWarn = "nw";
            public const string LastEventRefreshTime = "event";
            public const string Igneous = "igneous";
            public const string Yay = "yay";
        }

        internal static class Domains
        {
            public const string Eh = "e-hentai.org";
            public const string Ex = "exhentai.org";
        }

        private bool _CopyCookie(string name)
        {
            var cookie = CookieManager.GetCookies(DomainProvider.Eh.RootUri).SingleOrDefault(c => c.Name == name);
            if (cookie is null)
                return false;
            cookie = new HttpCookie(cookie.Name, Domains.Ex, "/")
            {
                Expires = cookie.Expires,
                HttpOnly = cookie.HttpOnly,
                Secure = cookie.Secure,
                Value = cookie.Value
            };
            CookieManager.SetCookie(cookie);
            return true;
        }

        private void _SetCookieEh(string name, string value)
        {
            CookieManager.SetCookie(new HttpCookie(name, Domains.Eh, "/") { Value = value });
        }

        private void _SetCookieEx(string name, string value)
        {
            CookieManager.SetCookie(new HttpCookie(name, Domains.Ex, "/") { Value = value });
        }

        private void _SetCookieAll(string name, string value)
        {
            CookieManager.SetCookie(new HttpCookie(name, Domains.Eh, "/") { Value = value });
            CookieManager.SetCookie(new HttpCookie(name, Domains.Ex, "/") { Value = value });
        }

        internal async Task ResetExCookieAsync()
        {
            foreach (var item in CookieManager.GetCookies(DomainProvider.Ex.RootUri))
                CookieManager.DeleteCookie(item);

            await HttpClient.GetAsync(DomainProvider.Ex.RootUri, HttpCompletionOption.ResponseHeadersRead, true);
            _CopyCookie(CookieNames.SettingsKey);
            _CopyCookie(CookieNames.HathPerks);
            _SetDefaultCookies();
        }

        public LogOnInfo GetLogOnInfo()
        {
            _SetDefaultCookies();
            return new LogOnInfo(this);
        }

        public async Task RefreshCookiesAsync()
        {
            if (NeedLogOn)
                throw new InvalidOperationException("Must log on first.");

            _SetDefaultCookies();
            await Task.WhenAll(_RefreshSettingsAsync(), _RefreshHathPerksAsync(), UserStatus?.RefreshAsync());
        }

        public void RestoreLogOnInfo(LogOnInfo info)
        {
            if (info is null)
                throw new ArgumentNullException(nameof(info));

            ClearLogOnInfo();
            foreach (var item in info.EhCookies)
                CookieManager.SetCookie(item);
            foreach (var item in info.ExCookies)
                CookieManager.SetCookie(item);
            _SetDefaultCookies();
        }

        public void ClearLogOnInfo()
        {
            foreach (var item in CookieManager.GetCookies(DomainProvider.Eh.RootUri)
                        .Concat(CookieManager.GetCookies(DomainProvider.Ex.RootUri)))
                CookieManager.DeleteCookie(item);
            _SetDefaultCookies();
        }

        private void _SetDefaultCookies()
        {
            // Avoid warn page for r18g galleries
            _SetCookieAll(CookieNames.NeverWarn, "1");
            // Set event cookie to a early timestamp to make sure new events can be triggered
            _SetCookieEh(CookieNames.LastEventRefreshTime, "1");
            // Remove yay cookie for ex
            CookieManager.DeleteCookie(new HttpCookie(CookieNames.Yay, Domains.Ex, "/"));
        }

        private async Task _RefreshSettingsAsync(CancellationToken token = default)
        {
            async Task refresh(Settings.SettingCollection setting, CancellationToken t)
            {
                await setting.FetchAsync(t);
                await setting.SendAsync(t);
            }

            await Task.WhenAll(refresh(DomainProvider.Eh.Settings, token), refresh(DomainProvider.Ex.Settings, token));
        }

        /// <summary>
        /// Log on with tokens.
        /// View <see cref="LogOnUri"/> to get the tokens.
        /// </summary>
        /// <param name="userID">cookie with name ipb_member_id</param>
        /// <param name="passHash">cookie with name ipb_pass_hash</param>
        /// <param name="igneous">cookie with name igneous (can be null)</param>
        public async Task LogOnAsync(long userID, string passHash, string igneous = null)
        {
            if (userID <= 0)
                throw new ArgumentOutOfRangeException(nameof(userID));
            if (string.IsNullOrWhiteSpace(passHash))
                throw new ArgumentNullException(nameof(passHash));

            passHash = passHash.Trim().ToLowerInvariant();
            if (passHash.Length != 32 || !passHash.All(c => (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f')))
                throw new ArgumentException("Should be 32 hex chars.", nameof(passHash));

            if (igneous != null)
            {
                igneous = igneous.Trim().ToLowerInvariant();
                if (string.IsNullOrEmpty(igneous))
                    igneous = null;
                else if (!igneous.All(c => (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f')))
                    throw new ArgumentException("Should be hex string or null.", nameof(igneous));
            }

            var cookieBackUp = GetLogOnInfo();
            ClearLogOnInfo();

            // add log on info to eh
            _SetCookieEh(CookieNames.MemberID, userID.ToString());
            _SetCookieEh(CookieNames.PassHash, passHash);

            try
            {
                // add log on info to ex
                if (!string.IsNullOrEmpty(igneous))
                {
                    // with igneous, set cookies directly
                    _CopyCookie(CookieNames.MemberID);
                    _CopyCookie(CookieNames.PassHash);
                    _SetCookieEx(CookieNames.Igneous, igneous);
                }
                else
                {
                    // otherwise, visit ex once to get them
                    await ResetExCookieAsync();
                }

                try
                {
                    await RefreshCookiesAsync();
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

        private async Task _RefreshHathPerksAsync()
        {
            CheckLogOn();
            try
            {
                await HttpClient.GetAsync(_HathperksUri, HttpCompletionOption.ResponseHeadersRead, true);
            }
            finally
            {
                _CopyCookie(CookieNames.HathPerks);
            }
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