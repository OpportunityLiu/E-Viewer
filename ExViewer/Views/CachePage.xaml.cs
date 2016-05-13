using ExClient;
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

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace ExViewer.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class CachePage : Page, IRootController
    {
        public CachePage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if(e.NavigationMode != NavigationMode.Back)
            {
                loadCache();
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            if(e.NavigationMode != NavigationMode.New)
            {
                lv.ItemsSource = null;
            }
        }

        private async void loadCache()
        {
            tb_Empty.Visibility = Visibility.Collapsed;
            var c = await Gallery.GetCachedGalleriesAsync();
            var cached = new List<Gallery>();
            foreach(var id in c)
            {
                cached.Add(await Gallery.LoadGalleryAsync(id));
            }
            lv.ItemsSource = cached;
            if(cached.Count == 0)
                tb_Empty.Visibility = Visibility.Visible;
            else
                tb_Empty.Visibility = Visibility.Collapsed;
        }

        public event EventHandler<RootControlCommand> CommandExecuted;

        private void btn_Pane_Click(object sender, RoutedEventArgs e)
        {
            cb_top.IsOpen = false;
            CommandExecuted?.Invoke(this, RootControlCommand.SwitchSplitView);
        }

        private void lv_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(GalleryPage), e.ClickedItem);
        }

        private void abb_Refresh_Click(object sender, RoutedEventArgs e)
        {
            loadCache();
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
                await Gallery.ClearCachedGalleriesAsync();
                loadCache();
            }
        }
    }
}
