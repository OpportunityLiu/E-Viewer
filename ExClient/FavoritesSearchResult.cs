using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExClient
{
    public class FavoritesSearchResult : SearchResultBase
    {
        private static readonly Uri searchUri = new Uri(Client.ExUri, "favorites.php");

        protected override Uri SearchUri => searchUri;

        internal static FavoritesSearchResult Search(Client client, string keyword, FavoriteCategory category)
        {
            if(category == null || category.Index < 0)
                category = FavoriteCategory.All;
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
            string cat;
            if(Category == null || Category.Index < 0)
                cat = "all";
            else
                cat = Category.Index.ToString();
            yield return new KeyValuePair<string, string>("favcat", cat);
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

        protected override void LoadPageOverride(HtmlDocument doc)
        {
            var noselNode = doc.DocumentNode
                .Element("html")
                .Element("body")
                .Element("div")
                .Elements("div").First();
            var fpNodes = noselNode.Elements("div").Take(10);
            fpNodes.Select(n =>
            {
                var fav = n.Elements("div").First(nn => nn.GetAttributeValue("class", null) == "i");
                return Owner.Favorites.GetCategory(fav);
            }).ToList();
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
