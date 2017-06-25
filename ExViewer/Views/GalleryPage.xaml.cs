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
using System.Diagnostics;
using ExViewer.Controls;
using ExClient.Galleries;
using Windows.UI.Core;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace ExViewer.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class GalleryPage : MyPage, IHasAppBar
    {
        public GalleryPage()
        {
            this.InitializeComponent();
            this.VisibleBoundHandledByDesign = true;
            this.gdInfo.RegisterPropertyChangedCallback(ActualHeightProperty, this.set_btn_Scroll_Rotation);
            this.sv_Content.RegisterPropertyChangedCallback(ScrollViewer.VerticalOffsetProperty, this.set_btn_Scroll_Rotation);
        }

        private void set_btn_Scroll_Rotation(DependencyObject d, DependencyProperty dp)
        {
            var infoHeight = this.gdInfo.ActualHeight;
            if (infoHeight < 1)
                this.ct_btn_Scroll.Rotation = 0;
            else
                this.ct_btn_Scroll.Rotation = this.sv_Content.VerticalOffset / infoHeight * 180d;
        }

        private async void pv_Content_ViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {
            await Dispatcher.YieldIdle();
            if (e.NextView.VerticalOffset < e.FinalView.VerticalOffset && this.sv_Content.VerticalOffset < 1)
            {
                changeViewTo(true, false);
            }
            else if (e.IsInertial && e.NextView.VerticalOffset < 1 && this.sv_Content.VerticalOffset > this.gdInfo.ActualHeight - 1)
            {
                changeViewTo(false, false);
            }
        }

        private void pv_Header_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            var prop = e.GetCurrentPoint(this).Properties;
            if (!prop.IsHorizontalMouseWheel && prop.MouseWheelDelta != 0)
            {
                changeViewTo(prop.MouseWheelDelta < 0, false);
                e.Handled = true;
            }
        }

        private bool needResetView, needRestoreView;

        protected override async void VisibleBoundsThicknessChanged(Thickness visibleBoundsThickness)
        {
            if (this.needResetView || this.needRestoreView)
                return;
            InvalidateMeasure();
            await Dispatcher.YieldIdle();
            changeViewTo(false, true);
            await Task.Delay(33);
            changeViewTo(false, true);
        }

        private Grid gdPvContentHeaderPresenter;

        protected override Size MeasureOverride(Size availableSize)
        {
            var t = VisibleBoundsThickness;
            var height = availableSize.Height - 48 - t.Top;
            var infoH = height - t.Bottom;
            if (RootControl.RootController.InputPane.OccludedRect.Height == 0)
            {
                if (this.gdPvContentHeaderPresenter == null)
                    this.gdPvContentHeaderPresenter = this.pv.Descendants<Grid>("HeaderPresenter").FirstOrDefault();
                if (this.gdPvContentHeaderPresenter != null)
                    infoH -= this.gdPvContentHeaderPresenter.ActualHeight - 24;
            }
            this.gdInfo.MaxHeight = Math.Min(infoH, 360);
            this.gd_Pivot.Height = height;
            return base.MeasureOverride(availableSize);
        }

        private void page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.needResetView)
            {
                this.needResetView = false;
                resetView();
            }
            else if (this.needRestoreView)
            {
                this.needRestoreView = false;
                restoreView();
            }
            else
            {
                changeView(true, true);
            }
        }

        private void resetView()
        {
            changeViewTo(false, true);
            this.gv.ScrollIntoView(this.VM.Gallery.FirstOrDefault());
            this.lv_Comments.ScrollIntoView(this.lv_Comments.Items.FirstOrDefault());
            this.lv_Torrents.ScrollIntoView(this.lv_Torrents.Items.FirstOrDefault());
        }

        private void restoreView()
        {
            var current = this.VM.GetCurrent();
            if (current != null)
            {
                this.gv.ScrollIntoView(current, ScrollIntoViewAlignment.Leading);
            }
            changeViewTo(true, true);
        }

        private bool isGdInfoHide
        {
            get
            {
                var fullOffset = this.gdInfo.ActualHeight;
                return this.sv_Content.VerticalOffset > fullOffset * 0.95;
            }
        }

        private void changeViewTo(bool hideGdInfo, bool disableAnimation)
        {
            if (hideGdInfo)
                this.sv_Content.ChangeView(null, this.gdInfo.ActualHeight, null, disableAnimation);
            else
                this.sv_Content.ChangeView(null, 0, null, disableAnimation);
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var reset = e.NavigationMode == NavigationMode.New;
            var restore = e.NavigationMode == NavigationMode.Back;
            if (reset)
                this.needResetView = true;
            else if (restore)
                this.needRestoreView = true;
            this.VM = await GalleryVM.GetVMAsync((long)e.Parameter);
            Control restoreElement = null;
            var idx = this.VM.CurrentIndex;
            if (reset)
            {
                resetView();
            }
            else if (restore)
            {
                var animation = ConnectedAnimationService.GetForCurrentView().GetAnimation("ImageAnimation");
                if (animation != null)
                {
                    if (idx < this.VM.Gallery.Count)
                    {
                        await this.gv.TryStartConnectedAnimationAsync(animation, this.VM.Gallery[idx], "Image");
                    }
                    else
                        animation.Cancel();
                }
            }
            await Dispatcher.YieldIdle();
            if (reset)
            {
                this.pv.Focus(FocusState.Programmatic);
                this.pv.SelectedIndex = 0;
            }
            else if (restore)
            {
                if (restoreElement == null)
                    restoreElement = (Control)this.gv.ContainerFromIndex(this.VM.CurrentIndex);
                restoreElement?.Focus(FocusState.Programmatic);
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
        }

        private void gv_ItemClick(object sender, ItemClickEventArgs e)
        {
            this.gv.PrepareConnectedAnimation("ImageAnimation", e.ClickedItem, "Image");
            this.VM.OpenImage.Execute((GalleryImage)e.ClickedItem);
        }

        public GalleryVM VM
        {
            get => (GalleryVM)GetValue(VMProperty);
            set => SetValue(VMProperty, value);
        }

        // Using a DependencyProperty as the backing store for VM.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VMProperty =
            DependencyProperty.Register("VM", typeof(GalleryVM), typeof(GalleryPage), new PropertyMetadata(null));

        private void changeView(bool keep, bool disableAnimation)
        {
            var state = this.isGdInfoHide;
            if (!keep) state = !state;
            changeViewTo(state, disableAnimation);
        }

        private async void btn_Scroll_Click(object sender, RoutedEventArgs e)
        {
            await Dispatcher.YieldIdle();
            changeView(false, false);
        }

        private async void pv_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (this.pv.SelectedIndex)
            {
            case 1://Comments
                if (!this.VM.Gallery.Comments.IsLoaded)
                    await this.VM.LoadComments();
                break;
            case 2://Torrents
                if (this.VM.Torrents == null)
                    await this.VM.LoadTorrents();
                await Task.Delay(150);
                if (this.lv_Torrents.Items.Count > 0)
                {
                    this.lv_Torrents.SelectedIndex = -1;
                    this.lv_Torrents.SelectedIndex = 0;
                }
                break;
            }
        }

        public void ChangePivotSelection(int index)
        {
            this.pv.SelectedIndex = index;
        }

        private void lv_Torrents_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var item in e.RemovedItems)
            {
                var con = (ListViewItem)this.lv_Torrents.ContainerFromItem(item);
                if (con == null)
                    continue;
                var gd = (FrameworkElement)((FrameworkElement)con.ContentTemplateRoot).FindName("gd_TorrentDetail");
                gd.Visibility = Visibility.Collapsed;
            }
            var added = e.AddedItems.FirstOrDefault();
            if (added != null)
            {
                var con = (ListViewItem)this.lv_Torrents.ContainerFromItem(added);
                if (con == null)
                    return;
                var gd = (FrameworkElement)((FrameworkElement)con.ContentTemplateRoot).FindName("gd_TorrentDetail");
                gd.Visibility = Visibility.Visible;
            }
        }

        private void pv_Loaded(object sender, RoutedEventArgs e)
        {
            var sv_pv = this.pv.Descendants("PivotItemPresenter").First();
            var sv_pv2 = this.pv.Descendants("HeaderClipper").First();
            sv_pv.PointerWheelChanged += this.pv_Header_PointerWheelChanged;
            sv_pv2.PointerWheelChanged += this.pv_Header_PointerWheelChanged;
            this.pv.Loaded -= this.pv_Loaded;
        }

        protected override void OnKeyUp(KeyRoutedEventArgs e)
        {
            base.OnKeyUp(e);
            e.Handled = true;
            switch (e.Key)
            {

            case VirtualKey.GamepadY:
                this.changeViewTo(false, false);
                this.tpTags.Focus(FocusState.Keyboard);
                break;
            case VirtualKey.GamepadMenu:
            case VirtualKey.Application:
                if (this.cb_top.IsOpen = !this.cb_top.IsOpen)
                {
                    if (this.btn_MoreButton == null)
                        this.btn_MoreButton = this.cb_top.Descendants<Button>("MoreButton").FirstOrDefault();
                    this.btn_MoreButton?.Focus(FocusState.Programmatic);
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
            sv_Content.ViewChanging += this.pv_Content_ViewChanging;
            fe_Content.Loaded -= this.pv_Content_Loaded;
        }

        public void CloseAll()
        {
            this.cb_top.IsOpen = false;
        }

        private void page_Loading(FrameworkElement sender, object args)
        {
            this.SetSplitViewButtonPlaceholderVisibility(null, RootControl.RootController.SplitViewButtonPlaceholderVisibility);
            RootControl.RootController.SplitViewButtonPlaceholderVisibilityChanged += this.SetSplitViewButtonPlaceholderVisibility;
        }

        private void page_Unloaded(object sender, RoutedEventArgs e)
        {
            RootControl.RootController.SplitViewButtonPlaceholderVisibilityChanged -= this.SetSplitViewButtonPlaceholderVisibility;
        }

        private AddToFavoritesDialog addToFavorites;

        private async void abbFavorites_Click(object sender, RoutedEventArgs e)
        {
            var addToFavorites = System.Threading.LazyInitializer.EnsureInitialized(ref this.addToFavorites);
            addToFavorites.Gallery = this.VM.Gallery;
            await addToFavorites.ShowAsync();
        }

        private void cb_top_Opening(object sender, object e)
        {
            this.tbGalleryName.MaxLines = 0;
            Grid.SetColumn(this.tbGalleryName, 0);
        }

        private void cb_top_Closed(object sender, object e)
        {
            this.tbGalleryName.ClearValue(TextBlock.MaxLinesProperty);
            Grid.SetColumn(this.tbGalleryName, 1);
        }

        public void SetSplitViewButtonPlaceholderVisibility(RootControl sender, bool visible)
        {
            if (visible)
                this.cdSplitViewPlaceholder.Width = new GridLength(48);
            else
                this.cdSplitViewPlaceholder.Width = new GridLength(0);
        }
    }

    class FavoriteCategoryToNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var cat = (FavoriteCategory)value;
            if (cat == null || cat.Index < 0)
                return Strings.Resources.Views.GalleryPage.FavoritesAppBarButton.Text;
            return cat.Name;

        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
