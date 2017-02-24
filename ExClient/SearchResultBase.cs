using HtmlAgilityPack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Data;
using Windows.Web.Http;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;
using Newtonsoft.Json;
using System.Net;
using System.Runtime.InteropServices;
using GalaSoft.MvvmLight.Threading;
using ExClient.Api;

namespace ExClient
{
    public abstract class SearchResultBase : IncrementalLoadingCollection<Gallery>
    {
        protected abstract Uri SearchUri { get; }

        internal SearchResultBase(Client owner)
            : base(0)
        {
            this.Owner = owner;
            Reset();
        }

        public void Reset()
        {
            ResetAll();
            this.PageCount = 1;
            this.RecordCount = -1;
        }

        private static readonly Regex recordCountMatcher = new Regex(@"Showing.+of\s+([0-9,]+)", RegexOptions.Compiled);

        private IAsyncOperation<IList<Gallery>> init()
        {
            return Run(async token =>
            {
                var uri = createUri();
                searchResultBaseUri = uri.OriginalString;
                var lans = Owner.HttpClient.GetInputStreamAsync(uri);
                token.Register(lans.Cancel);
                using(var ans = await lans)
                {
                    var doc = new HtmlDocument();
                    doc.Load(ans.AsStreamForRead());
                    var rcNode = doc.DocumentNode
                        .Element("html")
                        .Element("body")
                        .Element("div")
                        .Descendants("p")
                        .SingleOrDefault(node => node.GetAttributeValue("class", null) == "ip");
                    if(rcNode == null)
                    {
                        RecordCount = 0;
                        return Array.Empty<Gallery>();
                    }
                    var match = recordCountMatcher.Match(rcNode.InnerText);
                    if(match.Success)
                        RecordCount = int.Parse(match.Groups[1].Value, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture);
                    if(!IsEmpty)
                    {
                        var pcNodes = rcNode.NextSibling
                            .Element("tr")
                            .ChildNodes
                            .Select(node =>
                            {
                                int i;
                                var su = int.TryParse(node.InnerText, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out i);
                                if(su) return i;
                                return int.MinValue;
                            }).Max();
                        PageCount = Math.Max(1, pcNodes);
                        return await loadPage(doc);
                    }
                    else
                        return Array.Empty<Gallery>();
                }
            });
        }

        protected abstract IEnumerable<KeyValuePair<string, string>> GetUriQuery();

        private Uri createUri()
        {
            return new Uri(SearchUri, $"?{new HttpFormUrlEncodedContent(GetUriQuery())}");
        }

        private static readonly Regex gLinkMatcher = new Regex(@".+?/g/(\d+)/([0-9a-f]+).+?", RegexOptions.Compiled);

        protected virtual void HandleAdditionalInfo(HtmlNode trNode, Gallery gallery)
        {
            var infoNode = trNode.ChildNodes[2].FirstChild;
            var attributeNode = infoNode.ChildNodes[1]; //class = it3
            var favNode = attributeNode.ChildNodes.FirstOrDefault(n => n.Id.StartsWith("favicon"));
            gallery.FavoriteCategory = Owner.Favorites.GetCategory(favNode);
        }

        protected virtual void LoadPageOverride(HtmlDocument doc) { }

        private IAsyncOperation<IList<Gallery>> loadPage(HtmlDocument doc)
        {
            return Run(async token =>
            {
                var table = doc.DocumentNode.Descendants("table").Single(node => node.GetAttributeValue("class", "") == "itg");
                var gInfoList = new List<GalleryInfo>(25);
                var trNodeList = new List<HtmlNode>(25);
                foreach(var node in table.Elements("tr").Skip(1))//skip table header
                {
                    var infoNode = node.ChildNodes[2].FirstChild;
                    var detailNode = infoNode.ChildNodes[2]; //class = it5
                    var match = gLinkMatcher.Match(detailNode.FirstChild.GetAttributeValue("href", ""));
                    trNodeList.Add(node);
                    gInfoList.Add(new GalleryInfo(long.Parse(match.Groups[1].Value), match.Groups[2].Value));
                }
                var galleries = await Gallery.FetchGalleriesAsync(gInfoList);
                for(int i = 0; i < galleries.Count; i++)
                {
                    HandleAdditionalInfo(trNodeList[i], galleries[i]);
                }
                LoadPageOverride(doc);
                return galleries;
            });
        }

        protected Client Owner { get; }

        private string searchResultBaseUri;

        protected sealed override IAsyncOperation<IList<Gallery>> LoadPageAsync(int pageIndex)
        {
            if(pageIndex == 0)
                return init();

            return Run(async token =>
            {
                var uri = new Uri($"{this.searchResultBaseUri}&page={pageIndex}");
                var getStream = Owner.HttpClient.GetInputStreamAsync(uri);
                token.Register(getStream.Cancel);
                using(var stream = (await getStream).AsStreamForRead())
                {
                    var doc = new HtmlDocument();
                    doc.Load(stream);
                    var r = await loadPage(doc);
                    return r;
                }
            });
        }
    }
}
