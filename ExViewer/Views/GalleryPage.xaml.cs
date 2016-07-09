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
using System.Threading.Tasks;

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

        private async void GalleryPage_ViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {
            if(e.NextView.VerticalOffset < e.FinalView.VerticalOffset && sv_Content.VerticalOffset < 1)
            {
                await Task.Yield();
                changeViewTo(true, false);
            }
            else if(e.IsInertial && e.NextView.VerticalOffset < 1 && sv_Content.VerticalOffset > gd_info.ActualHeight - 1)
            {
                await Task.Yield();
                changeViewTo(false, false);
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            gd_Pivot.Height = availableSize.Height - 48;
            changeView(true);
            return base.MeasureOverride(availableSize);
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if(e.NavigationMode == NavigationMode.New)
            {
                pv.SelectedIndex = 0;
            }
            VM = await GalleryVM.GetVMAsync((long)e.Parameter);
            Bindings.Update();
            if(e.NavigationMode == NavigationMode.Back)
            {
                var current = VM.GetCurrent();
                if(current != null)
                    gv.ScrollIntoView(current, ScrollIntoViewAlignment.Leading);
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

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            VM = null;
            changeViewTo(false, true);
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

        private void gv_Tags_ItemClick(object sender, ItemClickEventArgs e)
        {
            var s = (ListViewBase)sender;
            var container = (SelectorItem)s.ContainerFromItem(e.ClickedItem);
            foreach(var item in mfo_Tag.Items)
            {
                item.DataContext = e.ClickedItem;
            }
            mfo_Tag.ShowAt(container);
            // Frame.Navigate(typeof(SearchPage), Cache.AddSearchResult(((Tag)e.ClickedItem).Search()));
        }

        private void lv_Torrents_ItemClick(object sender, ItemClickEventArgs e)
        {

        }

        private async void btn_Scroll_Click(object sender, RoutedEventArgs e)
        {
            await Task.Yield();
            changeView(false);
        }

        private bool currentState
        {
            get
            {
                var fullOffset = gd_info.ActualHeight;
                return sv_Content.VerticalOffset > fullOffset * 0.95;
            }
        }

        private void changeView(bool keep)
        {
            changeViewTo(!currentState ^ keep, false);
        }

        private void changeViewTo(bool view, bool disableAnimation)
        {
            var fullOffset = gd_info.ActualHeight;
            if(view)
                sv_Content.ChangeView(null, fullOffset, null, disableAnimation);
            else
                sv_Content.ChangeView(null, 0.000001, null, disableAnimation);
        }

        private async void pv_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch(pv.SelectedIndex)
            {
            case 1://Comments
                if(VM.Gallery.Comments == null)
                    await VM.LoadComments();
                break;
            case 2://Torrents
                if(VM.Torrents == null)
                    await VM.LoadTorrents();
                break;
            }
        }

        private void lv_Torrents_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach(var item in e.RemovedItems)
            {
                var con = (ListViewItem)lv_Torrents.ContainerFromItem(item);
                if(con == null)
                    continue;
                var gd = (FrameworkElement)((FrameworkElement)con.ContentTemplateRoot).FindName("gd_TorrentDetail");
                gd.Visibility = Visibility.Collapsed;
            }
            var added = e.AddedItems.FirstOrDefault();
            if(added != null)
            {
                var con = (ListViewItem)lv_Torrents.ContainerFromItem(added);
                if(con == null)
                    return;
                var gd = (FrameworkElement)((FrameworkElement)con.ContentTemplateRoot).FindName("gd_TorrentDetail");
                gd.Visibility = Visibility.Visible;
            }
        }

        private void gv_Loaded(object sender, RoutedEventArgs e)
        {
            var bd_Gv = VisualTreeHelper.GetChild(gv, 0);
            var sv_Gv = VisualTreeHelper.GetChild(bd_Gv, 0);
            (sv_Gv as ScrollViewer).ViewChanging += GalleryPage_ViewChanging;
            gv.Loaded -= gv_Loaded;
        }

        private void lv_Torrents_Loaded(object sender, RoutedEventArgs e)
        {
            var bd_Lv = VisualTreeHelper.GetChild(lv_Torrents, 0);
            var sv_Lv = VisualTreeHelper.GetChild(bd_Lv, 0);
            (sv_Lv as ScrollViewer).ViewChanging += GalleryPage_ViewChanging;
            lv_Torrents.Loaded -= lv_Torrents_Loaded;
        }

        private void lv_Comments_Loaded(object sender, RoutedEventArgs e)
        {
            var bd_Lv = VisualTreeHelper.GetChild(lv_Comments, 0);
            var sv_Lv = VisualTreeHelper.GetChild(bd_Lv, 0);
            (sv_Lv as ScrollViewer).ViewChanging += GalleryPage_ViewChanging;
            lv_Comments.Loaded -= lv_Comments_Loaded;
        }
    }
}
