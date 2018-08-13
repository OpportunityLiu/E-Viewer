using ExClient.Search;
using Opportunity.Helpers.Universal.AsyncHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;

namespace ExClient.Launch
{
    internal class SearchHandler : SearchHandlerBase
    {
        public override bool CanHandle(UriHandlerData data)
        {
            return data.Paths.Count == 0;
        }

        public override IAsyncOperation<LaunchResult> HandleAsync(UriHandlerData data)
        {
            var sr = data.Queries.GetString("f_shash").IsNullOrEmpty()
                 ? handleSearch(data)
                 : handleFileSearch(data);
            return AsyncOperation<LaunchResult>.CreateCompleted(new SearchLaunchResult(sr));
        }

        private CategorySearchResult handleSearch(UriHandlerData data)
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
