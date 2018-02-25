using ExClient.Search;
using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.Storage;
using Windows.Web.Http;

namespace ExClient
{
    public partial class Client
    {
        public PopularCollection Popular { get; } = new PopularCollection();

        public AdvancedSearchResult Search(string uploader, string keyword, Category category, AdvancedSearchOptions advancedSearch)
        {
            if (string.IsNullOrWhiteSpace(uploader))
                throw new ArgumentNullException(nameof(uploader));
            uploader = uploader.Trim();
            if (uploader.IndexOf(' ') >= 0)
                keyword = $"uploader:\"{uploader}\" " + keyword;
            else
                keyword = $"uploader:{uploader} " + keyword;
            return Search(keyword, category, advancedSearch);
        }

        public AdvancedSearchResult Search(string uploader, string keyword, Category category)
        {
            return Search(uploader, keyword, category, default(AdvancedSearchOptions));
        }

        public AdvancedSearchResult Search(string uploader, string keyword)
        {
            return Search(uploader, keyword, default(Category));
        }

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
