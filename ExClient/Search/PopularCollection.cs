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
    public sealed class PopularCollection : IncrementalLoadingList<Gallery>
    {
        internal PopularCollection() { }

        protected override void ClearItems()
        {
            base.ClearItems();
            Set(ref this.hasMoreItems, true);
        }

        protected override void InsertItems(int index, IReadOnlyList<Gallery> items)
        {
            base.InsertItems(index, items);
            Set(ref this.hasMoreItems, false);
        }

        protected override void RemoveItems(int index, int count)
        {
            base.RemoveItems(index, count);
            Set(ref this.hasMoreItems, this.Count == 0);
        }

        private bool hasMoreItems = true;
        public override bool HasMoreItems => this.hasMoreItems;

        private void handleAdditionalInfo(HtmlNode trNode, Gallery gallery)
        {
            var infoNode = trNode.ChildNodes[2].LastChild.FirstChild;
            var favNode = infoNode.ChildNodes.FirstOrDefault(n => n.Id.StartsWith("favicon"));
            gallery.FavoriteCategory = Client.Current.Favorites.GetCategory(favNode);
            gallery.Rating.AnalyzeNode(trNode.ChildNodes[2].ChildNodes[2]);
        }

        protected override IAsyncOperation<IEnumerable<Gallery>> LoadMoreItemsImplementAsync(int count)
        {
            return AsyncInfo.Run<IEnumerable<Gallery>>(async token =>
            {
                var doc = await Client.Current.HttpClient.GetDocumentAsync(UriProvider.Eh.RootUri);
                var pp = doc.GetElementbyId("pp");
                if (pp == null) // Disabled popular
                {
                    Set(ref this.hasMoreItems, false);
                    return Array.Empty<Gallery>();
                }
                var nodes = (from div in pp.Elements("div")
                             where div.GetAttributeValue("class", "") == "id1"
                             select div).ToList();
                var ginfo = nodes.Select(n =>
                {
                    var link = n.Descendants("a").First().GetAttribute("href", default(Uri));
                    return GalleryInfo.Parse(link);
                }).ToList();
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
