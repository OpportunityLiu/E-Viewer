using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Web.Http;

namespace ExClient.Search
{
    public class KeywordSearchResult : SearchResult
    {
        public override Uri SearchUri { get; }

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

        internal static KeywordSearchResult Search(string keyword, Category category, AdvancedSearchOptions advancedSearch)
        {
            return new KeywordSearchResult(keyword, category, advancedSearch);
        }

        protected KeywordSearchResult(string keyword, Category category, AdvancedSearchOptions advancedSearch)
        {
            if (category == Category.Unspecified)
                category = DefaultFliter;
            this.Keyword = keyword ?? "";
            this.Category = category;
            this.AdvancedSearch = advancedSearch;
            this.SearchUri = new Uri(Client.Current.Uris.RootUri, $"?{new HttpFormUrlEncodedContent(getUriQuery())}");
        }

        private IEnumerable<KeyValuePair<string, string>> getUriQuery1()
        {
            yield return new KeyValuePair<string, string>("f_search", this.Keyword);
            foreach (var item in searchFliterNames)
            {
                yield return new KeyValuePair<string, string>(item.Value, this.Category.HasFlag(item.Key) ? "1" : "0");
            }
            yield return new KeyValuePair<string, string>("f_apply", "Apply Filter");
        }

        private IEnumerable<KeyValuePair<string, string>> getUriQuery()
        {
            if (this.AdvancedSearch != default(AdvancedSearchOptions))
                return getUriQuery1().Concat(this.AdvancedSearch.AsEnumerable());
            else
                return getUriQuery1();
        }

        public string Keyword
        {
            get;
        }

        public Category Category
        {
            get;
        }

        public AdvancedSearchOptions AdvancedSearch
        {
            get;
        }
    }
}
