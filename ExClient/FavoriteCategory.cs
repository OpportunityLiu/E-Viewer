using ExClient.Api;
using HtmlAgilityPack;
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
        public static FavoriteCategory Removed { get; } = new FavoriteCategory(-1);

        public static FavoriteCategory All { get; } = new FavoriteCategory(-1) { Name = LocalizedStrings.Resources.AllFavorites };

        internal FavoriteCategory(int index)
        {
            this.Index = index;
        }

        public int Index
        {
            get;
        }

        public string Name
        {
            get
            {
                return this.name ?? $"favorites {this.Index}";
            }
            internal set
            {
                Set(ref this.name, value);
            }
        }

        private string name;

        private static readonly Regex favNoteMatcher = new Regex(@"'Note: (.+?) ';", RegexOptions.Compiled);

        private IAsyncOperationWithProgress<HttpResponseMessage, HttpProgress> post(Client client, long gId, string gToken, string note)
        {
            IEnumerable<KeyValuePair<string, string>> getInfo()
            {
                yield return new KeyValuePair<string, string>("apply", "Apply+Changes");
                var cat = this.Index.ToString();
                if(ReferenceEquals(this, All))
                    cat = "0";
                if(ReferenceEquals(this, Removed))
                    cat = "favdel";
                yield return new KeyValuePair<string, string>("favcat", cat);
                yield return new KeyValuePair<string, string>("favnote", note);
                yield return new KeyValuePair<string, string>("update", "1");
            }
            var requestUri = new Uri(client.Uris.RootUri, $"gallerypopups.php?gid={gId}&t={gToken}&act=addfav");
            var requestContent = new HttpFormUrlEncodedContent(getInfo());
            return client.HttpClient.PostAsync(requestUri, requestContent);
        }

        public IAsyncAction Add(Gallery gallery, string note)
        {
            return Run(async token =>
            {
                var response = await post(gallery.Owner, gallery.Id, gallery.Token, note);
                var responseContent = await response.Content.ReadAsStringAsync();
                var match = favNoteMatcher.Match(responseContent, 1300);
                if(match.Success)
                    gallery.FavoriteNote = HtmlEntity.DeEntitize(match.Groups[1].Value);
                else
                    gallery.FavoriteNote = null;
                gallery.FavoriteCategory = this;
            });
        }

        public IAsyncAction Add(GalleryInfo gallery, string note)
        {
            return Run(async token =>
            {
                var response = await post(Client.Current, gallery.Id, gallery.Token, note);
            });
        }

        public IAsyncAction Remove(Gallery gallery, string note)
        {
            return Removed.Add(gallery, note);
        }

        public IAsyncAction Remove(GalleryInfo gallery, string note)
        {
            return Removed.Add(gallery, note);
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
