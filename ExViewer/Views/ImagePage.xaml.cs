using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
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
            LoadStateToVisualStateConverter.AccentBrush = (Brush)this.Resources["SystemControlForegroundAccentBrush"];
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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var param = (ExClient.Gallery)e.Parameter;
            fv.ItemsSource= Gallery = param;
            base.OnNavigatedTo(e);
            fv.SelectedIndex = Gallery.CurrentPage;
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            Gallery.CurrentPage = fv.SelectedIndex;
            base.OnNavigatingFrom(e);
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
        }

        private async void abb_reload_Click(object sender, RoutedEventArgs e)
        {
            await Gallery[fv.SelectedIndex].LoadImage(true);
        }

        private async void abb_open_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(Gallery[fv.SelectedIndex].PageUri);
        }
    }

    public class LoadStateToVisualStateConverter : IValueConverter
    {
        public static Brush AccentBrush
        {
            get;
            set;
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var state = (ExClient.ImageLoadingState)value;
            if(targetType == typeof(Visibility))
            {
                if(state == ExClient.ImageLoadingState.Loaded)
                    return Visibility.Collapsed;
                else
                    return Visibility.Visible;
            }
            if(targetType == typeof(Brush))
            {
                if(state == ExClient.ImageLoadingState.Failed)
                    return new SolidColorBrush(Windows.UI.Colors.Red);
                else
                    return AccentBrush;
            }
            if(targetType == typeof(bool))
            {
                if(state == ExClient.ImageLoadingState.Waiting || state == ExClient.ImageLoadingState.Preparing)
                    return true;
                else
                    return false;
            }
            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
