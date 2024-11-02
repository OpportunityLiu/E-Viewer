using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Web.Http;

namespace ExClient.Internal {
    internal static class ThumbClient {
        private static readonly HttpClient _Client = new();
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

        public static IAsyncOperation<bool> FetchThumbAsync(Uri source, BitmapImage target) {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (target is null)
                throw new ArgumentNullException(nameof(target));
            return AsyncInfo.Run(async token => {
                var tail = _GetTail(source.ToString());
                if (tail.IsNullOrEmpty()) {
                    return await loadThumbAsync(source, target);
                }
                return
                    await loadThumbAsync(new Uri("https://ehgt.org/" + tail), target) ||
                    await loadThumbAsync(new Uri("https://ul.ehgt.org/" + tail), target) ||
                    await loadThumbAsync(new Uri("https://exhentai.org/t/" + tail), target) ||
                    await loadThumbAsync(new Uri("https://ehgt.org/t/" + tail), target) ||
                    await loadThumbAsync(new Uri("https://ul.ehgt.org/t/" + tail), target) ||
                    false;
            });
        }

        private static async Task<bool> loadThumbAsync(Uri source, BitmapImage target) {
            try {
                var buf = await _Client.GetBufferAsync(source);
                using (var stream = buf.AsRandomAccessStream()) {
                    await target.SetSourceAsync(stream);
                }
            } catch (Exception) {
                return false;
            }
            return true;
        }
    }
}
