using System;

namespace ExClient.Search
{
    public class AdvancedSearchResult : CategorySearchResult
    {
        internal static AdvancedSearchResult Search(string keyword, Category category, AdvancedSearchOptions advancedSearch)
        {
            return new AdvancedSearchResult(keyword, category, advancedSearch);
        }

        private string getQueryString() => _AdvSearchData.ToSearchQuery();

        protected AdvancedSearchResult(string keyword, Category category, AdvancedSearchOptions advancedSearch)
            : base(keyword, category)
        {
            _AdvSearchData = advancedSearch is null ? new AdvancedSearchOptions() : advancedSearch.Clone();
            SearchUri = _AdvSearchData == default
                ? base.SearchUri
                : new Uri(base.SearchUri.OriginalString + getQueryString());
        }

        private readonly AdvancedSearchOptions _AdvSearchData;

        public AdvancedSearchOptions AdvancedSearch => _AdvSearchData.Clone();

        public override Uri SearchUri { get; }
    }
}
