using Newtonsoft.Json;
using System;

namespace ExClient.Api
{
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
            return this.GalleryId == other.GalleryId
                && this.ImageToken == other.ImageToken
                && this.PageId == other.PageId;
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
}