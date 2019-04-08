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

        private static readonly Regex _RecordCountMatcher = new Regex(@"Showing\s*[0-9,]+\s*result", RegexOptions.Compiled);

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
            var pageCount = 1;
            if (rcNode is null || pttNode is null)
            {
                pageCount = 0;
            }
            else if (_RecordCountMatcher.Match(rcNode.InnerText).Success)
            {
                pageCount = pttNode.Descendants("td").Select(node =>
                {
                    var pageStr = node.GetInnerText();
                    if (int.TryParse(pageStr, out var i))
                        return i;
                    if (pageStr.IndexOf('-') is var idx && idx > 0 && int.TryParse(pageStr.Substring(idx + 1), out var j))
                        return j;
                    return -1;
                }).Max();
            }
            else
            {
                pageCount = 0;
            }

            if (PageCount < pageCount)
                PageCount = pageCount;
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
