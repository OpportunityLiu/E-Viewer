using ExClient.Api;
using ExClient.Galleries;
using ExClient.Internal;
using HtmlAgilityPack;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Web.Http;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;

namespace ExClient.Services
{
    public static class TorrentsExtension
    {
        public static IAsyncOperation<ReadOnlyCollection<TorrentInfo>> FetchTorrnetsAsync(this GalleryInfo galleryInfo)
            => TorrentInfo.FetchAsync(galleryInfo);

        public static IAsyncOperation<ReadOnlyCollection<TorrentInfo>> FetchTorrnetsAsync(this Gallery gallery)
            => TorrentInfo.FetchAsync(gallery);
    }

    public sealed class TorrentInfo
    {
        private static readonly Regex infoMatcher = new Regex(@"\s+Posted:\s([-\d:\s]+)\s+Size:\s([\d\.]+\s+[KMG]?B)\s+Seeds:\s(\d+)\s+Peers:\s(\d+)\s+Downloads:\s(\d+)\s+Uploader:\s+(.+)\s+", RegexOptions.Compiled);

        internal static IAsyncOperation<ReadOnlyCollection<TorrentInfo>> FetchAsync(GalleryInfo galleryInfo)
        {
            return Task.Run(async () =>
            {
                var torrentUri = new Uri(Client.Current.Uris.RootUri, $"gallerytorrents.php?gid={galleryInfo.ID}&t={galleryInfo.Token.ToTokenString()}");
                var doc = await Client.Current.HttpClient.GetDocumentAsync(torrentUri);
                if (doc.DocumentNode.ChildNodes.Count == 1 && doc.DocumentNode.FirstChild.NodeType == HtmlNodeType.Text)
                    throw new InvalidOperationException(doc.DocumentNode.FirstChild.InnerText);
                var nodes = from n in doc.DocumentNode.Descendants("table")
                            where n.GetAttribute("style", "") == "width:99%"
                            let reg = infoMatcher.Match(n.GetInnerText())
                            let name = n.Descendants("tr").Last()
                            let link = name.Descendants("a").SingleOrDefault()
                            select new TorrentInfo()
                            {
                                Name = name.GetInnerText().Trim(),
                                Posted = DateTimeOffset.Parse(reg.Groups[1].Value, System.Globalization.CultureInfo.CurrentCulture, System.Globalization.DateTimeStyles.AssumeUniversal),
                                Size = parseSize(reg.Groups[2].Value),
                                Seeds = int.Parse(reg.Groups[3].Value),
                                Peers = int.Parse(reg.Groups[4].Value),
                                Downloads = int.Parse(reg.Groups[5].Value),
                                Uploader = reg.Groups[6].Value,
                                TorrentUri = link?.GetAttribute("href", default(Uri)),
                            };
                return nodes.ToList().AsReadOnly();
            }).AsAsyncOperation();
        }

        private static long parseSize(string sizeStr)
        {
            var s = sizeStr.Split(' ');
            var value = double.Parse(s[0]);
            switch (s[1])
            {
            case "B":
                return (long)value;
            case "KB":
                return (long)(value * (1 << 10));
            case "MB":
                return (long)(value * (1 << 20));
            case "GB":
                return (long)(value * (1 << 30));
            default:
                return 0;
            }
        }

        private TorrentInfo() { }

        public string Name { get; private set; }

        public DateTimeOffset Posted { get; private set; }

        public long Size { get; private set; }

        public int Seeds { get; private set; }

        public int Peers { get; private set; }

        public int Downloads { get; private set; }

        public string Uploader { get; private set; }

        public Uri TorrentUri { get; private set; }

        public IAsyncOperation<StorageFile> DownloadTorrentAsync()
        {
            return Run(async token =>
            {
                if (this.TorrentUri is null)
                    throw new InvalidOperationException(LocalizedStrings.Resources.ExpungedTorrent);

                using (var client = new HttpClient())
                {
                    var loadT = client.GetBufferAsync(this.TorrentUri);
                    var filename = StorageHelper.ToValidFileName(this.Name + ".torrent");
                    var folder = await StorageHelper.CreateTempFolderAsync();
                    var buf = await loadT;
                    try
                    {
                        return await folder.SaveFileAsync(filename, CreationCollisionOption.ReplaceExisting, buf);
                    }
                    catch (Exception)
                    {
                        return await folder.SaveFileAsync("gallery.torrent", CreationCollisionOption.ReplaceExisting, buf);
                    }
                }
            });
        }
    }
}
