using ExClient.Api;
using ExClient.Settings;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Web.Http;
using Windows.Web.Http.Filters;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;
using ExClient.Internal;
using System.Threading.Tasks;

namespace ExClient
{
    public partial class Client : IDisposable
    {
        public static Client Current
        {
            get;
        } = new Client();

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

        public HostType Host { get; set; } = HostType.Exhentai;

        public bool NeedLogOn
            => this.CookieManager.GetCookies(UriProvider.Eh.RootUri).Count(isImportantCookie) < 3;

        public void ResetExCookie()
        {
            foreach(var item in this.CookieManager.GetCookies(UriProvider.Ex.RootUri))
            {
                this.CookieManager.DeleteCookie(item);
            }
            foreach(var item in this.getLogOnInfo().Where(isImportantCookie))
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
            return name == "ipb_member_id"
                || name == "ipb_pass_hash"
                || name == "s";
        }

        private List<HttpCookie> getLogOnInfo()
        {
            return this.CookieManager.GetCookies(UriProvider.Eh.RootUri).Concat(this.CookieManager.GetCookies(UriProvider.Ex.RootUri)).ToList();
        }

        public void ClearLogOnInfo()
        {
            foreach(var item in getLogOnInfo())
            {
                this.CookieManager.DeleteCookie(item);
            }
            setDefaultCookies();
        }

        private void setDefaultCookies()
        {
            this.CookieManager.SetCookie(new HttpCookie("nw", "e-hentai.org", "/") { Value = "1" });
            this.CookieManager.SetCookie(new HttpCookie("nw", "exhentai.org", "/") { Value = "1" });
            this.Settings.ApplyChanges();
        }

        public IAsyncAction LogOnAsync(string userName, string password, ReCaptcha reCaptcha)
        {
            if(string.IsNullOrWhiteSpace(userName))
                throw new ArgumentNullException(nameof(userName));
            if(string.IsNullOrWhiteSpace(password))
                throw new ArgumentNullException(nameof(password));
            return Run(async token =>
            {
                if(this.disposedValue)
                    throw new InvalidOperationException(LocalizedStrings.Resources.ClientDisposed);
                var cookieBackUp = getLogOnInfo();
                ClearLogOnInfo();
                IEnumerable<KeyValuePair<string, string>> getParams()
                {
                    yield return new KeyValuePair<string, string>("CookieDate", "1");
                    yield return new KeyValuePair<string, string>("UserName", userName);
                    yield return new KeyValuePair<string, string>("PassWord", password);
                    if(reCaptcha?.Answer != null)
                    {
                        yield return new KeyValuePair<string, string>("recaptcha_challenge_field", reCaptcha.Answer);
                        yield return new KeyValuePair<string, string>("recaptcha_response_field", "manual_challenge");
                    }
                }
                try
                {
                    var log = await this.HttpClient.PostAsync(logOnUri, new HttpFormUrlEncodedContent(getParams()));
                    var html = new HtmlDocument();
                    using(var stream = (await log.Content.ReadAsInputStreamAsync()).AsStreamForRead())
                    {
                        html.Load(stream);
                    }
                    var errorNode = html.DocumentNode.Descendants("span").Where(node => node.GetAttributeValue("class", "") == "postcolor").FirstOrDefault();
                    if(errorNode != null)
                    {
                        var errorText = errorNode.InnerText;
                        switch(errorText)
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
                catch(Exception)
                {
                    ClearLogOnInfo();
                    foreach(var item in cookieBackUp)
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
                var cookie = this.CookieManager.GetCookies(UriProvider.Eh.RootUri).FirstOrDefault(c => c.Name == "ipb_member_id");
                if(cookie == null)
                    return -1;
                return int.Parse(cookie.Value);
            }
        }

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
