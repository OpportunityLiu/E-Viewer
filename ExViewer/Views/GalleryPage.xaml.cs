using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace ExViewer.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class GalleryPage : Page, IMainPageController
    {
        public GalleryPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if(e.NavigationMode == NavigationMode.New)
            {
                Gallery = (ExClient.Gallery)e.Parameter;
                pb_save.Maximum = Gallery.RecordCount;
                pb_save.Value = 0;
                pb_save.Visibility = Visibility.Collapsed;
            }
            else if(e.NavigationMode == NavigationMode.Back)
            {
                gv.SelectedIndex = Gallery.CurrentPage;
                gv.ScrollIntoView(Gallery[Gallery.CurrentPage]);
                entranceElement = (UIElement)gv.ContainerFromIndex(Gallery.CurrentPage);
                EntranceNavigationTransitionInfo.SetIsTargetElement(entranceElement, true);
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            if(entranceElement != null)
                EntranceNavigationTransitionInfo.SetIsTargetElement(entranceElement, false);
        }

        UIElement entranceElement;

        private void gv_ItemClick(object sender, ItemClickEventArgs e)
        {
            Gallery.CurrentPage = Gallery.IndexOf((ExClient.GalleryImage)e.ClickedItem);
            Frame.Navigate(typeof(ImagePage), Gallery);
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
            DependencyProperty.Register("Gallery", typeof(ExClient.Gallery), typeof(GalleryPage), new PropertyMetadata(null));

        public event EventHandler<MainPageControlCommand> CommandExecuted;

        private void btn_pane_Click(object sender, RoutedEventArgs e)
        {
            cb_top.IsOpen = false;
            CommandExecuted?.Invoke(this, MainPageControlCommand.SwitchSplitView);
        }

        private async void abb_open_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(Gallery.GalleryUri);
        }

        private async void abb_save_Click(object sender, RoutedEventArgs e)
        {
            var save = Gallery.SaveGalleryAsync();
            var gid = Gallery.Id;
            this.pb_save.Visibility = Visibility.Visible;
            save.Progress = async (s, p) =>
            {
                if(this.Gallery.Id != gid)
                    return;
                await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, () =>
                {
                    this.pb_save.Visibility = Visibility.Visible;
                    this.pb_save.Value = p.ImageLoaded;
                });
            };
            await save;
        }
    }
}
