using ExClient;
using ExViewer.Settings;
using ImageLib;
using ImageLib.Gif;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

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
        } = DependencyProperty.Register("Image", typeof(GalleryImage), typeof(ImagePresenter), new PropertyMetadata(null));

        protected override Size MeasureOverride(Size availableSize)
        {
            gd_ContentRoot.MaxWidth = availableSize.Width;
            gd_ContentRoot.MaxHeight = availableSize.Height;
            sv.ZoomToFactor(1);
            return base.MeasureOverride(availableSize);
        }

        private void sv_Loading(FrameworkElement sender, object args)
        {
            sv.MaxZoomFactor = SettingCollection.Current.MaxFactor;
            Bindings.Update();
        }

        private void sv_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if(e.Handled)
                return;
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

        public void ZoomTo(DoubleTappedRoutedEventArgs e)
        {
            var fa = sv.ZoomFactor;
            if(fa == sv.MinZoomFactor)
            {
                var pi = e.GetPosition((UIElement)sv.Content);
                pi.X *= fa;
                pi.Y *= fa;
                var ps = e.GetPosition(sv);
                var df = SettingCollection.Current.DefaultFactor;
                sv.ZoomToFactor(fa * df);
                sv.ScrollToHorizontalOffset(pi.X * df - ps.X);
                sv.ScrollToVerticalOffset(pi.Y * df - ps.Y);
            }
            else
                ResetScale();
        }

        public void ZoomTo(TappedRoutedEventArgs e)
        {
            var fa = sv.ZoomFactor;
            if(fa == sv.MinZoomFactor)
            {
                var pi = e.GetPosition((UIElement)sv.Content);
                pi.X *= fa;
                pi.Y *= fa;
                var ps = e.GetPosition(sv);
                var df = SettingCollection.Current.DefaultFactor;
                sv.ZoomToFactor(fa * df);
                sv.ScrollToHorizontalOffset(pi.X * df - ps.X);
                sv.ScrollToVerticalOffset(pi.Y * df - ps.Y);
            }
            else
                ResetScale();
        }

        public void ResetScale()
        {
            sv.ZoomToFactor(1);
        }
    }

    internal class ImagePresenterSelector : DataTemplateSelector
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
