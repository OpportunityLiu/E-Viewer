using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Web.Http;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;

namespace ExClient
{
    public sealed class FavoriteCategory : ObservableObject
    {
        public static FavoriteCategory Removed { get; } = new FavoriteCategory(-1);

        internal FavoriteCategory(int index)
        {
            Index = index;
        }

        public int Index
        {
            get;
        }

        public string Name
        {
            get
            {
                return name;
            }
            internal set
            {
                Set(ref name, value);
            }
        }

        private string name;

        private IEnumerable<KeyValuePair<string, string>> getInfo(string favnote)
        {
            yield return new KeyValuePair<string, string>("apply", "Apply+Changes");
            yield return new KeyValuePair<string, string>("favcat", this.Index == -1 ? "favdel" : this.Index.ToString());
            yield return new KeyValuePair<string, string>("favnote", favnote);
            yield return new KeyValuePair<string, string>("update", "1");
        }

        private static readonly Regex favNoteMatcher = new Regex(@"'Note: (.+?) ';", RegexOptions.Compiled);

        private IAsyncOperationWithProgress<HttpResponseMessage, HttpProgress> post(Client client, long gId, string gToken, string note)
        {
            var requestUri = new Uri(Client.RootUri, $"gallerypopups.php?gid={gId}&t={gToken}&act=addfav");
            var requestContent = new HttpFormUrlEncodedContent(getInfo(note));
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
    }
}
