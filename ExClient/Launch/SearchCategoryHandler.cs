using Opportunity.Helpers.Universal.AsyncHelpers;
using System;
using System.Collections.Generic;
using Windows.Foundation;

namespace ExClient.Launch
{
    internal sealed class SearchCategoryHandler : SearchHandlerBase
    {
        public static SearchCategoryHandler Instance { get; } = new SearchCategoryHandler();

        private static Dictionary<string, Category> categoryDic = new Dictionary<string, Category>(StringComparer.OrdinalIgnoreCase)
        {
            ["Doujinshi"] = Category.Doujinshi,
            ["Manga"] = Category.Manga,
            ["ArtistCG"] = Category.ArtistCG,
            ["GameCG"] = Category.GameCG,
            ["Western"] = Category.Western,
            ["Non-H"] = Category.NonH,
            ["ImageSet"] = Category.ImageSet,
            ["Cosplay"] = Category.Cosplay,
            ["AsianPorn"] = Category.AsianPorn,
            ["Misc"] = Category.Misc
        };

        public override bool CanHandle(UriHandlerData data)
        {
            return data.Paths.Count >= 1 && (categoryDic.ContainsKey(data.Path0));
        }

        public override SearchLaunchResult Handle(UriHandlerData data)
        {
            var category = categoryDic[data.Path0];
            var keyword = GetKeyword(data);
            var advanced = GetAdvancedSearchOptions(data);
            return new SearchLaunchResult(Client.Current.Search(keyword, category, advanced));
        }
    }
}
