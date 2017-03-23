using ExClient;
using ExViewer.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.ComponentModel;
using Windows.ApplicationModel;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace ExViewer.Controls
{
    public sealed partial class ImagePresenter : UserControl
    {
        public ImagePresenter()
        {
            this.InitializeComponent();
        }

        public GalleryImage Image
        {
            get => (GalleryImage)GetValue(ImageProperty);
            set => SetValue(ImageProperty, value);
        }

        public static readonly DependencyProperty ImageProperty = DependencyProperty.Register("Image", typeof(GalleryImage), typeof(ImagePresenter), new PropertyMetadata(null, ImagePropertyChangedCallback));

        private static async void ImagePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(e.NewValue is GalleryImage image)
                await image.LoadImageAsync(false, SettingCollection.Current.GetStrategy(), false);
        }

        protected override void OnDisconnectVisualChildren()
        {
            ClearValue(ImageProperty);
            this.img_Content.ClearValue(ImageFileProperty);
            this.img_Thumb.ClearValue(Windows.UI.Xaml.Controls.Image.SourceProperty);
            base.OnDisconnectVisualChildren();
        }

        public static IStorageFile GetImageFile(DependencyObject obj)
            => (IStorageFile)obj.GetValue(ImageFileProperty);

        public static void SetImageFile(DependencyObject obj, IStorageFile value)
            => obj.SetValue(ImageFileProperty, value);

        public static readonly DependencyProperty ImageFileProperty =
            DependencyProperty.RegisterAttached("ImageFile", typeof(IStorageFile), typeof(Image), new PropertyMetadata(null, ImageFilePropertyChangedCallback));

        private static async void ImageFilePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = (Image)d;
            if(e.OldValue != null)
                sender.ClearValue(Windows.UI.Xaml.Controls.Image.SourceProperty);
            if(e.NewValue is IStorageFile f)
            {
                var bp = new BitmapImage();
                sender.Source = bp;
                using(var s = await f.OpenAsync(FileAccessMode.Read))
                {
                    await bp.SetSourceAsync(s);
                }
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Descendants<ScrollContentPresenter>().First().Clip = null;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            this.gd_ContentRoot.MaxWidth = availableSize.Width;
            this.gd_ContentRoot.MaxHeight = availableSize.Height;
            return base.MeasureOverride(availableSize);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            this.rgChip.Rect = new Rect(new Point(), finalSize);
            return base.ArrangeOverride(finalSize);
        }

        private void sv_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var dx = e.Delta.Translation.X;
            var dy = e.Delta.Translation.Y;
            this.sv.ChangeView(this.sv.HorizontalOffset - dx, this.sv.VerticalOffset - dy, null, true);
        }

        private void setSvManipulationMode(object sender, PointerRoutedEventArgs e)
        {
            switch(e.Pointer.PointerDeviceType)
            {
            case Windows.Devices.Input.PointerDeviceType.Touch:
                this.sv.ManipulationMode = ManipulationModes.System;
                break;
            case Windows.Devices.Input.PointerDeviceType.Pen:
            case Windows.Devices.Input.PointerDeviceType.Mouse:
                var mode = ManipulationModes.System | ManipulationModes.TranslateX | ManipulationModes.TranslateY | ManipulationModes.TranslateInertia;
                this.sv.ManipulationMode = mode;
                break;
            default:
                break;
            }
        }

        private void sv_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            setSvManipulationMode(sender, e);
        }

        private void sv_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            setSvManipulationMode(sender, e);
        }

        private void sv_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            setSvManipulationMode(sender, e);
        }

        public void ZoomTo(Point point, float factor)
        {
            if(factor > this.sv.MaxZoomFactor)
                factor = this.sv.MaxZoomFactor;
            else if(factor < this.sv.MinZoomFactor)
                factor = this.sv.MinZoomFactor;
            var pi = point;
            var psX = point.X * this.sv.ZoomFactor;
            var psY = point.Y * this.sv.ZoomFactor;
            if(this.sv.ScrollableWidth > 0)
                psX -= this.sv.HorizontalOffset;
            else
                psX += (this.sv.ActualWidth - this.sv.ExtentWidth) / 2;
            if(this.sv.ScrollableHeight > 0)
                psY -= this.sv.VerticalOffset;
            else
                psY += (this.sv.ActualHeight - this.sv.ExtentHeight) / 2;
            this.sv.ChangeView(pi.X * factor - psX, pi.Y * factor - psY, factor);
        }

        public void ZoomTo(Point point)
        {
            this.ZoomTo(point, 2);
        }

        public void ZoomTo(float factor)
        {
            double w, h;
            if(this.sv.ScrollableWidth > 0)
                w = (this.sv.ActualWidth / 2 + this.sv.HorizontalOffset) / this.sv.ZoomFactor;
            else
                w = this.gd_ContentRoot.ActualWidth / 2;
            if(this.sv.ScrollableHeight > 0)
                h = (this.sv.ActualHeight / 2 + this.sv.VerticalOffset) / this.sv.ZoomFactor;
            else
                h = this.gd_ContentRoot.ActualHeight / 2;
            this.ZoomTo(new Point(w, h), factor);
        }

        private void Zoom(Point p)
        {
            if(this.sv.ZoomFactor > 1.001)
                ResetZoom();
            else
                this.ZoomTo(p);
        }

        public void ResetZoom()
        {
            this.sv.ChangeView(null, null, 1);
        }

        protected override async void OnDoubleTapped(DoubleTappedRoutedEventArgs e)
        {
            base.OnDoubleTapped(e);
            var point = e.GetPosition(this.gd_ContentRoot);
            await Task.Yield();
            this.Zoom(point);
        }

        protected override void OnKeyDown(KeyRoutedEventArgs e)
        {
            base.OnKeyDown(e);
            e.Handled = true;
            switch(e.Key)
            {
            case Windows.System.VirtualKey.GamepadY:
            case (Windows.System.VirtualKey)221:
                ZoomTo(this.sv.ZoomFactor * 1.2f);
                break;
            case Windows.System.VirtualKey.GamepadX:
            case (Windows.System.VirtualKey)219:
                ZoomTo(this.sv.ZoomFactor / 1.2f);
                break;
            case Windows.System.VirtualKey.Up:
            case Windows.System.VirtualKey.Down:
                if(this.sv.ScrollableHeight < 1)
                    e.Handled = false;
                break;
            case Windows.System.VirtualKey.Left:
            case Windows.System.VirtualKey.Right:
                if(this.sv.ScrollableWidth < 1)
                    e.Handled = false;
                break;
            case Windows.System.VirtualKey.Space:
                if(this.spacePressed)
                    e.Handled = false;
                else
                {
                    Zoom(new Point(this.gd_ContentRoot.ActualWidth / 2, this.gd_ContentRoot.ActualHeight / 2));
                    this.spacePressed = true;
                    e.Handled = true;
                }
                break;
            default:
                e.Handled = false;
                break;
            }
        }

        protected override void OnKeyUp(KeyRoutedEventArgs e)
        {
            base.OnKeyUp(e);
            e.Handled = true;
            switch(e.Key)
            {
            case (Windows.System.VirtualKey)187:
                ZoomTo(this.sv.MaxZoomFactor);
                break;
            case (Windows.System.VirtualKey)189:
                ResetZoom();
                break;
            case Windows.System.VirtualKey.Space:
                this.spacePressed = false;
                e.Handled = false;
                break;
            default:
                e.Handled = false;
                break;
            }
        }

        private bool spacePressed;

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ResetZoom();
        }
    }
}
