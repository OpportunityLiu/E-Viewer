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

        #region Keyword search
        public AdvancedSearchResult Search(string keyword)
            => Search(keyword, Category.Unspecified);

        public AdvancedSearchResult Search(string keyword, Category category)
            => Search(keyword, category, default(AdvancedSearchOptions));

        public AdvancedSearchResult Search(string keyword, Category category, AdvancedSearchOptions advancedSearch)
            => AdvancedSearchResult.Search(keyword, category, advancedSearch);
        #endregion Keyword search

        #region Watched search
        public WatchingSearchResult SearchWatched(string keyword)
            => SearchWatched(keyword, Category.Unspecified);

        public WatchingSearchResult SearchWatched(string keyword, Category category)
            => SearchWatched(keyword, category, default);

        public WatchingSearchResult SearchWatched(string keyword, Category category, AdvancedSearchOptions advancedSearch)
            => WatchingSearchResult.Search(keyword, category, advancedSearch);
        #endregion Keyword search

        #region Uploader search
        public AdvancedSearchResult Search(string uploader, string keyword)
            => Search(uploader, keyword, default);

        public AdvancedSearchResult Search(string uploader, string keyword, Category category)
            => Search(uploader, keyword, category, default);

        public AdvancedSearchResult Search(string uploader, string keyword, Category category, AdvancedSearchOptions advancedSearch)
        {
            if (string.IsNullOrWhiteSpace(uploader))
                throw new ArgumentNullException(nameof(uploader));
            uploader = uploader.Trim();
            var formarttedUploader = uploader.IndexOf(' ') >= 0 ? $"uploader:\"{uploader}\" " : $"uploader:{uploader} ";
            return Search(formarttedUploader + keyword, category, advancedSearch);
        }
        #endregion Uploader search

        #region File search
        public IAsyncOperationWithProgress<FileSearchResult, HttpProgress> SearchAsync(string keyword, Category category, StorageFile file)
            => SearchAsync(keyword, category, file, true, false, false);

        public IAsyncOperationWithProgress<FileSearchResult, HttpProgress> SearchAsync(StorageFile file)
            => SearchAsync(null, default, file);

        public IAsyncOperationWithProgress<FileSearchResult, HttpProgress> SearchAsync(StorageFile file, bool searchSimilar, bool onlyCovers, bool searchExpunged)
            => SearchAsync(null, default, file, searchSimilar, onlyCovers, searchExpunged);

        public IAsyncOperationWithProgress<FileSearchResult, HttpProgress> SearchAsync(string keyword, Category category, StorageFile file, bool searchSimilar, bool onlyCovers, bool searchExpunged)
            => FileSearchResult.SearchAsync(keyword, category, file, searchSimilar, onlyCovers, searchExpunged);
        #endregion File search

        #region File hash search
        public FileSearchResult Search(IEnumerable<SHA1Value> fileHashes)
            => Search(fileHashes, null);

        public FileSearchResult Search(IEnumerable<SHA1Value> fileHashes, string fileName)
            => Search(null, default, fileHashes, fileName);

        public FileSearchResult Search(string keyword, Category category, IEnumerable<SHA1Value> fileHashes)
            => Search(keyword, category, fileHashes, null);

        public FileSearchResult Search(string keyword, Category category, IEnumerable<SHA1Value> fileHashes, string fileName)
            => Search(keyword, category, fileHashes, fileName, false, false);

        public FileSearchResult Search(string keyword, Category category, IEnumerable<SHA1Value> fileHashes, string fileName, bool onlyCovers, bool searchExpunged)
            => FileSearchResult.Search(keyword, category, fileHashes, fileName, false, onlyCovers, searchExpunged);
        #endregion File hash search
    }
}
