using ExClient.Galleries;
using ExViewer.Controls;
using ExViewer.Settings;
using ExViewer.ViewModels;
using Opportunity.Helpers.Universal.AsyncHelpers;
using Opportunity.MvvmUniverse;
using Opportunity.MvvmUniverse.Services;
using Opportunity.MvvmUniverse.Services.Navigation;
using Opportunity.MvvmUniverse.Views;
using System;
using System.Diagnostics;
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
    public sealed partial class ImagePage : MvvmPage, IHasAppBar, INavigationHandler
    {
        public ImagePage()
        {
            InitializeComponent();
            fv.Opacity = 0;
            fv.IsEnabled = false;
            sldIndex.AddHandler(PointerReleasedEvent, new PointerEventHandler(SldIndex_PointerReleased), true);
            _SlideTimer.Tick += _SlideTimer_Tick;
        }

        public new GalleryVM ViewModel
        {
            get => (GalleryVM)base.ViewModel;
            set => base.ViewModel = value;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            Debug.Assert(e.Parameter != null, "e.Parameter != null");
            Navigator.GetForCurrentView().Handlers.Add(this);
            tapToFlip = SettingCollection.Current.TapToFlip;
            _SlideTimer.Interval = TimeSpan.FromSeconds(SettingCollection.Current.SlideInterval);

            base.OnNavigatedTo(e);

            ViewModel = GalleryVM.GetVM((long)e.Parameter);

            var index = ViewModel.View.CurrentPosition;
            if (index < 0)
            {
                index = 0;
            }

            imgConnect.Source = ViewModel.Gallery[index].Thumb;
            var animation = ConnectedAnimationService.GetForCurrentView().GetAnimation("ImageAnimation");
            var animationSucceed = false;
            if (animation != null)
            {
                animationSucceed = animation.TryStart(imgConnect, new UIElement[]
                {
                    cbTop, bdTopLeft, bdTopRight, bdTop
                });
                if (animationSucceed)
                {
                    animation.Completed += Animation_Completed;
                }
            }

            await Dispatcher.YieldIdle();
            fv.ItemsSource = ViewModel.View;
            fv.SelectedIndex = index;
            setScale();
            if (!animationSucceed)
            {
                await Dispatcher.YieldIdle();
                Animation_Completed(null, null);
            }
            RootControl.RootController.SetFullScreen(StatusCollection.Current.FullScreenInImagePage);
        }

        private void Animation_Completed(ConnectedAnimation sender, object args)
        {
            imgConnect.Visibility = Visibility.Collapsed;
            imgConnect.Source = null;
            fv.ItemsSource = ViewModel.View;
            ViewModel.View.IsCurrentPositionLocked = false;
            fv.IsEnabled = true;
            fv.Opacity = 1;
            fv.Focus(FocusState.Programmatic);
            fv.SelectedIndex = ViewModel.View.CurrentPosition;
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            Navigator.GetForCurrentView().Handlers.Remove(this);
            base.OnNavigatingFrom(e);
            ViewModel.View.IsCurrentPositionLocked = true;

            if (!cbVisible)
            {
                changeCbVisibility();
            }

            var container = fv.ContainerFromIndex(fv.SelectedIndex);
            if (container != null)
            {
                ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("ImageAnimation", container.Descendants<Image>().First());
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            fv.Opacity = 0;
            fv.IsEnabled = false;
            _SlideTimer.IsEnabled = false;
            imgConnect.Visibility = Visibility.Visible;
            base.OnNavigatedFrom(e);
            CloseAll();
        }

        void IServiceHandler<Navigator>.OnAdd(Navigator navigator) { }
        void IServiceHandler<Navigator>.OnRemove(Navigator navigator) { }

        bool INavigationHandler.CanGoBack => false;
        IAsyncOperation<bool> INavigationHandler.GoBackAsync()
        {
            StatusCollection.Current.FullScreenInImagePage = isFullScreen ?? false;
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
            StatusCollection.Current.FullScreenInImagePage = isFullScreen ?? false;
            RootControl.RootController.SetFullScreen(false);
            return AsyncOperation<bool>.CreateCompleted(false);
        }
        IAsyncOperation<bool> INavigationHandler.NavigateAsync(Type sourcePageType, object parameter)
        {
            StatusCollection.Current.FullScreenInImagePage = isFullScreen ?? false;
            RootControl.RootController.SetFullScreen(false);
            return AsyncOperation<bool>.CreateCompleted(false);
        }

        // null fo unknown;
        // tracked by Av_VisibleBoundsChanged
        private bool? isFullScreen;
        private void Av_VisibleBoundsChanged(ApplicationView sender, object args)
        {
            var currentState = RootControl.RootController.IsFullScreen;
            if (currentState == isFullScreen)
            {
                return;
            }

            if (currentState)
            {
                abb_fullScreen.Icon = new SymbolIcon(Symbol.BackToWindow);
                abb_fullScreen.Label = Strings.Resources.Views.ImagePage.BackToWindow;
            }
            else
            {
                abb_fullScreen.Icon = new SymbolIcon(Symbol.FullScreen);
                abb_fullScreen.Label = Strings.Resources.Views.ImagePage.FullScreen;
            }
            isFullScreen = currentState;
        }

        private void setScale()
        {
            foreach (var item in fv.Descendants<ImagePresenter>())
            {
                item.ResetZoom(true);
            }
        }

        private void fv_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var index = fv.SelectedIndex;
            if (index < 0)
                return;

            var g = ViewModel?.Gallery;
            if (g is null)
                return;

            _SlideTimer.Reset();

            setScale();
        }


        private static readonly UISettings uiSettings = new UISettings();
        private bool tapToFlip;
        private System.Threading.CancellationTokenSource doubleTapToken;

        private async void fvi_Tapped(object sender, TappedRoutedEventArgs e)
        {
            doubleTapToken = new System.Threading.CancellationTokenSource();
            await Task.Delay((int)uiSettings.DoubleClickTime, doubleTapToken.Token);
            if (doubleTapToken.IsCancellationRequested)
                return;

            doubleTapToken.Cancel();

            var handled = false;
            if (tapToFlip && e.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch)
            {
                var orient = (fv.ItemsPanelRoot is VirtualizingStackPanel panel) ? panel.Orientation : Orientation.Horizontal;
                var point = e.GetPosition(fv);
                if (orient == Orientation.Horizontal)
                {
                    if (point.X < fv.ActualWidth * 0.3)
                    {
                        if (ViewModel.View.CurrentPosition != 0)
                            handled = ViewModel.View.MoveCurrentToPrevious();
                    }
                    else if (point.X > fv.ActualWidth * 0.7)
                    {
                        if (ViewModel.View.CurrentPosition != ViewModel.View.Count - 1)
                            handled = ViewModel.View.MoveCurrentToNext();
                    }
                }
                else
                {
                    if (point.Y < fv.ActualHeight * 0.3)
                    {
                        if (ViewModel.View.CurrentPosition != 0)
                            handled = ViewModel.View.MoveCurrentToPrevious();
                    }
                    else if (point.Y > fv.ActualHeight * 0.7)
                    {
                        if (ViewModel.View.CurrentPosition != ViewModel.View.Count - 1)
                            handled = ViewModel.View.MoveCurrentToNext();
                    }
                }
            }
            if (!handled)
                changeCbVisibility();
        }

        private void fvi_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (doubleTapToken != null)
            {
                doubleTapToken.Cancel();
                e.Handled = true;
            }
        }

        private bool cbVisible = true;

        private bool changeCbVisibility()
        {
            if (cbVisible)
            {
                cbTop_Hide.Begin();
                RootControl.RootController.SetSplitViewButtonOpacity(0.5);
                return cbVisible = false;
            }
            else
            {
                cbTop_Show.Begin();
                cbTop.Visibility = Visibility.Visible;
                RootControl.RootController.SetSplitViewButtonOpacity(1);
                return cbVisible = true;
            }
        }

        private void cbTop_Hide_Completed(object sender, object e)
        {
            if (!cbVisible)
            {
                cbTop.IsOpen = false;
                cbTop.Visibility = Visibility.Collapsed;
            }
        }

        private async void Flyout_Opening(object sender, object e)
        {
            await ViewModel.RefreshInfoAsync();
        }

        private async void cbTop_Opening(object sender, object e)
        {
            tb_Title.MaxLines = 0;
            Grid.SetColumn(tb_Title, 0);
            Grid.SetColumnSpan(tb_Title, 2);
            cbTop_Open.Begin();
            await ViewModel.RefreshInfoAsync();
        }

        private void cbTop_Closing(object sender, object e)
        {
            cbTop_Close.Begin();
        }

        private Thickness gdCbContentPadding(double minHeight)
        {
            var tb = (minHeight - 20) / 2;
            return new Thickness(0, tb, 0, tb);
        }

        private void cbTop_Closed(object sender, object e)
        {
            tb_Title.ClearValue(TextBlock.MaxLinesProperty);
            Grid.SetColumn(tb_Title, 1);
            Grid.SetColumnSpan(tb_Title, 1);
        }

        private void abb_fullScreen_Click(object sender, RoutedEventArgs e)
        {
            RootControl.RootController.ChangeFullScreen();
        }

        protected override void OnKeyDown(KeyRoutedEventArgs e)
        {
            base.OnKeyDown(e);
            if (!enterPressed && e.Key == VirtualKey.Enter)
            {
                RootControl.RootController.ChangeFullScreen();
                enterPressed = true;
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
                enterPressed = false;
                break;
            case VirtualKey.Application:
            case VirtualKey.GamepadMenu:
                if (!changeCbVisibility())
                {
                    fv.Focus(FocusState.Programmatic);
                }
                else
                {
                    cbTop.Focus(FocusState.Programmatic);
                }

                break;
            default:
                e.Handled = false;
                break;
            }
        }

        private bool enterPressed;

        public void CloseAll()
        {
            cbTop.IsOpen = false;
        }

        private readonly DisplayRequest displayRequest = new DisplayRequest();
        private bool displayActived;

        private Color currentBackColor, currentNeedColor;

        private void setCbColor()
        {
            var backColor = scbBack.Color;
            var needColor = scbNeed.Color;
            if (currentBackColor == backColor && currentNeedColor == needColor)
            {
                return;
            }

            currentBackColor = backColor;
            currentNeedColor = needColor;

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
            cbTop.Background = new SolidColorBrush(toColor);

            var kfc = cbTop_CloseAnimation.KeyFrames.Count;
            var offset = (255d - 85d) / (kfc - 1);
            for (var i = 0; i < kfc; i++)
            {
                var c = getCbColor((byte)(85 + i * offset));
                cbTop_OpenAnimation.KeyFrames[i].Value = c;
                cbTop_CloseAnimation.KeyFrames[kfc - 1 - i].Value = c;
            }
        }

        private void page_Loading(FrameworkElement sender, object args)
        {
            setCbColor();
            SetSplitViewButtonPlaceholderVisibility(null, RootControl.RootController.SplitViewButtonPlaceholderVisibility);
            RootControl.RootController.SplitViewButtonPlaceholderVisibilityChanged += SetSplitViewButtonPlaceholderVisibility;
            var av = ApplicationView.GetForCurrentView();
            Av_VisibleBoundsChanged(av, null);
            av.VisibleBoundsChanged += Av_VisibleBoundsChanged;
            if (SettingCollection.Current.KeepScreenOn)
            {
                displayRequest.RequestActive();
                displayActived = true;
            }
        }

        private void page_Loaded(object sender, RoutedEventArgs e)
        {
            fv.Descendants<ScrollContentPresenter>().First().Clip = null;
            fv.FlowDirection = SettingCollection.Current.ReverseFlowDirection ?
                FlowDirection.RightToLeft : FlowDirection.LeftToRight;
            if (!(this.fv.ItemsPanelRoot is VirtualizingStackPanel panel))
            {
                return;
            }

            switch (SettingCollection.Current.ImageViewOrientation)
            {
            case ViewOrientation.Vertical:
                panel.Orientation = Orientation.Vertical;
                break;
            case ViewOrientation.Auto:
                if (ViewModel.Gallery.Tags[ExClient.Tagging.Namespace.Misc].Any(t => t.Content.Content == "webtoon"))
                {
                    panel.Orientation = Orientation.Vertical;
                }
                else
                {
                    panel.Orientation = Orientation.Horizontal;
                }

                break;
            default:
                panel.Orientation = Orientation.Horizontal;
                break;
            }
        }

        private void page_Unloaded(object sender, RoutedEventArgs e)
        {
            var av = ApplicationView.GetForCurrentView();
            av.VisibleBoundsChanged -= Av_VisibleBoundsChanged;
            RootControl.RootController.SplitViewButtonPlaceholderVisibilityChanged -= SetSplitViewButtonPlaceholderVisibility;
            if (displayActived)
            {
                displayRequest.RequestRelease();
                displayActived = false;
            }
        }

        public void SetSplitViewButtonPlaceholderVisibility(RootControl sender, bool visible)
        {
            cdSplitViewPlaceholder.Width = visible ? new GridLength(48) : new GridLength(0);
        }

        private void cbTop_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (cbTop.IsOpen)
            {
                return;
            }

            if (e.OriginalKey == VirtualKey.GamepadDPadDown || e.OriginalKey == VirtualKey.GamepadLeftThumbstickDown)
            {
                e.Handled = true;
                fv.Focus(FocusState.Programmatic);
            }
        }

        private static readonly Brush pbLoading = (Brush)Application.Current.Resources["SystemControlForegroundAccentBrush"];
        private static readonly Brush pbFailed = new SolidColorBrush(Colors.Red);

        private static Brush loadStateToPbForeground(ImageLoadingState state)
        {
            if (state == ImageLoadingState.Failed)
            {
                return pbFailed;
            }
            else
            {
                return pbLoading;
            }
        }

        private static bool loadStateToPbIsIndeterminate(ImageLoadingState state)
        {
            if (state == ImageLoadingState.Waiting || state == ImageLoadingState.Preparing)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static Visibility loadStateToPbVisibility(ImageLoadingState state)
        {
            if (state == ImageLoadingState.Loaded)
            {
                return Visibility.Collapsed;
            }
            else
            {
                return Visibility.Visible;
            }
        }

        private static GalleryImage loadOriginalCommandParameter(GalleryImage image, bool originalLoaded) => image;

        private void SldIndex_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            fv.SelectedIndex = (int)sldIndex.Value - 1;
        }

        private void SldIndex_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            fv.SelectedIndex = (int)sldIndex.Value - 1;
        }

        private static double getIndexIndicatorWidth(int count)
        {
            var n = Math.Ceiling(Math.Log10(count + 0.5));
            return (n * 2 + 3) * 8 + 16;
        }

        private void _AbbSlide_Click(object sender, RoutedEventArgs e)
        {
            _SlideTimer.IsEnabled = !_SlideTimer.IsEnabled;
        }

        private readonly ObservableTimer _SlideTimer = new ObservableTimer();

        private void _SlideTimer_Tick(object sender, object e)
        {
            if (fv.SelectedIndex < ViewModel.View.Count - 1)
                fv.SelectedIndex++;
            else
                _SlideTimer.IsEnabled = false;
        }

        private static string _GetSlideLabel(bool isEnabled)
        {
            if (isEnabled)
                return Strings.Resources.Views.ImagePage.SlideAppBarButton.StopLabel;
            else
                return Strings.Resources.Views.ImagePage.SlideAppBarButton.StartLabel;
        }

        private static string _GetSlideSymbol(bool isEnabled)
            => (isEnabled ? (char)Symbol.StopSlideShow : (char)Symbol.SlideShow).ToString();
    }
}
