using Newtonsoft.Json;
using System;

namespace ExClient.Api
{
    [JsonConverter(typeof(GalleryInfoConverter))]
    public struct GalleryInfo : IEquatable<GalleryInfo>
    {
        public GalleryInfo(long id, string token)
        {
            Id = id;
            Token = token;
        }

        public long Id
        {
            get;
        }

        public string Token
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
            return Id.GetHashCode() ^ (Token ?? "").GetHashCode();
        }
    }

    internal class GalleryInfoConverter : JsonConverter
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
            string token = null;
            reader.Read();
            do
            {
                switch(reader.Value.ToString())
                {
                case "gid":
                    gid = reader.ReadAsInt32().GetValueOrDefault();
                    break;
                case "token":
                    token = reader.ReadAsString();
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
            writer.WriteValue(v.Token);
            writer.WriteEndArray();
        }
    }
}