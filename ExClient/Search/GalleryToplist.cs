using ExClient.Api;
using ExClient.Galleries;
using ExClient.Internal;
using ExClient.Status;
using HtmlAgilityPack;
using Opportunity.MvvmUniverse.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace ExClient.Search
{
    public sealed class GalleryToplist : PagingList<Gallery>
    {
        public GalleryToplist(ToplistName toplist)
        {
            this.PageCount = 200;
            this.Toplist = toplist;
        }

        public ToplistName Toplist { get; }

        protected override IAsyncOperation<IEnumerable<Gallery>> LoadItemsAsync(int pageIndex)
        {
            return AsyncInfo.Run<IEnumerable<Gallery>>(async token =>
            {
                var uri = new Uri($"https://e-hentai.org/toplist.php?tl={(int)Toplist}&p={pageIndex}");
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
                return await Gallery.FetchGalleriesAsync(gr);
            });
        }
    }
}
