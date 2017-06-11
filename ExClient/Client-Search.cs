using ExClient.Search;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.Storage;

namespace ExClient
{
    public partial class Client
    {
        public AdvancedSearchResult Search(string keyword, Category category, AdvancedSearchOptions advancedSearch)
        {
            return AdvancedSearchResult.Search(keyword, category, advancedSearch);
        }

        public AdvancedSearchResult Search(string keyword, Category category)
        {
            return Search(keyword, category, default(AdvancedSearchOptions));
        }

        public AdvancedSearchResult Search(string keyword)
        {
            return Search(keyword, Category.Unspecified);
        }

        public IAsyncOperation<FileSearchResult> SearchAsync(string keyword, Category category, StorageFile file, bool searchSimilar, bool onlyCovers, bool searchExpunged)
        {
            return FileSearchResult.SearchAsync(keyword, category, file, searchSimilar, onlyCovers, searchExpunged);
        }

        public IAsyncOperation<FileSearchResult> SearchAsync(string keyword, Category category, StorageFile file)
        {
            return SearchAsync(keyword, category, file, true, false, false);
        }

        public IAsyncOperation<FileSearchResult> SearchAsync(StorageFile file)
        {
            return SearchAsync(null, default(Category), file);
        }

        public FileSearchResult Search(string keyword, Category category, IEnumerable<SHA1Value> fileHashes, string fileName, bool onlyCovers, bool searchExpunged)
        {
            return FileSearchResult.Search(keyword, category, fileHashes, fileName, false, onlyCovers, searchExpunged);
        }

        public FileSearchResult Search(string keyword, Category category, IEnumerable<SHA1Value> fileHashes, string fileName)
        {
            return Search(keyword, category, fileHashes, fileName, false, false);
        }

        public FileSearchResult Search(string keyword, Category category, IEnumerable<SHA1Value> fileHashes)
        {
            return Search(keyword, category, fileHashes, null);
        }

        public FileSearchResult Search(IEnumerable<SHA1Value> fileHashes, string fileName)
        {
            return Search(null, default(Category), fileHashes, fileName);
        }

        public FileSearchResult Search(IEnumerable<SHA1Value> fileHashes)
        {
            return Search(fileHashes, null);
        }
    }
}
