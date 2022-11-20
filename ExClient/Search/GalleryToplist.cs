using ExClient.Galleries;
using ExClient.Internal;
using ExClient.Status;

using Opportunity.MvvmUniverse.Collections;

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;

using Windows.Foundation;

namespace ExClient.Search
{
    public sealed class GalleryToplist : PagingList<Gallery>
    {
        public GalleryToplist(ToplistName toplist)
        {
            PageCount = 200;
            Toplist = toplist;
        }

        public ToplistName Toplist { get; }

        private async Task<IEnumerable<Gallery>> _LoadCore(int pageIndex, CancellationToken token)
        {
            var uri = new Uri($"https://e-hentai.org/toplist.php?tl={(int)Toplist}&p={pageIndex}");
            var doctask = Client.Current.HttpClient.GetDocumentAsync(uri);
            token.Register(doctask.Cancel);
            var doc = await doctask;
            return await GalleryListParser.Parse(doc, token);
        }

        protected override IAsyncOperation<IEnumerable<Gallery>> LoadItemsAsync(int pageIndex)
            => AsyncInfo.Run(token => _LoadCore(pageIndex, token));
    }
}
