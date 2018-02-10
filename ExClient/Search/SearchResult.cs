using ExClient.Api;
using ExClient.Galleries;
using ExClient.Internal;
using HtmlAgilityPack;
using Opportunity.MvvmUniverse.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Foundation;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;

namespace ExClient.Search
{
    public abstract class SearchResult : PagingList<Gallery>
    {
        public abstract Uri SearchUri { get; }

        public string Keyword { get; }

        internal SearchResult(string keyword)
        {
            this.Keyword = keyword ?? "";
            Reset();
        }

        public void Reset()
        {
            ResetAll();
            this.PageCount = 1;
            this.RecordCount = -1;
        }

        private static readonly Regex recordCountMatcher = new Regex(@"Showing.+of\s+([0-9,]+)", RegexOptions.Compiled);

        private void updatePageCountAndRecordCount(HtmlDocument doc)
        {
            var rcNode = doc.DocumentNode
                .Element("html")
                .Element("body")
                .Element("div")
                .Descendants("p")
                .FirstOrDefault(node => node.HasClass("ip"));
            if (rcNode == null)
            {
                this.RecordCount = 0;
            }
            else
            {
                var match = recordCountMatcher.Match(rcNode.InnerText);
                if (match.Success)
                    this.RecordCount = int.Parse(match.Groups[1].Value, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture);
                else
                    this.RecordCount = 0;
            }
            if (!this.IsEmpty)
            {
                var pcNodes = rcNode.NextSibling
                    .Element("tr")
                    .ChildNodes
                    .Select(node =>
                    {
                        if (int.TryParse(node.InnerText, out var i))
                            return i;
                        return int.MinValue;
                    }).DefaultIfEmpty(1).Max();
                this.PageCount = pcNodes;
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

        private async Task<IEnumerable<Gallery>> loadPage(HtmlDocument doc)
        {
            var isList = true;
            var dataRoot = doc.DocumentNode.Descendants("table").SingleOrDefault(node => node.HasClass("itg"));
            if (dataRoot == null)
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
            var galleries = await Gallery.FetchGalleriesAsync(gInfoList);
            for (var i = 0; i < galleries.Count; i++)
            {
                HandleAdditionalInfo(dataNodeList[i], galleries[i], isList);
            }
            LoadPageOverride(doc);
            return galleries;
        }

        protected sealed override IAsyncOperation<IEnumerable<Gallery>> LoadPageAsync(int pageIndex)
        {
            return Run(async token =>
            {
                var uri = new Uri($"{this.SearchUri}&page={pageIndex.ToString()}");
                var getDoc = Client.Current.HttpClient.GetDocumentAsync(uri);
                token.Register(getDoc.Cancel);
                var doc = await getDoc;
                updatePageCountAndRecordCount(doc);
                if (this.IsEmpty)
                    return Array.Empty<Gallery>();
                return await loadPage(doc);
            });
        }
    }
}
