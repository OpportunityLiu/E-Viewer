using Newtonsoft.Json;
using System.Collections.Generic;

#pragma warning disable IDE1006 // 命名样式
namespace ExClient.Api
{
    internal sealed class GalleryTokenRequest : ApiRequest, IRequestOf<GalleryTokenResponse>
    {
        public override string Method => "gtoken";

        public IEnumerable<ImageInfo> pagelist
        {
            get;
        }

        public GalleryTokenRequest(IEnumerable<ImageInfo> pageList)
        {
            this.pagelist = pageList;
        }
    }

    internal class GalleryTokenResponse : ApiResponse, IResponseOf<GalleryTokenRequest>
    {
        [JsonProperty("tokenlist")]
        public List<GalleryInfo> TokenList { get; set; }
    }
}