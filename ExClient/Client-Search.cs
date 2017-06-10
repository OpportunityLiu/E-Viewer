using ExClient.Search;

namespace ExClient
{
    public partial class Client
    {
        public SearchResult Search(string keyword, Category category, AdvancedSearchOptions advancedSearch)
        {
            return SearchResult.Search(keyword, category, advancedSearch);
        }

        public SearchResult Search(string keyword, Category category)
        {
            return Search(keyword, category, default(AdvancedSearchOptions));
        }

        public SearchResult Search(string keyword)
        {
            return Search(keyword, Category.Unspecified);
        }
    }
}
