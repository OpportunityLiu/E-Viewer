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

        private void btn_pane_Click(object sender, RoutedEventArgs e)
        {
            RootControl.RootController.SwitchSplitView();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            var backColor = ((SolidColorBrush)fv.Background).Color;
            var needColor = cb_top_OpenAnimation.To.GetValueOrDefault();
            var toColor = Color.FromArgb(85,
                (byte)(backColor.R - 3 * (backColor.R - needColor.R)),
                (byte)(backColor.G - 3 * (backColor.G - needColor.G)),
                (byte)(backColor.B - 3 * (backColor.B - needColor.B)));
            cb_top.Background = new SolidColorBrush(toColor);
            fv.FlowDirection = SettingCollection.Current.ReverseFlowDirection ?
                FlowDirection.RightToLeft : FlowDirection.LeftToRight;

            base.OnNavigatedTo(e);

            cb_top.Visibility = Visibility.Visible;
            VM = await GalleryVM.GetVMAsync((long)e.Parameter);
            av.VisibleBoundsChanged += Av_VisibleBoundsChanged;
            Av_VisibleBoundsChanged(av, null);
            fv.Focus(FocusState.Pointer);
            RootControl.RootController.SetFullScreen(StatusCollection.Current.FullScreenInImagePage);
            if(SettingCollection.Current.KeepScreenOn)
            {
                displayRequest.RequestActive();
                displayActived = true;
            }
            if(!StatusCollection.Current.ImageViewTipShown)
                showTip();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            VM = null;

            StatusCollection.Current.FullScreenInImagePage = isFullScreen ?? false;
            RootControl.RootController.SetFullScreen(false);
            if(displayActived)
            {
                displayRequest.RequestRelease();
                displayActived = false;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            av.VisibleBoundsChanged -= Av_VisibleBoundsChanged;
        }

        bool? isFullScreen;

        private void Av_VisibleBoundsChanged(ApplicationView sender, object args)
        {
            var currentState = RootControl.RootController.IsFullScreen;
            if(currentState == isFullScreen)
                return;
            if(currentState)
            {
                abb_fullScreen.Icon = new SymbolIcon(Symbol.BackToWindow);
                abb_fullScreen.Label = LocalizedStrings.Resources.ImagePageBackToWindow;
            }
            else
            {
                abb_fullScreen.Icon = new SymbolIcon(Symbol.FullScreen);
                abb_fullScreen.Label = LocalizedStrings.Resources.ImagePageFullScreen;
            }
            isFullScreen = currentState;
        }

        private void setScale()
        {
            int lb = fv.SelectedIndex - 1;
            int ub = fv.SelectedIndex + 2;
            lb = lb < 0 ? 0 : lb;
            ub = ub > VM.Gallery.Count ? VM.Gallery.Count : ub;
            for(int i = lb; i < ub; i++)
            {
                if(i == fv.SelectedIndex)
                    continue;
                var selected = (FlipViewItem)fv.ContainerFromIndex(i);
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
            if(VM?.Gallery == null)
                return;
            var start = fv.SelectedIndex;
            if(start < 0)
                return;
            var end = start + 5;
            if(end > VM.Gallery.Count)
            {
                end = VM.Gallery.Count;
            }
            for(int i = start; i < end; i++)
            {
                VM.Gallery[i].LoadImageAsync(false, SettingCollection.Current.GetStrategy(), true).Completed =
                    (task, state) =>
                    {
                        if(state == AsyncStatus.Error)
                            RootControl.RootController.SendToast(task.ErrorCode, typeof(ImagePage));
                    };
            }
            if(end + 10 > VM.Gallery.Count && VM.Gallery.HasMoreItems)
            {
                if(loadItems == null || loadItems.Status != AsyncStatus.Started)
                {
                    loadItems = VM.Gallery.LoadMoreItemsAsync(5);
                }
            }
            setScale();
        }

        private System.Threading.CancellationTokenSource changingCbVisibility;

        private async void fvi_Tapped(object sender, TappedRoutedEventArgs e)
        {
            changingCbVisibility = new System.Threading.CancellationTokenSource();
            await Task.Delay(SettingCollection.Current.ChangeCommandBarDelay, this.changingCbVisibility.Token).ContinueWith(async t =>
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
            ApplicationView.GetForCurrentView().ShowStandardSystemOverlays();
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
            if(changingCbVisibility != null)
            {
                if(changingCbVisibility.IsCancellationRequested)
                    changeCbVisibility();
                else
                    changingCbVisibility.Cancel();
            }
            e.Handled = true;
        }

        private async void Flyout_Opening(object sender, object e)
        {
            VM.CurrentIndex = fv.SelectedIndex;
            await VM.RefreshInfoAsync();
        }

        private void cb_top_Opening(object sender, object e)
        {
            cb_top_Open.Begin();
        }

        private void cb_top_Closing(object sender, object e)
        {
            cb_top_Close.Begin();
        }

        private void abb_fullScreen_Click(object sender, RoutedEventArgs e)
        {
            RootControl.RootController.ChangeFullScreen();
        }

        protected override void OnKeyDown(KeyRoutedEventArgs e)
        {
            base.OnKeyDown(e);
            if(!enterPressed && e.Key == VirtualKey.Enter)
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
            switch(e.Key)
            {
            case VirtualKey.Enter:
                enterPressed = false;
                break;
            case VirtualKey.Application:
            case VirtualKey.GamepadMenu:
                if(!changeCbVisibility())
                    fv.Focus(FocusState.Programmatic);
                else
                    btn_pane.Focus(FocusState.Programmatic);
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
                Title = LocalizedStrings.Resources.ImageViewTipsTitle,
                Content = LocalizedStrings.Resources.ImageViewTipsContent,
                PrimaryButtonText = LocalizedStrings.Resources.OK,
                RequestedTheme = SettingCollection.Current.Theme.ToElementTheme()
            }.ShowAsync();
            StatusCollection.Current.ImageViewTipShown = true;
        }

        public void CloseAll()
        {
            cb_top.IsOpen = false;
        }

        public void SetImageIndex(int value)
        {
            this.fv.SelectedIndex = value;
        }
    }
}
