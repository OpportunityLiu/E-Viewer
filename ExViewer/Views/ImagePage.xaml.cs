using ExViewer.Controls;
using ExViewer.Settings;
using ExViewer.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.System;
using Windows.System.Display;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Color = Windows.UI.Color;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace ExViewer.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary> 
    public sealed partial class ImagePage : Page, IHasAppBar
    {
        public ImagePage()
        {
            this.InitializeComponent();
        }

        public GalleryVM VM
        {
            get => (GalleryVM)GetValue(VMProperty);
            set => SetValue(VMProperty, value);
        }

        // Using a DependencyProperty as the backing store for VM.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VMProperty =
            DependencyProperty.Register("VM", typeof(GalleryVM), typeof(ImagePage), new PropertyMetadata(null, VMPropertyChangedCallback));

        private ImagePageCollectionView collectionView = new ImagePageCollectionView();

        private static async void VMPropertyChangedCallback(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            var that = (ImagePage)dp;
            var pageFlipView = that.fv;
            var oldVM = (GalleryVM)e.OldValue;
            var newVM = (GalleryVM)e.NewValue;
            if(oldVM != null)
            {
                if(pageFlipView.SelectedIndex < oldVM.Gallery.Count)
                    oldVM.CurrentIndex = pageFlipView.SelectedIndex;
                else
                    oldVM.CurrentIndex = oldVM.Gallery.Count - 1;
            }
            pageFlipView.ItemsSource = null;
            if(newVM == null)
            {
                that.collectionView.Collection = null;
            }
            else
            {
                that.collectionView.Collection = newVM.Gallery;
                pageFlipView.ItemsSource = that.collectionView;
                pageFlipView.SelectedIndex = newVM.CurrentIndex;
                await Task.Delay(50);
                pageFlipView.SelectedIndex = newVM.CurrentIndex;
            }
        }

        private readonly ApplicationView av = ApplicationView.GetForCurrentView();
        private readonly DisplayRequest displayRequest = new DisplayRequest();
        private bool displayActived;

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            var backColor = this.scbBack.Color;
            var needColor = this.scbNeed.Color;

            var toColor = getCbColor(backColor, needColor, 85);
            this.cb_top.Background = new SolidColorBrush(toColor);
            this.fv.FlowDirection = SettingCollection.Current.ReverseFlowDirection ?
                FlowDirection.RightToLeft : FlowDirection.LeftToRight;

            var kfc = this.cb_top_CloseAnimation.KeyFrames.Count;
            var offset = (255d - 85d) / (kfc - 1);
            for(var i = 0; i < kfc; i++)
            {
                var c = getCbColor(backColor, needColor, (byte)(85 + i * offset));
                this.cb_top_OpenAnimation.KeyFrames[i].Value = c;
                this.cb_top_CloseAnimation.KeyFrames[kfc - 1 - i].Value = c;
            }

            base.OnNavigatedTo(e);

            this.cb_top.Visibility = Visibility.Visible;
            this.VM = await GalleryVM.GetVMAsync((long)e.Parameter);
            this.av.VisibleBoundsChanged += this.Av_VisibleBoundsChanged;
            Av_VisibleBoundsChanged(this.av, null);
            this.fv.Focus(FocusState.Pointer);
            RootControl.RootController.SetFullScreen(StatusCollection.Current.FullScreenInImagePage);
            if(SettingCollection.Current.KeepScreenOn)
            {
                this.displayRequest.RequestActive();
                this.displayActived = true;
            }
            if(!StatusCollection.Current.ImageViewTipShown)
                showTip();
        }

        private static Color getCbColor(Color backColor, Color needColor, byte alpha)
        {
            var ratio = alpha / 255d;
            var ratio_1 = ratio - 1;
            return Color.FromArgb(alpha,
                (byte)((needColor.R + ratio_1 * backColor.R) / ratio),
                (byte)((needColor.G + ratio_1 * backColor.G) / ratio),
                (byte)((needColor.B + ratio_1 * backColor.B) / ratio));
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            this.VM = null;

            StatusCollection.Current.FullScreenInImagePage = this.isFullScreen ?? false;
            RootControl.RootController.SetFullScreen(false);
            if(this.displayActived)
            {
                this.displayRequest.RequestRelease();
                this.displayActived = false;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            this.av.VisibleBoundsChanged -= this.Av_VisibleBoundsChanged;
        }

        bool? isFullScreen;

        private void Av_VisibleBoundsChanged(ApplicationView sender, object args)
        {
            var currentState = RootControl.RootController.IsFullScreen;
            if(currentState == this.isFullScreen)
                return;
            if(currentState)
            {
                this.abb_fullScreen.Icon = new SymbolIcon(Symbol.BackToWindow);
                this.abb_fullScreen.Label = Strings.Resources.Views.ImagePage.BackToWindow;
            }
            else
            {
                this.abb_fullScreen.Icon = new SymbolIcon(Symbol.FullScreen);
                this.abb_fullScreen.Label = Strings.Resources.Views.ImagePage.FullScreen;
            }
            this.isFullScreen = currentState;
        }

        private void setScale()
        {
            var lb = this.fv.SelectedIndex - 1;
            var ub = this.fv.SelectedIndex + 2;
            lb = lb < 0 ? 0 : lb;
            ub = ub > this.VM.Gallery.Count ? this.VM.Gallery.Count : ub;
            for(var i = lb; i < ub; i++)
            {
                if(i == this.fv.SelectedIndex)
                    continue;
                var selected = (FlipViewItem)this.fv.ContainerFromIndex(i);
                if(selected == null)
                    continue;
                var inner = (Grid)selected.ContentTemplateRoot;
                if(inner == null)
                    continue;
                var ip = (ImagePresenter)inner.FindName("ip");
                ip.ResetZoom();
            }
        }

        IAsyncOperation<LoadMoreItemsResult> loadItems;

        private void fv_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(this.VM?.Gallery == null)
                return;
            var start = this.fv.SelectedIndex;
            if(start < 0)
                return;
            var end = start + 5;
            if(end > this.VM.Gallery.Count)
            {
                end = this.VM.Gallery.Count;
            }
            for(var i = start; i < end; i++)
            {
                this.VM.Gallery[i].LoadImageAsync(false, SettingCollection.Current.GetStrategy(), true).Completed =
                    (task, state) =>
                    {
                        if(state == AsyncStatus.Error)
                            RootControl.RootController.SendToast(task.ErrorCode, typeof(ImagePage));
                    };
            }
            if(end + 10 > this.VM.Gallery.Count && this.VM.Gallery.HasMoreItems)
            {
                if(this.loadItems == null || this.loadItems.Status != AsyncStatus.Started)
                {
                    this.loadItems = this.VM.Gallery.LoadMoreItemsAsync(5);
                }
            }
            setScale();
        }

        private System.Threading.CancellationTokenSource changingCbVisibility;

        private async void fvi_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.changingCbVisibility = new System.Threading.CancellationTokenSource();
            await Task.Delay(150, this.changingCbVisibility.Token).ContinueWith(async t =>
            {
                if(t.IsCanceled)
                    return;
                await this.cb_top.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    this.changingCbVisibility.Cancel();
                    changeCbVisibility();
                });
            });
        }

        private bool changeCbVisibility()
        {
            switch(this.cb_top.Visibility)
            {
                case Visibility.Visible:
                    this.cb_top.Visibility = Visibility.Collapsed;
                    return false;
                case Visibility.Collapsed:
                    this.cb_top.Visibility = Visibility.Visible;
                    return true;
            }
            return false;
        }

        private void fvi_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if(this.changingCbVisibility != null)
            {
                if(this.changingCbVisibility.IsCancellationRequested)
                    changeCbVisibility();
                else
                    this.changingCbVisibility.Cancel();
            }
            e.Handled = true;
        }

        private async void Flyout_Opening(object sender, object e)
        {
            this.VM.CurrentIndex = this.fv.SelectedIndex;
            await this.VM.RefreshInfoAsync();
        }

        private async void cb_top_Opening(object sender, object e)
        {
            this.cb_top_Open.Begin();
            await Task.Delay(100);
            Grid.SetColumn(this.tb_Title, 0);
            Grid.SetColumnSpan(this.tb_Title, 2);
        }

        private void cb_top_Closing(object sender, object e)
        {
            this.cb_top_Close.Begin();
        }

        private void cb_top_Closed(object sender, object e)
        {
            Grid.SetColumn(this.tb_Title, 1);
            Grid.SetColumnSpan(this.tb_Title, 1);
        }

        private void abb_fullScreen_Click(object sender, RoutedEventArgs e)
        {
            RootControl.RootController.ChangeFullScreen();
        }

        protected override void OnKeyDown(KeyRoutedEventArgs e)
        {
            base.OnKeyDown(e);
            if(!this.enterPressed && e.Key == VirtualKey.Enter)
            {
                RootControl.RootController.ChangeFullScreen();
                this.enterPressed = true;
                e.Handled = true;
            }
        }

        protected override void OnKeyUp(KeyRoutedEventArgs e)
        {
            base.OnKeyUp(e);
            e.Handled = true;
            switch(e.Key)
            {
                case VirtualKey.Enter:
                    this.enterPressed = false;
                    break;
                case VirtualKey.Application:
                case VirtualKey.GamepadMenu:
                    if(!changeCbVisibility())
                        this.fv.Focus(FocusState.Programmatic);
                    else
                        this.abb_fullScreen.Focus(FocusState.Programmatic);
                    break;
                default:
                    e.Handled = false;
                    break;
            }
        }

        private bool enterPressed;

        private void abb_Help_Click(object sender, RoutedEventArgs e)
        {
            showTip();
        }

        private async void showTip()
        {
            await new ContentDialog
            {
                Title = Strings.Resources.Views.ImagePage.ImageViewTipsTitle,
                Content = Strings.Resources.Views.ImagePage.ImageViewTipsContent,
                PrimaryButtonText = Strings.Resources.OK,
                RequestedTheme = SettingCollection.Current.Theme.ToElementTheme()
            }.ShowAsync();
            StatusCollection.Current.ImageViewTipShown = true;
        }

        public void CloseAll()
        {
            this.cb_top.IsOpen = false;
        }

        public void SetImageIndex(int value)
        {
            this.fv.SelectedIndex = value;
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

        public void SetSplitViewButtonPlaceholderVisibility(RootControl sender, bool visible)
        {
            if(visible)
                this.cdSplitViewPlaceholder.Width = new GridLength(48);
            else
                this.cdSplitViewPlaceholder.Width = new GridLength(0);
        }
    }
}
