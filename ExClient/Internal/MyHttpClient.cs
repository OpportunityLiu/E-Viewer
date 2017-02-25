using System;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.Web.Http;
using Windows.Web.Http.Headers;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;
using IHttpAsyncOperation = Windows.Foundation.IAsyncOperationWithProgress<Windows.Web.Http.HttpResponseMessage, Windows.Web.Http.HttpProgress>;

namespace ExClient.Internal
{
    internal class MyHttpClient : IDisposable
    {
        public MyHttpClient(HttpClient inner)
        {
            this.inner = inner;
        }

        private HttpClient inner;

        public HttpRequestHeaderCollection DefaultRequestHeaders => inner.DefaultRequestHeaders;

        public IHttpAsyncOperation DeleteAsync(Uri uri)
        {
            return inner.DeleteAsync(uri);
        }

        public IHttpAsyncOperation GetAsync(Uri uri)
        {
            return this.GetAsync(uri, HttpCompletionOption.ResponseContentRead);
        }

        public IHttpAsyncOperation GetAsync(Uri uri, HttpCompletionOption completionOption)
        {
            var request = inner.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);
            if(completionOption == HttpCompletionOption.ResponseHeadersRead)
                return request;
            return Run<HttpResponseMessage, HttpProgress>(async (token, progress) =>
            {
                token.Register(request.Cancel);
                var response = await request;
                var buffer = response.Content.BufferAllAsync();
                var length = 0ul;
                if(!response.Content.TryComputeLength(out length))
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
            return Run<string, HttpProgress>(async (token, progress) =>
            {
                var request = GetAsync(uri);
                token.Register(request.Cancel);
                request.Progress = (t, p) => progress.Report(p);
                var response = await request;
                return await response.Content.ReadAsStringAsync();
            });
        }

        public IHttpAsyncOperation PostAsync(Uri uri, IHttpContent content)
        {
            return inner.PostAsync(uri, content);
        }

        public IHttpAsyncOperation PutAsync(Uri uri, IHttpContent content)
        {
            return inner.PutAsync(uri, content);
        }

        public void Dispose()
        {
            inner.Dispose();
        }
    }
}
