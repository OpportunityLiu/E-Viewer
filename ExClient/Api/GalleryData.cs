using ExClient.Internal;
using Opportunity.MvvmUniverse.Collections;
using System.Collections.Generic;

#pragma warning disable IDE1006 // 命名样式
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
            this.gidlist = new RangedCollectionView<GalleryInfo>(list, startIndex, count);
        }
    }
}