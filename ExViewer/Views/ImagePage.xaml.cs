using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace ExViewer.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ImagePage : Page, IMainPageController
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
            cb_top.Background = new SolidColorBrush(toColor) { Opacity = 0.3 };
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

        // Using a DependencyProperty as the backing store for Gallery.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GalleryProperty =
            DependencyProperty.Register("Gallery", typeof(ExClient.Gallery), typeof(ImagePage), new PropertyMetadata(null));

        public event EventHandler<MainPageControlCommand> CommandExecuted;

        private void btn_pane_Click(object sender, RoutedEventArgs e)
        {
            cb_top.IsOpen = false;
            CommandExecuted?.Invoke(this, MainPageControlCommand.SwitchSplitView);
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if(e.NavigationMode == NavigationMode.New)
            {
                cb_top.Visibility = Visibility.Visible;
            }
            var param = (ExClient.Gallery)e.Parameter;
            fv.ItemsSource = Gallery = param;
            base.OnNavigatedTo(e);
            fv.SelectedIndex = Gallery.CurrentPage;
            await Task.Delay(10);
            fv.SelectionChanged += fv_SelectionChanged;
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            Gallery.CurrentPage = fv.SelectedIndex;
            fv.SelectionChanged -= fv_SelectionChanged;
            base.OnNavigatingFrom(e);
        }

        private void setScaleCore(ScrollViewer sv, Image img)
        {
            var factor = Math.Min(fv.ActualHeight / img.ActualHeight, fv.ActualWidth / img.ActualWidth);
            if(double.IsInfinity(factor) || double.IsNaN(factor))
                factor = Math.Min(fv.ActualHeight / 1000, fv.ActualWidth / 1000);
            sv.MinZoomFactor = (float)factor;
            sv.MaxZoomFactor = (float)factor * 8;
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
                var ignore = Gallery[i].LoadImage(false);
            }
            setScale();
        }

        private async void abb_reload_Click(object sender, RoutedEventArgs e)
        {
            await Gallery[fv.SelectedIndex].LoadImage(true);
        }

        private async void abb_open_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(Gallery[fv.SelectedIndex].PageUri);
        }

        private System.Threading.CancellationTokenSource change_cb_topVisibility;

        private void fvi_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if(e.Handled)
                return;
            change_cb_topVisibility = new System.Threading.CancellationTokenSource();
            Task.Delay(500, change_cb_topVisibility.Token).ContinueWith(async t =>
            {
                if(t.IsCanceled)
                    return;
                await cb_top.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    switch(cb_top.Visibility)
                    {
                    case Visibility.Visible:
                        cb_top.Visibility = Visibility.Collapsed;
                        break;
                    case Visibility.Collapsed:
                        cb_top.Visibility = Visibility.Visible;
                        break;
                    }
                });
            });
            e.Handled = true;
        }

        private void fvi_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if(e.Handled)
                return;
            change_cb_topVisibility?.Cancel();
            var sv = (ScrollViewer)((FrameworkElement)sender).FindName("sv");
            var fa = sv.ZoomFactor;
            if(fa == sv.MinZoomFactor)
            {
                var pi = e.GetPosition((UIElement)sv.Content);
                pi.X *= fa;
                pi.Y *= fa;
                var ps = e.GetPosition(sv);
                sv.ZoomToFactor(fa * 2);
                sv.ScrollToHorizontalOffset(pi.X * 2 - ps.X);
                sv.ScrollToVerticalOffset(pi.Y * 2 - ps.Y);
            }
            else
                sv.ZoomToFactor(sv.MinZoomFactor);
            e.Handled = true;
        }

        private void Image_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var s = (Image)sender;
            var p = (ScrollViewer)s.Parent;
            setScaleCore(p, s);
        }
    }
}
