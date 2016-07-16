using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Threading;
using Windows.Foundation;
using Windows.UI.Xaml.Media.Imaging;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;

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
            Loaded = false;
        }

        public void StartLoading()
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() => getImage());
        }

        private WeakReference<BitmapImage> image;

        public BitmapImage Image
        {
            get
            {
                return getImage();
            }
        }

        private BitmapImage getImage()
        {
            BitmapImage image;
            if(this.image != null && this.image.TryGetTarget(out image))
                return image;
            image = new BitmapImage();
            this.image = new WeakReference<BitmapImage>(image);
            var loadImage = imageLoader(image);
            loadImage.Completed = (sender, e) =>
            {
                Loaded = true;
                var temp = ImageLoaded;
                if(temp != null)
                    DispatcherHelper.CheckBeginInvokeOnUI(() => temp.Invoke(this, EventArgs.Empty));
            };
            return image;
        }

        public bool Loaded
        {
            get;
            private set;
        }

        public event EventHandler ImageLoaded;
    }

    internal delegate IAsyncAction ImageLoader(BitmapImage image);
}
