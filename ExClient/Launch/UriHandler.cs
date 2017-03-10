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
            this.Paths = uri.AbsolutePath.Split(split0, StringSplitOptions.RemoveEmptyEntries);
            if(this.Paths.Count != 0)
                this.Path0 = this.Paths[0].ToLowerInvariant();
            else
                this.Path0 = "";
            this.queriesLoader = new Lazy<IReadOnlyDictionary<string, string>>(this.getQueries);
        }

        public Uri Uri { get; }
        public IReadOnlyList<string> Paths { get; }
        public string Path0 { get; }

        private Lazy<IReadOnlyDictionary<string, string>> queriesLoader;
        public IReadOnlyDictionary<string, string> Queries => this.queriesLoader.Value;

        private static readonly char[] split0 = "/".ToCharArray();
        private static readonly char[] split1 = "&".ToCharArray();
        private static readonly char[] split2 = "=".ToCharArray();
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
                   .ToDictionary(i => i[0], i => i[1].Unescape()));
        }
    }

    internal static class UriHelper
    {
        public static string Unescape(this string value)
        {
            value = value.Replace('+', ' ');
            value = Uri.UnescapeDataString(value);
            return value;
        }

        /// <summary>
        /// Unescape twice for special usage.
        /// </summary>
        /// <param name="value">string to unescape</param>
        /// <returns>unescaped string</returns>
        public static string Unescape2(this string value)
        {
            value = Uri.UnescapeDataString(value);
            return Unescape(value);
        }
        public static bool QueryValueAsBoolean(this string value)
        {
            return value != "0" && value != "";
        }

        public static int QueryValueAsInt32(this string value)
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
    }

    internal abstract class UriHandler
    {

        public abstract bool CanHandle(UriHandlerData data);
        public abstract IAsyncOperation<LaunchResult> HandleAsync(UriHandlerData data);
    }

    internal class GalleryHandler : UriHandler
    {

        public override bool CanHandle(UriHandlerData data)
        {
            return GalleryInfo.TryParseGallery(data, out var info);
        }

        public override IAsyncOperation<LaunchResult> HandleAsync(UriHandlerData data)
        {
            GalleryInfo.TryParseGallery(data, out var info);
            return Run(async token =>
            {
                var g = await info.FetchGalleryAsync();
                return (LaunchResult)new GalleryLaunchResult(g, -1, GalleryLaunchStatus.Default);
            });
        }
    }

    internal sealed class GalleryTorrentHandler : GalleryHandler
    {
        public override bool CanHandle(UriHandlerData data)
        {
            return GalleryInfo.TryParseGalleryTorrent(data, out var info);
        }

        public override IAsyncOperation<LaunchResult> HandleAsync(UriHandlerData data)
        {
            GalleryInfo.TryParseGalleryTorrent(data, out var info);
            return Run(async token =>
            {
                var g = await info.FetchGalleryAsync();
                return (LaunchResult)new GalleryLaunchResult(g, -1, GalleryLaunchStatus.Torrent);
            });
        }
    }

    internal sealed class GalleryImageHandler : GalleryHandler
    {
        public override bool CanHandle(UriHandlerData data)
        {
            return ImageInfo.TryParse(data, out var info);
        }

        public override IAsyncOperation<LaunchResult> HandleAsync(UriHandlerData data)
        {
            ImageInfo.TryParse(data, out var info);
            return Run(async token =>
            {
                var gInfo = await info.FetchGalleryInfoAsync();
                var g = await gInfo.FetchGalleryAsync();
                return (LaunchResult)new GalleryLaunchResult(g, info.PageId, GalleryLaunchStatus.Image);
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
                var b = item.Value.QueryValueAsBoolean();
                switch(item.Key)
                {
                case "f_apply":
                    ap = b;
                    break;
                case "f_doujinshi":
                    if(b) category |= Category.Doujinshi;
                    break;
                case "f_manga":
                    if(b) category |= Category.Manga;
                    break;
                case "f_artistcg":
                    if(b) category |= Category.ArtistCG;
                    break;
                case "f_gamecg":
                    if(b) category |= Category.GameCG;
                    break;
                case "f_western":
                    if(b) category |= Category.Western;
                    break;
                case "f_non-h":
                    if(b) category |= Category.NonH;
                    break;
                case "f_imageset":
                    if(b) category |= Category.ImageSet;
                    break;
                case "f_cosplay":
                    if(b) category |= Category.Cosplay;
                    break;
                case "f_asianporn":
                    if(b) category |= Category.AsianPorn;
                    break;
                case "f_misc":
                    if(b) category |= Category.Misc;
                    break;
                case "f_search":
                    keyword = UnescapeKeyword(item.Value);
                    break;
                case "advsearch":
                    av = b;
                    break;
                case "f_sname":
                    advanced.SearchName = b;
                    break;
                case "f_stags":
                    advanced.SearchTags = b;
                    break;
                case "f_sdesc":
                    advanced.SearchDescription = b;
                    break;
                case "f_storr":
                    advanced.SearchTorrentFilenames = b;
                    break;
                case "f_sto":
                    advanced.GalleriesWithTorrentsOnly = b;
                    break;
                case "f_sdt1":
                    advanced.SearchLowPowerTags = b;
                    break;
                case "f_sdt2":
                    advanced.SearchDownvotedTags = b;
                    break;
                case "f_sh":
                    advanced.ShowExpungedGalleries = b;
                    break;
                case "f_sr":
                    advanced.SearchMinimumRating = b;
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
            var v = data.Paths[1].Unescape2();
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
                    ap = item.Value.QueryValueAsBoolean();
                    break;
                case "favcat":
                    if(item.Value != "all")
                    {
                        var index = item.Value.QueryValueAsInt32();
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
