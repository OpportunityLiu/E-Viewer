using ExClient.Galleries;
using ExViewer.Settings;
using Opportunity.MvvmUniverse;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace ExViewer.Controls
{
    public sealed partial class ImagePresenter : UserControl
    {
        [ThreadStatic]
        private static BitmapImage defaultImage;

        public ImagePresenter()
        {
            InitializeComponent();
            if (defaultImage is null)
            {
                defaultImage = new BitmapImage();
                Dispatcher.BeginIdle(async p =>
                {
                    using (var img = await StorageHelper.GetIconOfExtensionAsync("jpg"))
                    {
                        await defaultImage.SetSourceAsync(img);
                    }
                });
            }
        }

        public GalleryImage Image
        {
            get => (GalleryImage)GetValue(ImageProperty);
            set => SetValue(ImageProperty, value);
        }

        /// <summary>
        /// Indentify <see cref="Image"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register(nameof(Image), typeof(GalleryImage), typeof(ImagePresenter), new PropertyMetadata(null, ImagePropertyChanged));

        private static async void ImagePropertyChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            var oldValue = (GalleryImage)e.OldValue;
            var newValue = (GalleryImage)e.NewValue;
            if (oldValue == newValue)
            {
                return;
            }

            var sender = (ImagePresenter)dp;
            if (newValue != null)
            {
                try
                {
                    await newValue.LoadImageAsync(false, SettingCollection.Current.GetStrategy(), false);
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    Views.RootControl.RootController.SendToast(ex, null);
                }
            }
            else
            {
                sender.img_Thumb.Source = defaultImage;
            }
        }

        protected override void OnDisconnectVisualChildren()
        {
            ClearValue(ImageProperty);
            img_Content.ClearValue(Windows.UI.Xaml.Controls.Image.SourceProperty);
            img_Thumb.ClearValue(Windows.UI.Xaml.Controls.Image.SourceProperty);
            base.OnDisconnectVisualChildren();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Descendants<ScrollContentPresenter>().First().Clip = null;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            gd_ContentRoot.MaxWidth = availableSize.Width;
            gd_ContentRoot.MaxHeight = availableSize.Height;
            return base.MeasureOverride(availableSize);
        }

        private void sv_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var dx = e.Delta.Translation.X;
            var dy = e.Delta.Translation.Y;
            sv.ChangeView(sv.HorizontalOffset - dx, sv.VerticalOffset - dy, null, true);
        }

        private void setSvManipulationMode(object sender, PointerRoutedEventArgs e)
        {
            switch (e.Pointer.PointerDeviceType)
            {
            case Windows.Devices.Input.PointerDeviceType.Touch:
                sv.ManipulationMode = ManipulationModes.System;
                break;
            case Windows.Devices.Input.PointerDeviceType.Pen:
            case Windows.Devices.Input.PointerDeviceType.Mouse:
                var mode = ManipulationModes.System | ManipulationModes.TranslateX | ManipulationModes.TranslateY | ManipulationModes.TranslateInertia;
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
            if (factor > sv.MaxZoomFactor)
            {
                factor = sv.MaxZoomFactor;
            }
            else if (factor < sv.MinZoomFactor)
            {
                factor = sv.MinZoomFactor;
            }

            var pi = point;
            var psX = point.X * sv.ZoomFactor;
            var psY = point.Y * sv.ZoomFactor;
            if (sv.ScrollableWidth > 0)
            {
                psX -= sv.HorizontalOffset;
            }
            else
            {
                psX += (sv.ActualWidth - sv.ExtentWidth) / 2;
            }

            if (sv.ScrollableHeight > 0)
            {
                psY -= sv.VerticalOffset;
            }
            else
            {
                psY += (sv.ActualHeight - sv.ExtentHeight) / 2;
            }

            sv.ChangeView(pi.X * factor - psX, pi.Y * factor - psY, factor);
        }

        public void ZoomTo(Point point)
        {
            ZoomTo(point, 2);
        }

        public void ZoomTo(float factor)
        {
            double w, h;
            if (sv.ScrollableWidth > 0)
            {
                w = (sv.ActualWidth / 2 + sv.HorizontalOffset) / sv.ZoomFactor;
            }
            else
            {
                w = gd_ContentRoot.ActualWidth / 2;
            }

            if (sv.ScrollableHeight > 0)
            {
                h = (sv.ActualHeight / 2 + sv.VerticalOffset) / sv.ZoomFactor;
            }
            else
            {
                h = gd_ContentRoot.ActualHeight / 2;
            }

            ZoomTo(new Point(w, h), factor);
        }

        private void Zoom(Point p)
        {
            if (sv.ZoomFactor > 1.001)
            {
                ResetZoom(false);
            }
            else
            {
                ZoomTo(p);
            }
        }

        public void ResetZoom(bool disableAnimation)
        {
            sv.ChangeView(Padding.Left, Padding.Top, 1, disableAnimation);
        }

        protected override async void OnDoubleTapped(DoubleTappedRoutedEventArgs e)
        {
            base.OnDoubleTapped(e);
            var point = e.GetPosition(gd_ContentRoot);
            if (e.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch)
            {
                await Task.Delay(100);
            }

            Zoom(point);
        }

        protected override void OnKeyDown(KeyRoutedEventArgs e)
        {
            base.OnKeyDown(e);
            e.Handled = true;
            switch (e.Key)
            {
            case Windows.System.VirtualKey.GamepadRightThumbstickUp:
            case (Windows.System.VirtualKey)221:
                ZoomTo(sv.ZoomFactor * 1.2f);
                break;
            case Windows.System.VirtualKey.GamepadRightThumbstickDown:
            case (Windows.System.VirtualKey)219:
                ZoomTo(sv.ZoomFactor / 1.2f);
                break;
            case Windows.System.VirtualKey.Up:
            case Windows.System.VirtualKey.Down:
                if (sv.ScrollableHeight < 1)
                {
                    e.Handled = false;
                }

                break;
            case Windows.System.VirtualKey.Left:
            case Windows.System.VirtualKey.Right:
                if (sv.ScrollableWidth < 1)
                {
                    e.Handled = false;
                }

                break;
            case Windows.System.VirtualKey.Space:
                if (spacePressed)
                {
                    e.Handled = false;
                }
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
            switch (e.Key)
            {
            case (Windows.System.VirtualKey)187:
                ZoomTo(sv.MaxZoomFactor);
                break;
            case (Windows.System.VirtualKey)189:
                ResetZoom(false);
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

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ResetZoom(true);
        }
    }
}
