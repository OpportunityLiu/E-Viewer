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
        private static readonly Regex _FavStyleMatcher1 = new Regex(@"background-position:\s*0\s*px\s+-(\d+)\s*px", RegexOptions.Compiled);
        private static readonly Regex _FavStyleMatcher2 = new Regex(@"border-color:\s*#([A-Fa-f0-9]+)", RegexOptions.Compiled);

        internal FavoriteCategory GetCategory(HtmlNode favoriteIconNode)
        {
            if (favoriteIconNode is null)
                return Removed;

            var favName = favoriteIconNode.GetAttribute("title", "");
            if (favName is null)
                return Removed;

            int favIndex;

            var favStyle = favoriteIconNode.GetAttribute("style", "");
            var match1 = _FavStyleMatcher1.Match(favStyle);
            if (!match1.Success)
            {
                var match2 = _FavStyleMatcher2.Match(favStyle);
                if (!match2.Success)
                    return Removed;
                switch (match2.Groups[1].Value.ToLowerInvariant())
                {
                case "000": favIndex = 0; break;
                case "f00": favIndex = 1; break;
                case "fa0": favIndex = 2; break;
                case "dd0": favIndex = 3; break;
                case "080": favIndex = 4; break;
                case "9f4": favIndex = 5; break;
                case "4bf": favIndex = 6; break;
                case "00f": favIndex = 7; break;
                case "508": favIndex = 8; break;
                case "e8e": favIndex = 9; break;
                default:
                    return Removed;
                }
            }
            else
            {
                var favImgOffset = int.Parse(match1.Groups[1].Value);
                favIndex = favImgOffset / 19;
            }

            var fav = this[favIndex];
            var settings = Client.Current.Settings;
            settings.FavoriteCategoryNames[favIndex] = favName;
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
