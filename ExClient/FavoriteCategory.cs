using ExClient.Api;
using ExClient.Galleries;
using ExClient.Search;

using HtmlAgilityPack;

using Opportunity.MvvmUniverse;

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Windows.Web.Http;

namespace ExClient
{
    public sealed class FavoriteCategory : ObservableObject
    {
        internal FavoriteCategory(int index, string name)
        {
            Index = index;
            _Name = name;
        }

        public FavoritesSearchResult Search(string keyword)
            => FavoritesSearchResult.Search(keyword, this);

        public int Index { get; }

        public string Name
        {
            get
            {
                if (Index < 0)
                    return _Name;
                else
                    return Client.Current.Settings.FavoriteCategoryNames[Index];
            }
        }

        internal void OnNameChanged() => OnPropertyChanged(nameof(Name));

        private readonly string _Name;

        private async Task<string> _PostAddFav(long gId, EToken gToken, string note, CancellationToken token)
        {
            IEnumerable<KeyValuePair<string, string>> getInfo()
            {
                yield return new KeyValuePair<string, string>("apply", "Apply+Changes");
                yield return new KeyValuePair<string, string>("favcat", Index < 0 ? "favdel" : Index.ToString());
                yield return new KeyValuePair<string, string>("favnote", note);
                yield return new KeyValuePair<string, string>("update", "1");
            }
            var requestUri = new Uri($"/gallerypopups.php?gid={gId}&t={gToken.ToString()}&act=addfav", UriKind.Relative);
            var post = Client.Current.HttpClient.PostAsync(requestUri, new HttpFormUrlEncodedContent(getInfo()));
            token.Register(post.Cancel);
            var res = await post;
            return await res.Content.ReadAsStringAsync();
        }

        private static readonly Regex _FavNoteMatcher = new Regex(@"fn\.innerHTML\s*=\s*'(?:Note: )?(.*?) ';", RegexOptions.Compiled);
        private static readonly Regex _FavNameMatcher = new Regex(@"fi\.title\s*=\s*'(.*?)';", RegexOptions.Compiled);

        public async Task AddAsync(Gallery gallery, string note, CancellationToken token = default)
        {
            var response = await _PostAddFav(gallery.Id, gallery.Token, note, token);
            var start = response.Length > 1300 ? 1300 : 0;
            var match = _FavNoteMatcher.Match(response, start);
            if (match.Success)
                gallery.FavoriteNote = HtmlEntity.DeEntitize(match.Groups[1].Value);
            else
                gallery.FavoriteNote = null;

            if (Index >= 0)
            {
                var match2 = _FavNameMatcher.Match(response, start);
                if (match2.Success)
                {
                    var settings = Client.Current.Settings;
                    settings.FavoriteCategoryNames[Index] = HtmlEntity.DeEntitize(match2.Groups[1].Value);
                    settings.StoreCache();
                }
            }
            gallery.FavoriteCategory = this;
        }

        public async Task AddAsync(GalleryInfo gallery, string note, CancellationToken token = default)
        {
            _ = await _PostAddFav(gallery.ID, gallery.Token, note, token);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
