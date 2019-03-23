using ExClient.Api;
using ExClient.Galleries;
using ExClient.Internal;
using HtmlAgilityPack;
using Opportunity.MvvmUniverse.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
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

        private static readonly Regex _RecordCountMatcher = new Regex(@"Showing page\s*[0-9,]+\s*of\s*([0-9,]+)\s*result", RegexOptions.Compiled);

        private void _UpdatePageCount(HtmlDocument doc)
        {
            var idoNode = doc.DocumentNode
                .Element("html")
                .Element("body")
                .Element("div", "ido");
            var rcNode = idoNode.Descendants("p")
                .FirstOrDefault(node => node.HasClass("ip"));
            var pttNode = idoNode.Descendants("table")
                .FirstOrDefault(node => node.HasClass("ptt"));
            if (rcNode is null || pttNode is null)
            {
                PageCount = 0;
            }
            else if (_RecordCountMatcher.Match(rcNode.InnerText).Success)
            {
                PageCount = pttNode.Descendants("td").Select(node =>
                {
                    if (!int.TryParse(node.GetInnerText(), out var i))
                        i = -1;
                    return i;
                }).Max();
            }
            else
            {
                PageCount = 0;
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
                var listCount = Count;
                var uri = new Uri($"{SearchUri}&page={pageIndex.ToString()}");
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
                    for (var i = 0; i < list.Count; i++)
                    {
                        var id = list[i].Id;
                        if (this.Any(g => g.Id == id))
                            list.RemoveAt(i);
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
