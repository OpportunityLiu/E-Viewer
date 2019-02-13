using ExClient.Api;
using ExClient.Galleries;
using ExClient.Internal;
using HtmlAgilityPack;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Web.Http;

namespace ExClient.Services
{
    public static class TorrentsExtension
    {
        public static IAsyncOperation<ReadOnlyCollection<TorrentInfo>> FetchTorrnetsAsync(this GalleryInfo galleryInfo)
            => TorrentInfo.FetchAsync(galleryInfo);

        public static IAsyncOperation<ReadOnlyCollection<TorrentInfo>> FetchTorrnetsAsync(this Gallery gallery)
            => TorrentInfo.FetchAsync(gallery);
    }

    public readonly struct TorrentInfo : IEquatable<TorrentInfo>
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
                            select new TorrentInfo(
                                name.GetInnerText().Trim(),
                                DateTimeOffset.Parse(reg.Groups[1].Value, System.Globalization.CultureInfo.CurrentCulture, System.Globalization.DateTimeStyles.AssumeUniversal),
                                parseSize(reg.Groups[2].Value),
                                int.Parse(reg.Groups[3].Value),
                                int.Parse(reg.Groups[4].Value),
                                int.Parse(reg.Groups[5].Value),
                                reg.Groups[6].Value,
                                link?.GetAttribute("href", default(Uri))
                            );
                return nodes.ToList().AsReadOnly();

                long parseSize(string sizeStr)
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
            }).AsAsyncOperation();
        }

        internal TorrentInfo(string name, DateTimeOffset posted, long size, int seeds, int peers, int downloads, string uploader, Uri torrentUri)
        {
            this.Name = name;
            this.Posted = posted;
            this.Size = size;
            this.Seeds = seeds;
            this.Peers = peers;
            this.Downloads = downloads;
            this.Uploader = uploader;
            this.TorrentUri = torrentUri;
        }

        public string Name { get; }

        public DateTimeOffset Posted { get; }

        public long Size { get; }

        public int Seeds { get; }

        public int Peers { get; }

        public int Downloads { get; }

        public string Uploader { get; }

        public Uri TorrentUri { get; }

        public bool IsExpunged => TorrentUri is null;

        public IAsyncOperation<StorageFile> DownloadTorrentAsync()
        {
            if (IsExpunged)
                throw new InvalidOperationException(LocalizedStrings.Resources.ExpungedTorrent);
            var uri = this.TorrentUri;
            var name = this.Name + ".torrent";
            return AsyncInfo.Run(async token =>
            {
                using (var client = new HttpClient())
                {
                    var loadT = client.GetBufferAsync(uri);
                    var filename = Windows.Storage.StorageHelper.ToValidFileName(name);
                    var buf = await loadT;
                    try
                    {
                        return await StorageFile.CreateStreamedFileAsync(filename, dataRequested, null);
                    }
                    catch (Exception)
                    {
                        return await StorageFile.CreateStreamedFileAsync("gallery.torrent", dataRequested, null);
                    }

                    async void dataRequested(StreamedFileDataRequest stream)
                    {
                        try
                        {
                            await stream.WriteAsync(buf);
                            await stream.FlushAsync();
                            stream.Dispose();
                        }
                        catch
                        {
                            stream.FailAndClose(StreamedFileFailureMode.Failed);
                            throw;
                        }
                    }
                }
            });
        }

        public bool Equals(TorrentInfo other)
            => this.Posted == other.Posted
            && this.Size == other.Size
            && this.Seeds == other.Seeds
            && this.Peers == other.Peers
            && this.Downloads == other.Downloads
            && this.TorrentUri == other.TorrentUri
            && this.Name == other.Name
            && this.Uploader == other.Uploader;

        public override bool Equals(object obj) => obj is TorrentInfo i && Equals(i);

        public override int GetHashCode() => unchecked(Posted.GetHashCode() ^ Size.GetHashCode() * 3);

        public static bool operator ==(in TorrentInfo left, in TorrentInfo right) => left.Equals(right);
        public static bool operator !=(in TorrentInfo left, in TorrentInfo right) => !left.Equals(right);
    }
}
