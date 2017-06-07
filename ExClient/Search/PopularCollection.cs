using ExClient.Api;
using ExClient.Galleries;
using ExClient.Internal;
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
    public class PopularCollection : IncrementalLoadingCollection<Gallery>
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

        protected override IAsyncOperation<IList<Gallery>> LoadPageAsync(int pageIndex)
        {
            return AsyncInfo.Run(async token =>
            {
                var doc = await Client.Current.HttpClient.GetDocumentAsync(UriProvider.Eh.RootUri);
                var pp = doc.GetElementbyId("pp");
                var ginfo = (from div in pp.Elements("div")
                             where div.GetAttributeValue("class", "") == "id1"
                             let link = div.Descendants("a").First().GetAttributeValue("href", "")
                             select GalleryInfo.Parse(new Uri(link))).ToList();
                this.RecordCount = ginfo.Count;
                return await Gallery.FetchGalleriesAsync(ginfo);
            });
        }
    }
}
