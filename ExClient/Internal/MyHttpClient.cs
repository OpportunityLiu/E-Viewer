﻿using ExClient.Api;
using ExClient.HentaiVerse;

using HtmlAgilityPack;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.Web.Http;
using Windows.Web.Http.Filters;
using Windows.Web.Http.Headers;

using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;

using IHttpAsyncOperation = Windows.Foundation.IAsyncOperationWithProgress<Windows.Web.Http.HttpResponseMessage, Windows.Web.Http.HttpProgress>;

namespace ExClient.Internal {
    /*
     * 由于使用了自定义 Filter 后发生异常会丢失异常详细信息，故使用此类封装，以保留异常信息。
     * */
    internal class MyHttpClient : IDisposable {
        private void _CheckStringResponse(string responseString) {
            if (responseString.Length > 200)
                return;
            if (responseString.Contains("This gallery is currently unavailable."))
                throw new InvalidOperationException(LocalizedStrings.Resources.GalleryRemoved);
            if (responseString.Contains("Your IP address has been temporarily banned for excessive pageloads"))
                throw new InvalidOperationException(LocalizedStrings.Resources.IPBannedOfPageLoad);
            if (responseString.Contains("This page is currently not available, as your account has been suspended."))
                throw new InvalidOperationException(LocalizedStrings.Resources.AccountSuspended);
            if (responseString.Contains("https://exhentai.org/img/kokomade.jpg")) {
                _ = _Owner.ResetExCookieAsync();
                throw new InvalidOperationException(LocalizedStrings.Resources.Kokomade);
            }
        }

        private void _CheckSadPanda(HttpResponseMessage response) {
            if (response.Content.Headers.ContentDisposition?.FileName == "sadpanda.jpg") {
                _ = _Owner.ResetExCookieAsync();
                throw new InvalidOperationException(LocalizedStrings.Resources.SadPanda);
            }
        }

        public MyHttpClient(Client owner, HttpClient inner) {
            _Inner = inner;
            _Owner = owner;
            _Nocookie = new HttpClient(new HttpBaseProtocolFilter {
                CookieUsageBehavior = HttpCookieUsageBehavior.NoCookies,
            });

            var ua = new HttpProductInfoHeaderValue(Package.Current.Id.Name, Package.Current.Id.Version.ToVersion().ToString());
            _Inner.DefaultRequestHeaders.UserAgent.Add(ua);
            _Nocookie.DefaultRequestHeaders.UserAgent.Add(ua);
        }

        private void _ReformUri(ref Uri uri) {
            if (!uri.IsAbsoluteUri)
                uri = new Uri(_Owner.Uris.RootUri, uri);
        }

        private readonly HttpClient _Inner;
        private readonly HttpClient _Nocookie;
        private readonly Client _Owner;

        public HttpRequestHeaderCollection DefaultRequestHeaders => _Inner.DefaultRequestHeaders;

        public IHttpAsyncOperation GetAsync(Uri uri, HttpCompletionOption completionOption, bool checkStatusCode) {
            _ReformUri(ref uri);
            var client = uri.Host.EndsWith("hentai.org") ? _Inner : _Nocookie;
            var request = client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);
            if (completionOption == HttpCompletionOption.ResponseHeadersRead) {
                return request;
            }

            return Run<HttpResponseMessage, HttpProgress>(async (token, progress) => {
                token.Register(request.Cancel);
                request.Progress = (t, p) => progress.Report(p);
                var response = await request;
                _CheckSadPanda(response);
                if (checkStatusCode) {
                    response.EnsureSuccessStatusCode();
                }

                var buffer = response.Content.BufferAllAsync();
                if (!response.Content.TryComputeLength(out var length)) {
                    var contentLength = response.Content.Headers.ContentLength;
                    length = contentLength ?? ulong.MaxValue;
                }
                buffer.Progress = (t, p) => {
                    progress.Report(new HttpProgress {
                        TotalBytesToReceive = length,
                        BytesReceived = p,
                        Stage = HttpProgressStage.ReceivingContent
                    });
                };
                await buffer;
                return response;
            });
        }

        public IHttpAsyncOperation GetAsync(Uri uri) {
            return GetAsync(uri, HttpCompletionOption.ResponseContentRead, true);
        }

        public IAsyncOperationWithProgress<IBuffer, HttpProgress> GetBufferAsync(Uri uri) {
            _ReformUri(ref uri);
            return Run<IBuffer, HttpProgress>(async (token, progress) => {
                var request = GetAsync(uri);
                token.Register(request.Cancel);
                request.Progress = (t, p) => progress.Report(p);
                var response = await request;
                return await response.Content.ReadAsBufferAsync();
            });
        }

        public IAsyncOperationWithProgress<IInputStream, HttpProgress> GetInputStreamAsync(Uri uri) {
            _ReformUri(ref uri);
            return Run<IInputStream, HttpProgress>(async (token, progress) => {
                var request = GetAsync(uri);
                token.Register(request.Cancel);
                request.Progress = (t, p) => progress.Report(p);
                var response = await request;
                return await response.Content.ReadAsInputStreamAsync();
            });
        }

        public IAsyncOperationWithProgress<string, HttpProgress> GetStringAsync(Uri uri) {
            _ReformUri(ref uri);
            return Run<string, HttpProgress>(async (token, progress) => {
                var request = GetAsync(uri);
                token.Register(request.Cancel);
                request.Progress = (t, p) => progress.Report(p);
                var response = await request;
                var str = await response.Content.ReadAsStringAsync();
                _CheckStringResponse(str);
                return str;
            });
        }

        public IAsyncOperationWithProgress<HtmlDocument, HttpProgress> GetDocumentAsync(Uri uri) {
            _ReformUri(ref uri);
            return Run<HtmlDocument, HttpProgress>(async (token, progress) => {
                var request = GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, false);
                token.Register(request.Cancel);
                request.Progress = (t, p) => progress.Report(p);
                var doc = new HtmlDocument();
                var response = await request;
                _CheckSadPanda(response);
                var resStream = await response.Content.ReadAsInputStreamAsync();
                using var reader = new StreamReader(resStream.AsStreamForRead(), Encoding.UTF8);
                var resStr = reader.ReadToEnd();
                _CheckStringResponse(resStr);
                doc.LoadHtml(resStr);
                var rootNode = doc.DocumentNode;

                do {
                    if (response.StatusCode != HttpStatusCode.NotFound)
                        break;

                    var title = rootNode.Element("html").Element("head").Element("title");
                    if (title is null)
                        break;
                    if (!title.GetInnerText().StartsWith("Gallery Not Available - "))
                        break;

                    var error = rootNode.Element("html").Element("body")?.Element("div")?.Element("p");
                    if (error is null)
                        break;

                    var msg = error.GetInnerText();
                    switch (msg) {
                        case "This gallery has been removed or is unavailable.":
                            throw new InvalidOperationException(LocalizedStrings.Resources.GalleryRemoved);
                        case "This gallery has been locked for review. Please check back later.":
                            throw new InvalidOperationException(LocalizedStrings.Resources.GalleryReviewing);
                        default:
                            throw new InvalidOperationException(msg);
                    }
                } while (false);
                response.EnsureSuccessStatusCode();
                HentaiVerseInfo.AnalyzePage(doc);
                ApiToken.Update(resStr);
                return doc;
            });
        }

        public IHttpAsyncOperation PostAsync(Uri uri, IHttpContent content) {
            _ReformUri(ref uri);
            return _Inner.PostAsync(uri, content);
        }

        public IHttpAsyncOperation PostAsync(Uri uri, params KeyValuePair<string, string>[] content)
            => PostAsync(uri, (IEnumerable<KeyValuePair<string, string>>)content);

        public IHttpAsyncOperation PostAsync(Uri uri, IEnumerable<KeyValuePair<string, string>> content) {
            _ReformUri(ref uri);
            return _Inner.PostAsync(uri, new HttpFormUrlEncodedContent(content));
        }

        public IAsyncOperationWithProgress<string, HttpProgress> PostStringAsync(Uri uri, IHttpContent content) {
            _ReformUri(ref uri);
            return Run<string, HttpProgress>(async (token, progress) => {
                var op = PostAsync(uri, content);
                token.Register(op.Cancel);
                op.Progress = (sender, value) => progress.Report(value);
                var res = await op;
                res.EnsureSuccessStatusCode();
                var str = await res.Content.ReadAsStringAsync();
                _CheckStringResponse(str);
                return str;
            });
        }

        public void Dispose() {
            _Inner.Dispose();
            _Nocookie.Dispose();
        }
    }
}
