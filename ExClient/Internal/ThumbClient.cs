using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Web.Http;

namespace ExClient.Internal
{
    internal static class ThumbClient
    {
        private static readonly HttpClient client = new HttpClient();
        private static readonly Regex thumbUriRegex = new Regex(@"^(http|https)://(ehgt\.org(/t|)|exhentai\.org/t|ul\.ehgt\.org(/t|))/(.+)$", RegexOptions.Compiled | RegexOptions.Singleline);

        public static Uri FormatThumbUri(string uri)
        {
            if (uri.IsNullOrWhiteSpace())
                return null;
            var match = thumbUriRegex.Match(uri);
            if (!match.Success)
            {
                return new Uri(uri);
            }
            var tail = match.Groups[5].Value;
            return new Uri("https://ul.ehgt.org/" + tail);
        }

        public static Uri FormatThumbUri(Uri uri) => FormatThumbUri(uri?.ToString());

        public static IAsyncOperation<bool> FetchThumbAsync(Uri source, BitmapImage target)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (target is null)
                throw new ArgumentNullException(nameof(target));
            return AsyncInfo.Run(async token =>
            {
                var match = thumbUriRegex.Match(source.ToString());
                if (!match.Success)
                {
                    return await loadThumbAsync(source, target);
                }
                var tail = match.Groups[5].Value;
                return
                    await loadThumbAsync(new Uri("https://ehgt.org/" + tail), target) ||
                    await loadThumbAsync(new Uri("https://ul.ehgt.org/" + tail), target) ||
                    await loadThumbAsync(new Uri("https://exhentai.org/t/" + tail), target) ||
                    await loadThumbAsync(new Uri("https://ehgt.org/t/" + tail), target) ||
                    await loadThumbAsync(new Uri("https://ul.ehgt.org/t/" + tail), target) ||
                    false;
            });
        }

        private static async Task<bool> loadThumbAsync(Uri source, BitmapImage target)
        {
            try
            {
                var buf = await client.GetBufferAsync(source);
                using (var stream = buf.AsRandomAccessStream())
                {
                    await target.SetSourceAsync(stream);
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
}
