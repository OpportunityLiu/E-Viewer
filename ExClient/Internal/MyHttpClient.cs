using ExClient.Api;
using Newtonsoft.Json;
using System;
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
            if(!uri.IsAbsoluteUri)
            {
                uri = new Uri(this.owner.Uris.RootUri, uri);
            }
        }

        private HttpClient inner;
        private Client owner;

        public HttpRequestHeaderCollection DefaultRequestHeaders => inner.DefaultRequestHeaders;

        public IHttpAsyncOperation GetAsync(Uri uri)
        {
            return this.GetAsync(uri, HttpCompletionOption.ResponseContentRead);
        }

        public IHttpAsyncOperation GetAsync(Uri uri, HttpCompletionOption completionOption)
        {
            reformUri(ref uri);
            var request = this.inner.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);
            if(completionOption == HttpCompletionOption.ResponseHeadersRead)
                return request;
            return Run<HttpResponseMessage, HttpProgress>(async (token, progress) =>
            {
                token.Register(request.Cancel);
                request.Progress = (t, p) => progress.Report(p);
                var response = await request;
                response.EnsureSuccessStatusCode();
                var buffer = response.Content.BufferAllAsync();
                if(!response.Content.TryComputeLength(out var length))
                {
                    var contentLength = response.Content.Headers.ContentLength;
                    if(contentLength.HasValue)
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

        public IAsyncOperationWithProgress<IBuffer, HttpProgress> GetBufferAsync(Uri uri)
        {
            reformUri(ref uri);
            return Run<IBuffer, HttpProgress>(async (token, progress) =>
            {
                var request = GetAsync(uri);
                token.Register(request.Cancel);
                request.Progress = (t, p) => progress.Report(p);
                var response = await request;
                response.EnsureSuccessStatusCode();
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
                response.EnsureSuccessStatusCode();
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
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
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

        public IAsyncOperationWithProgress<string, HttpProgress> PostApiAsync(ApiRequest request)
        {
            return PostStringAsync(this.owner.Uris.ApiUri, JsonConvert.SerializeObject(request));
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
