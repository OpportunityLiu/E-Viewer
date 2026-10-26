using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Windows.Foundation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Web.Http;

namespace ExClient.Internal {
    internal static class ThumbClient {
        private static readonly HttpClient _Client = new();
        private const int MaxResponseBytes = 16 * 1024 * 1024;
        private static readonly SemaphoreSlim Downloads = new SemaphoreSlim(4);
        private static readonly SemaphoreSlim Loads = new SemaphoreSlim(8);
        private static readonly ThumbnailDownloadCache Cache = new ThumbnailDownloadCache(
            DownloadAsync, 32 * 1024 * 1024, 8, 64, TimeSpan.FromMinutes(2));
        private static readonly Regex _ThumbUriRegex = new(@"^(http|https)://((ul\.|)ehgt\.org(/t|)|(s\.|)exhentai\.org/t)/(?<path>.+)$", RegexOptions.Compiled | RegexOptions.Singleline);

        private static string _GetTail(string uri) {
            var match = _ThumbUriRegex.Match(uri);
            if (!match.Success) {
                return null;
            }
            return match.Groups["tail"].Value;
        }

        public static Uri FormatThumbUri(string uri) {
            if (uri.IsNullOrWhiteSpace())
                return null;
            var tail = _GetTail(uri);
            if (tail.IsNullOrEmpty())
                return new Uri(uri);
            return new Uri("https://ehgt.org/" + tail);
        }

        public static Uri FormatThumbUri(Uri uri) => FormatThumbUri(uri?.ToString());

        public static IAsyncOperation<bool> FetchThumbAsync(Uri source, BitmapImage target)
            => FetchThumbAsync(source == null ? null : new ThumbnailSource(source), target);

        internal static IAsyncOperation<bool> FetchThumbAsync(ThumbnailSource source, BitmapImage target) {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (target is null)
                throw new ArgumentNullException(nameof(target));
            return AsyncInfo.Run(async token => {
                var tail = _GetTail(source.Uri.ToString());
                if (tail.IsNullOrEmpty()) {
                    return await loadThumbAsync(source, target);
                }
                return
                    await loadThumbAsync(source.WithUri(new Uri("https://ehgt.org/" + tail)), target) ||
                    await loadThumbAsync(source.WithUri(new Uri("https://ul.ehgt.org/" + tail)), target) ||
                    await loadThumbAsync(source.WithUri(new Uri("https://exhentai.org/t/" + tail)), target) ||
                    await loadThumbAsync(source.WithUri(new Uri("https://ehgt.org/t/" + tail)), target) ||
                    await loadThumbAsync(source.WithUri(new Uri("https://ul.ehgt.org/t/" + tail)), target) ||
                    false;
            });
        }

        private static async Task<bool> loadThumbAsync(ThumbnailSource source, BitmapImage target) {
            await Loads.WaitAsync();
            try {
                return await ThumbnailBitmap.LoadAsync(await Cache.GetAsync(source.Uri), source.Region, target);
            } finally {
                Loads.Release();
            }
        }

        private static async Task<byte[]> DownloadAsync(Uri source) {
            await Downloads.WaitAsync().ConfigureAwait(false);
            try {
                using (var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30)))
                using (var response = await _Client.GetAsync(source, HttpCompletionOption.ResponseHeadersRead).AsTask(timeout.Token).ConfigureAwait(false)) {
                    response.EnsureSuccessStatusCode();
                    if (response.Content.Headers.ContentLength > MaxResponseBytes) return null;
                    using (var input = (await response.Content.ReadAsInputStreamAsync().AsTask(timeout.Token).ConfigureAwait(false)).AsStreamForRead())
                    using (var output = new MemoryStream()) {
                        var buffer = new byte[64 * 1024];
                        int count;
                        while ((count = await input.ReadAsync(buffer, 0, buffer.Length, timeout.Token).ConfigureAwait(false)) != 0) {
                            if (output.Length > MaxResponseBytes - count) return null;
                            output.Write(buffer, 0, count);
                        }
                        return output.ToArray();
                    }
                }
            } finally {
                Downloads.Release();
            }
        }
    }
}
