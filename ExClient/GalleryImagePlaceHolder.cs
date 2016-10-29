using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Media;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;

namespace ExClient
{
    public sealed class GalleryImagePlaceHolder : GalleryImage
    {
        public GalleryImagePlaceHolder(CachedGallery owner, int pageId)
            : base(owner, pageId, null, null)
        {
        }

        internal void Init(string imageKey, ImageSource thumb)
        {
            this.ImageKey = imageKey;
            this.thumb = thumb;
            RaisePropertyChanged(nameof(Thumb));
        }

        private ImageSource thumb;

        public override ImageSource Thumb
        {
            get
            {
                if(thumb != null && State != ImageLoadingState.Loaded)
                    return thumb;
                return base.Thumb;
            }
        }

        public override IAsyncAction LoadImageAsync(bool reload, ConnectionStrategy strategy, bool throwIfFailed)
        {
            if(thumb == null)
                return Run(async token =>
                {
                    await ((CachedGallery)Owner).LoadImageAsync(this);
                    await base.LoadImageAsync(reload, strategy, throwIfFailed);
                });
            return base.LoadImageAsync(reload, strategy, throwIfFailed);
        }
    }
}
