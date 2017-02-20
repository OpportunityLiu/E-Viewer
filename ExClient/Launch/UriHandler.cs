using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;

namespace ExClient.Launch
{
    internal sealed class UriHandlerData
    {
        public UriHandlerData(Uri uri)
        {
            Uri = uri;
            Paths = uri.AbsolutePath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if(Paths.Count != 0)
                Path0 = Paths[0].ToLowerInvariant();
            else
                Path0 = "";
            queriesLoader = new Lazy<IReadOnlyDictionary<string, string>>(getQueries);
        }

        public Uri Uri { get; }
        public IReadOnlyList<string> Paths { get; }
        public string Path0 { get; }

        private Lazy<IReadOnlyDictionary<string, string>> queriesLoader;
        public IReadOnlyDictionary<string, string> Queries => queriesLoader.Value;

        private static char[] split1 = "&".ToCharArray();
        private static char[] split2 = "=".ToCharArray();
        private static IReadOnlyDictionary<string, string> empty = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());

        private IReadOnlyDictionary<string, string> getQueries()
        {
            var query = Uri.Query;
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
            value = Uri.UnescapeDataString(value);
            value = value.Replace('+', ' ');
            value = Uri.UnescapeDataString(value);
            return value;
        }
    }

    internal abstract class UriHandler
    {
        protected static bool QueryValueAsBoolean(string value)
        {
            return value != "0" && value != "";
        }

        public abstract bool CanHandle(UriHandlerData data);
        public abstract IAsyncOperation<LaunchResult> HandleAsync(UriHandlerData data);
    }

    internal class GalleryHandler : UriHandler
    {
        [JsonConverter(typeof(ImageInfoConverter))]
        protected struct ImageInfo : IEquatable<ImageInfo>
        {
            public ImageInfo(long galleryId, string imageToken, int pageId)
            {
                GalleryId = galleryId;
                ImageToken = imageToken;
                PageId = pageId;
            }

            public long GalleryId { get; }
            public int PageId { get; }
            public string ImageToken { get; }

            public bool Equals(ImageInfo other)
            {
                return this.GalleryId == other.GalleryId && this.ImageToken == other.ImageToken && this.PageId == other.PageId;
            }

            public override bool Equals(object obj)
            {
                if(obj == null || typeof(GalleryInfo) != obj.GetType())
                {
                    return false;
                }
                return Equals((ImageInfo)obj);
            }

            public override int GetHashCode()
            {
                return GalleryId.GetHashCode() ^ (ImageToken ?? "").GetHashCode() ^ PageId.GetHashCode();
            }
        }

        private class ImageInfoConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return typeof(ImageInfo) == objectType;
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                var v = (ImageInfo)value;
                writer.WriteStartArray();
                writer.WriteValue(v.GalleryId);
                writer.WriteValue(v.ImageToken);
                writer.WriteValue(v.PageId);
                writer.WriteEndArray();
            }
        }

        private sealed class GalleryToken : Internal.ApiRequest
        {
            public override string Method => "gtoken";

            public IEnumerable<ImageInfo> pagelist
            {
                get;
            }

            public GalleryToken(IEnumerable<ImageInfo> pageList)
            {
                pagelist = pageList;
            }
        }

        protected static IAsyncOperation<IList<GalleryInfo>> GetGalleryInfoAsync(IEnumerable<ImageInfo> pageList)
        {
            return Run(async token =>
            {
                var result = await Client.Current.PostApiAsync(new GalleryToken(pageList));
                var type = new
                {
                    tokenlist = (IList<GalleryInfo>)null
                };
                return JsonConvert.DeserializeAnonymousType(result, type).tokenlist;
            });
        }

        public override bool CanHandle(UriHandlerData data)
        {
            if(data.Path0 == "g" && data.Paths.Count == 3)
            {
                long gId;
                if(!long.TryParse(data.Paths[1], out gId))
                    return false;
                return true;
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
                long gId;
                if(!data.Queries.ContainsKey("gid"))
                    return false;
                if(!data.Queries.ContainsKey("t"))
                    return false;
                if(!long.TryParse(data.Queries["gid"], out gId))
                    return false;
                return true;
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
                if(sp.Length != 2)
                    return false;
                long gId;
                int pId;
                if(!long.TryParse(sp[0], out gId))
                    return false;
                if(!int.TryParse(sp[1], out pId))
                    return false;
                return true;
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
                    keyword = item.Value;
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

    internal sealed class SearchUploaderAndTagHandler : SearchHandler
    {
        public override bool CanHandle(UriHandlerData data)
        {
            return data.Paths.Count == 2 && (data.Path0 == "tag" || data.Path0 == "uploader");
        }

        public override IAsyncOperation<LaunchResult> HandleAsync(UriHandlerData data)
        {
            var v = UriHandlerData.Unescape(data.Paths[1]);
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

    internal sealed class SearchCategoryHandler : SearchHandler
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
}
