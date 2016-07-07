using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Web.Http;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;
using HtmlAgilityPack;

namespace ExClient
{
    public class TorrentInfo
    {
        private static readonly Regex infoMatcher = new Regex(@"\s+Posted:\s([-\d:\s]+)\s+Size:\s([\d\.]+\s+[KMG]?B)\s+Seeds:\s(\d+)\s+Peers:\s(\d+)\s+Downloads:\s(\d+)\s+Uploader:\s+(.+)\s+", RegexOptions.Compiled);

        internal static IAsyncOperation<List<TorrentInfo>> LoadTorrentsAsync(Gallery gallery)
        {
            return Task.Run(async () =>
            {
                var torrentHtml = await gallery.Owner.HttpClient.GetStringAsync(new Uri($"http://exhentai.org/gallerytorrents.php?gid={gallery.Id}&t={gallery.Token}"));
                var doc = new HtmlDocument();
                doc.LoadHtml(torrentHtml);
                var nodes = (from n in doc.DocumentNode.Descendants("table")
                             where n.GetAttributeValue("style", "") == "width:99%"
                             let reg = infoMatcher.Match(n.InnerText)
                             let link = n.Descendants("a").Single()
                             select new TorrentInfo()
                             {
                                 Name = link.InnerText,
                                 Posted = DateTimeOffset.Parse(reg.Groups[1].Value, System.Globalization.CultureInfo.CurrentCulture, System.Globalization.DateTimeStyles.AssumeUniversal),
                                 Size = parseSize(reg.Groups[2].Value),
                                 Seeds = int.Parse(reg.Groups[3].Value),
                                 Peers = int.Parse(reg.Groups[4].Value),
                                 Downloads = int.Parse(reg.Groups[5].Value),
                                 Uploader = reg.Groups[6].Value,
                                 TorrentUri = new Uri(link.GetAttributeValue("href", ""))
                             }).ToList();
                return nodes;
            }).AsAsyncOperation();
        }

        private static long parseSize(string sizeStr)
        {
            var s = sizeStr.Split(' ');
            var value = double.Parse(s[0]);
            switch(s[1])
            {
            case "B":
                return (long)value;
            case "KB":
                return (long)(value * 1024);
            case "MB":
                return (long)(value * 1024 * 1024);
            case "GB":
                return (long)(value * 1024 * 1024 * 1024);
            default:
                return 0;
            }
        }

        private TorrentInfo()
        {
        }

        public string Name
        {
            get;
            private set;
        }

        public DateTimeOffset Posted
        {
            get;
            private set;
        }

        public long Size
        {
            get;
            private set;
        }

        public int Seeds
        {
            get;
            private set;
        }

        public int Peers
        {
            get;
            private set;
        }

        public int Downloads
        {
            get;
            private set;
        }

        public string Uploader
        {
            get;
            private set;
        }

        public Uri TorrentUri
        {
            get;
            private set;
        }

        public IAsyncOperation<StorageFile> LoadTorrentAsync()
        {
            return Run(async token =>
            {
                using(var client = new HttpClient())
                {
                    var filename = StorageHelper.ToValidFolderName(Name + ".torrent");
                    var result = await client.GetBufferAsync(TorrentUri);
                    var file = await (await StorageHelper.CreateTempFolderAsync()).SaveFileAsync(filename, result);
                    return file;
                }
            });
        }
    }
}
