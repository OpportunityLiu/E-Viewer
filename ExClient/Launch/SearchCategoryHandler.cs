using Opportunity.MvvmUniverse.AsyncHelpers;
using System;
using System.Collections.Generic;
using Windows.Foundation;

namespace ExClient.Launch
{
    internal sealed class SearchCategoryHandler : UriHandler
    {
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
            return data.Paths.Count == 1 && (categoryDic.ContainsKey(data.Path0));
        }

        public override IAsyncOperation<LaunchResult> HandleAsync(UriHandlerData data)
        {
            var category = categoryDic[data.Path0];
            return AsyncWrapper.CreateCompleted<LaunchResult>(new SearchLaunchResult(Client.Current.Search("", category)));
        }
    }
}
