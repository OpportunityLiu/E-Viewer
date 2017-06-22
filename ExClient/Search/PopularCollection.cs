using ExClient.Api;
using ExClient.Galleries;
using ExClient.Internal;
using HtmlAgilityPack;
using Opportunity.MvvmUniverse.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;

namespace ExClient.Search
{
    public sealed class PopularCollection : IncrementalLoadingCollection<Gallery>
    {
        public PopularCollection()
        {
            Reset();
        }

        public void Reset()
        {
            this.ResetAll();
            this.RecordCount = -1;
            this.PageCount = 1;
        }

        private void handleAdditionalInfo(HtmlNode trNode, Gallery gallery)
        {
            var infoNode = trNode.ChildNodes[2].LastChild.FirstChild;
            var favNode = infoNode.ChildNodes.FirstOrDefault(n => n.Id.StartsWith("favicon"));
            gallery.FavoriteCategory = Client.Current.Favorites.GetCategory(favNode);
        }

        protected override IAsyncOperation<IList<Gallery>> LoadPageAsync(int pageIndex)
        {
            return AsyncInfo.Run(async token =>
            {
                var doc = await Client.Current.HttpClient.GetDocumentAsync(UriProvider.Eh.RootUri);
                var pp = doc.GetElementbyId("pp");
                var nodes = (from div in pp.Elements("div")
                             where div.GetAttributeValue("class", "") == "id1"
                             select div).ToList();
                var ginfo = nodes.Select(n =>
                {
                    var link = n.Descendants("a").First().GetAttributeValue("href", "");
                    return GalleryInfo.Parse(new Uri(link));
                }).ToList();
                this.RecordCount = ginfo.Count;
                var galleries = await Gallery.FetchGalleriesAsync(ginfo);
                for (var i = 0; i < ginfo.Count; i++)
                {
                    handleAdditionalInfo(nodes[i], galleries[i]);
                }
                return galleries;
            });
        }
    }
}
