using ExClient.Galleries;
using ExClient.Status;
using Opportunity.MvvmUniverse.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using HtmlAgilityPack;
using ExClient.Api;
using ExClient.Internal;

namespace ExClient.Search
{
    public sealed class GalleryToplist : IncrementalLoadingList<Gallery>
    {
        public GalleryToplist(ToplistName toplist)
        {
            this.Toplist = toplist;
        }

        public override bool HasMoreItems => this.Count < 10000;

        public ToplistName Toplist { get; }

        protected override IAsyncOperation<LoadItemsResult<Gallery>> LoadItemsAsync(int count)
        {
            var page = Count / 50;
            return AsyncInfo.Run(async token =>
            {
                var uri = new Uri($"https://e-hentai.org/toplist.php?tl={(int)Toplist}&p={page}");
                var doctask = Client.Current.HttpClient.GetDocumentAsync(uri);
                token.Register(doctask.Cancel);
                var doc = await doctask;
                token.ThrowIfCancellationRequested();
                var records = doc.DocumentNode.SelectNodes("//table[@class='itg']/tr[position()>1]/td[5]/div/div[3]/a/@href").ToList();
                var gr = new List<GalleryInfo>(records.Count);
                foreach (var item in records)
                {
                    var guri = item.GetAttribute("href", DomainProvider.Eh.RootUri, null);
                    gr.Add(GalleryInfo.Parse(guri));
                }
                var galleries = await Gallery.FetchGalleriesAsync(gr);
                return LoadItemsResult.Create(page * 50, galleries);
            });
        }
    }
}
