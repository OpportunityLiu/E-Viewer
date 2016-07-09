using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Web.Http;
using Windows.Web.Http.Filters;
using Windows.Foundation;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;
using HtmlAgilityPack;
using System.IO;
using System.Runtime.InteropServices;

namespace ExClient
{
    public partial class Client : IDisposable
    {
        public static Client Current
        {
            get;
        } = new Client();

        internal static readonly Uri RootUri = new Uri("http://exhentai.org/");
        internal static readonly Uri EhUri = new Uri("http://e-hentai.org/");
        private static readonly Uri apiUri = new Uri(RootUri, "api.php");
        internal static readonly Uri LogOnUri = new Uri("https://forums.e-hentai.org/index.php?act=Login&CODE=01");

        private Client()
        {
            httpFilter = new HttpBaseProtocolFilter();
            HttpClient = new HttpClient(httpFilter);
        }

        private HttpBaseProtocolFilter httpFilter;

        internal HttpClient HttpClient
        {
            get;
        }

        public bool NeedLogOn => httpFilter.CookieManager.GetCookies(RootUri).Count < 2 && httpFilter.CookieManager.GetCookies(EhUri).Count < 2;

        public IAsyncOperation<Client> LogOnAsync(string userName, string password, ReCaptcha reCaptcha)
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
                try
                {
                    var clientParam = new Dictionary<string, string>()
                    {
                        ["CookieDate"] = "1",
                        ["UserName"] = userName,
                        ["PassWord"] = password
                    };
                    if(reCaptcha?.Answer != null)
                    {
                        clientParam.Add("recaptcha_challenge_field", reCaptcha.Answer);
                        clientParam.Add("recaptcha_response_field", "manual_challenge");
                    }
                    var log = await HttpClient.PostAsync(LogOnUri, new HttpFormUrlEncodedContent(clientParam));
                    var html = new HtmlDocument();
                    using(var stream = await log.Content.ReadAsInputStreamAsync())
                    {
                        html.Load(stream.AsStreamForRead());
                    }
                    var errorNode = html.DocumentNode.Descendants("span").Where(node => node.GetAttributeValue("class", "") == "postcolor").FirstOrDefault();
                    if(errorNode != null)
                    {
                        throw new InvalidOperationException(errorNode.InnerText);
                    }
                    var init = await HttpClient.GetAsync(RootUri, HttpCompletionOption.ResponseHeadersRead);
                    if(httpFilter.CookieManager.GetCookies(RootUri).FirstOrDefault(c => c.Name == "igneous")?.Value == "mystery")
                    {
                        throw new NotSupportedException(LocalizedStrings.Resources.AccountDenied);
                    }
                    return this;
                }
                catch(Exception)
                {
                    ClearLogOnInfo();
                    foreach(var item in cookieBackUp)
                    {
                        httpFilter.CookieManager.SetCookie(item);
                    }
                    throw;
                }
            });
        }

        private List<HttpCookie> getLogOnInfo()
        {
            return httpFilter.CookieManager.GetCookies(RootUri).Concat(httpFilter.CookieManager.GetCookies(EhUri)).ToList();
        }

        public void ClearLogOnInfo()
        {
            foreach(var item in httpFilter.CookieManager.GetCookies(RootUri))
            {
                httpFilter.CookieManager.DeleteCookie(item);
            }
            foreach(var item in httpFilter.CookieManager.GetCookies(EhUri))
            {
                httpFilter.CookieManager.DeleteCookie(item);
            }
        }

        public IAsyncOperationWithProgress<string, HttpProgress> PostStrAsync(Uri uri, string content)
        {
            if(!uri.IsAbsoluteUri)
                uri = new Uri(RootUri, uri);
            if(content == null)
                return HttpClient.GetStringAsync(uri);
            return Run<string, HttpProgress>(async (token, progress) =>
            {
                var op = HttpClient.PostAsync(uri, content == null ? null : new HttpStringContent(content));
                token.Register(op.Cancel);
                op.Progress = (sender, value) => progress.Report(value);
                var res = await op;
                return await res.Content.ReadAsStringAsync();
            });
        }

        public int UserID
        {
            get
            {
                var cookie = httpFilter.CookieManager.GetCookies(EhUri).FirstOrDefault(c => c.Name == "ipb_member_id");
                if(cookie == null)
                    return -1;
                return int.Parse(cookie.Value);
            }
        }

        public IAsyncOperationWithProgress<string, HttpProgress> PostApiAsync(string requestJson)
        {
            return PostStrAsync(apiUri, requestJson);
        }

        public void SetHahProxy(HahProxyConfig hah)
        {
            var cm = httpFilter.CookieManager;
            if(hah == null)
            {
                var uconfig = cm.GetCookies(RootUri).FirstOrDefault(c => c.Name == "uconfig");
                if(uconfig != null)
                    cm.DeleteCookie(uconfig);
            }
            else
            {
                cm.SetCookie(hah.GetCookie());
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if(!disposedValue)
            {
                if(disposing)
                {
                    // 释放托管状态(托管对象)。
                }

                // 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // 将大型字段设置为 null。
                HttpClient.Dispose();

                disposedValue = true;
            }
        }

        // 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~Client() {
        //   // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
        //   Dispose(false);
        // }

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            // 如果在以上内容中替代了终结器，则取消注释以下行。
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
