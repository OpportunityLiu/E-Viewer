using ExClient.Galleries;
using Newtonsoft.Json;
using Opportunity.MvvmUniverse.Collections;
using System.Collections.Generic;

#pragma warning disable IDE1006 // 命名样式
namespace ExClient.Api
{
    internal class GalleryDataRequest : ApiRequest, IRequestOf<GalleryDataRequest, GalleryDataResponse>
    {
        public override string Method => "gdata";

        [JsonProperty("namespace")]
        public int Namespace => 1;

        [JsonProperty("gidlist")]
        public IReadOnlyCollection<GalleryInfo> GalleryIDList { get; }

        public GalleryDataRequest(IReadOnlyList<GalleryInfo> list, int startIndex, int count)
        {
            this.GalleryIDList = new RangedListView<GalleryInfo>(list, startIndex, count);
        }
    }

    internal class GalleryDataResponse : ApiResponse, IResponseOf<GalleryDataRequest, GalleryDataResponse>
    {
        [JsonProperty("gmetadata")]
        public List<Gallery> GalleryMetaData { get; private set; }
    }
}