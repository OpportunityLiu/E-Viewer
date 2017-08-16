using Newtonsoft.Json;
using System.Collections.Generic;

#pragma warning disable IDE1006 // 命名样式
namespace ExClient.Api
{
    internal sealed class GalleryTokenRequest : ApiRequest, IRequestOf<GalleryTokenRequest, GalleryTokenResponse>
    {
        public override string Method => "gtoken";

        [JsonProperty("pagelist")]
        public IEnumerable<ImageInfo> PageList { get; }

        public GalleryTokenRequest(IEnumerable<ImageInfo> pageList)
        {
            this.PageList = pageList;
        }
    }

    internal class GalleryTokenResponse : ApiResponse, IResponseOf<GalleryTokenRequest, GalleryTokenResponse>
    {
        [JsonProperty("tokenlist")]
        public List<GalleryInfo> TokenList { get; set; }
    }
}