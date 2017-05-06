using Opportunity.MvvmUniverse;
using Opportunity.MvvmUniverse.Helpers;
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
            this.image = null;
            this.Loaded = false;
        }

        public void StartLoading()
        {
            DispatcherHelper.BeginInvokeOnUIThread(() => getImage());
        }

        private WeakReference<BitmapImage> image;

        public BitmapImage Image => getImage();

        private BitmapImage getImage()
        {
            if (this.image != null && this.image.TryGetTarget(out var image))
                return image;
            image = new BitmapImage();
            this.image = new WeakReference<BitmapImage>(image);
            this.imageLoader(new ImageLoaderData(this, image));
            return image;
        }

        public bool Loaded
        {
            get;
            private set;
        }

        public event TypedEventHandler<ImageHandle, EventArgs> ImageLoaded;
        public event TypedEventHandler<ImageHandle, Exception> ImageFailed;

        internal void Finished()
        {
            this.Loaded = true;
            var temp = ImageLoaded;
            if (temp != null)
                DispatcherHelper.BeginInvokeOnUIThread(() => temp.Invoke(this, EventArgs.Empty));
        }
        internal void Failed(Exception error)
        {
            var temp3 = ImageFailed;
            if (temp3 != null)
                DispatcherHelper.BeginInvokeOnUIThread(() => temp3.Invoke(this, error));

        }
    }

    internal delegate void ImageLoader(ImageLoaderData data);

    internal class ImageLoaderData
    {
        public ImageLoaderData(ImageHandle handle, BitmapImage image)
        {
            this.Handle = handle;
            this.Image = image;
        }

        public BitmapImage Image { get; }
        public ImageHandle Handle { get; }

        public void ReportFailed(Exception error)
        {
            this.Handle.Failed(error);
        }

        public void ReportFailed()
        {
            ReportFailed(null);
        }

        public void ReportFinished()
        {
            this.Handle.Finished();
        }
    }
}
