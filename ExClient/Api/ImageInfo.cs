using ExClient.Internal;
using ExClient.Launch;
using Newtonsoft.Json;
using System;
using Windows.Foundation;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;

namespace ExClient.Api
{
    [JsonConverter(typeof(ImageInfoConverter))]
    public struct ImageInfo : IEquatable<ImageInfo>
    {
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
                writer.WriteValue(v.GalleryID);
                writer.WriteValue(v.ImageToken.ToTokenString());
                writer.WriteValue(v.PageID);
                writer.WriteEndArray();
            }
        }

        internal static bool TryParse(UriHandlerData data, out ImageInfo info)
        {
            if (data.Path0 == "s" && data.Paths.Count == 3)
            {
                var sp = data.Paths[2].Split('-');
                if ((sp.Length == 2)
                    && long.TryParse(sp[0], out var gID)
                    && int.TryParse(sp[1], out var pID))
                {
                    info = new ImageInfo(gID, data.Paths[1].Substring(0, 10).ToToken(), pID);
                    return true;
                }
            }
            info = default(ImageInfo);
            return false;
        }

        public static bool TryParse(Uri uri, out ImageInfo info)
        {
            var data = new UriHandlerData(uri);
            if (TryParse(data, out info))
                return true;
            info = default(ImageInfo);
            return false;
        }

        public static ImageInfo Parse(Uri uri)
        {
            if (TryParse(uri, out var r))
                return r;
            throw new FormatException();
        }

        public ImageInfo(long galleryID, ulong imageToken, int pageID)
        {
            this.GalleryID = galleryID;
            this.ImageToken = imageToken;
            this.PageID = pageID;
        }

        public IAsyncOperation<GalleryInfo> FetchGalleryInfoAsync()
        {
            var info = new[] { this };
            return Run(async token =>
            {
                var result = await GalleryInfo.FetchGalleryInfoListAsync(info);
                return result[0];
            });
        }

        public long GalleryID { get; }
        public int PageID { get; }
        public ulong ImageToken { get; }

        public bool Equals(ImageInfo other)
        {
            return this.GalleryID == other.GalleryID
                && this.ImageToken == other.ImageToken
                && this.PageID == other.PageID;
        }

        public override bool Equals(object obj)
        {
            if (obj is ImageInfo info)
                return Equals(info);
            return false;
        }

        public override int GetHashCode()
        {
            return this.GalleryID.GetHashCode() ^ this.ImageToken.GetHashCode() ^ this.PageID.GetHashCode();
        }
    }
}