using ExClient.Api;
using ExClient.Galleries;
using ExClient.Internal;
using ExClient.Search;
using HtmlAgilityPack;
using Opportunity.MvvmUniverse;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Windows.Foundation;
using Windows.Web.Http;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;

namespace ExClient
{
    public sealed class FavoriteCategory : ObservableObject
    {
        public static FavoriteCategory Removed { get; } = new FavoriteCategory(-1, LocalizedStrings.Resources.RemoveFromFavorites);

        public static FavoriteCategory All { get; } = new FavoriteCategory(-1, LocalizedStrings.Resources.AllFavorites);

        internal FavoriteCategory(int index, string name)
        {
            this.Index = index;
            this.name = name;
        }

        public FavoritesSearchResult Search(string keyword)
        {
            return FavoritesSearchResult.Search(keyword, this);
        }

        public int Index { get; }

        public string Name
        {
            get
            {
                if (this.Index < 0)
                    return this.name;
                else
                    return Client.Current.Settings.FavoriteCategoryNames[this.Index];
            }
        }

        internal void OnNameChanged() => OnPropertyChanged(nameof(this.Name));

        private readonly string name;

        private IAsyncOperationWithProgress<HttpResponseMessage, HttpProgress> post(long gId, ulong gToken, string note)
        {
            IEnumerable<KeyValuePair<string, string>> getInfo()
            {
                yield return new KeyValuePair<string, string>("apply", "Apply+Changes");
                var cat = this.Index.ToString();
                if (ReferenceEquals(this, All))
                    cat = "0";
                if (ReferenceEquals(this, Removed))
                    cat = "favdel";
                yield return new KeyValuePair<string, string>("favcat", cat);
                yield return new KeyValuePair<string, string>("favnote", note);
                yield return new KeyValuePair<string, string>("update", "1");
            }
            var requestUri = new Uri($"/gallerypopups.php?gid={gId}&t={gToken.ToTokenString()}&act=addfav", UriKind.Relative);
            var requestContent = new HttpFormUrlEncodedContent(getInfo());
            return Client.Current.HttpClient.PostAsync(requestUri, requestContent);
        }

        private static readonly Regex favNoteMatcher = new Regex(@"fn\.innerHTML\s*=\s*'(?:Note: )?(.*?) ';", RegexOptions.Compiled);
        private static readonly Regex favNameMatcher = new Regex(@"fi\.title\s*=\s*'(.*?)';", RegexOptions.Compiled);

        public IAsyncAction AddAsync(Gallery gallery, string note)
        {
            return Run(async token =>
            {
                var response = await this.post(gallery.ID, gallery.Token, note);
                var responseContent = await response.Content.ReadAsStringAsync();
                var match = favNoteMatcher.Match(responseContent, 1300);
                if (match.Success)
                    gallery.FavoriteNote = HtmlEntity.DeEntitize(match.Groups[1].Value);
                else
                    gallery.FavoriteNote = null;
                if (this.Index >= 0)
                {
                    var match2 = favNameMatcher.Match(responseContent, 1300);
                    if (match2.Success)
                    {
                        var settings = Client.Current.Settings;
                        settings.FavoriteCategoryNames[this.Index] = HtmlEntity.DeEntitize(match2.Groups[1].Value);
                        settings.StoreCache();
                    }
                }
                gallery.FavoriteCategory = this;
            });
        }

        public IAsyncAction AddAsync(GalleryInfo gallery, string note)
        {
            return Run(async token =>
            {
                var response = await post(gallery.ID, gallery.Token, note);
            });
        }

        public IAsyncAction RemoveAsync(Gallery gallery, string note)
        {
            return Removed.AddAsync(gallery, note);
        }

        public IAsyncAction RemoveAsync(GalleryInfo gallery, string note)
        {
            return Removed.AddAsync(gallery, note);
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
