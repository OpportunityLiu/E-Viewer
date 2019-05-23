using ExClient.Galleries;
using ExClient.Launch;
using HtmlAgilityPack;
using Opportunity.Helpers.Universal.AsyncHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Data;
using Windows.Web.Http;

namespace ExClient.Search
{
    public sealed class FavoritesSearchResult : SearchResult
    {
        public static bool TryParse(Uri uri, out FavoritesSearchResult result)
        {
            result = default;
            if (uri is null)
                return false;
            var data = new UriHandlerData(uri);
            if (!FavoritesSearchHandler.Instance.CanHandle(data))
                return false;
            result = (FavoritesSearchResult)FavoritesSearchHandler.Instance.Handle(data).Data;
            return true;
        }

        public static FavoritesSearchResult Parse(Uri uri)
        {
            if (TryParse(uri, out var r))
                return r;
            throw new FormatException($"Failed to parse uri `{uri}` as FavoritesSearchResult");
        }

        public static Uri SearchBaseUri => new Uri(Client.Current.Uris.RootUri, "favorites.php");

        internal static FavoritesSearchResult Search(string keyword, FavoriteCategory category)
        {
            if (category is null || category.Index < 0)
                category = Client.Current.Favorites.All;

            var result = new FavoritesSearchResult(keyword, category);
            return result;
        }

        private FavoritesSearchResult(string keyword, FavoriteCategory category)
            : base(keyword)
        {
            Category = category;
            SearchUri = new Uri(SearchBaseUri, _GetUriQuery());
        }

        public FavoriteCategory Category { get; }

        public override Uri SearchUri { get; }

        private string _GetUriQuery()
        {
            // ?favcat=all&f_search=d&sn=on&st=on&sf=off
            return
                $"?favcat={(Category.Index < 0 ? "all" : Category.Index.ToString())}" +
                $"&f_search={Uri.EscapeDataString(Keyword)}" +
                $"&sn=1&st=1&sf=1";
        }

        protected override void LoadPageOverride(HtmlDocument doc)
        {
            // read and update favcat names
            var noselNode = doc.DocumentNode
                .Element("html").Element("body")
                .Element("div", "ido")
                .Element("div", "nosel");
            var fpNodes = noselNode.Elements("div").Take(10);
            foreach (var n in fpNodes)
            {
                var fav = n.Element("div", "i");
                Client.Current.Favorites.GetCategory(fav);
            }
        }

        public async Task AddToCategoryAsync(IReadOnlyList<ItemIndexRange> items, FavoriteCategory categoty, CancellationToken token = default)
        {
            if (categoty is null)
                throw new ArgumentNullException(nameof(categoty));
            if (items is null || items.Count == 0)
                return;

            var ddact = categoty.Index < 0 ? "delete" : $"fav{categoty.Index}";
            var post = Client.Current.HttpClient.PostAsync(SearchUri, getParameters());
            token.Register(post.Cancel);
            var r = await post;
            if (categoty.Index < 0)
                Reset();
            else
            {
                foreach (var range in items)
                {
                    for (var i = range.FirstIndex; i <= range.LastIndex; i++)
                    {
                        this[i].FavoriteCategory = categoty;
                    }
                }
            }

            IEnumerable<KeyValuePair<string, string>> getParameters()
            {
                yield return new KeyValuePair<string, string>("apply", "Apply");
                yield return new KeyValuePair<string, string>("ddact", ddact);
                foreach (var range in items)
                {
                    for (var i = range.FirstIndex; i <= range.LastIndex; i++)
                    {
                        yield return new KeyValuePair<string, string>("modifygids[]", this[i].Id.ToString());
                    }
                }
            }
        }
    }
}
