using GalaSoft.MvvmLight.Threading;
using System;
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
            Loaded = false;
        }

        public void StartLoading()
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() => getImage());
        }

        private WeakReference<BitmapImage> image;

        public BitmapImage Image => getImage();

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
                switch(e)
                {
                case AsyncStatus.Completed:
                    Loaded = true;
                    var temp = ImageLoaded;
                    if(temp != null)
                        DispatcherHelper.CheckBeginInvokeOnUI(() => temp.Invoke(this, EventArgs.Empty));
                    break;
                case AsyncStatus.Canceled:
                    var temp2 = ImageFailed;
                    if(temp2 != null)
                        DispatcherHelper.CheckBeginInvokeOnUI(() => temp2.Invoke(this, new TaskCanceledException()));
                    break;
                case AsyncStatus.Error:
                    var temp3 = ImageFailed;
                    if(temp3 != null)
                        DispatcherHelper.CheckBeginInvokeOnUI(() => temp3.Invoke(this, sender.ErrorCode));
                    break;
                }
            };
            return image;
        }

        public bool Loaded
        {
            get;
            private set;
        }

        public event TypedEventHandler<ImageHandle,EventArgs> ImageLoaded;
        public event TypedEventHandler<ImageHandle,Exception> ImageFailed;
    }

    internal delegate IAsyncAction ImageLoader(BitmapImage image);
}
