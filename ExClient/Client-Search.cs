using ExClient.Search;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.Storage;
using Windows.Web.Http;

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

        public IAsyncOperationWithProgress<FileSearchResult, HttpProgress> SearchAsync(string keyword, Category category, StorageFile file, bool searchSimilar, bool onlyCovers, bool searchExpunged)
        {
            return FileSearchResult.SearchAsync(keyword, category, file, searchSimilar, onlyCovers, searchExpunged);
        }

        public IAsyncOperationWithProgress<FileSearchResult, HttpProgress> SearchAsync(string keyword, Category category, StorageFile file)
        {
            return SearchAsync(keyword, category, file, true, false, false);
        }

        public IAsyncOperationWithProgress<FileSearchResult, HttpProgress> SearchAsync(StorageFile file, bool searchSimilar, bool onlyCovers, bool searchExpunged)
        {
            return FileSearchResult.SearchAsync(null, default, file, searchSimilar, onlyCovers, searchExpunged);
        }

        public IAsyncOperationWithProgress<FileSearchResult, HttpProgress> SearchAsync(StorageFile file)
        {
            return SearchAsync(null, default, file);
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
            return Search(null, default, fileHashes, fileName);
        }

        public FileSearchResult Search(IEnumerable<SHA1Value> fileHashes)
        {
            return Search(fileHashes, null);
        }
    }
}
