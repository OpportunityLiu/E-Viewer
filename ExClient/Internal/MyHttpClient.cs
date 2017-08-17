using ExClient.HentaiVerse;
using HtmlAgilityPack;
using System;
using System.IO;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.Web.Http;
using Windows.Web.Http.Headers;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;
using IHttpAsyncOperation = Windows.Foundation.IAsyncOperationWithProgress<Windows.Web.Http.HttpResponseMessage, Windows.Web.Http.HttpProgress>;

namespace ExClient.Internal
{
    /*
     * 由于使用了自定义 Filter 后发生异常会丢失异常详细信息，故使用此类封装，以保留异常信息。
     * */
    internal class MyHttpClient : IDisposable
    {
        public MyHttpClient(Client owner, HttpClient inner)
        {
            this.inner = inner;
            this.owner = owner;
        }

        private void reformUri(ref Uri uri)
        {
            if (!uri.IsAbsoluteUri)
            {
                uri = new Uri(this.owner.Uris.RootUri, uri);
            }
        }

        private HttpClient inner;
        private Client owner;

        public HttpRequestHeaderCollection DefaultRequestHeaders => this.inner.DefaultRequestHeaders;

        public IHttpAsyncOperation GetAsync(Uri uri, HttpCompletionOption completionOption, bool checkStatusCode)
        {
            reformUri(ref uri);
            var request = this.inner.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);
            if (completionOption == HttpCompletionOption.ResponseHeadersRead)
                return request;
            return Run<HttpResponseMessage, HttpProgress>(async (token, progress) =>
            {
                token.Register(request.Cancel);
                request.Progress = (t, p) => progress.Report(p);
                var response = await request;
                if (response.Content.Headers.ContentDisposition?.FileName == "sadpanda.jpg")
                {
                    this.owner.ResetExCookie();
                    throw new InvalidOperationException(LocalizedStrings.Resources.SadPanda);
                }
                if (checkStatusCode)
                    response.EnsureSuccessStatusCode();
                var buffer = response.Content.BufferAllAsync();
                if (!response.Content.TryComputeLength(out var length))
                {
                    var contentLength = response.Content.Headers.ContentLength;
                    if (contentLength.HasValue)
                        length = contentLength.Value;
                    else
                        length = ulong.MaxValue;
                }
                buffer.Progress = (t, p) =>
                {
                    progress.Report(new HttpProgress
                    {
                        TotalBytesToReceive = length,
                        BytesReceived = p,
                        Stage = HttpProgressStage.ReceivingContent
                    });
                };
                await buffer;
                return response;
            });
        }

        public IHttpAsyncOperation GetAsync(Uri uri)
        {
            return this.GetAsync(uri, HttpCompletionOption.ResponseContentRead, true);
        }

        public IAsyncOperationWithProgress<IBuffer, HttpProgress> GetBufferAsync(Uri uri)
        {
            reformUri(ref uri);
            return Run<IBuffer, HttpProgress>(async (token, progress) =>
            {
                var request = GetAsync(uri);
                token.Register(request.Cancel);
                request.Progress = (t, p) => progress.Report(p);
                var response = await request;
                return await response.Content.ReadAsBufferAsync();
            });
        }

        public IAsyncOperationWithProgress<IInputStream, HttpProgress> GetInputStreamAsync(Uri uri)
        {
            reformUri(ref uri);
            return Run<IInputStream, HttpProgress>(async (token, progress) =>
            {
                var request = GetAsync(uri);
                token.Register(request.Cancel);
                request.Progress = (t, p) => progress.Report(p);
                var response = await request;
                return await response.Content.ReadAsInputStreamAsync();
            });
        }

        public IAsyncOperationWithProgress<string, HttpProgress> GetStringAsync(Uri uri)
        {
            reformUri(ref uri);
            return Run<string, HttpProgress>(async (token, progress) =>
            {
                var request = GetAsync(uri);
                token.Register(request.Cancel);
                request.Progress = (t, p) => progress.Report(p);
                var response = await request;
                return await response.Content.ReadAsStringAsync();
            });
        }

        public IAsyncOperationWithProgress<HtmlDocument, HttpProgress> GetDocumentAsync(Uri uri)
        {
            reformUri(ref uri);
            return Run<HtmlDocument, HttpProgress>(async (token, progress) =>
            {
                var request = GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, false);
                token.Register(request.Cancel);
                request.Progress = (t, p) => progress.Report(p);
                var doc = new HtmlDocument();
                var response = await request;
                using (var stream = (await response.Content.ReadAsInputStreamAsync()).AsStreamForRead())
                {
                    doc.Load(stream);
                }
                do
                {
                    if (response.StatusCode != HttpStatusCode.NotFound)
                        break;
                    var title = doc.DocumentNode.Element("html").Element("head").Element("title");
                    if (title == null)
                        break;
                    if (!title.InnerText.DeEntitize().StartsWith("Gallery Not Available - "))
                        break;
                    var error = doc.DocumentNode.Element("html").Element("body")?.Element("div")?.Element("p");
                    if (error == null)
                        break;
                    var msg = error.InnerText.DeEntitize();
                    switch (msg)
                    {
                    case "This gallery has been removed or is unavailable.":
                        throw new InvalidOperationException(LocalizedStrings.Resources.GalleryRemoved);
                    case "This gallery has been locked for review. Please check back later.":
                        throw new InvalidOperationException(LocalizedStrings.Resources.GalleryReviewing);
                    default:
                        throw new InvalidOperationException(msg);
                    }
                } while (false);
                response.EnsureSuccessStatusCode();
                if (HentaiVerseInfo.IsEnabled)
                    HentaiVerseInfo.AnalyzePage(doc);
                return doc;
            });
        }

        public IHttpAsyncOperation PostAsync(Uri uri, IHttpContent content)
        {
            reformUri(ref uri);
            return this.inner.PostAsync(uri, content);
        }

        public IAsyncOperationWithProgress<string, HttpProgress> PostStringAsync(Uri uri, string content)
        {
            reformUri(ref uri);
            return Run<string, HttpProgress>(async (token, progress) =>
            {
                var op = PostAsync(uri, content == null ? null : new HttpStringContent(content));
                token.Register(op.Cancel);
                op.Progress = (sender, value) => progress.Report(value);
                var res = await op;
                res.EnsureSuccessStatusCode();
                return await res.Content.ReadAsStringAsync();
            });
        }

        public IHttpAsyncOperation PutAsync(Uri uri, IHttpContent content)
        {
            reformUri(ref uri);
            return this.inner.PutAsync(uri, content);
        }

        public void Dispose()
        {
            this.inner.Dispose();
        }
    }
}
