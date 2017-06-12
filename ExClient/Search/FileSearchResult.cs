using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Web.Http;

namespace ExClient.Search
{
    public sealed class FileSearchResult : CategorySearchResult
    {
        private static readonly Uri fileSearchUriEh = new Uri("https://upload.e-hentai.org/image_lookup.php");
        private static readonly Uri fileSearchUriEx = new Uri("https://exhentai.org/upload/image_lookup.php");

        private static Uri fileSearchUri => Client.Current.Host == HostType.Ehentai ? fileSearchUriEh : fileSearchUriEx;

        internal static IAsyncOperation<FileSearchResult> SearchAsync(string keyword, Category category, StorageFile file, bool searchSimilar, bool onlyCovers, bool searchExpunged)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));
            if (searchSimilar)
            {
                return AsyncInfo.Run(async token =>
                {
                    var read = FileIO.ReadBufferAsync(file);
                    HttpStringContent contentOf(bool v) => new HttpStringContent(v ? "1" : "0");
                    var data = new HttpMultipartFormDataContent
                    {
                        { contentOf(true), "fs_similar" },
                        { contentOf(onlyCovers), "fs_covers" },
                        { contentOf(searchExpunged), "fs_exp" }
                    };
                    var buf = await read;
                    data.Add(new HttpBufferContent(buf), "sfile", file.Name);
                    await data.BufferAllAsync();
                    var r = await Client.Current.HttpClient.PostAsync(fileSearchUri, data);
                    var uri = r.RequestMessage.RequestUri;
                    var query = uri.Query.Split("?&".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    var hashstr = query.Single(s => s.StartsWith("f_shash=")).Substring(8).Split(';');
                    if (hashstr.Any(value => value.Length != 40))
                        throw new ArgumentException(LocalizedStrings.Resources.UnsupportedFile, nameof(file));
                    return new FileSearchResult(keyword, category, hashstr.Select(SHA1Value.Parse), file.Name, true, onlyCovers, searchExpunged);
                });
            }
            else
            {
                return AsyncInfo.Run(async token =>
                {
                    var hash = await SHA1Value.ComputeAsync(file);
                    return new FileSearchResult(keyword, category, Enumerable.Repeat(hash, 1), file.Name, false, onlyCovers, searchExpunged);
                });
            }
        }

        internal static FileSearchResult Search(string keyword, Category category, IEnumerable<SHA1Value> fileHashes, string fileName, bool searchSimilarUriFlag, bool onlyCovers, bool searchExpunged)
        {
            if (fileHashes == null)
                throw new ArgumentNullException(nameof(fileHashes));
            return new FileSearchResult(keyword, category, fileHashes, fileName, searchSimilarUriFlag, onlyCovers, searchExpunged);
        }

        private FileSearchResult(string keyword, Category category, IEnumerable<SHA1Value> hashes, string fileName, bool searchSimilar, bool onlyCovers, bool searchExpunged) : base(keyword, category)
        {
            this.SearchSimilar = searchSimilar;
            this.OnlyCovers = onlyCovers;
            this.SearchExpunged = searchExpunged;
            this.FileName = fileName ?? "";
            this.FileHashList = hashes.ToArray();
        }

        private string createSearchUriQuery()
        {
            return
                $"&f_shash={string.Join(";", FileHashList)}" +
                $"&fs_from={Uri.EscapeDataString(FileName)}" +
                $"&fs_similar={(SearchSimilar ? 1 : 0)}" +
                $"&fs_covers={(OnlyCovers ? 1 : 0)}" +
                $"&fs_exp={(SearchExpunged ? 1 : 0)}";
        }

        public override Uri SearchUri => new Uri(base.SearchUri + createSearchUriQuery());

        public bool SearchSimilar { get; }

        public bool OnlyCovers { get; }

        public bool SearchExpunged { get; }

        public IReadOnlyList<SHA1Value> FileHashList { get; }

        public string FileName { get; }
    }
}
