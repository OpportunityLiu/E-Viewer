using Opportunity.Helpers.Universal.AsyncHelpers;
using System;
using Windows.Foundation;

namespace ExClient.Launch
{
    internal sealed class FavoritesSearchHandler : SearchHandler
    {
        public static new FavoritesSearchHandler Instance { get; } = new FavoritesSearchHandler();

        public override bool CanHandle(UriHandlerData data)
        {
            return data.Paths.Count >= 1 && data.Path0 == "favorites.php";
        }

        public override SearchLaunchResult Handle(UriHandlerData data)
        {
            var keyword = UnescapeKeyword(data.Queries.GetString("f_search") ?? "");
            var category = Client.Current.Favorites.All;
            var ap = data.Queries.GetBoolean("f_apply");
            {
                var cat = data.Queries.GetString("favcat") ?? "all";
                if (cat != "all")
                {
                    var index = cat.QueryValueAsInt32();
                    index = Math.Max(0, index);
                    index = Math.Min(9, index);
                    category = Client.Current.Favorites[index];
                }
            }
            if (!ap)
            {
                return new SearchLaunchResult(category.Search(""));
            }
            else
            {
                return new SearchLaunchResult(category.Search(keyword));
            }
        }
    }
}
