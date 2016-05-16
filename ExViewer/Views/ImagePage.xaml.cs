using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Color = Windows.UI.Color;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.ViewManagement;
using ExViewer.Settings;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace ExViewer.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ImagePage : Page, IRootController
    {
        public ImagePage()
        {
            this.InitializeComponent();
            var backColor = ((SolidColorBrush)Resources["ApplicationPageBackgroundThemeBrush"]).Color;
            var needColor = (Color)Resources["SystemChromeMediumColor"];
            var toColor = Color.FromArgb(255,
                (byte)(backColor.R - 2 * (backColor.R - needColor.R)),
                (byte)(backColor.G - 2 * (backColor.G - needColor.G)),
                (byte)(backColor.B - 2 * (backColor.B - needColor.B)));
            cb_top.Background = new SolidColorBrush(toColor) { Opacity = 0.29 };
        }

        public ExClient.Gallery Gallery
        {
            get
            {
                return (ExClient.Gallery)GetValue(GalleryProperty);
            }
            set
            {
                SetValue(GalleryProperty, value);
            }
        }

        ApplicationView av = ApplicationView.GetForCurrentView();

        // Using a DependencyProperty as the backing store for Gallery.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GalleryProperty =
            DependencyProperty.Register("Gallery", typeof(ExClient.Gallery), typeof(ImagePage), new PropertyMetadata(null));

        public event EventHandler<RootControlCommand> CommandExecuted;

        private void btn_pane_Click(object sender, RoutedEventArgs e)
        {
            cb_top.IsOpen = false;
            CommandExecuted?.Invoke(this, RootControlCommand.SwitchSplitView);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if(e.NavigationMode == NavigationMode.New)
            {
                cb_top.Visibility = Visibility.Visible;
            }
            this.mouseInertialFactor = Settings.Settings.Current.MouseInertialFactor;
            enableMouseInertia = mouseInertialFactor > 0.05;

            var param = (ExClient.Gallery)e.Parameter;
            fv.ItemsSource = Gallery = param;
            base.OnNavigatedTo(e);
            fv.SelectedIndex = Gallery.CurrentImage;

            av.VisibleBoundsChanged += Av_VisibleBoundsChanged;
            Av_VisibleBoundsChanged(av, null);
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            Gallery.CurrentImage = fv.SelectedIndex;
            base.OnNavigatingFrom(e);
            av.VisibleBoundsChanged -= Av_VisibleBoundsChanged;
        }

        private void Av_VisibleBoundsChanged(ApplicationView sender, object args)
        {
            if(av.IsFullScreenMode)
            {
                abb_fullScreen.Icon = new SymbolIcon(Symbol.BackToWindow);
                abb_fullScreen.Label = "Back to window";
            }
            else
            {
                abb_fullScreen.Icon = new SymbolIcon(Symbol.FullScreen);
                abb_fullScreen.Label = "Full screen";
            }
        }

        private void setFactor(ScrollViewer sv, Image img)
        {
            var factor = Math.Min(fv.ActualHeight / img.ActualHeight, fv.ActualWidth / img.ActualWidth);
            if(double.IsInfinity(factor) || double.IsNaN(factor))
                factor = Math.Min(fv.ActualHeight / 1000, fv.ActualWidth / 1000);
            sv.MinZoomFactor = (float)factor;
            sv.MaxZoomFactor = (float)factor * Settings.Settings.Current.MaxFactor;
            sv.ZoomToFactor(sv.MinZoomFactor);
        }

        private void setScale()
        {
            int lb = fv.SelectedIndex - 2;
            int ub = fv.SelectedIndex + 3;
            lb = lb < 0 ? 0 : lb;
            ub = ub > Gallery.Count ? Gallery.Count : ub;
            for(int i = lb; i < ub; i++)
            {
                if(i == fv.SelectedIndex)
                    continue;
                var selected = ((FlipViewItem)fv.ContainerFromIndex(i));
                if(selected == null)
                    continue;
                var inner = (Grid)selected.ContentTemplateRoot;
                var sv = (ScrollViewer)inner.FindName("sv");
                sv.ZoomToFactor(sv.MinZoomFactor);
            }
        }

        private void fv_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(Gallery == null)
                return;
            var start = fv.SelectedIndex;
            if(start < 0)
                return;
            var end = start + 5;
            if(end > Gallery.Count)
            {
                end = Gallery.Count;
            }
            if(end + 10 > Gallery.Count && Gallery.RecordCount > Gallery.Count)
            {
                var ignore = Gallery.LoadMoreItemsAsync(5);
            }
            for(int i = start; i < end; i++)
            {
                var ignore = Gallery[i].LoadImage(false, false);
            }
            setScale();
        }

        private async void abb_reload_Click(object sender, RoutedEventArgs e)
        {
            await Gallery[fv.SelectedIndex].LoadImage(true, false);
        }

        private async void abb_open_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(Gallery[fv.SelectedIndex].PageUri);
        }

        private System.Threading.CancellationTokenSource changeCbVisibility;

        private async void fvi_Tapped(object sender, TappedRoutedEventArgs e)
        {
            changeCbVisibility = new System.Threading.CancellationTokenSource();
            await Task.Delay(Settings.Settings.Current.ChangeCommandBarDelay, this.changeCbVisibility.Token).ContinueWith(async t =>
            {
                if(t.IsCanceled)
                    return;
                await this.cb_top.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    this.changeCbVisibility.Cancel();
                    switch(this.cb_top.Visibility)
                    {
                    case Visibility.Visible:
                        this.cb_top.Visibility = Visibility.Collapsed;
                        break;
                    case Visibility.Collapsed:
                        this.cb_top.Visibility = Visibility.Visible;
                        break;
                    }
                });
            });
        }

        private void fvi_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if(e.Handled)
                return;
            if(changeCbVisibility != null)
            {
                if(changeCbVisibility.IsCancellationRequested)
                    switch(cb_top.Visibility)
                    {
                    case Visibility.Visible:
                        cb_top.Visibility = Visibility.Collapsed;
                        break;
                    case Visibility.Collapsed:
                        cb_top.Visibility = Visibility.Visible;
                        break;
                    }
                else
                    changeCbVisibility.Cancel();
            }
            var sv = (ScrollViewer)((FrameworkElement)sender).FindName("sv");
            var fa = sv.ZoomFactor;
            if(fa == sv.MinZoomFactor)
            {
                var pi = e.GetPosition((UIElement)sv.Content);
                pi.X *= fa;
                pi.Y *= fa;
                var ps = e.GetPosition(sv);
                var df = Settings.Settings.Current.DefaultFactor;
                sv.ZoomToFactor(fa * df);
                sv.ScrollToHorizontalOffset(pi.X * df - ps.X);
                sv.ScrollToVerticalOffset(pi.Y * df - ps.Y);
            }
            else
                sv.ZoomToFactor(sv.MinZoomFactor);
            e.Handled = true;
        }

        private void Image_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var s = (Image)sender;
            var p = (ScrollViewer)s.Parent;
            setFactor(p, s);
        }

        private void sv_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var s = (ScrollViewer)sender;
            var p = (Image)s.Content;
            setFactor(s, p);
        }

        private bool enableMouseInertia;
        private double mouseInertialFactor;

        private void sv_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if(e.Handled)
                return;
            if(!enableMouseInertia && e.IsInertial)
                return;
            var dx = e.Delta.Translation.X;
            var dy = e.Delta.Translation.Y;
            if(e.IsInertial)
            {
                dx *= mouseInertialFactor;
                dy *= mouseInertialFactor;
            }
            var sv = (ScrollViewer)sender;
            sv.ScrollToHorizontalOffset(sv.HorizontalOffset - dx);
            sv.ScrollToVerticalOffset(sv.VerticalOffset - dy);
        }

        private void setSvManipulationMode(object sender, PointerRoutedEventArgs e)
        {
            var sv = (ScrollViewer)sender;
            switch(e.Pointer.PointerDeviceType)
            {
            case Windows.Devices.Input.PointerDeviceType.Touch:
                sv.ManipulationMode = ManipulationModes.System;
                break;
            case Windows.Devices.Input.PointerDeviceType.Pen:
            case Windows.Devices.Input.PointerDeviceType.Mouse:
                var mode = ManipulationModes.System | ManipulationModes.TranslateX | ManipulationModes.TranslateY;
                if(enableMouseInertia)
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

        private void abb_fullScreen_Click(object sender, RoutedEventArgs e)
        {
            if(!av.IsFullScreenMode)
            {
                av.TryEnterFullScreenMode();
            }
            else
            {
                av.ExitFullScreenMode();
            }
        }
    }
}
