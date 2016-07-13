using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Media.Imaging;

namespace ExClient.Internal
{
    internal class ImageHandle
    {
        private ImageLoader imageLoader;

        public ImageHandle(ImageLoader imageLoader)
        {
            this.imageLoader = imageLoader;
        }

        public void Reset()
        {
            image = null;
        }

        private WeakReference<BitmapImage> image;

        public BitmapImage Image
        {
            get
            {
                BitmapImage image;
                if(this.image != null && this.image.TryGetTarget(out image))
                    return image;
                image = new BitmapImage();
                this.image = new WeakReference<BitmapImage>(image);
                var loadImage = imageLoader(image);
                return image;
            }
        }
    }

    internal delegate IAsyncAction ImageLoader(BitmapImage image);
}
