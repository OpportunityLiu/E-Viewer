using ExClient.Search;
using Opportunity.MvvmUniverse.AsyncHelpers;
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
            var sr = data.Queries.ContainsKey("f_shash")
                 ? handleFileSearch(data)
                 : handleSearch(data);
            return AsyncWrapper.CreateCompleted<LaunchResult>(new SearchLaunchResult(sr));
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
            var fn = default(string);
            var sm = false;
            var cv = false;
            var exp = false;
            var hashes = default(IEnumerable<SHA1Value>);
            foreach (var item in data.Queries)
            {
                switch (item.Key)
                {
                case "f_shash":
                    hashes = item.Value.Split(",;".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Select(SHA1Value.Parse);
                    break;
                case "fs_from":
                    fn = item.Value;
                    break;
                case "fs_similar":
                    sm = item.Value.QueryValueAsBoolean();
                    break;
                case "fs_covers":
                    cv = item.Value.QueryValueAsBoolean();
                    break;
                case "fs_exp":
                    exp = item.Value.QueryValueAsBoolean();
                    break;
                }
            }
            var otherdata = handleSearch(data);
            return FileSearchResult.Search(otherdata.Keyword, otherdata.Category, hashes, fn, sm, cv, exp);
        }
    }
}
