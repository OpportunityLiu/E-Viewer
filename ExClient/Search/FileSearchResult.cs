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
    public sealed class FileSearchResult : SearchResultBase
    {
        internal static IAsyncOperation<FileSearchResult> SearchAsync(Client owner, StorageFile file, bool includeSimilar, bool onlyCovers, bool includeExpunged)
        {
            if (includeSimilar)
            {
                //TODO:
                throw null;
            }
            else
            {
                return AsyncInfo.Run(async token =>
                {
                    var hash = await SHA1Value.ComputeAsync(file);
                    return new FileSearchResult(owner, Enumerable.Repeat(hash, 1), file.Name, false, onlyCovers, includeExpunged);
                });
            }
        }

        internal static FileSearchResult Search(Client owner, string fileName, IEnumerable<SHA1Value> fileHashes, bool includeSimilar, bool onlyCovers, bool includeExpunged)
        {
            if (fileHashes == null)
                throw new ArgumentNullException(nameof(fileHashes));
            return new FileSearchResult(owner, fileHashes, fileName, false, onlyCovers, includeExpunged);
        }

        private FileSearchResult(Client owner, IEnumerable<SHA1Value> hashes, string fileName, bool includeSimilar, bool onlyCovers, bool includeExpunged) 
        {
            this.IncludeSimilar = includeSimilar;
            this.OnlyCovers = onlyCovers;
            this.IncludeExpunged = includeExpunged;
            this.FileName = fileName ?? "";
            this.FileHashList = hashes.ToArray();
        }

        public override Uri SearchUri => throw new NotImplementedException();

        public bool IncludeSimilar { get; }

        public bool OnlyCovers { get; }

        public bool IncludeExpunged { get; }

        public IReadOnlyList<SHA1Value> FileHashList { get; }

        public string FileName { get; }
    }
}
