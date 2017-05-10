using System;
using System.Linq;
using System.Text.RegularExpressions;
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
            this.inner = innerFilter;
        }

        IHttpFilter inner;

        private class HttpAsyncOperation : IHttpAsyncOperation
        {
            private RedirectFilter parent;
            private HttpRequestMessage request;
            private IHttpAsyncOperation current;

            private void current_Completed(IHttpAsyncOperation asyncInfo, AsyncStatus asyncStatus)
            {
                if(asyncInfo != this.current)
                    return;
                if(asyncStatus == AsyncStatus.Completed)
                {
                    var response = asyncInfo.GetResults();
                    if(needRedirect(response))
                    {
                        asyncStatus = AsyncStatus.Started;
                        buildNewRequest(response);
                        sendRequest();
                    }
                }
                this.Status = asyncStatus;
                if(asyncStatus != AsyncStatus.Started)
                {
                    this.Completed?.Invoke(this, asyncStatus);
                }
            }

            private void sendRequest()
            {
                this.current = this.parent.inner.SendRequestAsync(this.request);
                this.current.Progress = this.current_Progress;
                this.current.Completed = this.current_Completed;
            }

            private void current_Progress(IHttpAsyncOperation asyncInfo, HttpProgress progressInfo)
            {
                if(asyncInfo != this.current)
                    return;
                this.Progress?.Invoke(this, progressInfo);
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
                var oldRequest = this.request;
                if((response.StatusCode == HttpStatusCode.Found || response.StatusCode == HttpStatusCode.SeeOther) && oldRequest.Method == HttpMethod.Post)
                    newRequest.Method = HttpMethod.Get;
                else
                    newRequest.Method = oldRequest.Method;
                foreach(var item in oldRequest.Headers.ToList())
                {
                    newRequest.Headers.Add(item);
                }
                foreach(var item in oldRequest.Properties.ToList())
                {
                    newRequest.Properties.Add(item);
                }
                this.request = newRequest;
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

            public Exception ErrorCode => this.current?.ErrorCode;

            public uint Id => this.current.Id;

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
                this.current.Cancel();
                this.Status = AsyncStatus.Canceled;
            }

            public void Close()
            {
                this.current?.Close();
                this.parent = null;
                this.request = null;
            }

            public HttpResponseMessage GetResults()
            {
                return this.current.GetResults();
            }
        }

        public IHttpAsyncOperation SendRequestAsync(HttpRequestMessage request)
        {
            return new HttpAsyncOperation(this, request);
        }

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if(!this.disposedValue)
            {
                if(disposing)
                {
                    this.inner.Dispose();
                }
                this.inner = null;
                this.disposedValue = true;
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
