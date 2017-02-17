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
    public sealed partial class GalleryPage : Page, IHasAppBar
    {
        public GalleryPage()
        {
            this.InitializeComponent();
            gd_Info.RegisterPropertyChangedCallback(ActualHeightProperty, set_btn_Scroll_Rotation);
            sv_Content.RegisterPropertyChangedCallback(ScrollViewer.VerticalOffsetProperty, set_btn_Scroll_Rotation);
        }

        private void set_btn_Scroll_Rotation(DependencyObject d, DependencyProperty dp)
        {
            var infoHeight = gd_Info.ActualHeight;
            if(infoHeight < 1)
                this.ct_btn_Scroll.Rotation = 0;
            else
                this.ct_btn_Scroll.Rotation = this.sv_Content.VerticalOffset / infoHeight * 180d;
        }

        private async void pv_Content_ViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {
            await Task.Yield();
            if(e.NextView.VerticalOffset < e.FinalView.VerticalOffset && sv_Content.VerticalOffset < 1)
            {
                changeViewTo(true, false);
            }
            else if(e.IsInertial && e.NextView.VerticalOffset < 1 && sv_Content.VerticalOffset > gd_Info.ActualHeight - 1)
            {
                changeViewTo(false, false);
            }
        }

        private void pv_Content_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            var prop = e.GetCurrentPoint(this).Properties;
            if(!prop.IsHorizontalMouseWheel && prop.MouseWheelDelta != 0)
            {
                changeViewTo(prop.MouseWheelDelta < 0, false);
                e.Handled = true;
            }
        }

        private bool needResetView, needRestoreView;

        protected override Size MeasureOverride(Size availableSize)
        {
            gd_Pivot.Height = availableSize.Height - 48;
            return base.MeasureOverride(availableSize);
        }

        private void page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if(needResetView)
            {
                needResetView = false;
                resetView();
            }
            else if(needRestoreView)
            {
                needRestoreView = false;
                restoreView();
            }
            else
            {
                changeView(true);
            }
        }

        private void resetView()
        {
            changeViewTo(false, true);
            gv.ScrollIntoView(VM.Gallery.FirstOrDefault());
            lv_Comments.ScrollIntoView(lv_Comments.Items.FirstOrDefault());
            lv_Torrents.ScrollIntoView(lv_Torrents.Items.FirstOrDefault());
            lv_Tags.ScrollIntoView(lv_Tags.Items.FirstOrDefault());
        }

        private void restoreView()
        {
            changeViewTo(isGd_InfoHideWhenLeave, true);
            var current = VM.GetCurrent();
            if(current != null)
            {
                gv.ScrollIntoView(current, ScrollIntoViewAlignment.Leading);
            }
        }

        private bool isGd_InfoHideWhenLeave;

        private bool IsGd_InfoHide
        {
            get
            {
                var fullOffset = gd_Info.ActualHeight;
                return sv_Content.VerticalOffset > fullOffset * 0.95;
            }
        }

        private void changeView(bool keep)
        {
            changeViewTo(!IsGd_InfoHide ^ keep, false);
        }

        private void changeViewTo(bool view, bool disableAnimation)
        {
            var fullOffset = gd_Info.ActualHeight;
            if(view)
                sv_Content.ChangeView(null, fullOffset, null, disableAnimation);
            else
                sv_Content.ChangeView(null, 0, null, disableAnimation);
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if(e.NavigationMode == NavigationMode.New)
            {
                pv.SelectedIndex = 0;
                needResetView = true;
            }
            VM = await GalleryVM.GetVMAsync((long)e.Parameter);
            if(e.NavigationMode == NavigationMode.Back)
            {
                entranceElement = (UIElement)gv.ContainerFromIndex(VM.CurrentIndex);
                if(entranceElement != null)
                {
                    EntranceNavigationTransitionInfo.SetIsTargetElement(entranceElement, true);
                }
            }
            await Task.Delay(20);
            switch(e.NavigationMode)
            {
            case NavigationMode.New:
                pv.Focus(FocusState.Programmatic);
                break;
            case NavigationMode.Back:
                needRestoreView = true;
                ((Control)entranceElement)?.Focus(FocusState.Programmatic);
                break;
            }
            if(needResetView)
                sv_Content.ChangeView(null, 0, null);
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            isGd_InfoHideWhenLeave = IsGd_InfoHide;
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
            RootControl.RootController.SwitchSplitView();
        }

        private EhWikiDialog ewd = new EhWikiDialog();

        private void gv_Tags_ItemClick(object sender, ItemClickEventArgs e)
        {
            var s = (ListViewBase)sender;
            var container = (SelectorItem)s.ContainerFromItem(e.ClickedItem);
            foreach(var item in mfo_Tag.Items)
            {
                item.DataContext = e.ClickedItem;
            }

            ewd.RequestedTheme = SettingCollection.Current.Theme.ToElementTheme();
            ewd.SetTag((Tag)e.ClickedItem);
            mfo_Tag.ShowAt(container);
        }

        private async void mfi_EhWiki_Click(object sender, RoutedEventArgs e)
        {
            await ewd.ShowAsync();
        }

        private void lv_Torrents_ItemClick(object sender, ItemClickEventArgs e)
        {

        }

        private async void btn_Scroll_Click(object sender, RoutedEventArgs e)
        {
            await Task.Delay(50);
            changeView(false);
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
                await Task.Delay(150);
                if(lv_Torrents.Items.Count > 0)
                {
                    lv_Torrents.SelectedIndex = -1;
                    lv_Torrents.SelectedIndex = 0;
                }
                break;
            }
        }

        public void ChangePivotSelection(int index)
        {
            pv.SelectedIndex = index;
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

        private void pv_Loaded(object sender, RoutedEventArgs e)
        {
            var sv_pv = (UIElement)VisualTreeHelperEx.GetFirstNamedChild(pv, "PivotItemPresenter");
            var sv_pv2 = (UIElement)VisualTreeHelperEx.GetFirstNamedChild(pv, "HeaderClipper");
            sv_pv.PointerWheelChanged += pv_Content_PointerWheelChanged;
            sv_pv2.PointerWheelChanged += pv_Content_PointerWheelChanged;
            pv.Loaded -= pv_Loaded;
        }

        private void gd_Info_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            bd_GvFooter.Height = e.NewSize.Height;
        }

        protected override void OnKeyUp(KeyRoutedEventArgs e)
        {
            base.OnKeyUp(e);
            e.Handled = true;
            switch(e.Key)
            {
            case VirtualKey.GamepadMenu:
            case VirtualKey.Application:
                if(cb_top.IsOpen = !cb_top.IsOpen)
                {
                    if(btn_MoreButton == null)
                        btn_MoreButton = ((Button)VisualTreeHelperEx.GetFirstNamedChild(cb_top, "MoreButton"));
                    btn_MoreButton.Focus(FocusState.Programmatic);
                }
                break;
            default:
                e.Handled = false;
                break;
            }
        }

        private Button btn_MoreButton;

        private void pv_Content_Loaded(object sender, RoutedEventArgs e)
        {
            var fe_Content = (FrameworkElement)sender;
            var bd_Content = VisualTreeHelper.GetChild(fe_Content, 0);
            var sv_Content = (ScrollViewer)VisualTreeHelper.GetChild(bd_Content, 0);
            sv_Content.ViewChanging += pv_Content_ViewChanging;
            fe_Content.Loaded -= pv_Content_Loaded;
        }

        public void CloseAll()
        {
            cb_top.IsOpen = false;
        }
    }
}
