using System;

namespace ExClient.Search
{
    public sealed class WatchingSearchResult : AdvancedSearchResult
    {
        internal new static WatchingSearchResult Search(string keyword, Category category, AdvancedSearchOptions advancedSearch)
        {
            return new WatchingSearchResult(keyword, category, advancedSearch);
        }
        public new static Uri SearchBaseUri => new Uri(Client.Current.Uris.RootUri, "watched");

        private WatchingSearchResult(string keyword, Category category, AdvancedSearchOptions advancedSearch)
            : base(keyword, category, advancedSearch)
        {
            SearchUri = new Uri(SearchBaseUri, base.SearchUri.Query);
        }

        public override Uri SearchUri { get; }
    }
}
