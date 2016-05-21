using ExClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace ExViewer.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class CachePage : Page
    {
        public CachePage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if(e.NavigationMode != NavigationMode.Back)
            {
                await loadCache();
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            if(!(e.NavigationMode == NavigationMode.New && e.SourcePageType == typeof(GalleryPage)))
            {
                cached.Clear();
            }
        }

        ObservableCollection<CachedGallery> cached = new ObservableCollection<CachedGallery>();

        private async Task loadCache()
        {
            cached.Clear();
            var c = await CachedGallery.GetCachedGalleries().GetFilesAsync();
            foreach(var id in c)
            {
                cached.Add(await CachedGallery.LoadGalleryAsync(id));
            }
        }

        private void btn_Pane_Click(object sender, RoutedEventArgs e)
        {
            cb_top.IsOpen = false;
            RootControl.RootController.SwitchSplitView();
        }

        private void lv_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(GalleryPage), e.ClickedItem);
        }

        private async void abb_Refresh_Click(object sender, RoutedEventArgs e)
        {
            await loadCache();
        }

        private ContentDialog cdg_ConfirmClear = new ContentDialog()
        {
            Title = "ARE YOU SURE",
            Content = "All saved galleries will be deleted.",
            PrimaryButtonText = "Ok",
            SecondaryButtonText = "Cancel"
        };

        private async void abb_ClearCache_Click(object sender, RoutedEventArgs e)
        {
            var result = await cdg_ConfirmClear.ShowAsync();
            if(result == ContentDialogResult.Primary)
            {
                await CachedGallery.ClearCachedGalleriesAsync();
                await loadCache();
            }
        }

        private async void mfi_DeleteGallery_Click(object sender, RoutedEventArgs e)
        {
            var s = (FrameworkElement)sender;
            var cg = (CachedGallery)s.DataContext;
            await cg.DeleteAsync();
            cached.Remove(cg);
        }

        private void gv_Gallery_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        private void gv_Gallery_Holding(object sender, HoldingRoutedEventArgs e)
        {
            var s = (FrameworkElement)sender;
            switch(e.HoldingState)
            {
            case Windows.UI.Input.HoldingState.Started:
                FlyoutBase.ShowAttachedFlyout(s);
                break;
            case Windows.UI.Input.HoldingState.Canceled:
                FlyoutBase.GetAttachedFlyout(s).Hide();
                break;
            default:
                break;
            }
        }
    }
}
