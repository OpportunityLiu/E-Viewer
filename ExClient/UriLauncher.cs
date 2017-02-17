using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;

namespace ExClient
{
    public static class UriLauncher
    {
        public static bool CanHandle(Uri uri)
        {
            if(uri == null)
                return false;
            if(uri.Host != Client.RootUri.Host && uri.Host != Client.EhUri.Host)
                return false;
            var paths = uri.AbsolutePath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            switch(paths.Length)
            {
            case 0:
                return true;
            case 1:
                switch(paths[0])
                {
                case "gallerytorrents.php":
                    return true;
                }
                break;
            case 2:
                switch(paths[0])
                {
                case "tag":
                case "uploader":
                    return true;
                }
                break;
            case 3:
                switch(paths[0])
                {
                case "g":
                case "s":
                    return true;
                }
                break;
            }
            return false;
        }

        public static IAsyncOperation<LanunchResult> HandleAsync(Uri uri)
        {
            var paths = uri.AbsolutePath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            switch(paths.Length)
            {
            case 0:
                return handleSearchAsync(uri);
            case 1:
                switch(paths[0])
                {
                case "gallerytorrents.php":
                    return handleGalleryAsync(uri);
                }
                break;
            case 2:
                switch(paths[0])
                {
                case "tag":
                case "uploader":
                    return handleSearchAsync(paths);
                }
                break;
            case 3:
                switch(paths[0])
                {
                case "g":
                    return handleGalleryAsync(paths);
                case "s":
                    return handleGalleryImageAsync(paths);
                }
                break;
            }
            throw new NotSupportedException("Unsupported uri.");
        }

        private static bool check(string va)
        {
            return va != "0" && va != "";
        }

        private static IEnumerable<KeyValuePair<string, string>> getQueries(Uri uri)
        {
            var query = uri.Query;
            if(string.IsNullOrWhiteSpace(query) || query.Length <= 1 || query[0] != '?')
                yield break;
            query = query.Substring(1);
            var divided = query.Split("&".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach(var item in divided)
            {
                var kv = item.Split("=".ToCharArray(), 2, StringSplitOptions.None);
                var v = Uri.UnescapeDataString(kv[1]);
                v = v.Replace('+', ' ');
                v = Uri.UnescapeDataString(v);
                yield return new KeyValuePair<string, string>(kv[0], v);
            }
        }

        private static IAsyncOperation<LanunchResult> handleSearchAsync(string[] paths)
        {
            var v = Uri.UnescapeDataString(paths[1]);
            v = v.Replace('+', ' ');
            v = Uri.UnescapeDataString(v);
            switch(paths[0])
            {
            case "tag":
                return Helpers.AsyncWarpper.Create((LanunchResult)new SearchLaunchResult(new Tag(null, v).Search()));
            case "uploader":
                return Helpers.AsyncWarpper.Create<LanunchResult>(new SearchLaunchResult(Client.Current.Search($"uploader:\"{v}\"")));
            }
            throw new NotSupportedException("Unsupported uri.");
        }

        private static IAsyncOperation<LanunchResult> handleSearchAsync(Uri uri)
        {
            var query = uri.Query;
            var keyword = "";
            var category = Category.Unspecified;
            var advanced = new AdvancedSearchOptions();
            var ap = false;
            var av = false;
            foreach(var item in getQueries(uri))
            {
                switch(item.Key)
                {
                case "f_apply":
                    ap = check(item.Value);
                    break;
                case "f_doujinshi":
                    if(check(item.Value)) category |= Category.Doujinshi;
                    break;
                case "f_manga":
                    if(check(item.Value)) category |= Category.Manga;
                    break;
                case "f_artistcg":
                    if(check(item.Value)) category |= Category.ArtistCG;
                    break;
                case "f_gamecg":
                    if(check(item.Value)) category |= Category.GameCG;
                    break;
                case "f_western":
                    if(check(item.Value)) category |= Category.Western;
                    break;
                case "f_non-h":
                    if(check(item.Value)) category |= Category.NonH;
                    break;
                case "f_imageset":
                    if(check(item.Value)) category |= Category.ImageSet;
                    break;
                case "f_cosplay":
                    if(check(item.Value)) category |= Category.Cosplay;
                    break;
                case "f_asianporn":
                    if(check(item.Value)) category |= Category.AsianPorn;
                    break;
                case "f_misc":
                    if(check(item.Value)) category |= Category.Misc;
                    break;
                case "f_search":
                    keyword = Uri.UnescapeDataString(item.Value);
                    break;
                case "advsearch":
                    av = check(item.Value);
                    break;
                case "f_sname":
                    advanced.SearchName = check(item.Value);
                    break;
                case "f_stags":
                    advanced.SearchTags = check(item.Value);
                    break;
                case "f_sdesc":
                    advanced.SearchDescription = check(item.Value);
                    break;
                case "f_storr":
                    advanced.SearchTorrentFilenames = check(item.Value);
                    break;
                case "f_sto":
                    advanced.GalleriesWithTorrentsOnly = check(item.Value);
                    break;
                case "f_sdt1":
                    advanced.SearchLowPowerTags = check(item.Value);
                    break;
                case "f_sdt2":
                    advanced.SearchDownvotedTags = check(item.Value);
                    break;
                case "f_sh":
                    advanced.ShowExpungedGalleries = check(item.Value);
                    break;
                case "f_sr":
                    advanced.SearchMinimumRating = check(item.Value);
                    break;
                case "f_srdd":
                    advanced.MinimumRating = int.Parse(item.Value);
                    break;
                }
            }
            if(!ap)
                return Helpers.AsyncWarpper.Create<LanunchResult>(new SearchLaunchResult(Client.Current.Search("")));
            else if(av)
                return Helpers.AsyncWarpper.Create<LanunchResult>(new SearchLaunchResult(Client.Current.Search(keyword, category, advanced)));
            else
                return Helpers.AsyncWarpper.Create<LanunchResult>(new SearchLaunchResult(Client.Current.Search(keyword, category)));
        }

        private static IAsyncOperation<LanunchResult> handleGalleryAsync(Uri uri)
        {
            return Run(async token =>
            {
                var q = getQueries(uri).ToDictionary(kv => kv.Key, kv => kv.Value);
                var gInfo = new GalleryInfo(long.Parse(q["gid"]), q["t"]);
                var g = (await Gallery.FetchGalleriesAsync(new[] { gInfo })).Single();
                return (LanunchResult)new GalleryLaunchResult(g, -1, GalleryLaunchStatus.Torrent);
            });
        }

        private static IAsyncOperation<LanunchResult> handleGalleryAsync(string[] paths)
        {
            return Run(async token =>
            {
                var gInfo = new GalleryInfo(long.Parse(paths[1]), paths[2]);
                var g = (await Gallery.FetchGalleriesAsync(new[] { gInfo })).Single();
                return (LanunchResult)new GalleryLaunchResult(g, -1, GalleryLaunchStatus.Default);
            });
        }

        private static IAsyncOperation<LanunchResult> handleGalleryImageAsync(string[] paths)
        {
            var imgKey = paths[1];
            var sp = paths[2].Split('-');
            if(sp.Length != 2)
                throw new NotSupportedException("Unsupported uri.");
            var gId = long.Parse(sp[0]);
            var pId = int.Parse(sp[1]);
            return Run(async token =>
            {
                var iInfo = new ImageInfo(gId, imgKey, pId);
                var gInfo = await getGalleryInfoAsync(new[] { iInfo });
                var g = (await Gallery.FetchGalleriesAsync(gInfo)).Single();
                return (LanunchResult)new GalleryLaunchResult(g, pId, GalleryLaunchStatus.Image);
            });
        }

        [JsonConverter(typeof(ImageInfoConverter))]
        private struct ImageInfo : IEquatable<ImageInfo>
        {
            public ImageInfo(long galleryId, string imageToken, int pageId)
            {
                GalleryId = galleryId;
                ImageToken = imageToken;
                PageId = pageId;
            }

            public long GalleryId
            {
                get;
            }

            public int PageId
            {
                get;
            }

            public string ImageToken
            {
                get;
            }

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

        private class GalleryToken : Internal.ApiRequest
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

        private static IAsyncOperation<IList<GalleryInfo>> getGalleryInfoAsync(IEnumerable<ImageInfo> pageList)
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
    }
}
