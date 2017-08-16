using System;
using Windows.Foundation;
using Windows.UI.Xaml.Media;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;

namespace ExClient.Galleries
{
    public sealed class GalleryImagePlaceHolder : GalleryImage
    {
        public GalleryImagePlaceHolder(CachedGallery owner, int pageID)
            : base(owner, pageID, 0, null)
        {
        }

        public override IAsyncAction LoadImageAsync(bool reload, ConnectionStrategy strategy, bool throwIfFailed)
        {
            return Run(async token =>
            {
                await ((CachedGallery)Owner).LoadImageAsync(this);
            });
        }

        public override ImageSource Thumb => DefaultThumb;
    }
}
