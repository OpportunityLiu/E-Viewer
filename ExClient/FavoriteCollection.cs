using ExClient.Api;
using ExClient.Galleries;
using ExClient.Search;
using HtmlAgilityPack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Windows.Foundation;

namespace ExClient
{
    public sealed class FavoriteCollection : IReadOnlyList<FavoriteCategory>, IList
    {
        private static readonly Regex favStyleMatcher = new Regex(@"background-position:\s*0\s*px\s+-(\d+)\s*px", RegexOptions.Compiled);

        internal FavoriteCategory GetCategory(HtmlNode favoriteIconNode)
        {
            if (favoriteIconNode is null)
                return Removed;

            var favName = favoriteIconNode.GetAttribute("title", "");
            if (favName is null)
                return Removed;

            var favStyle = favoriteIconNode.GetAttribute("style", "");
            var mat = favStyleMatcher.Match(favStyle);
            if (!mat.Success)
                return Removed;

            var favImgOffset = int.Parse(mat.Groups[1].Value);
            var favIdx = favImgOffset / 19;
            var fav = this[favIdx];
            var settings = Client.Current.Settings;
            settings.FavoriteCategoryNames[favIdx] = favName;
            settings.StoreCache();
            return fav;
        }

        internal FavoriteCollection()
        {
            data = new FavoriteCategory[10];
            for (var i = 0; i < data.Length; i++)
            {
                data[i] = new FavoriteCategory(i, null);
            }
        }

        public FavoriteCategory Removed { get; } = new FavoriteCategory(-1, LocalizedStrings.Resources.RemoveFromFavorites);

        public FavoriteCategory All { get; } = new FavoriteCategory(-1, LocalizedStrings.Resources.AllFavorites);

        private readonly FavoriteCategory[] data;

        public int Count => data.Length;

        bool IList.IsFixedSize => true;
        bool IList.IsReadOnly => true;
        bool ICollection.IsSynchronized => false;
        object ICollection.SyncRoot => data;

        public FavoriteCategory this[int index] => data[index];
        object IList.this[int index] { get => this[index]; set => throw new NotSupportedException(); }

        public IEnumerator<FavoriteCategory> GetEnumerator()
            => data.Cast<FavoriteCategory>().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        int IList.Add(object value) => throw new NotSupportedException();
        void IList.Clear() => throw new NotSupportedException();
        bool IList.Contains(object value) => ((IList)data).Contains(value);
        int IList.IndexOf(object value) => ((IList)data).IndexOf(value);
        void IList.Insert(int index, object value) => throw new NotSupportedException();
        void IList.Remove(object value) => throw new NotSupportedException();
        void IList.RemoveAt(int index) => throw new NotSupportedException();
        void ICollection.CopyTo(Array array, int index) => data.CopyTo(array, index);
    }
}
