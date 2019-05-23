using System;

namespace ExClient.Search
{
    public sealed class WatchedSearchResult : AdvancedSearchResult
    {
        internal new static WatchedSearchResult Search(string keyword, Category category, AdvancedSearchOptions advancedSearch)
        {
            return new WatchedSearchResult(keyword, category, advancedSearch);
        }
        public new static Uri SearchBaseUri => new Uri(Client.Current.Uris.RootUri, "watched");

        private WatchedSearchResult(string keyword, Category category, AdvancedSearchOptions advancedSearch)
            : base(keyword, category, advancedSearch)
        {
            SearchUri = new Uri(SearchBaseUri, base.SearchUri.Query);
        }

        public override Uri SearchUri { get; }
    }
}
