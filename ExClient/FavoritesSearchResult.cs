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
    public class FavoritesSearchResult : SearchResultBase
    {
        private static readonly Uri searchUri = new Uri(Client.RootUri, "favorites.php");

        protected override Uri SearchUri => searchUri;

        internal static FavoritesSearchResult Search(Client client, string keyword, FavoriteCategory category)
        {
            if(category?.Index < 0)
                category = null;
            var result = new FavoritesSearchResult(client, keyword, category);
            return result;
        }

        private FavoritesSearchResult(Client client, string keyword, FavoriteCategory category)
            : base(client)
        {
            this.Keyword = keyword ?? "";
            this.Category = category;
        }

        protected override IEnumerable<KeyValuePair<string, string>> GetUriQuery()
        {
            //?favcat=all&f_search=&f_apply=Search+Favorites
            yield return new KeyValuePair<string, string>("favcat", Category == null ? "all" : Category.Index.ToString());
            yield return new KeyValuePair<string, string>("f_search", Keyword);
            yield return new KeyValuePair<string, string>("f_apply", "Search Favorites");
        }

        protected override void HandleAdditionalInfo(HtmlNode trNode, Gallery gallery)
        {
            base.HandleAdditionalInfo(trNode, gallery);
            var favNode = trNode.ChildNodes[2].LastChild;
            var favNote = HtmlEntity.DeEntitize(favNode.InnerText);
            if(favNote.StartsWith("Note: "))
                gallery.FavoriteNote = favNote.Substring(6);
            else
                gallery.FavoriteNote = "";
        }

        public string Keyword
        {
            get;
        }

        public FavoriteCategory Category
        {
            get;
        }
    }
}
