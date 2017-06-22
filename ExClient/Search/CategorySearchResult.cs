using System;
using System.Collections.Generic;
using Windows.Web.Http;

namespace ExClient.Search
{
    public abstract class CategorySearchResult : SearchResult
    {
        public static Uri SearchBaseUri => Client.Current.Uris.RootUri;

        public static readonly Category DefaultFliter = Category.All;
        private static readonly IReadOnlyDictionary<Category, string> searchFliterNames = new Dictionary<Category, string>()
        {
            [Category.Doujinshi] = "f_doujinshi",
            [Category.Manga] = "f_manga",
            [Category.ArtistCG] = "f_artistcg",
            [Category.GameCG] = "f_gamecg",
            [Category.Western] = "f_western",
            [Category.NonH] = "f_non-h",
            [Category.ImageSet] = "f_imageset",
            [Category.Cosplay] = "f_cosplay",
            [Category.AsianPorn] = "f_asianporn",
            [Category.Misc] = "f_misc"
        };

        protected CategorySearchResult(string keyword, Category category)
            : base(keyword)
        {
            if (category == Category.Unspecified)
                category = DefaultFliter;
            this.Category = category;
        }

        public Category Category { get; }

        public override Uri SearchUri => new Uri(SearchBaseUri, $"?{new HttpFormUrlEncodedContent(getUriQuery())}");

        private IEnumerable<KeyValuePair<string, string>> getUriQuery()
        {
            yield return new KeyValuePair<string, string>("f_search", this.Keyword);
            foreach (var item in searchFliterNames)
            {
                yield return new KeyValuePair<string, string>(item.Value, this.Category.HasFlag(item.Key) ? "1" : "0");
            }
            yield return new KeyValuePair<string, string>("f_apply", "Apply Filter");
        }
    }
}
