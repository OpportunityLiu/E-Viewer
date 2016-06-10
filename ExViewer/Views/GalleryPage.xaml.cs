using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using ExViewer.Settings;
using ExClient;
using ExViewer.ViewModels;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace ExViewer.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class GalleryPage : Page
    {
        public GalleryPage()
        {
            this.InitializeComponent();
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            pv.Height = availableSize.Height - 48;
            return base.MeasureOverride(availableSize);
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            VM = await GalleryVM.GetVMAsync((long)e.Parameter);
            if(e.NavigationMode == NavigationMode.Back)
            {
                gv.ScrollIntoView(VM.GetCurrent());
                entranceElement = (UIElement)gv.ContainerFromIndex(VM.CurrentIndex);
                if(entranceElement != null)
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
            if(VM.OpenImage.CanExecute(e.ClickedItem))
                VM.OpenImage.Execute(e.ClickedItem);
        }

        public GalleryVM VM
        {
            get
            {
                return (GalleryVM)GetValue(VMProperty);
            }
            set
            {
                SetValue(VMProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for VM.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VMProperty =
            DependencyProperty.Register("VM", typeof(GalleryVM), typeof(GalleryPage), new PropertyMetadata(null));

        private void btn_pane_Click(object sender, RoutedEventArgs e)
        {
            cb_top.IsOpen = false;
            RootControl.RootController.SwitchSplitView();
        }

        private void lv_Tags_ItemClick(object sender, ItemClickEventArgs e)
        {
            // Frame.Navigate(typeof(SearchPage), Cache.AddSearchResult(((Tag)e.ClickedItem).Search()));
        }

        private void lv_Torrents_ItemClick(object sender, ItemClickEventArgs e)
        {

        }

        private async void lv_Torrents_Loaded(object sender, RoutedEventArgs e)
        {
            await VM.LoadTorrents();
        }
    }
}
