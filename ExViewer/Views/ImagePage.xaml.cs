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
    public sealed partial class ImagePage : MyPage, IHasAppBar
    {
        public ImagePage()
        {
            this.InitializeComponent();
            this.VisibleBoundHandledByDesign = true;
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
            if (oldVM != null)
            {
                if (pageFlipView.SelectedIndex < oldVM.Gallery.Count)
                    oldVM.CurrentIndex = pageFlipView.SelectedIndex;
                else
                    oldVM.CurrentIndex = oldVM.Gallery.Count - 1;
            }
            pageFlipView.ItemsSource = null;
            if (newVM == null)
            {
                that.collectionView.Collection = null;
            }
            else
            {
                var i = newVM.CurrentIndex;
                that.collectionView.Collection = newVM.Gallery;
                pageFlipView.ItemsSource = that.collectionView;
                pageFlipView.SelectedIndex = i;
                await that.Dispatcher.RunIdleAsync(idle => pageFlipView.SelectedIndex = i);
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
            for (var i = 0; i < kfc; i++)
            {
                var c = getCbColor(backColor, needColor, (byte)(85 + i * offset));
                this.cb_top_OpenAnimation.KeyFrames[i].Value = c;
                this.cb_top_CloseAnimation.KeyFrames[kfc - 1 - i].Value = c;
            }

            base.OnNavigatedTo(e);

            this.VM = await GalleryVM.GetVMAsync((long)e.Parameter);
            this.av.VisibleBoundsChanged += this.Av_VisibleBoundsChanged;
            Av_VisibleBoundsChanged(this.av, null);
            RootControl.RootController.SetFullScreen(StatusCollection.Current.FullScreenInImagePage);
            if (SettingCollection.Current.KeepScreenOn)
            {
                this.displayRequest.RequestActive();
                this.displayActived = true;
            }
            if (!StatusCollection.Current.ImageViewTipShown)
                await showTip();
            else
                await Task.Delay(50);
            this.fv.Focus(FocusState.Programmatic);
            setScale();
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

            if (!this.cbVisible)
                changeCbVisibility();

            StatusCollection.Current.FullScreenInImagePage = this.isFullScreen ?? false;
            RootControl.RootController.SetFullScreen(false);
            if (this.displayActived)
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
            if (currentState == this.isFullScreen)
                return;
            if (currentState)
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
            foreach (var item in this.fv.Descendants<FlipViewItem>())
            {
                var inner = (Grid)item.ContentTemplateRoot;
                if (inner == null)
                    continue;
                var ip = (ImagePresenter)inner.FindName("ip");
                ip.ResetZoom(true);
            }
        }

        private async void fv_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var g = this.VM?.Gallery;
            if (g == null)
                return;
            setScale();
            if (this.fv.SelectedItem is IImagePageImageView gi && gi.Image != null)
            {

                gi.Image.LoadImageAsync(false, SettingCollection.Current.GetStrategy(), true).Completed =
                    (task, state) =>
                    {
                        if (state == AsyncStatus.Error)
                            RootControl.RootController.SendToast(task.ErrorCode, typeof(ImagePage));
                    };

            }
            var target = this.fv.SelectedIndex;
            if (target < 0)
                return;
            target += 5;
            if (target >= g.RecordCount)
                target = g.RecordCount - 1;
            if (g.Count > target)
                return;
            while (target >= g.Count && g.HasMoreItems)
            {
                await g.LoadMoreItemsAsync(5);
            }
        }

        private System.Threading.CancellationTokenSource changingCbVisibility;

        private async void fvi_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.changingCbVisibility = new System.Threading.CancellationTokenSource();
            await Task.Delay(200, this.changingCbVisibility.Token).ContinueWith(async t =>
            {
                if (t.IsCanceled)
                    return;
                await this.cb_top.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    this.changingCbVisibility.Cancel();
                    changeCbVisibility();
                });
            });
        }

        private bool cbVisible = true;

        private bool changeCbVisibility()
        {
            if (this.cbVisible)
            {
                this.cb_top_Hide.Begin();
                RootControl.RootController.SetSplitViewButtonOpacity(0.5);
                return this.cbVisible = false;
            }
            else
            {
                this.cb_top_Show.Begin();
                this.cb_top.Visibility = Visibility.Visible;
                RootControl.RootController.SetSplitViewButtonOpacity(1);
                return this.cbVisible = true;
            }
        }

        private void cb_top_Hide_Completed(object sender, object e)
        {
            if (!this.cbVisible)
                this.cb_top.Visibility = Visibility.Collapsed;
        }

        private void fvi_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (this.changingCbVisibility != null)
            {
                if (this.changingCbVisibility.IsCancellationRequested)
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

        private void cb_top_Opening(object sender, object e)
        {
            this.tb_Title.MaxLines = 0;
            Grid.SetColumn(this.tb_Title, 0);
            Grid.SetColumnSpan(this.tb_Title, 2);
            this.cb_top_Open.Begin();
        }

        private void cb_top_Closing(object sender, object e)
        {
            this.cb_top_Close.Begin();
        }

        private void cb_top_Closed(object sender, object e)
        {
            this.tb_Title.ClearValue(TextBlock.MaxLinesProperty);
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
            if (!this.enterPressed && e.Key == VirtualKey.Enter)
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
            switch (e.OriginalKey)
            {
            case VirtualKey.Enter:
                this.enterPressed = false;
                break;
            case VirtualKey.Application:
            case VirtualKey.GamepadMenu:
                if (!changeCbVisibility())
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

        private async Task showTip()
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

        private void page_Loaded(object sender, RoutedEventArgs e)
        {
            this.fv.Descendants<ScrollContentPresenter>().First().Clip = null;
        }

        private void page_Unloaded(object sender, RoutedEventArgs e)
        {
            RootControl.RootController.SplitViewButtonPlaceholderVisibilityChanged -= this.SetSplitViewButtonPlaceholderVisibility;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var vb = VisibleBoundsThickness;
            this.clipFv.Rect = new Rect(vb.Left, -vb.Top, finalSize.Width - vb.Left - vb.Right, finalSize.Height);
            return base.ArrangeOverride(finalSize);
        }

        public void SetSplitViewButtonPlaceholderVisibility(RootControl sender, bool visible)
        {
            if (visible)
                this.cdSplitViewPlaceholder.Width = new GridLength(48);
            else
                this.cdSplitViewPlaceholder.Width = new GridLength(0);
        }

        private void cb_top_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (this.cb_top.IsOpen)
                return;
            if (e.OriginalKey == VirtualKey.GamepadDPadDown || e.OriginalKey == VirtualKey.GamepadLeftThumbstickDown)
            {
                e.Handled = true;
                this.fv.Focus(FocusState.Programmatic);
            }
        }
    }
}
