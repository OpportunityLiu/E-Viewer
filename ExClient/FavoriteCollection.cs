using HtmlAgilityPack;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ExClient
{
    public sealed partial class FavoriteCollection : IReadOnlyList<FavoriteCategory>
    {

        private static readonly Regex favStyleMatcher = new Regex(@"background-position:\s*0\s*px\s+-(\d+)\s*px", RegexOptions.Compiled);

        internal FavoriteCategory GetCategory(HtmlNode favoriteIconNode)
        {
            if(favoriteIconNode == null)
                return FavoriteCategory.Removed;
            var favName = HtmlEntity.DeEntitize(favoriteIconNode.GetAttributeValue("title", null));
            if(favName == null)
                return FavoriteCategory.Removed;
            var favStyle = favoriteIconNode.GetAttributeValue("style", "");
            var mat = favStyleMatcher.Match(favStyle);
            if(!mat.Success)
                return FavoriteCategory.Removed;
            var favImgOffset = int.Parse(mat.Groups[1].Value);
            var favIdx = favImgOffset / 19;
            var fav = this[favIdx];
            fav.Name = favName;
            return fav;
        }

        internal FavoriteCollection(Client owner)
        {
            this.data = new FavoriteCategory[10];
            for(var i = 0; i < this.data.Length; i++)
            {
                this.data[i] = new FavoriteCategory(i);
            }
            this.Owner = owner;
        }

        internal Client Owner
        {
            get;
        }

        public FavoritesSearchResult Search(string keyword, FavoriteCategory category)
        {
            return FavoritesSearchResult.Search(Owner, keyword, category);
        }

        public FavoritesSearchResult Search(string keyword)
        {
            return Search(keyword, null);
        }

        private FavoriteCategory[] data;

        public int Count => this.data.Length;

        public FavoriteCategory this[int index] => this.data[index];

        public IEnumerator<FavoriteCategory> GetEnumerator()
            => this.data.Cast<FavoriteCategory>().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
