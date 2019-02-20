using Newtonsoft.Json;
using System.Collections.Generic;

#pragma warning disable IDE1006 // 命名样式
namespace ExClient.Api
{
    internal sealed class GalleryTokenRequest : ApiRequest<GalleryTokenResponse>
    {
        public override string Method => "gtoken";

        [JsonProperty("pagelist")]
        public IEnumerable<ImageInfo> PageList { get; }

        public GalleryTokenRequest(IEnumerable<ImageInfo> pageList)
        {
            PageList = pageList;
        }
    }

    internal class GalleryTokenResponse : ApiResponse
    {
        [JsonProperty("tokenlist")]
        public List<GalleryInfo> TokenList { get; set; }
    }
}