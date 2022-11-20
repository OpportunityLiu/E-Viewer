using System;
using System.Linq;

using Windows.Foundation;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

using IHttpAsyncOperation = Windows.Foundation.IAsyncOperationWithProgress<Windows.Web.Http.HttpResponseMessage, Windows.Web.Http.HttpProgress>;

namespace ExClient.Internal
{
    internal class RedirectFilter : IHttpFilter
    {
        public RedirectFilter(IHttpFilter innerFilter)
        {
            inner = innerFilter;
        }

        IHttpFilter inner;

        private class HttpAsyncOperation : IHttpAsyncOperation
        {
            private RedirectFilter parent;
            private HttpRequestMessage request;
            private IHttpAsyncOperation current;

            private void current_Completed(IHttpAsyncOperation asyncInfo, AsyncStatus asyncStatus)
            {
                if (asyncInfo != current)
                {
                    return;
                }

                if (asyncStatus == AsyncStatus.Completed)
                {
                    var response = asyncInfo.GetResults();
                    if (needRedirect(response))
                    {
                        asyncStatus = AsyncStatus.Started;
                        buildNewRequest(response);
                        sendRequest();
                    }
                }
                Status = asyncStatus;
                if (asyncStatus != AsyncStatus.Started)
                {
                    Completed?.Invoke(this, asyncStatus);
                }
            }

            private void sendRequest()
            {
                current = parent.inner.SendRequestAsync(request);
                current.Progress = current_Progress;
                current.Completed = current_Completed;
            }

            private void current_Progress(IHttpAsyncOperation asyncInfo, HttpProgress progressInfo)
            {
                if (asyncInfo != current)
                {
                    return;
                }

                Progress?.Invoke(this, progressInfo);
            }

            public HttpAsyncOperation(RedirectFilter parent, HttpRequestMessage request)
            {
                this.parent = parent;
                this.request = request;
                sendRequest();
            }

            private void buildNewRequest(HttpResponseMessage response)
            {
                var newRequest = new HttpRequestMessage { RequestUri = response.Headers.Location };
                var oldRequest = request;
                if ((response.StatusCode == HttpStatusCode.Found || response.StatusCode == HttpStatusCode.SeeOther) && oldRequest.Method == HttpMethod.Post)
                {
                    newRequest.Method = HttpMethod.Get;
                }
                else
                {
                    newRequest.Method = oldRequest.Method;
                }

                foreach (var item in oldRequest.Headers.ToList())
                {
                    newRequest.Headers.Add(item);
                }
                foreach (var item in oldRequest.Properties.ToList())
                {
                    newRequest.Properties.Add(item);
                }
                request = newRequest;
            }

            private static bool needRedirect(HttpResponseMessage response)
            {
                return response.StatusCode == HttpStatusCode.MultipleChoices ||
                   response.StatusCode == HttpStatusCode.MovedPermanently ||
                   response.StatusCode == HttpStatusCode.Found ||
                   response.StatusCode == HttpStatusCode.SeeOther ||
                   response.StatusCode == HttpStatusCode.TemporaryRedirect ||
                   response.StatusCode == HttpStatusCode.PermanentRedirect;
            }

            public AsyncOperationWithProgressCompletedHandler<HttpResponseMessage, HttpProgress> Completed
            {
                get;
                set;
            }

            public Exception ErrorCode => current?.ErrorCode;

            public uint Id => current.Id;

            public AsyncOperationProgressHandler<HttpResponseMessage, HttpProgress> Progress
            {
                get;
                set;
            }

            public AsyncStatus Status
            {
                get;
                private set;
            }

            public void Cancel()
            {
                current.Cancel();
                Status = AsyncStatus.Canceled;
            }

            public void Close()
            {
                current?.Close();
                parent = null;
                request = null;
            }

            public HttpResponseMessage GetResults()
            {
                return current.GetResults();
            }
        }

        //private static Dictionary<string, string> _Host = new Dictionary<string, string>
        //{
        //    ["exhentai.org"] = "178.175.128.252",
        //    ["e-hentai.org"] = "104.20.27.25",
        //    ["api.e-hentai.org"] = "37.48.89.16",
        //};

        public IHttpAsyncOperation SendRequestAsync(HttpRequestMessage request)
        {
            //var uri = request.RequestUri;
            //request.Headers.Host = new Windows.Networking.HostName(uri.Host);
            //if (_Host.TryGetValue(uri.Host, out var ip))
            //{
            //    var newUri = new UriBuilder(uri);
            //    newUri.Host = ip;
            //    request.RequestUri = newUri.Uri;
            //}
            return new HttpAsyncOperation(this, request);
        }

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    inner.Dispose();
                }
                inner = null;
                disposedValue = true;
            }
        }

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
