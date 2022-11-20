using System;

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
            var cat = data.Queries.GetString("favcat") ?? "all";
            if (cat != "all")
            {
                var index = data.Queries.GetInt32("favcat");
                index = Math.Max(0, index);
                index = Math.Min(9, index);
                category = Client.Current.Favorites[index];
            }
            return new SearchLaunchResult(category.Search(keyword));
        }
    }
}
