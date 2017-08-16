using ExClient.Galleries;
using ExClient.Internal;
using ExClient.Launch;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Windows.Foundation;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;

namespace ExClient.Api
{
    [JsonConverter(typeof(GalleryInfoConverter))]
    public struct GalleryInfo : IEquatable<GalleryInfo>
    {
        private class GalleryInfoConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return typeof(GalleryInfo) == objectType;
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                if (reader.TokenType != JsonToken.StartObject)
                    return null;
                long gid = 0;
                ulong token = 0;
                reader.Read();
                do
                {
                    switch (reader.Value.ToString())
                    {
                    case "gid":
                        gid = reader.ReadAsInt32().GetValueOrDefault();
                        break;
                    case "token":
                        token = reader.ReadAsString().ToToken();
                        break;
                    default:
                        break;
                    }
                    reader.Read();
                } while (reader.TokenType != JsonToken.EndObject);

                return new GalleryInfo(gid, token);
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                var v = (GalleryInfo)value;
                writer.WriteStartArray();
                writer.WriteValue(v.ID);
                writer.WriteValue(v.Token.ToTokenString());
                writer.WriteEndArray();
            }
        }

        public static IAsyncOperation<IReadOnlyList<GalleryInfo>> FetchGalleryInfoListAsync(IEnumerable<ImageInfo> pageList)
        {
            return Run<IReadOnlyList<GalleryInfo>>(async token =>
            {
                var res = await new GalleryTokenRequest(pageList).GetResponseAsync();
                return res.TokenList;
            });
        }

        internal static bool TryParseGallery(UriHandlerData data, out GalleryInfo info)
        {
            if (data.Path0 == "g" && data.Paths.Count == 3)
            {
                if (long.TryParse(data.Paths[1], out var gId))
                {
                    info = new GalleryInfo(gId, data.Paths[2].ToToken());
                    return true;
                }
            }
            info = default(GalleryInfo);
            return false;
        }

        internal static bool TryParseGalleryTorrent(UriHandlerData data, out GalleryInfo info)
        {
            if (data.Path0 == "gallerytorrents.php" && data.Paths.Count == 1)
            {
                if (data.Queries.TryGetValue("gid", out var gidStr)
                    && data.Queries.TryGetValue("t", out var gtoken)
                    && long.TryParse(data.Queries["gid"], out var gId))
                {
                    info = new GalleryInfo(gId, gtoken.ToToken());
                    return true;
                }
            }
            info = default(GalleryInfo);
            return false;
        }

        public static bool TryParse(Uri uri, out GalleryInfo info)
        {
            var data = new UriHandlerData(uri);
            if (TryParseGallery(data, out info))
                return true;
            if (TryParseGalleryTorrent(data, out info))
                return true;

            info = default(GalleryInfo);
            return false;
        }

        public static GalleryInfo Parse(Uri uri)
        {
            if (TryParse(uri, out var r))
                return r;
            throw new FormatException();
        }

        public GalleryInfo(long id, ulong token)
        {
            this.ID = id;
            this.Token = token;
        }

        public IAsyncOperation<Gallery> FetchGalleryAsync()
        {
            var galleryInfo = new[] { this };
            return Run(async token =>
            {
                var d = await Gallery.FetchGalleriesAsync(galleryInfo);
                return d[0];
            });
        }

        public long ID
        {
            get;
        }

        public ulong Token
        {
            get;
        }

        public bool Equals(GalleryInfo other)
        {
            return this.ID == other.ID && this.Token == other.Token;
        }

        public override bool Equals(object obj)
        {
            if (obj is GalleryInfo info)
                return Equals(info);
            return false;
        }

        public override int GetHashCode()
        {
            return this.ID.GetHashCode() ^ this.Token.GetHashCode();
        }
    }
}