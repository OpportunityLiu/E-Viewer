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

namespace ExClient
{
    public partial class Client : IDisposable
    {
        private static Client current;

        public static Client Current
        {
            get
            {
                return current;
            }
            protected set
            {
                current?.Dispose();
                current = value;
            }
        }

        public static IAsyncOperation<Client> CreateClient(string userName, string password)
        {
            if(string.IsNullOrWhiteSpace(userName))
                throw new ArgumentNullException(nameof(userName));
            if(string.IsNullOrWhiteSpace(password))
                throw new ArgumentNullException(nameof(password));
            return Run(async token =>
            {
                var c = new Client();
                var init= c.initAsync(userName, password);
                token.Register(init.Cancel);
                await init;
                Current = c;
                return c;
            });
        }

        internal static readonly Uri RootUri = new Uri("http://exhentai.org/");
        private static readonly Uri apiUri = new Uri(RootUri, "api.php");
        internal static readonly Uri logOnUri = new Uri("https://forums.e-hentai.org/index.php?act=Login&CODE=01");

        private Client()
        {
            httpFilter = new HttpBaseProtocolFilter();
            httpClient = new HttpClient(httpFilter);
        }

        private string userName, password;

        private HttpClient httpClient;

        private HttpBaseProtocolFilter httpFilter;

        internal HttpClient HttpClient
        {
            get
            {
                return httpClient;
            }
        }

        private IAsyncOperation<Client> initAsync(string userName, string password)
        {
            this.userName = userName;
            this.password = password;
            return Run(async token =>
            {
                if(this.disposedValue)
                    throw new InvalidOperationException("The client has been disposed.");
                var clientParam = new Dictionary<string, string>()
                {
                    ["CookieDate"] = "1",
                    ["UserName"] = userName,
                    ["PassWord"] = password
                };
                var log = await httpClient.PostAsync(logOnUri, new HttpFormUrlEncodedContent(clientParam));
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
                foreach(var cookie in httpFilter.CookieManager.GetCookies(new Uri("http://e-hentai.org")))
                {
                    httpFilter.CookieManager.SetCookie(new HttpCookie(cookie.Name, "exhentai.org", cookie.Path)
                    {
                        Expires = cookie.Expires,
                        HttpOnly = cookie.HttpOnly,
                        Secure = cookie.Secure,
                        Value = cookie.Value
                    }, true);
                }
                return this;
            });
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

        public IAsyncOperationWithProgress<string, HttpProgress> PostApiAsync(string requestJson)
        {
            return PostStrAsync(apiUri, requestJson);
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
                httpClient.Dispose();
                httpClient = null;

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
