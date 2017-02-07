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
            if(paths.Length != 3)
                return false;
            if(paths[0] == "s" || paths[0] == "g")
                return true;
            return false;
        }

        public static IAsyncOperation<Tuple<Gallery, int>> HandleAsync(Uri uri)
        {
            var paths = uri.AbsolutePath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            switch(paths[0])
            {
            case "g":
                return Run(async token =>
                {
                    var gInfo = new GalleryInfo(long.Parse(paths[1]), paths[2]);
                    var g = (await Gallery.FetchGalleriesAsync(new[] { gInfo })).Single();
                    return Tuple.Create(g, -1);
                });
            case "s":
                var imgKey = paths[1];
                var sp = paths[2].Split('-');
                if(sp.Length != 2)
                    break;
                var gId = long.Parse(sp[0]);
                var pId = int.Parse(sp[1]);
                return Run(async token =>
                {
                    var iInfo = new ImageInfo(gId, imgKey, pId);
                    var gInfo = await getGalleryInfoAsync(new[] { iInfo });
                    var g = (await Gallery.FetchGalleriesAsync(gInfo)).Single();
                    return Tuple.Create(g, pId);
                });
            }
            throw new NotSupportedException("Unsupported uri.");
        }

        [JsonConverter(typeof(ImageInfoConverter))]
        internal struct ImageInfo : IEquatable<ImageInfo>
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

        internal class ImageInfoConverter : JsonConverter
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

        private static IAsyncOperation<IList<GalleryInfo>> getGalleryInfoAsync(IEnumerable<ImageInfo> pageList)
        {
            return Run(async token =>
            {
                var json = JsonConvert.SerializeObject(new
                {
                    method = "gtoken",
                    pagelist = pageList
                });
                var result = await Client.Current.PostApiAsync(json);
                var type = new
                {
                    tokenlist = (IList<GalleryInfo>)null
                };
                return JsonConvert.DeserializeAnonymousType(result, type).tokenlist;
            });
        }
    }
}
