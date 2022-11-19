using ExClient.Api;
using ExClient.Galleries;
using ExClient.Internal;

using HtmlAgilityPack;

using Opportunity.MvvmUniverse.Collections;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Windows.Foundation;

using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;

namespace ExClient.Search
{
    [DebuggerDisplay(@"\{Count = {Count} Page = {FirstPage}-{FirstPage+LoadedPageCount-1}/{PageCount}\}")]
    public abstract class SearchResult : PagingList<Gallery>
    {
        public abstract Uri SearchUri { get; }

        public string Keyword { get; }

        internal SearchResult(string keyword)
        {
            Keyword = keyword ?? "";
            Reset();
        }

        public void Reset()
        {
            Clear();
            PageCount = 1;
        }

        private void _UpdatePageCount(HtmlDocument doc)
        {
            var nextLink = doc.GetElementbyId("unext");
            if (nextLink is null)
            {
                PageCount = 0;
            }
            else if (nextLink.Name is "a")
            {
                PageCount++;
            }
        }

        protected virtual void LoadPageOverride(HtmlDocument doc) { }

        private async Task<IList<Gallery>> _LoadPage(HtmlDocument doc, CancellationToken token)
        {
            var galleries = await GalleryListParser.Parse(doc, token);
            LoadPageOverride(doc);
            return galleries;
        }

        protected override IAsyncOperation<IEnumerable<Gallery>> LoadItemsAsync(int pageIndex)
        {
            return Run(async token =>
            {
                var uri = Count == 0 ? SearchUri : new Uri($"{SearchUri}&next={this.Last().Id}");
                var getDoc = Client.Current.HttpClient.GetDocumentAsync(uri);
                token.Register(getDoc.Cancel);
                var doc = await getDoc;
                _UpdatePageCount(doc);
                if (PageCount == 0)
                    return Enumerable.Empty<Gallery>();

                var list = await _LoadPage(doc, token);
                token.ThrowIfCancellationRequested();
                if (list.Count == 0)
                    return Enumerable.Empty<Gallery>();

                if (pageIndex > FirstPage)
                {
                    for (var i = 0; i < list.Count;)
                    {
                        var id = list[i].Id;
                        if (this.Any(g => g.Id == id))
                            list.RemoveAt(i);
                        else
                            i++;
                    }
                }
                else if (pageIndex < FirstPage)
                {
                    for (var i = 0; i < list.Count; i++)
                    {
                        var id = list[i].Id;
                        var ga = this.FirstOrDefault(g => g.Id == id);
                        if (ga != null)
                            Remove(ga);
                    }
                }
                return list;
            });
        }
    }
}
