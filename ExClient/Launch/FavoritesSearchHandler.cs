using Opportunity.MvvmUniverse.AsyncWrappers;
using System;
using Windows.Foundation;

namespace ExClient.Launch
{
    internal sealed class FavoritesSearchHandler : SearchHandler
    {
        public override bool CanHandle(UriHandlerData data)
        {
            return data.Paths.Count == 1 && data.Path0 == "favorites.php";
        }

        public override IAsyncOperation<LaunchResult> HandleAsync(UriHandlerData data)
        {
            var keyword = "";
            var category = (FavoriteCategory)null;
            var ap = false;
            foreach(var item in data.Queries)
            {
                switch(item.Key)
                {
                case "f_apply":
                    ap = item.Value.QueryValueAsBoolean();
                    break;
                case "favcat":
                    if(item.Value != "all")
                    {
                        var index = item.Value.QueryValueAsInt32();
                        index = Math.Max(0, index);
                        index = Math.Min(9, index);
                        category = Client.Current.Favorites[index];
                    }
                    break;
                case "f_search":
                    keyword = UnescapeKeyword(item.Value);
                    break;
                }
            }
            if(!ap)
                return AsyncWrapper.CreateCompleted<LaunchResult>(new FavoritesSearchLaunchResult(Client.Current.Favorites.Search("", category)));
            else
                return AsyncWrapper.CreateCompleted<LaunchResult>(new FavoritesSearchLaunchResult(Client.Current.Favorites.Search(keyword, category)));
        }
    }
}
