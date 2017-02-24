using System.Collections.Generic;

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
            pagelist = pageList;
        }
    }
}