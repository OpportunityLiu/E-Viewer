using Newtonsoft.Json;

using System;

namespace ExClient.Api
{
    internal class ImageDataRequest : ApiRequest<ImageDataResponse>
    {
        public override string Method => "showpage";

        [JsonProperty("gid")]
        public long GalleryId { get; }
        [JsonProperty("page")]
        public int PageId { get; }
        [JsonProperty("imgkey")]
        public string ImageKey { get; }
        [JsonProperty("showkey")]
        public string ShowKey { get; }

        public ImageDataRequest(string showKey, ImageInfo imageInfo)
        {
            ShowKey = showKey;
            GalleryId = imageInfo.GalleryId;
            PageId = imageInfo.PageId;
            ImageKey = imageInfo.ImageKey.ToString();
        }
    }

    internal sealed class ImageDataResponse : ApiResponse
    {
        protected override void CheckResponseOverride(ApiRequest request)
        {
            if (Error == "Key mismatch")
                throw new ArgumentException("Key mismatch");
        }

#pragma warning disable IDE1006 // 命名样式
        [JsonProperty] internal int p { get; set; }
        [JsonProperty] internal string s { get; set; }
        [JsonProperty] internal string n { get; set; }
        [JsonProperty] internal string i { get; set; }
        [JsonProperty] internal string k { get; set; }
        [JsonProperty] internal string i3 { get; set; }
        [JsonProperty] internal string i5 { get; set; }
        [JsonProperty] internal string i6 { get; set; }
        [JsonProperty] internal int si { get; set; }
        [JsonProperty] internal string x { get; set; }
        [JsonProperty] internal string y { get; set; }
#pragma warning restore IDE1006 // 命名样式
    }
}
