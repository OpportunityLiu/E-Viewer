using ExClient.Galleries;
using ExViewer.Controls;
using ExViewer.Settings;
using ExViewer.ViewModels;
using Opportunity.Helpers.Universal.AsyncHelpers;
using Opportunity.MvvmUniverse.Views;
using System;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.System;
using Windows.System.Display;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Color = Windows.UI.Color;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace ExViewer.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary> 
    public sealed partial class ImagePage : MyPage, IHasAppBar, INavigationHandler
    {
        public ImagePage()
        {
            this.InitializeComponent();
            this.fv.Opacity = 0;
            this.fv.IsEnabled = false;
            this.VisibleBoundHandledByDesign = true;
        }

        public GalleryVM VM
        {
            get => (GalleryVM)GetValue(VMProperty);
            set => SetValue(VMProperty, value);
        }

        // Using a DependencyProperty as the backing store for VM.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VMProperty =
            DependencyProperty.Register("VM", typeof(GalleryVM), typeof(ImagePage), new PropertyMetadata(null));

        private readonly ApplicationView av = ApplicationView.GetForCurrentView();
        private readonly DisplayRequest displayRequest = new DisplayRequest();
        private bool displayActived;

        private Color currentBackColor, currentNeedColor;

        private void setCbColor()
        {
            var backColor = this.scbBack.Color;
            var needColor = this.scbNeed.Color;
            if (this.currentBackColor == backColor && this.currentNeedColor == needColor)
                return;
            this.currentBackColor = backColor;
            this.currentNeedColor = needColor;

            Color getCbColor(byte alpha)
            {
                var ratio = alpha / 255d;
                var ratio_1 = ratio - 1;
                return Color.FromArgb(alpha,
                    (byte)((needColor.R + ratio_1 * backColor.R) / ratio),
                    (byte)((needColor.G + ratio_1 * backColor.G) / ratio),
                    (byte)((needColor.B + ratio_1 * backColor.B) / ratio));
            }

            var toColor = getCbColor(85);
            this.cb_top.Background = new SolidColorBrush(toColor);
            this.fv.FlowDirection = SettingCollection.Current.ReverseFlowDirection ?
                FlowDirection.RightToLeft : FlowDirection.LeftToRight;

            var kfc = this.cb_top_CloseAnimation.KeyFrames.Count;
            var offset = (255d - 85d) / (kfc - 1);
            for (var i = 0; i < kfc; i++)
            {
                var c = getCbColor((byte)(85 + i * offset));
                this.cb_top_OpenAnimation.KeyFrames[i].Value = c;
                this.cb_top_CloseAnimation.KeyFrames[kfc - 1 - i].Value = c;
            }
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            Navigator.GetForCurrentView().Handlers.Add(this);
            setCbColor();
            Av_VisibleBoundsChanged(this.av, null);
            this.av.VisibleBoundsChanged += this.Av_VisibleBoundsChanged;
            if (SettingCollection.Current.KeepScreenOn)
            {
                this.displayRequest.RequestActive();
                this.displayActived = true;
            }

            base.OnNavigatedTo(e);

            this.VM = GalleryVM.GetVM((long)e.Parameter);

            var index = this.VM.View.CurrentPosition;
            if (index < 0)
                index = 0;
            this.fv.ItemsSource = this.VM.View;
            this.fv.SelectedIndex = index;
            this.imgConnect.Source = this.VM.Gallery[index].Thumb;
            var animation = ConnectedAnimationService.GetForCurrentView().GetAnimation("ImageAnimation");
            var animationSucceed = false;
            if (animation != null)
            {
                animationSucceed = animation.TryStart(this.imgConnect, new UIElement[]
                {
                    this.cb_top, this.bdLeft, this.bdRight, this.bdTop
                });
                if (animationSucceed)
                    animation.Completed += this.Animation_Completed;
            }

            await Dispatcher.YieldIdle();
            setScale();
            if (!animationSucceed)
                Animation_Completed(null, null);
            RootControl.RootController.SetFullScreen(StatusCollection.Current.FullScreenInImagePage);
        }

        private void Animation_Completed(ConnectedAnimation sender, object args)
        {
            this.imgConnect.Visibility = Visibility.Collapsed;
            this.imgConnect.Source = null;
            this.fv.IsEnabled = true;
            this.fv.Opacity = 1;
            this.fv.Focus(FocusState.Programmatic);
            this.fv.SelectedIndex = VM.View.CurrentPosition;
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            this.GetNavigator().Handlers.Remove(this);

            base.OnNavigatingFrom(e);

            if (!this.cbVisible)
                changeCbVisibility();

            this.av.VisibleBoundsChanged -= this.Av_VisibleBoundsChanged;
            if (this.displayActived)
            {
                this.displayRequest.RequestRelease();
                this.displayActived = false;
            }

            var container = this.fv.ContainerFromIndex(this.fv.SelectedIndex);
            if (container != null)
                ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("ImageAnimation", container.Descendants<Image>().First());
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.fv.Opacity = 0;
            this.fv.IsEnabled = false;
            this.imgConnect.Visibility = Visibility.Visible;
            base.OnNavigatedFrom(e);
            CloseAll();
            cb_top_Closed(this.cb_top, null);
        }

        void INavigationHandler.OnAdd(Navigator navigator) { }
        void INavigationHandler.OnRemove() { }

        bool INavigationHandler.CanGoBack => false;
        IAsyncOperation<bool> INavigationHandler.GoBackAsync()
        {
            StatusCollection.Current.FullScreenInImagePage = this.isFullScreen ?? false;
            RootControl.RootController.SetFullScreen(false);
            return AsyncInfo.Run(async token =>
            {
                await Task.Delay(100);
                return false;
            });
        }
        bool INavigationHandler.CanGoForward => false;
        IAsyncOperation<bool> INavigationHandler.GoForwardAsync()
        {
            StatusCollection.Current.FullScreenInImagePage = this.isFullScreen ?? false;
            RootControl.RootController.SetFullScreen(false);
            return AsyncOperation<bool>.CreateCompleted(false);
        }
        IAsyncOperation<bool> INavigationHandler.NavigateAsync(Type sourcePageType, object parameter)
        {
            StatusCollection.Current.FullScreenInImagePage = this.isFullScreen ?? false;
            RootControl.RootController.SetFullScreen(false);
            return AsyncOperation<bool>.CreateCompleted(false);
        }

        // null fo unknown;
        // tracked by Av_VisibleBoundsChanged
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

        private void fv_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var index = this.fv.SelectedIndex;
            if (index < 0)
                return;
            var g = this.VM?.Gallery;
            if (g == null)
                return;
            setScale();
        }

        private System.Threading.CancellationTokenSource changingCbVisibility;

        private static UISettings uiSettings = new UISettings();

        private async void fvi_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.changingCbVisibility = new System.Threading.CancellationTokenSource();
            await Task.Delay((int)uiSettings.DoubleClickTime, this.changingCbVisibility.Token).ContinueWith(async t =>
            {
                if (t.IsCanceled)
                    return;
                await Dispatcher.Yield();
                this.changingCbVisibility.Cancel();
                changeCbVisibility();
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
            {
                this.cb_top.IsOpen = false;
                this.cb_top.Visibility = Visibility.Collapsed;
            }
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
            await this.VM.RefreshInfoAsync();
        }

        private async void cb_top_Opening(object sender, object e)
        {
            this.tb_Title.MaxLines = 0;
            Grid.SetColumn(this.tb_Title, 0);
            Grid.SetColumnSpan(this.tb_Title, 2);
            this.cb_top_Open.Begin();
            await this.VM.RefreshInfoAsync();
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

        public void CloseAll()
        {
            this.cb_top.IsOpen = false;
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

        private static readonly Brush pbLoading = (Brush)Application.Current.Resources["SystemControlForegroundAccentBrush"];
        private static readonly Brush pbFailed = new SolidColorBrush(Colors.Red);

        private static Brush loadStateToPbForeground(ImageLoadingState state)
        {
            if (state == ImageLoadingState.Failed)
                return pbFailed;
            else
                return pbLoading;
        }

        private static bool loadStateToPbIsIndeterminate(ImageLoadingState state)
        {
            if (state == ImageLoadingState.Waiting || state == ImageLoadingState.Preparing)
                return true;
            else
                return false;
        }

        private static Visibility loadStateToPbVisibility(ImageLoadingState state)
        {
            if (state == ImageLoadingState.Loaded)
                return Visibility.Collapsed;
            else
                return Visibility.Visible;
        }

        private static GalleryImage loadOriginalCommandParameter(GalleryImage image, bool originalLoaded) => image;
    }
}
