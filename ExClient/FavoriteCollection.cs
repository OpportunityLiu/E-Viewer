using ExClient.Search;
using HtmlAgilityPack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ExClient
{
    public sealed class FavoriteCollection : IReadOnlyList<FavoriteCategory>, IList
    {

        private static readonly Regex favStyleMatcher = new Regex(@"background-position:\s*0\s*px\s+-(\d+)\s*px", RegexOptions.Compiled);

        internal FavoriteCategory GetCategory(HtmlNode favoriteIconNode)
        {
            if (favoriteIconNode == null)
                return FavoriteCategory.Removed;
            var favName = favoriteIconNode.GetAttributeValue("title", null);
            if (favName == null)
                return FavoriteCategory.Removed;
            var favStyle = favoriteIconNode.GetAttributeValue("style", "");
            var mat = favStyleMatcher.Match(favStyle);
            if (!mat.Success)
                return FavoriteCategory.Removed;
            var favImgOffset = int.Parse(mat.Groups[1].Value);
            var favIdx = favImgOffset / 19;
            var fav = this[favIdx];
            Client.Current.Settings.FavoriteCategoryNames[favIdx] = favName;
            return fav;
        }

        internal FavoriteCollection()
        {
            this.data = new FavoriteCategory[10];
            for (var i = 0; i < this.data.Length; i++)
            {
                this.data[i] = new FavoriteCategory(i, null);
            }
        }

        public FavoritesSearchResult Search(string keyword, FavoriteCategory category)
        {
            return FavoritesSearchResult.Search(keyword, category);
        }

        public FavoritesSearchResult Search(string keyword)
        {
            return Search(keyword, null);
        }

        private FavoriteCategory[] data;

        public int Count => this.data.Length;

        bool IList.IsFixedSize => true;
        bool IList.IsReadOnly => true;
        bool ICollection.IsSynchronized => false;
        object ICollection.SyncRoot => this.data;

        public FavoriteCategory this[int index] => this.data[index];
        object IList.this[int index] { get => this[index]; set => throw new NotSupportedException(); }

        public IEnumerator<FavoriteCategory> GetEnumerator()
            => this.data.Cast<FavoriteCategory>().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        int IList.Add(object value) => throw new NotSupportedException();
        void IList.Clear() => throw new NotSupportedException();
        bool IList.Contains(object value) => ((IList)this.data).Contains(value);
        int IList.IndexOf(object value) => ((IList)this.data).IndexOf(value);
        void IList.Insert(int index, object value) => throw new NotSupportedException();
        void IList.Remove(object value) => throw new NotSupportedException();
        void IList.RemoveAt(int index) => throw new NotSupportedException();
        void ICollection.CopyTo(Array array, int index) => this.data.CopyTo(array, index);
    }
}
