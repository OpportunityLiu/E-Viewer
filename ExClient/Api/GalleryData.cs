using ExClient.Internal;
using System.Collections;
using System.Collections.Generic;

namespace ExClient.Api
{
    internal class GalleryData : ApiRequest
    {
        public override string Method => "gdata";

        public int @namespace => 1;

        public IReadOnlyCollection<GalleryInfo> gidlist
        {
            get;
        }

        public GalleryData(IReadOnlyList<GalleryInfo> list, int startIndex, int count)
        {
            gidlist = new RangedCollection<GalleryInfo>(list, startIndex, count);
        }
    }
}