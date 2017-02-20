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

namespace ExClient
{
    public class SearchResult : IncrementalLoadingCollection<Gallery>
    {
        private static readonly Uri searchUri = Client.RootUri;

        public static readonly Category DefaultFliter = Category.All;
        private static readonly IReadOnlyDictionary<Category, string> searchFliterNames = new Dictionary<Category, string>()
        {
            [Category.Doujinshi] = "f_doujinshi",
            [Category.Manga] = "f_manga",
            [Category.ArtistCG] = "f_artistcg",
            [Category.GameCG] = "f_gamecg",
            [Category.Western] = "f_western",
            [Category.NonH] = "f_non-h",
            [Category.ImageSet] = "f_imageset",
            [Category.Cosplay] = "f_cosplay",
            [Category.AsianPorn] = "f_asianporn",
            [Category.Misc] = "f_misc"
        };

        internal static SearchResult Search(Client client, string keyWord, Category category, AdvancedSearchOptions advancedSearch)
        {
            if(category == Category.Unspecified)
                category = DefaultFliter;
            var result = new SearchResult(client, keyWord, category, advancedSearch?.Clone(true));
            return result;
        }

        private SearchResult(Client client, string keyWord, Category category, AdvancedSearchOptions advancedSearch)
            : base(0)
        {
            this.owner = client;
            this.KeyWord = keyWord ?? "";
            this.Category = category;
            this.AdvancedSearch = advancedSearch;
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
                var lans = owner.HttpClient.GetInputStreamAsync(uri);
                token.Register(lans.Cancel);
                using(var ans = await lans)
                {
                    var doc = new HtmlDocument();
                    doc.Load(ans.AsStreamForRead());
                    var rcNode = doc.DocumentNode.Descendants("p").Where(node => node.GetAttributeValue("class", null) == "ip").SingleOrDefault();
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
                        var pcNodes = doc.DocumentNode.Descendants("td")
                            .Where(node => "document.location=this.firstChild.href" == node.GetAttributeValue("onclick", ""))
                            .Select(node =>
                            {
                                int i;
                                var su = int.TryParse(node.InnerText, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out i);
                                return Tuple.Create(su, i);
                            })
                            .Where(select => select.Item1)
                            .DefaultIfEmpty(Tuple.Create(true, 1))
                            .Max(select => select.Item2);
                        PageCount = pcNodes;
                        return await loadPage(doc);
                    }
                    else
                        return Array.Empty<Gallery>();
                }
            });
        }

        private Uri createUri()
        {
            var args = new Dictionary<string, string>()
            {
                ["f_search"] = KeyWord
            };
            foreach(var item in searchFliterNames)
            {
                if(Category.HasFlag(item.Key))
                    args.Add(item.Value, "1");
                else
                    args.Add(item.Value, "0");
            }
            args.Add("f_apply", "Apply Filter");
            HttpFormUrlEncodedContent query;
            if(AdvancedSearch != null)
                query = new HttpFormUrlEncodedContent(args.Concat(AdvancedSearch.AsEnumerable()));
            else
                query = new HttpFormUrlEncodedContent(args);
            return new Uri(searchUri, $"?{query}");
        }

        private static readonly Regex gLinkMatcher = new Regex(@".+?/g/(\d+)/([0-9a-f]+).+?", RegexOptions.Compiled);

        private IAsyncOperation<IList<Gallery>> loadPage(HtmlDocument doc)
        {
            return Run(async token =>
            {
                var table = (from node in doc.DocumentNode.Descendants("table")
                             where node.GetAttributeValue("class", "") == "itg"
                             select node).Single();
                var gInfoList = new List<GalleryInfo>(25);
                var gFavList = new List<FavoriteCategory>(25);
                foreach(var node in table.ChildNodes.Skip(3))//skip table header
                {
                    if(node.NodeType == HtmlNodeType.Text)
                        continue;
                    var infoNode = node.ChildNodes[2].FirstChild;
                    var attributeNode = infoNode.ChildNodes[1];
                    var detailNode = infoNode.ChildNodes[2];
                    var match = gLinkMatcher.Match(detailNode.FirstChild.GetAttributeValue("href", ""));
                    var favNode = attributeNode.FirstChild;
                    gInfoList.Add(new GalleryInfo(long.Parse(match.Groups[1].Value), match.Groups[2].Value));
                    gFavList.Add(owner.Favorites.GetCategory(favNode));
                }
                var galleries = await Gallery.FetchGalleriesAsync(gInfoList);
                for(int i = 0; i < galleries.Count; i++)
                {
                    galleries[i].FavoriteCategory = gFavList[i];
                }
                return galleries;
            });
        }

        public string KeyWord
        {
            get;
        }

        public Category Category
        {
            get;
        }

        public AdvancedSearchOptions AdvancedSearch
        {
            get;
        }

        private Client owner;

        private string searchResultBaseUri;

        protected override IAsyncOperation<IList<Gallery>> LoadPageAsync(int pageIndex)
        {
            if(pageIndex == 0)
                return init();

            return Run(async token =>
            {
                var uri = new Uri($"{this.searchResultBaseUri}&page={pageIndex}");
                var getStream = owner.HttpClient.GetInputStreamAsync(uri);
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
