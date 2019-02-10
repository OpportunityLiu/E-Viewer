using ExClient.Api;
using ExClient.Galleries;
using ExClient.Internal;
using HtmlAgilityPack;
using Opportunity.MvvmUniverse.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;

namespace ExClient.Search
{
    [System.Diagnostics.DebuggerDisplay(@"\{Count = {Count} Page = {FirstPage}-{FirstPage+LoadedPageCount-1}/{PageCount}\}")]
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

        private static readonly Regex recordCountMatcher = new Regex(@"Showing.+?-\s*[0-9,]+\s*of\s*([0-9,]+)", RegexOptions.Compiled);

        private void updatePageCount(HtmlDocument doc)
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
            else if (recordCountMatcher.Match(rcNode.InnerText).Success)
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

        private static readonly Regex gLinkMatcher = new Regex(@".+?/g/(\d+)/([0-9a-f]+).+?", RegexOptions.Compiled);

        protected virtual void HandleAdditionalInfo(HtmlNode dataNode, Gallery gallery, bool isList)
        {
            if (isList)
            {
                var infoNode = dataNode.ChildNodes[2].FirstChild;
                var attributeNode = infoNode.ChildNodes[1]; //class = it3
                var favNode = attributeNode.ChildNodes.FirstOrDefault(n => n.Id.StartsWith("favicon"));
                gallery.FavoriteCategory = Client.Current.Favorites.GetCategory(favNode);
                gallery.Rating.AnalyzeNode(infoNode.LastChild.FirstChild);
            }
            else
            {
                var infoNode = dataNode.Element("div", "id4");
                gallery.Rating.AnalyzeNode(infoNode.Element("div", "id43"));
                var attributeNode = infoNode.Element("div", "id44").Element("div");
                var favNode = attributeNode.ChildNodes.FirstOrDefault(n => n.Id.StartsWith("favicon"));
                gallery.FavoriteCategory = Client.Current.Favorites.GetCategory(favNode);
            }
        }

        protected virtual void LoadPageOverride(HtmlDocument doc) { }

        private async Task<IList<Gallery>> loadPage(HtmlDocument doc, CancellationToken token)
        {
            var isList = true;
            var dataRoot = doc.DocumentNode.Descendants("table").SingleOrDefault(node => node.HasClass("itg"));
            if (dataRoot is null)
            {
                isList = false;
                dataRoot = doc.DocumentNode.Descendants("div").SingleOrDefault(node => node.HasClass("itg"));
            }
            var gInfoList = new List<GalleryInfo>(dataRoot.ChildNodes.Count);
            var dataNodeList = new List<HtmlNode>(dataRoot.ChildNodes.Count);
            if (isList)
            {
                foreach (var node in dataRoot.Elements("tr").Skip(1))//skip table header
                {
                    var infoNode = node.ChildNodes[2].FirstChild;
                    var detailNode = infoNode.ChildNodes[2]; //class = it5
                    var match = gLinkMatcher.Match(detailNode.FirstChild.GetAttribute("href", ""));
                    dataNodeList.Add(node);
                    gInfoList.Add(new GalleryInfo(long.Parse(match.Groups[1].Value), match.Groups[2].Value.ToToken()));
                }
            }
            else
            {
                foreach (var node in dataRoot.Elements("div", "id1"))
                {
                    var link = node.Element("div", "id2").Element("a");
                    var match = gLinkMatcher.Match(link.GetAttribute("href", ""));
                    dataNodeList.Add(node);
                    gInfoList.Add(new GalleryInfo(long.Parse(match.Groups[1].Value), match.Groups[2].Value.ToToken()));
                }
            }
            var getG = Gallery.FetchGalleriesAsync(gInfoList);
            token.Register(getG.Cancel);
            var galleries = await getG;
            token.ThrowIfCancellationRequested();
            for (var i = 0; i < galleries.Count; i++)
            {
                HandleAdditionalInfo(dataNodeList[i], galleries[i], isList);
            }
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
                updatePageCount(doc);
                if (PageCount == 0)
                    return Enumerable.Empty<Gallery>();

                var list = await loadPage(doc, token);
                token.ThrowIfCancellationRequested();
                if (list.Count == 0)
                    return Enumerable.Empty<Gallery>();

                if (pageIndex > FirstPage)
                {
                    for (var i = 0; i < list.Count; i++)
                    {
                        var id = list[i].ID;
                        if (this.Any(g => g.ID == id))
                            list.RemoveAt(i);
                    }
                }
                else if (pageIndex < FirstPage)
                {
                    for (var i = 0; i < list.Count; i++)
                    {
                        var id = list[i].ID;
                        var ga = this.FirstOrDefault(g => g.ID == id);
                        if (ga != null)
                            Remove(ga);
                    }
                }

                return list;
            });
        }
    }
}
