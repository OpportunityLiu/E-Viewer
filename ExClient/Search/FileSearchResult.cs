using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;

namespace ExClient.Search
{
    public sealed class FileSearchResult : KeywordSearchResult
    {
        internal static IAsyncOperation<FileSearchResult> SearchAsync(string keyword, Category category, StorageFile file, bool searchSimilar, bool onlyCovers, bool searchExpunged)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));
            if (searchSimilar)
            {
                //TODO:
                throw null;
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

        internal static FileSearchResult Search(string keyword, Category category, IEnumerable<SHA1Value> fileHashes, string fileName, bool onlyCovers, bool searchExpunged)
        {
            if (fileHashes == null)
                throw new ArgumentNullException(nameof(fileHashes));
            return new FileSearchResult(keyword, category, fileHashes, fileName, false, onlyCovers, searchExpunged);
        }

        private FileSearchResult(string keyword, Category category, IEnumerable<SHA1Value> hashes, string fileName, bool searchSimilar, bool onlyCovers, bool searchExpunged) : base(keyword, category, default(AdvancedSearchOptions))
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

        public override Uri SearchUri => new Uri($"{base.SearchUri}{createSearchUriQuery()}");

        public bool SearchSimilar { get; }

        public bool OnlyCovers { get; }

        public bool SearchExpunged { get; }

        public IReadOnlyList<SHA1Value> FileHashList { get; }

        public string FileName { get; }
    }
}
