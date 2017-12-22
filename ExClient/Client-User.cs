using ExClient.Internal;
using ExClient.Status;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Web.Http;

namespace ExClient
{
    public partial class Client
    {
        public static Uri ForumsUri { get; } = new Uri("https://forums.e-hentai.org/");

        public static Uri LogOnUri { get; } = new Uri(ForumsUri, "index.php?act=Login");

        public bool NeedLogOn
            => this.CookieManager.GetCookies(UriProvider.Eh.RootUri).Count(isImportantCookie) < 3;

        private static class CookieNames
        {
            public const string MemberID = "ipb_member_id";
            public const string PassHash = "ipb_pass_hash";
            public const string S = "s";
            public const string NeverWarn = "nw";
            public const string HathPerks = "hath_perks";
        }

        private static class Domains
        {
            public const string Eh = "e-hentai.org";
            public const string Ex = "exhentai.org";
        }

        public void ResetExCookie()
        {
            foreach (var item in this.CookieManager.GetCookies(UriProvider.Ex.RootUri))
            {
                this.CookieManager.DeleteCookie(item);
            }
            foreach (var item in this.GetLogOnInfo().Cookies.Where(isImportantCookie))
            {
                var cookie = new HttpCookie(item.Name, Domains.Ex, "/")
                {
                    Expires = item.Expires,
                    Value = item.Value
                };
                this.CookieManager.SetCookie(cookie);
            }
            setDefaultCookies();
        }

        private static bool isImportantCookie(HttpCookie cookie)
        {
            var name = cookie?.Name;
            return name == CookieNames.MemberID
                || name == CookieNames.PassHash
                || name == CookieNames.S;
        }

        public LogOnInfo GetLogOnInfo()
        {
            return new LogOnInfo(this);
        }

        public void RestoreLogOnInfo(LogOnInfo info)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));
            ClearLogOnInfo();
            foreach (var item in info.Cookies)
            {
                this.CookieManager.SetCookie(item);
            }
        }

        public void ClearLogOnInfo()
        {
            foreach (var item in GetLogOnInfo().Cookies)
            {
                this.CookieManager.DeleteCookie(item);
            }
            setDefaultCookies();
        }

        private void setDefaultCookies()
        {
            this.CookieManager.SetCookie(new HttpCookie(CookieNames.NeverWarn, Domains.Eh, "/") { Value = "1" });
            this.CookieManager.SetCookie(new HttpCookie(CookieNames.NeverWarn, Domains.Ex, "/") { Value = "1" });
            this.Settings.ApplyChanges();
        }

        /// <summary>
        /// Log on with tokens.
        /// View <see cref="LogOnUri"/> to get the tokens.
        /// </summary>
        /// <param name="userID">cookie with name ipb_member_id</param>
        /// <param name="passHash">cookie with name ipb_pass_hash</param>
        public IAsyncAction LogOnAsync(long userID, string passHash)
        {
            if (userID <= 0)
                throw new ArgumentOutOfRangeException(nameof(userID));
            if (string.IsNullOrWhiteSpace(passHash))
                throw new ArgumentNullException(nameof(passHash));
            passHash = passHash.Trim().ToLowerInvariant();
            if (passHash.Length != 32 || !passHash.All(c => (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f')))
                throw new ArgumentException("Should be 32 hex chars.", nameof(passHash));
            var memberID = userID.ToString();
            return AsyncInfo.Run(async token =>
            {
                var cookieBackUp = GetLogOnInfo();
                ClearLogOnInfo();
                this.CookieManager.SetCookie(new HttpCookie(CookieNames.MemberID, Domains.Eh, "/") { Value = memberID, Expires = DateTimeOffset.UtcNow.AddYears(5) });
                this.CookieManager.SetCookie(new HttpCookie(CookieNames.PassHash, Domains.Eh, "/") { Value = passHash, Expires = DateTimeOffset.UtcNow.AddYears(5) });
                try
                {
                    await this.HttpClient.GetAsync(new Uri(UriProvider.Eh.RootUri, "hathperks.php"), HttpCompletionOption.ResponseHeadersRead, true);
                    if (this.NeedLogOn)
                        throw new InvalidOperationException(LocalizedStrings.Resources.WrongAccountInfo);
                    ResetExCookie();
                    var ignore = this.UserStatus?.RefreshAsync();
                }
                catch (Exception)
                {
                    RestoreLogOnInfo(cookieBackUp);
                    throw;
                }
            });
        }

        public long UserID
        {
            get
            {
                var cookie = this.CookieManager.GetCookies(UriProvider.Eh.RootUri).SingleOrDefault(c => c.Name == CookieNames.MemberID);
                if (cookie == null)
                    return -1;
                return long.Parse(cookie.Value);
            }
        }

        internal string PassHash
        {
            get
            {
                var cookie = this.CookieManager.GetCookies(UriProvider.Eh.RootUri).SingleOrDefault(c => c.Name == CookieNames.PassHash);
                if (cookie == null)
                    return null;
                return cookie.Value;
            }
        }

        public IAsyncOperation<UserInfo> FetchCurrentUserInfoAsync()
        {
            if (this.UserID < 0)
                throw new InvalidOperationException("Hasn't log in");
            return UserInfo.FeachAsync(this.UserID);
        }
    }
}