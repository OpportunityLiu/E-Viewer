using ExClient.Internal;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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

        private static readonly Uri logOnUri = new Uri(ForumsUri, "index.php?act=Login&CODE=01");

        public bool NeedLogOn
            => this.CookieManager.GetCookies(UriProvider.Eh.RootUri).Count(isImportantCookie) < 3;

        private static class CookieNames
        {
            public const string MemberID = "ipb_member_id";
            public const string PassHash = "ipb_pass_hash";
            public const string S = "s";
            public const string NeverWarn = "nw";
        }

        public void ResetExCookie()
        {
            foreach (var item in this.CookieManager.GetCookies(UriProvider.Ex.RootUri))
            {
                this.CookieManager.DeleteCookie(item);
            }
            foreach (var item in this.getLogOnInfo().Where(isImportantCookie))
            {
                var cookie = new HttpCookie(item.Name, "exhentai.org", "/")
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

        private List<HttpCookie> getLogOnInfo()
        {
            return this.CookieManager.GetCookies(UriProvider.Eh.RootUri).Concat(this.CookieManager.GetCookies(UriProvider.Ex.RootUri)).ToList();
        }

        public void ClearLogOnInfo()
        {
            foreach (var item in getLogOnInfo())
            {
                this.CookieManager.DeleteCookie(item);
            }
            setDefaultCookies();
        }

        private void setDefaultCookies()
        {
            this.CookieManager.SetCookie(new HttpCookie(CookieNames.NeverWarn, "e-hentai.org", "/") { Value = "1" });
            this.CookieManager.SetCookie(new HttpCookie(CookieNames.NeverWarn, "exhentai.org", "/") { Value = "1" });
            this.Settings.ApplyChanges();
        }

        public IAsyncAction LogOnAsync(string userName, string password, ReCaptcha reCaptcha)
        {
            if (string.IsNullOrWhiteSpace(userName))
                throw new ArgumentNullException(nameof(userName));
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentNullException(nameof(password));
            return AsyncInfo.Run(async token =>
            {
                var cookieBackUp = getLogOnInfo();
                ClearLogOnInfo();
                IEnumerable<KeyValuePair<string, string>> getParams()
                {
                    yield return new KeyValuePair<string, string>("CookieDate", "1");
                    yield return new KeyValuePair<string, string>("UserName", userName);
                    yield return new KeyValuePair<string, string>("PassWord", password);
                    if (reCaptcha?.Answer != null)
                    {
                        yield return new KeyValuePair<string, string>("recaptcha_challenge_field", reCaptcha.Answer);
                        yield return new KeyValuePair<string, string>("recaptcha_response_field", "manual_challenge");
                    }
                }
                try
                {
                    var log = await this.HttpClient.PostAsync(logOnUri, new HttpFormUrlEncodedContent(getParams()));
                    var html = new HtmlDocument();
                    using (var stream = (await log.Content.ReadAsInputStreamAsync()).AsStreamForRead())
                    {
                        html.Load(stream);
                    }
                    var errorNode = html.DocumentNode.Descendants("span").Where(node => node.GetAttributeValue("class", "") == "postcolor").FirstOrDefault();
                    if (errorNode != null)
                    {
                        var errorText = errorNode.InnerText;
                        switch (errorText)
                        {
                        case "Username or password incorrect":
                            errorText = LocalizedStrings.Resources.WrongAccountInfo;
                            break;
                        case "The captcha was not entered correctly. Please try again.":
                            errorText = LocalizedStrings.Resources.WrongCaptcha;
                            break;
                        }
                        throw new InvalidOperationException(errorText);
                    }
                    await this.HttpClient.GetAsync(new Uri(UriProvider.Eh.RootUri, "favorites.php"), HttpCompletionOption.ResponseHeadersRead);
                    ResetExCookie();
                }
                catch (Exception)
                {
                    ClearLogOnInfo();
                    foreach (var item in cookieBackUp)
                    {
                        this.CookieManager.SetCookie(item);
                    }
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
                return int.Parse(cookie.Value);
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