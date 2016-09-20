using ExClient;
using ExViewer.Settings;
using ImageLib;
using ImageLib.Gif;
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
            get
            {
                return (GalleryImage)GetValue(ImageProperty);
            }
            set
            {
                SetValue(ImageProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for Image.  This enables animation, styling, binding, etc...
        public static DependencyProperty ImageProperty
        {
            get;
        } = DependencyProperty.Register("Image", typeof(GalleryImage), typeof(ImagePresenter), new PropertyMetadata(null, ImagePropertyChangedCallback));

        private static void ImagePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = (ImagePresenter)d;

            if(e.NewValue == null)
            {
                sender.FindName(nameof(img_Loading));
            }
            if(!sender.loaded)
            {
                if(e.OldValue != null)
                {
                    stopTrackImage(sender, (GalleryImage)e.OldValue);
                }
            }
            else
            {
                if(e.NewValue != null)
                {
                    startTrackImage(sender, (GalleryImage)e.NewValue);
                }
            }
        }

        private bool loaded;

        private static void startTrackImage(ImagePresenter sender, GalleryImage image)
        {
            image.PropertyChanged += sender.Image_PropertyChanged;
            if(image.State == ImageLoadingState.Waiting)
            {
                var ignore = image.LoadImageAsync(false, SettingCollection.Current.GetStrategy(), false);
            }
        }

        private static void stopTrackImage(ImagePresenter sender, GalleryImage image)
        {
            image.PropertyChanged -= sender.Image_PropertyChanged;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if(this.Image != null)
            {
                startTrackImage(this, this.Image);
            }
            loaded = true;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if(this.Image != null)
                stopTrackImage(this, this.Image);
            loaded = false;
        }

        private void Image_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(GalleryImage.ImageFile))
            {
                this.cc_Image.ClearValue(ContentControl.ContentProperty);
                this.cc_Image.Content = Image;
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            gd_ContentRoot.MaxWidth = availableSize.Width;
            gd_ContentRoot.MaxHeight = availableSize.Height;
            Task.Yield().GetAwaiter().OnCompleted(() => sv.ChangeView(null, null, 1, true));
            return base.MeasureOverride(availableSize);
        }

        private void sv_Loading(FrameworkElement sender, object args)
        {
            sv.MaxZoomFactor = SettingCollection.Current.MaxFactor;
            Bindings.Update();
        }

        private void sv_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if(!SettingCollection.Current.MouseInertial && e.IsInertial)
                return;
            var dx = e.Delta.Translation.X;
            var dy = e.Delta.Translation.Y;
            sv.ScrollToHorizontalOffset(sv.HorizontalOffset - dx);
            sv.ScrollToVerticalOffset(sv.VerticalOffset - dy);
        }

        private void setSvManipulationMode(object sender, PointerRoutedEventArgs e)
        {
            switch(e.Pointer.PointerDeviceType)
            {
            case Windows.Devices.Input.PointerDeviceType.Touch:
                sv.ManipulationMode = ManipulationModes.System;
                break;
            case Windows.Devices.Input.PointerDeviceType.Pen:
            case Windows.Devices.Input.PointerDeviceType.Mouse:
                var mode = ManipulationModes.System | ManipulationModes.TranslateX | ManipulationModes.TranslateY;
                if(SettingCollection.Current.MouseInertial)
                    mode |= ManipulationModes.TranslateInertia;
                sv.ManipulationMode = mode;
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
            if(factor > sv.MaxZoomFactor)
                factor = sv.MaxZoomFactor;
            else if(factor < sv.MinZoomFactor)
                factor = sv.MinZoomFactor;
            var pi = point;
            var psX = point.X * sv.ZoomFactor;
            var psY = point.Y * sv.ZoomFactor;
            if(sv.ScrollableWidth > 0)
                psX -= sv.HorizontalOffset;
            else
                psX += (sv.ActualWidth - sv.ExtentWidth) / 2;
            if(sv.ScrollableHeight > 0)
                psY -= sv.VerticalOffset;
            else
                psY += (sv.ActualHeight - sv.ExtentHeight) / 2;
            sv.ChangeView(pi.X * factor - psX, pi.Y * factor - psY, factor);
        }

        public void ZoomTo(Point point)
        {
            this.ZoomTo(point, SettingCollection.Current.DefaultFactor);
        }

        public void ZoomTo(float factor)
        {
            double w, h;
            if(sv.ScrollableWidth > 0)
                w = (sv.ActualWidth / 2 + sv.HorizontalOffset) / sv.ZoomFactor;
            else
                w = gd_ContentRoot.ActualWidth / 2;
            if(sv.ScrollableHeight > 0)
                h = (sv.ActualHeight / 2 + sv.VerticalOffset) / sv.ZoomFactor;
            else
                h = gd_ContentRoot.ActualHeight / 2;
            this.ZoomTo(new Point(w, h), factor);
        }

        private void Zoom(Point p)
        {
            if(sv.ZoomFactor > 1.001)
                ResetZoom();
            else
                this.ZoomTo(p);
        }

        public void ResetZoom()
        {
            sv.ChangeView(null, null, 1);
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
            case (Windows.System.VirtualKey)221:
                ZoomTo(sv.ZoomFactor * 1.2f);
                break;
            case (Windows.System.VirtualKey)219:
                ZoomTo(sv.ZoomFactor / 1.2f);
                break;
            case Windows.System.VirtualKey.Up:
            case Windows.System.VirtualKey.Down:
                if(sv.ScrollableHeight < 1)
                    e.Handled = false;
                break;
            case Windows.System.VirtualKey.Left:
            case Windows.System.VirtualKey.Right:
                if(sv.ScrollableWidth < 1)
                    e.Handled = false;
                break;
            case Windows.System.VirtualKey.Space:
                if(spacePressed)
                    e.Handled = false;
                else
                {
                    Zoom(new Point(gd_ContentRoot.ActualWidth / 2, gd_ContentRoot.ActualHeight / 2));
                    spacePressed = true;
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
                ZoomTo(sv.MaxZoomFactor);
                break;
            case (Windows.System.VirtualKey)189:
                ResetZoom();
                break;
            case Windows.System.VirtualKey.Space:
                spacePressed = false;
                e.Handled = false;
                break;
            default:
                e.Handled = false;
                break;
            }
        }

        private bool spacePressed;
    }

    internal class ImagePresenterSelector : Windows.UI.Xaml.Controls.DataTemplateSelector
    {
        public DataTemplate Template
        {
            get;
            set;
        }

        public DataTemplate GifTemplate
        {
            get;
            set;
        }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            var img = item as GalleryImage;
            if(SettingCollection.Current.EnableGif && IsGif(img))
            {
                initGif();
                return GifTemplate;
            }
            return Template;
        }

        public static bool IsGif(GalleryImage img)
        {
            return img?.ImageFile?.Name.EndsWith(".gif", StringComparison.OrdinalIgnoreCase) == true;
        }

        private static bool gifInitialized;

        private static void initGif()
        {
            if(gifInitialized)
                return;
            gifInitialized = true;
            ImageLoader.Initialize(new ImageConfig.Builder()
            {
                CacheMode = ImageLib.Cache.CacheMode.NoCache
            }.AddDecoder<GifDecoder>().Build());
        }
    }
}
