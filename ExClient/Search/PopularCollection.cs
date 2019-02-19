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
            OnPropertyChanged(nameof(HasMoreItems));
        }

        protected override void InsertItem(int index, Gallery item)
        {
            base.InsertItem(index, item);
            if (Count == 1)
            {
                OnPropertyChanged(nameof(HasMoreItems));
            }
        }

        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);
            if (Count == 0)
            {
                OnPropertyChanged(nameof(HasMoreItems));
            }
        }

        public override bool HasMoreItems => Count == 0;

        private void handleAdditionalInfo(HtmlNode trNode, Gallery gallery)
        {
            var infoNode = trNode.ChildNodes[2].LastChild.FirstChild;
            var favNode = infoNode.ChildNodes.FirstOrDefault(n => n.Id.StartsWith("favicon"));
            gallery.FavoriteCategory = Client.Current.Favorites.GetCategory(favNode);
            gallery.Rating.AnalyzeNode(trNode.ChildNodes[2].ChildNodes[2]);
        }

        private IAsyncOperation<LoadItemsResult<Gallery>> loadCore(bool reIn)
        {
            return AsyncInfo.Run(async token =>
            {
                var doc = await Client.Current.HttpClient.GetDocumentAsync(DomainProvider.Eh.RootUri);
                var pp = doc.GetElementbyId("pp");
                if (pp is null) // Disabled popular
                {
                    if (reIn)
                    {
                        return LoadItemsResult.Empty<Gallery>();
                    }
                    else
                    {
                        await DomainProvider.Eh.Settings.FetchAsync();
                        await DomainProvider.Eh.Settings.SendAsync();
                        return await loadCore(true);
                    }
                }
                var nodes = (from div in pp.Elements("div")
                             where div.HasClass("id1")
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
                return LoadItemsResult.Create(0, galleries);
            });
        }

        protected override IAsyncOperation<LoadItemsResult<Gallery>> LoadItemsAsync(int count)
            => loadCore(false);
    }
}
