using HtmlAgilityPack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Data;
using Windows.Web.Http;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;
using Newtonsoft.Json;
using System.Net;
using System.Runtime.InteropServices;
using GalaSoft.MvvmLight.Threading;

namespace ExClient
{
    public class SearchResult : SearchResultBase
    {
        protected override Uri SearchUri => Client.ExUri;

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

        internal static SearchResult Search(Client client, string keyword, Category category, AdvancedSearchOptions advancedSearch)
        {
            if(category == Category.Unspecified)
                category = DefaultFliter;
            var result = new SearchResult(client, keyword, category, advancedSearch?.Clone(true));
            return result;
        }

        private SearchResult(Client client, string keyword, Category category, AdvancedSearchOptions advancedSearch)
            : base(client)
        {
            this.Keyword = keyword ?? "";
            this.Category = category;
            this.AdvancedSearch = advancedSearch;
        }

        private IEnumerable<KeyValuePair<string, string>> getUeiQuery1()
        {
            yield return new KeyValuePair<string, string>("f_search", Keyword);
            foreach(var item in searchFliterNames)
            {
                yield return new KeyValuePair<string, string>(item.Value, Category.HasFlag(item.Key) ? "1" : "0");
            }
            yield return new KeyValuePair<string, string>("f_apply", "Apply Filter");
        }

        protected override IEnumerable<KeyValuePair<string, string>> GetUriQuery()
        {
            if(AdvancedSearch != null)
                return getUeiQuery1().Concat(AdvancedSearch.AsEnumerable());
            else
                return getUeiQuery1();
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
