using ExClient.Search;

using System;
using System.Linq;

namespace ExClient.Launch
{
    internal class SearchHandler : SearchHandlerBase
    {
        public static SearchHandler Instance { get; } = new SearchHandler();

        public override bool CanHandle(UriHandlerData data)
        {
            return data.Paths.Count == 0;
        }

        public override SearchLaunchResult Handle(UriHandlerData data)
        {
            var sr = data.Queries.GetString("f_shash").IsNullOrEmpty()
                 ? handleSearch(data)
                 : (CategorySearchResult)handleFileSearch(data);
            return new SearchLaunchResult(sr);
        }

        private AdvancedSearchResult handleSearch(UriHandlerData data)
        {
            var keyword = GetKeyword(data);
            var category = GetCategory(data);
            var advanced = GetAdvancedSearchOptions(data);
            return Client.Current.Search(keyword, category, advanced);
        }

        private FileSearchResult handleFileSearch(UriHandlerData data)
        {
            var filename = data.Queries.GetString("fs_from");
            var sm = data.Queries.GetBoolean("fs_similar");
            var cv = data.Queries.GetBoolean("fs_covers");
            var exp = data.Queries.GetBoolean("fs_exp");
            var hashes = (data.Queries.GetString("f_shash") ?? "")
                .Split(",; ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                .Where(str => str.All(ch => ('0' <= ch && ch <= '9') || ('a' <= ch && ch <= 'f') || ('A' <= ch && ch <= 'F')))
                .Select(SHA1Value.Parse);
            var otherdata = handleSearch(data);
            return FileSearchResult.Search(otherdata.Keyword, otherdata.Category, hashes, filename, sm, cv, exp);
        }
    }
}
