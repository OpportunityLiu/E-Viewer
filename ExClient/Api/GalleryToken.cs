using System.Collections.Generic;

#pragma warning disable IDE1006 // 命名样式
namespace ExClient.Api
{
    internal sealed class GalleryToken : ApiRequest
    {
        public override string Method => "gtoken";

        public IEnumerable<ImageInfo> pagelist
        {
            get;
        }

        public GalleryToken(IEnumerable<ImageInfo> pageList)
        {
            this.pagelist = pageList;
        }
    }
}