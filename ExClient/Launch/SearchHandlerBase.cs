using ExClient.Search;
using System.Collections.Generic;

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

        protected static Category GetCategory(UriHandlerData data)
        {
            var category = Category.Unspecified;
            foreach (var item in data.Queries)
            {
                var b = item.Value.QueryValueAsBoolean();
                switch (item.Key)
                {
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
                }
            }
            return category;
        }

        protected static string GetKeyword(UriHandlerData data)
        {
            if (!data.Queries.TryGetValue("f_search", out var r))
                r = "";
            return UnescapeKeyword(r);
        }
    }
}
