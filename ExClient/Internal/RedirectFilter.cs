using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Web.Http;
using Windows.Web.Http.Filters;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;
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
            private IHttpAsyncOperation _current;

            private IHttpAsyncOperation current
            {
                get
                {
                    return _current;
                }
                set
                {
                    _current = value;
                    _current.Progress = current_Progress;
                    _current.Completed = current_Completed;
                }
            }

            private void current_Completed(IHttpAsyncOperation asyncInfo, AsyncStatus asyncStatus)
            {
                if(asyncInfo != _current)
                    return;
                if(asyncStatus == AsyncStatus.Completed)
                {
                    var response = asyncInfo.GetResults();
                    if(needRedirect(response))
                    {
                        asyncStatus = AsyncStatus.Started;
                        buildNewRequest(response);
                        current = parent.inner.SendRequestAsync(request);
                    }
                }
                this.Status = asyncStatus;
                if(asyncStatus != AsyncStatus.Started)
                {
                    this.Completed?.Invoke(this, asyncStatus);
                }
            }

            private void current_Progress(IHttpAsyncOperation asyncInfo, HttpProgress progressInfo)
            {
                if(asyncInfo != _current)
                    return;
                this.Progress?.Invoke(this, progressInfo);
            }

            public HttpAsyncOperation(RedirectFilter parent, HttpRequestMessage request)
            {
                this.parent = parent;
                this.request = request;
                this.current = parent.inner.SendRequestAsync(request);
            }

            private void buildNewRequest(HttpResponseMessage response)
            {
                var newRequest = new HttpRequestMessage();
                newRequest.RequestUri = response.Headers.Location;
                if((response.StatusCode == HttpStatusCode.Found || response.StatusCode == HttpStatusCode.SeeOther) && request.Method == HttpMethod.Post)
                    newRequest.Method = HttpMethod.Get;
                else
                    newRequest.Method = request.Method;
                foreach(var item in request.Headers.ToList())
                {
                    newRequest.Headers.Add(item);
                }
                foreach(var item in request.Properties.ToList())
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
                this.current.Cancel();
                this.Status = AsyncStatus.Canceled;
            }

            public void Close()
            {
                current?.Close();
                this.parent = null;
                this.request = null;
            }

            public HttpResponseMessage GetResults()
            {
                return current.GetResults();
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
            if(!disposedValue)
            {
                if(disposing)
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
