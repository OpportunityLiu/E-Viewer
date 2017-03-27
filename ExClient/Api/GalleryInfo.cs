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
                if(reader.TokenType != JsonToken.StartObject)
                    return null;
                long gid = 0;
                ulong token = 0;
                reader.Read();
                do
                {
                    switch(reader.Value.ToString())
                    {
                    case "gid":
                        gid = reader.ReadAsInt32().GetValueOrDefault();
                        break;
                    case "token":
                        token = reader.ReadAsString().StringToToken();
                        break;
                    default:
                        break;
                    }
                    reader.Read();
                } while(reader.TokenType != JsonToken.EndObject);

                return new GalleryInfo(gid, token);
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                var v = (GalleryInfo)value;
                writer.WriteStartArray();
                writer.WriteValue(v.Id);
                writer.WriteValue(v.Token.TokenToString());
                writer.WriteEndArray();
            }
        }

        private class GalleryInfoResult : ApiResponse
        {
#pragma warning disable IDE1006
#pragma warning disable CS0649
            public List<GalleryInfo> tokenlist;
#pragma warning restore CS0649
#pragma warning restore IDE1006 
        }

        public static IAsyncOperation<IReadOnlyList<GalleryInfo>> FetchGalleryInfoListAsync(IEnumerable<ImageInfo> pageList)
        {
            return Run<IReadOnlyList<GalleryInfo>>(async token =>
            {
                var result = await Client.Current.HttpClient.PostApiAsync(new GalleryToken(pageList));
                var res = JsonConvert.DeserializeObject<GalleryInfoResult>(result);
                res.CheckResponse();
                return res.tokenlist;
            });
        }

        internal static bool TryParseGallery(UriHandlerData data, out GalleryInfo info)
        {
            if(data.Path0 == "g" && data.Paths.Count == 3)
            {
                if(long.TryParse(data.Paths[1], out var gId))
                {
                    info = new GalleryInfo(gId, data.Paths[2].StringToToken());
                    return true;
                }
            }
            info = default(GalleryInfo);
            return false;
        }

        internal static bool TryParseGalleryTorrent(UriHandlerData data, out GalleryInfo info)
        {
            if(data.Path0 == "gallerytorrents.php" && data.Paths.Count == 1)
            {
                if(data.Queries.TryGetValue("gid", out var gidStr)
                    && data.Queries.TryGetValue("t", out var gtoken)
                    && long.TryParse(data.Queries["gid"], out var gId))
                {
                    info = new GalleryInfo(gId, gtoken.StringToToken());
                    return true;
                }
            }
            info = default(GalleryInfo);
            return false;
        }

        public static bool TryParse(Uri uri, out GalleryInfo info)
        {
            var data = new UriHandlerData(uri);
            if(TryParseGallery(data, out info))
                return true;
            if(TryParseGalleryTorrent(data, out info))
                return true;

            info = default(GalleryInfo);
            return false;
        }

        public static GalleryInfo Parse(Uri uri)
        {
            if(TryParse(uri, out var r))
                return r;
            throw new FormatException();
        }

        public GalleryInfo(long id, ulong token)
        {
            this.Id = id;
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

        public long Id
        {
            get;
        }

        public ulong Token
        {
            get;
        }

        public bool Equals(GalleryInfo other)
        {
            return this.Id == other.Id && this.Token == other.Token;
        }

        public override bool Equals(object obj)
        {
            if(obj == null || typeof(GalleryInfo) != obj.GetType())
            {
                return false;
            }
            return Equals((GalleryInfo)obj);
        }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode() ^ this.Token.GetHashCode();
        }
    }
}