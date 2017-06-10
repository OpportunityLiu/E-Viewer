using ExClient.Search;
using Opportunity.MvvmUniverse.AsyncHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;

namespace ExClient.Launch
{
    internal class SearchHandler : UriHandler
    {
        protected static string UnescapeKeyword(string query)
        {
            return query.Replace("+", "").Replace("&", "");
        }

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

        private KeywordSearchResult handleSearch(UriHandlerData data)
        {
            var keyword = "";
            var category = Category.Unspecified;
            var advanced = new AdvancedSearchOptions();
            var ap = false;
            var av = false;
            foreach (var item in data.Queries)
            {
                var b = item.Value.QueryValueAsBoolean();
                switch (item.Key)
                {
                case "f_apply":
                    ap = b;
                    break;
                case "f_doujinshi":
                    if (b) category |= Category.Doujinshi;
                    break;
                case "f_manga":
                    if (b) category |= Category.Manga;
                    break;
                case "f_artistcg":
                    if (b) category |= Category.ArtistCG;
                    break;
                case "f_gamecg":
                    if (b) category |= Category.GameCG;
                    break;
                case "f_western":
                    if (b) category |= Category.Western;
                    break;
                case "f_non-h":
                    if (b) category |= Category.NonH;
                    break;
                case "f_imageset":
                    if (b) category |= Category.ImageSet;
                    break;
                case "f_cosplay":
                    if (b) category |= Category.Cosplay;
                    break;
                case "f_asianporn":
                    if (b) category |= Category.AsianPorn;
                    break;
                case "f_misc":
                    if (b) category |= Category.Misc;
                    break;
                case "f_search":
                    keyword = UnescapeKeyword(item.Value);
                    break;
                case "advsearch":
                    av = b;
                    break;
                case "f_sname":
                    advanced.SearchName = b;
                    break;
                case "f_stags":
                    advanced.SearchTags = b;
                    break;
                case "f_sdesc":
                    advanced.SearchDescription = b;
                    break;
                case "f_storr":
                    advanced.SearchTorrentFilenames = b;
                    break;
                case "f_sto":
                    advanced.GalleriesWithTorrentsOnly = b;
                    break;
                case "f_sdt1":
                    advanced.SearchLowPowerTags = b;
                    break;
                case "f_sdt2":
                    advanced.SearchDownvotedTags = b;
                    break;
                case "f_sh":
                    advanced.ShowExpungedGalleries = b;
                    break;
                case "f_sr":
                    advanced.SearchMinimumRating = b;
                    break;
                case "f_srdd":
                    advanced.MinimumRating = int.Parse(item.Value);
                    break;
                }
            }
            if (!ap)
                return Client.Current.Search("");
            else if (av)
                return Client.Current.Search(keyword, category, advanced);
            else
                return Client.Current.Search(keyword, category);
        }

        private SearchResult handleFileSearch(UriHandlerData data)
        {
            var fn = default(string);
            var cv = false;
            var exp = false;
            var hashes = default(IEnumerable<SHA1Value>);
            foreach (var item in data.Queries)
            {
                var b = item.Value.QueryValueAsBoolean();
                switch (item.Key)
                {
                case "f_shash":
                    hashes = item.Value.Split(",;".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Select(SHA1Value.Parse);
                    break;
                case "fs_from":
                    fn = item.Value;
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
            return Client.Current.Search(otherdata.Keyword, otherdata.Category, hashes, fn, cv, exp);
        }
    }
}
