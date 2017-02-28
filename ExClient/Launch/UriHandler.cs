using ExClient.Api;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.Foundation;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;

namespace ExClient.Launch
{
    internal sealed class UriHandlerData
    {
        public UriHandlerData(Uri uri)
        {
            this.Uri = uri;
            this.Paths = uri.AbsolutePath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if(this.Paths.Count != 0)
                this.Path0 = this.Paths[0].ToLowerInvariant();
            else
                this.Path0 = "";
            this.queriesLoader = new Lazy<IReadOnlyDictionary<string, string>>(getQueries);
        }

        public Uri Uri { get; }
        public IReadOnlyList<string> Paths { get; }
        public string Path0 { get; }

        private Lazy<IReadOnlyDictionary<string, string>> queriesLoader;
        public IReadOnlyDictionary<string, string> Queries => this.queriesLoader.Value;

        private static char[] split1 = "&".ToCharArray();
        private static char[] split2 = "=".ToCharArray();
        private static IReadOnlyDictionary<string, string> empty = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());

        private IReadOnlyDictionary<string, string> getQueries()
        {
            var query = this.Uri.Query;
            if(string.IsNullOrWhiteSpace(query) || query.Length <= 1 || query[0] != '?')
                return empty;
            query = query.Substring(1);
            var divided = query.Split(split1, StringSplitOptions.RemoveEmptyEntries);
            return new ReadOnlyDictionary<string, string>((from item in divided
                                                           select item.Split(split2, 2, StringSplitOptions.None))
                   .ToDictionary(i => i[0], i => Unescape(i[1])));
        }

        public static string Unescape(string value)
        {
            value = value.Replace('+', ' ');
            value = Uri.UnescapeDataString(value);
            return value;
        }

        public static string Unescape2(string value)
        {
            value = Uri.UnescapeDataString(value);
            return Unescape(value);
        }
    }

    internal abstract class UriHandler
    {
        protected static bool QueryValueAsBoolean(string value)
        {
            return value != "0" && value != "";
        }

        protected static int QueryValueAsInt32(string value)
        {
            if(int.TryParse(value, out var r))
                return r;
            value = value.Trim();
            var i = 0;
            for(; i < value.Length; i++)
            {
                if(value[i] < '0' || value[i] > '9')
                    break;
            }
            if(int.TryParse(value.Substring(0, i), out r))
                return r;
            return 0;
        }

        public abstract bool CanHandle(UriHandlerData data);
        public abstract IAsyncOperation<LaunchResult> HandleAsync(UriHandlerData data);
    }

    internal class GalleryHandler : UriHandler
    {
        protected static IAsyncOperation<IReadOnlyList<GalleryInfo>> GetGalleryInfoAsync(IEnumerable<ImageInfo> pageList)
        {
            return Run(async token =>
            {
                var result = await Client.Current.HttpClient.PostApiAsync(new GalleryToken(pageList));
                var type = new
                {
                    tokenlist = (IReadOnlyList<GalleryInfo>)null
                };
                return JsonConvert.DeserializeAnonymousType(result, type).tokenlist;
            });
        }

        public override bool CanHandle(UriHandlerData data)
        {
            if(data.Path0 == "g" && data.Paths.Count == 3)
            {
                return long.TryParse(data.Paths[1], out var gId);
            }
            return false;
        }

        public override IAsyncOperation<LaunchResult> HandleAsync(UriHandlerData data)
        {
            var gInfo = new GalleryInfo(long.Parse(data.Paths[1]), data.Paths[2]);
            return Run(async token =>
            {
                var g = (await Gallery.FetchGalleriesAsync(new[] { gInfo })).Single();
                return (LaunchResult)new GalleryLaunchResult(g, -1, GalleryLaunchStatus.Default);
            });
        }
    }

    internal sealed class GalleryTorrentHandler : GalleryHandler
    {
        public override bool CanHandle(UriHandlerData data)
        {
            if(data.Path0 == "gallerytorrents.php" && data.Paths.Count == 1)
            {
                return data.Queries.ContainsKey("gid") 
                    && data.Queries.ContainsKey("t") 
                    && long.TryParse(data.Queries["gid"], out var gId);
            }
            return false;
        }

        public override IAsyncOperation<LaunchResult> HandleAsync(UriHandlerData data)
        {
            var gInfo = new GalleryInfo(long.Parse(data.Queries["gid"]), data.Queries["t"]);
            return Run(async token =>
            {
                var g = (await Gallery.FetchGalleriesAsync(new[] { gInfo })).Single();
                return (LaunchResult)new GalleryLaunchResult(g, -1, GalleryLaunchStatus.Torrent);
            });
        }
    }

    internal sealed class GalleryImageHandler : GalleryHandler
    {
        public override bool CanHandle(UriHandlerData data)
        {
            if(data.Path0 == "s" && data.Paths.Count == 3)
            {
                var sp = data.Paths[2].Split('-');
                return (sp.Length == 2)
                    && long.TryParse(sp[0], out var gId)
                    && int.TryParse(sp[1], out var pId);
            }
            return false;
        }

        public override IAsyncOperation<LaunchResult> HandleAsync(UriHandlerData data)
        {
            var imgKey = data.Paths[1];
            var sp = data.Paths[2].Split('-');
            var gId = long.Parse(sp[0]);
            var pId = int.Parse(sp[1]);
            var iInfo = new ImageInfo(gId, imgKey, pId);
            return Run(async token =>
            {
                var gInfo = await GetGalleryInfoAsync(new[] { iInfo });
                var g = (await Gallery.FetchGalleriesAsync(gInfo)).Single();
                return (LaunchResult)new GalleryLaunchResult(g, pId, GalleryLaunchStatus.Image);
            });
        }

    }

    internal class SearchHandler : UriHandler
    {
        protected static string UnescapeKeyword(string query)
        {
            return query.Replace("+", "").Replace("&", "");
        }

        public override bool CanHandle(UriHandlerData data)
        {
            return data.Paths.Count == 0;
        }

        public override IAsyncOperation<LaunchResult> HandleAsync(UriHandlerData data)
        {
            var keyword = "";
            var category = Category.Unspecified;
            var advanced = new AdvancedSearchOptions();
            var ap = false;
            var av = false;
            foreach(var item in data.Queries)
            {
                switch(item.Key)
                {
                case "f_apply":
                    ap = QueryValueAsBoolean(item.Value);
                    break;
                case "f_doujinshi":
                    if(QueryValueAsBoolean(item.Value)) category |= Category.Doujinshi;
                    break;
                case "f_manga":
                    if(QueryValueAsBoolean(item.Value)) category |= Category.Manga;
                    break;
                case "f_artistcg":
                    if(QueryValueAsBoolean(item.Value)) category |= Category.ArtistCG;
                    break;
                case "f_gamecg":
                    if(QueryValueAsBoolean(item.Value)) category |= Category.GameCG;
                    break;
                case "f_western":
                    if(QueryValueAsBoolean(item.Value)) category |= Category.Western;
                    break;
                case "f_non-h":
                    if(QueryValueAsBoolean(item.Value)) category |= Category.NonH;
                    break;
                case "f_imageset":
                    if(QueryValueAsBoolean(item.Value)) category |= Category.ImageSet;
                    break;
                case "f_cosplay":
                    if(QueryValueAsBoolean(item.Value)) category |= Category.Cosplay;
                    break;
                case "f_asianporn":
                    if(QueryValueAsBoolean(item.Value)) category |= Category.AsianPorn;
                    break;
                case "f_misc":
                    if(QueryValueAsBoolean(item.Value)) category |= Category.Misc;
                    break;
                case "f_search":
                    keyword = UnescapeKeyword(item.Value);
                    break;
                case "advsearch":
                    av = QueryValueAsBoolean(item.Value);
                    break;
                case "f_sname":
                    advanced.SearchName = QueryValueAsBoolean(item.Value);
                    break;
                case "f_stags":
                    advanced.SearchTags = QueryValueAsBoolean(item.Value);
                    break;
                case "f_sdesc":
                    advanced.SearchDescription = QueryValueAsBoolean(item.Value);
                    break;
                case "f_storr":
                    advanced.SearchTorrentFilenames = QueryValueAsBoolean(item.Value);
                    break;
                case "f_sto":
                    advanced.GalleriesWithTorrentsOnly = QueryValueAsBoolean(item.Value);
                    break;
                case "f_sdt1":
                    advanced.SearchLowPowerTags = QueryValueAsBoolean(item.Value);
                    break;
                case "f_sdt2":
                    advanced.SearchDownvotedTags = QueryValueAsBoolean(item.Value);
                    break;
                case "f_sh":
                    advanced.ShowExpungedGalleries = QueryValueAsBoolean(item.Value);
                    break;
                case "f_sr":
                    advanced.SearchMinimumRating = QueryValueAsBoolean(item.Value);
                    break;
                case "f_srdd":
                    advanced.MinimumRating = int.Parse(item.Value);
                    break;
                }
            }
            if(!ap)
                return Helpers.AsyncWarpper.Create<LaunchResult>(new SearchLaunchResult(Client.Current.Search("")));
            else if(av)
                return Helpers.AsyncWarpper.Create<LaunchResult>(new SearchLaunchResult(Client.Current.Search(keyword, category, advanced)));
            else
                return Helpers.AsyncWarpper.Create<LaunchResult>(new SearchLaunchResult(Client.Current.Search(keyword, category)));
        }
    }

    internal sealed class SearchUploaderAndTagHandler : UriHandler
    {
        public override bool CanHandle(UriHandlerData data)
        {
            return data.Paths.Count == 2 && (data.Path0 == "tag" || data.Path0 == "uploader");
        }

        public override IAsyncOperation<LaunchResult> HandleAsync(UriHandlerData data)
        {
            var v = UriHandlerData.Unescape2(data.Paths[1]);
            switch(data.Path0)
            {
            case "tag":
                return Helpers.AsyncWarpper.Create((LaunchResult)new SearchLaunchResult(Tag.Parse(v).Search()));
            case "uploader":
                return Helpers.AsyncWarpper.Create<LaunchResult>(new SearchLaunchResult(Client.Current.Search($"uploader:\"{v}\"")));
            }
            throw new NotSupportedException("Unsupported uri.");
        }
    }

    internal sealed class SearchCategoryHandler : UriHandler
    {
        private static Dictionary<string, Category> categoryDic = new Dictionary<string, Category>(StringComparer.OrdinalIgnoreCase)
        {
            ["Doujinshi"] = Category.Doujinshi,
            ["Manga"] = Category.Manga,
            ["ArtistCG"] = Category.ArtistCG,
            ["GameCG"] = Category.GameCG,
            ["Western"] = Category.Western,
            ["Non-H"] = Category.NonH,
            ["ImageSet"] = Category.ImageSet,
            ["Cosplay"] = Category.Cosplay,
            ["AsianPorn"] = Category.AsianPorn,
            ["Misc"] = Category.Misc
        };

        public override bool CanHandle(UriHandlerData data)
        {
            return data.Paths.Count == 1 && (categoryDic.ContainsKey(data.Path0));
        }

        public override IAsyncOperation<LaunchResult> HandleAsync(UriHandlerData data)
        {
            var category = categoryDic[data.Path0];
            return Helpers.AsyncWarpper.Create((LaunchResult)new SearchLaunchResult(Client.Current.Search("", category)));
        }
    }

    internal sealed class FavoritesSearchHandler : SearchHandler
    {
        public override bool CanHandle(UriHandlerData data)
        {
            return data.Paths.Count == 1 && data.Path0 == "favorites.php";
        }

        public override IAsyncOperation<LaunchResult> HandleAsync(UriHandlerData data)
        {
            var keyword = "";
            var category = (FavoriteCategory)null;
            var ap = false;
            foreach(var item in data.Queries)
            {
                switch(item.Key)
                {
                case "f_apply":
                    ap = QueryValueAsBoolean(item.Value);
                    break;
                case "favcat":
                    if(item.Value != "all")
                    {
                        var index = QueryValueAsInt32(item.Value);
                        index = Math.Max(0, index);
                        index = Math.Min(9, index);
                        category = Client.Current.Favorites[index];
                    }
                    break;
                case "f_search":
                    keyword = UnescapeKeyword(item.Value);
                    break;
                }
            }
            if(!ap)
                return Helpers.AsyncWarpper.Create<LaunchResult>(new FavoritesSearchLaunchResult(Client.Current.Favorites.Search("", category)));
            else
                return Helpers.AsyncWarpper.Create<LaunchResult>(new FavoritesSearchLaunchResult(Client.Current.Favorites.Search(keyword, category)));
        }
    }
}
