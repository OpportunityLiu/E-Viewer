using ExClient.Search;
using Opportunity.Helpers.Universal.AsyncHelpers;
using System.Collections.Generic;
using Windows.Foundation;

namespace ExClient.Launch
{
    internal abstract class SearchHandlerBase : UriHandler
    {
        protected static string UnescapeKeyword(string query)
        {
            return query.Replace("+", "").Replace("&", "");
        }

        protected static AdvancedSearchOptions GetAdvancedSearchOptions(UriHandlerData data)
            => AdvancedSearchOptions.ParseUri(data);

        public abstract SearchLaunchResult Handle(UriHandlerData data);

        public override IAsyncOperation<LaunchResult> HandleAsync(UriHandlerData data)
            => AsyncOperation<LaunchResult>.CreateCompleted(Handle(data));

        protected static Category GetCategory(UriHandlerData data)
        {
            var category = Category.Unspecified;
            foreach (var item in data.Queries)
            {
                var b = item.Value.QueryValueAsBoolean();
                switch (item.Name)
                {
                case "f_doujinshi":
                    if (b)
                        category |= Category.Doujinshi;
                    break;
                case "f_manga":
                    if (b)
                        category |= Category.Manga;
                    break;
                case "f_artistcg":
                    if (b)
                        category |= Category.ArtistCG;
                    break;
                case "f_gamecg":
                    if (b)
                        category |= Category.GameCG;
                    break;
                case "f_western":
                    if (b)
                        category |= Category.Western;
                    break;
                case "f_non-h":
                    if (b)
                        category |= Category.NonH;
                    break;
                case "f_imageset":
                    if (b)
                        category |= Category.ImageSet;
                    break;
                case "f_cosplay":
                    if (b)
                        category |= Category.Cosplay;
                    break;
                case "f_asianporn":
                    if (b)
                        category |= Category.AsianPorn;
                    break;
                case "f_misc":
                    if (b)
                        category |= Category.Misc;
                    break;
                }
            }
            return category;
        }

        protected static string GetKeyword(UriHandlerData data)
        {
            return UnescapeKeyword(data.Queries.GetString("f_search") ?? "");
        }
    }
}
