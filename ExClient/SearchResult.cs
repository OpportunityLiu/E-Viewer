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
        private static readonly Uri searchUri = new Uri("http://exhentai.org/");

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
            var result = new SearchResult(client, keyWord, category, advancedSearch);
            return result;
        }

        private SearchResult(Client client, string keyWord, Category category, AdvancedSearchOptions advancedSearch)
            : base(0)
        {
            this.client = client;
            this.KeyWord = keyWord ?? "";
            this.Category = category;
            this.AdvancedSearch = advancedSearch;
            this.PageCount = 1;
            this.RecordCount = -1;
        }

        private IAsyncOperation<uint> init()
        {
            return Task.Run(async () =>
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
                    query = new HttpFormUrlEncodedContent(args.Concat(AdvancedSearch.GetParamDictionary()));
                else
                    query = new HttpFormUrlEncodedContent(args);
                var uri = new Uri(searchUri, $"?{query}");
                searchResultBaseUri = uri.OriginalString;
                var lans = client.HttpClient.GetInputStreamAsync(uri);
                using(var ans = await lans)
                {
                    var doc = new HtmlDocument();
                    doc.Load(ans.AsStreamForRead());
                    var rcNode = doc.DocumentNode.Descendants("p").Where(node => node.GetAttributeValue("class", null) == "ip").SingleOrDefault();
                    if(rcNode == null)
                    {
                        RecordCount = 0;
                        return 0u;
                    }
                    var match = Regex.Match(rcNode.InnerText, @"Showing.+of\s+([0-9,]+)");
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
                        return 0u;
                }
            }).AsAsyncOperation();
        }

        private IAsyncOperation<uint> loadPage(HtmlDocument doc)
        {
            return Task.Run(async () =>
            {
                var table = (from node in doc.DocumentNode.Descendants("table")
                             where node.GetAttributeValue("class", "") == "itg"
                             select node).Single();
                var records = from node in table.Descendants("tr")
                              where node.GetAttributeValue("class", null) != null
                              let detail = (from node2 in node.Descendants("a")
                                            where node2.GetAttributeValue("onmouseover", "").StartsWith("show_image_pane")
                                            select node2).SingleOrDefault()
                              where detail != null
                              let match = Regex.Match(detail.GetAttributeValue("href", ""), @".+?/g/(\d+)/([0-9a-f]+).+?")
                              where match.Success
                              select new gdataRecord
                              {
                                  gid = long.Parse(match.Groups[1].Value),
                                  gtoken = match.Groups[2].Value
                              };
                var json = JsonConvert.SerializeObject(new
                {
                    method = "gdata",
                    @namespace = 1,
                    gidlist = records
                });
                var type = new
                {
                    gmetadata = (IEnumerable<Gallery>)null
                };
                var str = await client.PostApiAsync(json);
                var toAdd = JsonConvert.DeserializeAnonymousType(str, type).gmetadata.Select(item =>
                {
                    item.Owner = client;
                    var ignore = item.InitAsync();
                    return item;
                });
                return (uint)AddRange(toAdd);
            }).AsAsyncOperation();
        }

        [JsonArray]
        private class gdataRecord : IEnumerable
        {
            public long gid;
            public string gtoken;

            public IEnumerator GetEnumerator()
            {
                yield return gid;
                yield return gtoken;
            }
        }

        public string KeyWord
        {
            get;
            private set;
        }

        public Category Category
        {
            get;
            private set;
        }

        public AdvancedSearchOptions AdvancedSearch
        {
            get;
            private set;
        }

        private Client client;

        private string searchResultBaseUri;

        protected override IAsyncOperation<uint> LoadPageAsync(int pageIndex)
        {
            if(pageIndex == 0)
                return init();

            return Task.Run(async () =>
            {
                var uri = new Uri($"{this.searchResultBaseUri}&page={pageIndex.ToString()}");
                using(var stream = (await client.HttpClient.GetInputStreamAsync(uri)).AsStreamForRead())
                {
                    var doc = new HtmlDocument();
                    doc.Load(stream);
                    return await loadPage(doc);
                }
            }).AsAsyncOperation();
        }
    }
}
